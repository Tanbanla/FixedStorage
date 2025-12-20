namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    /// <summary>
    /// Đăng nhập
    /// </summary>
    public class LogoutDto
    {
        /// <summary>
        /// Tên tài khoản đăng nhập
        /// Cho phép nhập Tên tài khoản đăng nhập trong khoảng 4 đến 50 ký tự
        /// </summary>
        public Guid UserId { get; set; }     
    }
}
