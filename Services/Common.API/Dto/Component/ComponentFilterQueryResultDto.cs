namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentFilterQueryResultDto
    {
        public Guid Id { get; set; }
        public string? Layout { get; set; }
        public Guid? FactoryId { get; set; }
        public string? ComponentCode { get; set; }
        public string? ComponentName { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public string? ComponentPosition { get; set; }
        public double? InventoryNumber { get; set; }
        public double? MaxInventoryNumber { get; set; }
        public double? MinInventoryNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? SupplierShortName { get; set; }
        public string? ComponentInfo { get; set; }
        public string? Note { get; set; }
    }
}
