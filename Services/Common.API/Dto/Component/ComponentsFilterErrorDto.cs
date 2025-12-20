namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentsFilterErrorDto
    {
        public string? ComponentCode { get; set; }
        public string? ComponentName { get; set; }
        public string? SupplierName { get; set; }
        public string? ComponentPosition { get; set; }
        public string ComponentLayout { get; set; }
        public int? ComponentInventoryQtyStart { get; set; }
        public int? ComponentInventoryQtyEnd { get; set; }
        public string? InventoryStatus { get; set; }
    }
}
