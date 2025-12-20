namespace BIVN.FixedStorage.Services.Common.API.Enum.Storage
{
    public enum InventoryStatus
    {

        [Display(Name = "Tất cả")]
        All = 0,

        [Display(Name = "Vị trí gần hết linh kiện")]
        PositionNearlyOutOfStock = 1,

        [Display(Name = "Linh kiện mới được cập nhật")]
        NewComponentsHasJustUpdated = 2
    }
}
