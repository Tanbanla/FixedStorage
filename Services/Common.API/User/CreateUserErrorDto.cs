namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class CreateUserErrorDto
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Code { get; set; }

        public string DepartmentId { get; set; }

        public string RoleId { get; set; }

        public string AccountType { get; set; }

        public string Status { get; set; }

        public string LockPwdSetting { get; set; }

        public string LockPwTime { get; set; }

        public string LockActSetting { get; set; }

        public string LockActTime { get; set; }
    }
}
