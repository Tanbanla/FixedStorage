namespace Inventory.API.Infrastructure.Entity.Enums
{
    public enum InventoryAccountRoleType
    {
        [Display(Name = "Kiểm kê hoặc xác nhận")]
        Inventory = 0,
        [Display(Name = "Giám sát")]
        Audit = 1,
        [Display(Name = "Xúc tiến")]
        Promotion = 2,
        [Display(Name = "Xúc tiến - Người phụ trách")]
        PromotionPersonInCharge = 3,
        [Display(Name = "Xúc tiến - Người quản lý")]
        PromotionPersonInManagerment = 4,
        //[Display(Name = "Điều tra sai số")]
        //ErrorInvestigation = 3,
    }
}
