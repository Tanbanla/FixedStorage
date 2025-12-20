namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class CreateUserDto
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Code { get; set; }

        public string DepartmentId { get; set; }
        public List<DepartmentListDto> DepartmentList { get; set; } = new List<DepartmentListDto>();

        public string RoleId { get; set; }
        public List<CreateAppRoleDto> RoleList { get; set; } = new List<CreateAppRoleDto>();

        public int AccountType { get; set; }

        public List<CreateAccountTypeDto> AccountTypeList { get; set; } = new List<CreateAccountTypeDto>();

        public int Status { get; set; }

        public List<CreateUserStatusDto> UserStatusList { get; set; } = new List<CreateUserStatusDto>();

        public bool LockPwdSetting { get; set; } = false;

        public string? LockPwdTime { get; set; }

        public bool LockActSetting { get; set; } = false;

        public string? LockActTime { get; set; }
        public string? UserId { get; set; }
    }
}
