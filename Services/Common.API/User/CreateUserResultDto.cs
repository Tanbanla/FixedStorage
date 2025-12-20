namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class CreateUserResultDto
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Code { get; set; }

        public string DepartmentId { get; set; }

        public string RoleId { get; set; }

        public int AccountType { get; set; }

        public int Status { get; set; }

        public bool LockPwdSetting { get; set; } = false;

        public int? LockPwTime { get; set; }

        public bool LockActSetting { get; set; } = false;

        public int? LockActTime { get; set; }
    }
}
