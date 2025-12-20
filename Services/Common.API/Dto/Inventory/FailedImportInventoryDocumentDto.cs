namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class FailedImportInventoryDocumentDto
    {
        public int? No { get; set; }
        public string Plant { get; set; }
        public string WarehouseLocation { get; set; }
        public string ComponentCode { get; set; }
        public string PositionCode { get; set; }
        public string Errors { get; set; }
    }
}
