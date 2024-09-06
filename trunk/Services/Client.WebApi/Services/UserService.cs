using Entities.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.WebApi.Services
{
    public interface IUserService
    {
        long GetUserId();
        HRMUser GetUserInfo();
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _context;
        private IConfiguration _config;
        public UserService(IHttpContextAccessor context, IConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config;
        }

        public long GetUserId()
        {

            try
            {
                AES256 aes256 = new AES256();
                return long.Parse(aes256.Decrypt(_context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId").Value, _config["AES256:Key"]));
            }
            catch
            {
                return 0;
            }
        }

        public HRMUser GetUserInfo()
        {
            HRMUser user = new HRMUser();
            try
            {
                AES256 aes256 = new AES256();
                user.EmployeeId = long.Parse(aes256.Decrypt(_context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeId").Value, _config["AES256:Key"]));
                user.EmployeeName = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeName").Value;
                user.EmployeeCode = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeCode").Value;
                user.EmployeeEmail = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeEmail").Value;
                user.ProfileId = long.Parse(aes256.Decrypt(_context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "ProfileId").Value, _config["AES256:Key"]));
                user.ProfileName = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "ProfileName").Value;
                if (_context.HttpContext.User.Claims.Where(c => c.Type == "Extension").Any())
                {
                    user.Extension = _context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Extension").Value;
                }
            }
            catch
            {
                throw new NotImplementedException();
            }
            return user;
        }
    }
}
