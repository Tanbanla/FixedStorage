namespace Inventory.API.Infrastructure.Entity.Enums
{
    public enum DocHistoryActionType
    {
        [Display(Name = "Kiểm kê")]
        Inventory = 0,
        [Display(Name = "Xác nhận")]
        Confirm = 1,
        [Display(Name = "Giám sát")]
        Audit = 2,
        [Display(Name = "Chỉnh sửa thông tin")]
        EditInfo = 3,
        [Display(Name = "Tiếp nhận")]
        Received = 4,
        [Display(Name = "Khác")]
        Other = 5,
    }
}
