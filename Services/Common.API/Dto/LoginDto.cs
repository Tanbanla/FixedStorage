namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    /// <summary>
    /// Đăng nhập
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Tên tài khoản đăng nhập
        /// Cho phép nhập Tên tài khoản đăng nhập trong khoảng 4 đến 50 ký tự
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Mật khẩu
        /// Cho phép nhập Mật khẩu trong khoảng 8 đến 15 ký tự
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Id thiết bị 
        /// Mobile phải truyền lên Id của thiết bị
        /// Website không cần truyền lên Id thiết bị
        /// </summary>
        public string DeviceId { get; set; }
    }
}
