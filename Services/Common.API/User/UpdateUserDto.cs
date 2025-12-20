namespace BIVN.FixedStorage.Services.Common.API.User
{
    public class UpdateUserDto
    {
        public string Id { get; set; }
        
        public string Username { get; set; }

        public string FullName { get; set; }
       
        public string Code { get; set; }

        public string DepartmentId { get; set; }
        
        public string RoleId { get; set; }
       
        public int AccountType { get; set; }
        
        public int Status { get; set; }
        
        /// <summary>
        /// Cho phép khóa tài khoản khi hết hạn cập nhật mật khẩu
        /// </summary>
        public bool LockPwdSetting { get; set; }

        /// <summary>
        /// Số ngày để khóa tài khoản khi hết hạn cập nhật mật khẩu, bắt đầu từ ngày tương tác cuối cùng
        /// </summary>
        public string? LockPwdTime { get; set; }

        /// <summary>
        /// Cho phép khóa tài khoản khi đã lâu không tương tác
        /// </summary>
        public bool LockActSetting { get; set; }

        /// <summary>
        /// Số ngày để khóa tài khoản khi đã lâu không tương tác, tính từ ngày tương tác cuối cùng
        /// </summary>
        public string? LockActTime { get; set; }

        public string UpdatedBy { get; set; }
    }
}
