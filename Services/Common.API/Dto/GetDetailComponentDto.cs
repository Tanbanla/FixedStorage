namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class GetDetailComponentDto
    {
        public Guid Id { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierShortName { get; set; }
        public string PositionCode { get; set; }
        public string ComponentInfo { get; set; }
        public string Note { get; set; }
        public string CreatedName { get; set; }
        public string UpdatedName { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public double MinInventoryNumber { get; set; }
        public double MaxInventoryNumber { get; set; }
        public double InventoryNumber { get; set; }

    }
}
