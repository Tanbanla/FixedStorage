namespace Inventory.API.Infrastructure.Entity.Enums
{
    public enum InventoryStatus
    {
        [Display(Name = "Chưa kiểm kê")]
        NotYet = 0,
        [Display(Name = "Đang kiểm kê")]
        Doing = 1,
        [Display(Name = "Đang giám sát")]
        Auditing = 2,
        [Display(Name = "Hoàn thành")]
        Finish = 3,
    }
}
