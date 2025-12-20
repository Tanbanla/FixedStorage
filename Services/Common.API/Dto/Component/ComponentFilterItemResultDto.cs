namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentFilterItemResultDto
    {
        public Guid? Id { get; set; }        
        public string? ComponentCode { get; set; }
        public string? ComponentName { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public string? ComponentPosition { get; set; }
        public double? InventoryNumber { get; set; }
        public double? MaxInventoryNumber { get; set; }
        public double? MinInventoryNumber { get; set; }
        public bool IsNewCreateAt { get; set; }
        public bool IsNewUpdateAt { get; set; }
        public string? SupplierShortName { get; set; }
        public string? ComponentInfo { get; set; }
        public string? Note { get; set; }
    }
}
