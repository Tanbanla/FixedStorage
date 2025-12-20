namespace Inventory.API.Infrastructure.Entity.Enums
{
    public enum DocHistoryStatus
    {
        [Display(Name = "Chưa tiếp nhận")]
        NotReceiveYet = 0,
        [Display(Name = "Không kiểm kê")]
        NoInventory = 1,
        [Display(Name = "Chưa kiểm kê")]
        NotInventoryYet = 2,
        [Display(Name = "Chờ xác nhận")]
        WaitingConfirm = 3,
        [Display(Name = "Cần chỉnh sửa")]
        MustEdit = 4,
        [Display(Name = "Đã xác nhận")]
        Confirmed = 5,
        [Display(Name = "Giám sát đạt")]
        AuditPassed = 6,
        [Display(Name = "Giám sát không đạt")]
        AuditFailed = 7,
    }
}
