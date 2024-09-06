using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Common
{
    public class HRMUser
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeUserName { get; set; }

        public long BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }

        public long ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string Extension { get; set; }
    }
}
