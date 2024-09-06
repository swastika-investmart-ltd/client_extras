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
using ElmahCore;
using ElmahCore.Mvc;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using StackExchange.Redis;
using AspNetCoreRateLimit;
using System.Collections.Generic;
using Microsoft.AspNetCore.HttpOverrides;
using Client.WebApi.Services;
using System.ComponentModel;
using Client.WebApi;
using Prometheus;
using Client.WebApi.Extensions;


namespace Client.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            ///LogManager.LoadConfiguration(Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"));
            LogManager.LoadConfiguration(Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"));
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //Nlog Setting
            services.AddSingleton<ILog, LogNLog>();

            //Get AppSetting
            var appSettingSection = Configuration.GetSection("AppSettings");
            var strUrl = Configuration["APIURL:URL"];
            //Swagger bearer token pass setting
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Client.WebApi", Version = "v1" });
                //services.AddSwaggerGen(c =>
                //{
                //    c.SwaggerDoc("v1", new OpenApiInfo { Title ="CRM", Version = "v1", });
                //});
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement());
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                   {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      new string[] { }
                    }
                  });
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)System.Net.HttpStatusCode.TemporaryRedirect;
                options.HttpsPort = 5001;
            });
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

            //Get SQL Connectionstring

            services.AddTransient<SqlConnection>((sp) => new SqlConnection(this.Configuration.GetConnectionString("sqlconnection")));

            //For Validation Message
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            //If you want PascalCase serialization use this code: (for example FirstName)
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            //Added Code For JWT Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:AccessTokenSecret"]))
                };
            });

            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpClient();
            services.AddScoped<IClientReferralService, ClientReferralService>();
            services.AddScoped<IWealthBagPortfolioService, WealthBagPortfolioService>();
            services.AddSingleton<IWealthBagPortfolioService, WealthBagPortfolioService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICommunicationService, CommunicationService>();
            services.AddSingleton<ApiKeyAuthorizationFilter>();
            services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<IHttpClientPostService, HttpClientPostService>();
            services.AddScoped<IRPTradingoService, RPTradingoService>();

            //services.AddCors(c =>
            //{
            //    c.AddPolicy("AllowAllOfOrigin", options => options
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    //.AllowCredentials()
            //    .SetPreflightMaxAge(TimeSpan.FromSeconds(3600)));
            //});
            //Elmah Setting
            services.AddElmah<XmlFileErrorLog>(options =>
            {
                options.LogPath = Path.Combine(AppContext.BaseDirectory, "/Log"); // OR options.LogPath = "с:\errors";
            });

            //services.AddCors(options => {
            //    options.AddPolicy(name: "AllowSpecificOrigins",
            //            policy =>
            //            {
            //                policy.WithOrigins("https://swastika.co.in", "https://jarvis.swastika.co.in", "http://stagingcrm.swastika.co.in", "http://192.168.0.24:85/", "http://localhost:4200/");
            //            });
            //});

            services.AddCors();
            // services.AddControllers();
            services.AddSignalR();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.AddMvc(op => op.EnableEndpointRouting = false);


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
        }
        //builder => builder.WithOrigins("*", "http://localhost:4200")
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false,
                ForwardLimit = null,
            });
            app.UseIpRateLimiting();
            if (env.EnvironmentName.Equals("Development"))
            {
                app.UseDeveloperExceptionPage();
            }

            // Exporting Metrics
            // TODO: Add authentication for /metrics end-point
            app.UseHttpMetrics();
            app.Map("/metrics", metricsApp =>
            {
                metricsApp.UseMiddleware<PrometheusAuthentication>("");

                // We already specified URL prefix in .Map() above, no need to specify it again here.
                metricsApp.UseMetricServer("");
            });

            app.UseRouting();
            // app.UseCors("AllowSpecificOrigins");
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Client.WebApi");
            });
            app.UseApiResponseAndExceptionWrapper();
            app.UseElmah();

            app.UseMvc();

            var portfolioService = serviceProvider.GetRequiredService<IWealthBagPortfolioService>();
            portfolioService.SavePortfolioDataInMemory().Wait();
        }
    }
}
