namespace BIVN.FixedStorage.Services.Common.API.User
{
    public enum AccountType
    {
        [Display(Name = "Tài khoản riêng")]
        TaiKhoanRieng = 0,

        [Display(Name = "Tài khoản chung")]
        TaiKhoanChung = 1,

        [Display(Name = "Tài khoản giám sát")]
        TaiKhoanGiamSat = 2,
    }
}
