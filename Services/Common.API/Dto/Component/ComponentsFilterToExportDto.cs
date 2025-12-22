namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentsFilterToExportDto
    {
        public string? ComponentCode { get; set; }
        public string? ComponentName { get; set; }
        public string? SupplierName { get; set; }
        public string? ComponentPosition { get; set; }
        public int? ComponentInventoryQtyStart { get; set; }
        public int? ComponentInventoryQtyEnd { get; set; }
        public IList<string> LayoutIds { get; set; }
        public string? AllLayouts { get; set; }
        public string? InventoryStatus { get; set; }
        public IList<Guid> FactoryIds { get; set; }
    }
}
