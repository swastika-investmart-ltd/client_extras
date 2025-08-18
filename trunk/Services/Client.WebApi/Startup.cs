using Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using AspNetCoreRateLimit;
using System.Collections.Generic;
using Microsoft.AspNetCore.HttpOverrides;
using Client.WebApi.Services;
using Prometheus;
using Client.WebApi.Extensions;
using Client.WebApi.HostedService;
using Client.WebApi.Models.Base;

namespace Client.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            LogManager.LoadConfiguration(Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"));
            Configuration = configuration;
            Environment = environment;
        }
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // BaseService configuration
            var configurationService = new ConfigurationService(Configuration);
            BaseService.SetConfigurationService(configurationService);

            // Other singletons and services
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ILog, LogNLog>();

            // Register the custom authorization filter globally
            services.AddControllers(config =>
            {
                config.Filters.Add<TokenAuthorizationFilter>();
            });

            // Registering XApi Auth Filter
            services.AddSingleton<ApiKeyAuthFilter>();

            // Authentication and JWT Bearer Configuration
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.RequireHttpsMetadata = false;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:AccessTokenSecret"]))
                };
            });

            // Add Controller Serialization Options - For PascalCase serialization(for example FirstName) 
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Customize Validation Message Behavior - Custom validation message handling
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Swagger Configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Client.WebApi", Version = "v1" });

                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                 {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                 }
                });
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)System.Net.HttpStatusCode.TemporaryRedirect;
                options.HttpsPort = 5001;
            });

            // Additional Services and CORS Configuration
            services.AddCors();
            services.AddSignalR();

            // Adds MVC services with the latest compatibility version settings to the service container.
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

            // Adds MVC services and disables endpoint routing to use the traditional MVC routing system.
            services.AddMvc(op => op.EnableEndpointRouting = false);

            //Get SQL Connectionstring
            services.AddTransient<SqlConnection>((sp) => new SqlConnection(this.Configuration.GetConnectionString("sqlconnection")));

            services.AddHttpClient();

            services.AddScoped<IWealthBagService, WealthBagService>();
            services.AddScoped<ICommunicationService, CommunicationService>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<IHttpClientPostService, HttpClientPostService>();
            services.AddScoped<IRPTradingoService, RPTradingoService>();
            services.AddScoped<IXApiKeysLoader, XApiKeysLoader>();
            services.AddScoped<IContactsService, ContactsService>();


            #region API Rate Limit Integration
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = 429;
                options.RealIpHeader = "X-Real-IP";
                //options.ClientIdHeader = "X-ClientId";
                options.ClientIdHeader = null; // Set to null to rely on IP address as the client identifier
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = Configuration["RateLimitSetting:Period"],
                        Limit = Convert.ToDouble(Configuration["RateLimitSetting:Limit"])
                    }
                };
            });
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddInMemoryRateLimiting();
            #endregion

            // Registering the hosted service - for load memory data
            services.AddHostedService<DataLoader>();

            // Register path sets
            ConfigurePathSets(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.EnvironmentName.Equals("Development"))
            {
                app.UseDeveloperExceptionPage();
            }

            //IPAddress setting
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false,
                ForwardLimit = null,
            });

            app.UseIpRateLimiting();
            app.UseRouting();

            // Exporting Metrics
            // TODO: Add authentication for /metrics end-point
            app.UseHttpMetrics();
            app.Map("/metrics", metricsApp =>
            {
                metricsApp.UseMiddleware<PrometheusAuthentication>("");

                // We already specified URL prefix in .Map() above, no need to specify it again here.
                metricsApp.UseMetricServer("");
            });

            // Move CORS to be executed early in the pipeline, before authentication
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials 

            // Ensure authentication middleware is called before authorization
            app.UseAuthentication(); // This must come before UseAuthorization	
            app.UseAuthorization();

            //This line enables the app to use Swagger, with the configuration in the ConfigureServices method.
            app.UseSwagger();

            //This line enables Swagger UI, which provides us with a nice, simple UI with which we can view our API calls.
            if (Configuration.GetValue<bool>("Swagger:isSwaggerEnabled"))
            {
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Client.WebApi"); });
            }
            else
            {
                app.Map("/swagger", HandleSwaggerRequests);
            }

            // Custom Middleware
            app.UseApiResponseAndExceptionWrapper();

            // Disable endpoint-based routing and switch to the legacy routing system using UseMvc().
            app.UseMvc();
        }

        private static void HandleSwaggerRequests(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                // Block the request by returning a 404 status code
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Swagger endpoint not found.");
            });
        }

        private void ConfigurePathSets(IServiceCollection services)
        {
            // Configure named options for recomondation key 
            services.Configure<PathOptions>("Recomkeys", options =>
            {
                options.Paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                            // "New",
                            "Commodity"
                            ,"Delivery"
                            ,"FNO"
                            ,"Intraday"
                            ,"Commodity_Delivery"
                            ,"Commodity_FNO"
                            ,"Commodity_Intraday"
                            ,"Delivery_FNO"
                            ,"Delivery_Intraday"
                            ,"FNO_Intraday"
                            ,"Commodity_Delivery_FNO"
                            ,"Commodity_Delivery_Intraday"
                            ,"Commodity_FNO_Intraday"
                            ,"Delivery_FNO_Intraday"
                            ,"Commodity_Delivery_FNO_Intraday"
                    };
            });
        }
    }
}
