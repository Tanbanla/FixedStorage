namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class UserDetailDto
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Fullname { get; set; }

        public string Code { get; set; }

        public string DepartmentId { get; set; }

        public List<DropDownListItem> DepartmentList { get; set; } = new List<DropDownListItem>();

        public string RoleId { get; set; }

        public List<DropDownListItem> RoleList { get; set; } = new List<DropDownListItem>();

        public int? AccountType { get; set; }
        public List<DropDownListItemInteger> AccountTypeList { get; set; } = new List<DropDownListItemInteger>();

        public int? Status { get; set; }

        public List<DropDownListItemInteger> UserStatusList { get; set; } = new List<DropDownListItemInteger>();

        public bool? LockPwdSetting { get; set; }

        public int? LockPwdTime { get; set; }

        public bool? LockActSetting { get; set; }

        public int? LockActTime { get; set; }

        public string DepartmentName { get; set; }
        public string RoleName { get; set; }
        public string StatusName { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string AccountTypeName { get; set; }
        public string LastLoginTime { get; set; }
    }
}
