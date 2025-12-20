namespace BIVN.FixedStorage.Services.Common.API.User
{
    public enum UserStatus
    {
        [Display(Name = "Đang sử dụng")]
        Active = 0,

        [Display(Name = "Khóa do mật khẩu hết hạn")]
        LockByExpiredPassword = 1,

        [Display(Name = "Khóa do không tương tác")]
        LockByUnactive = 2,

        [Display(Name = "Khóa bởi admin")]
        LockByAdmin = 3,

        [Display(Name = "Đã xóa")]
        Deleted = 4
    }
}
