namespace BIVN.FixedStorage.Services.Common.API.Dto.Component
{
    public class ComponentItemFailedImportResultDto
    {
        public List<ComponentCellDto> ComponentItems { get; set; } = new List<ComponentCellDto>();
    }

    public class ComponentCellDto
    {
        public int? No { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierShortName { get; set; }
        public string PositionCode { get; set; }
        public string MinInventoryNumber { get; set; }
        public string MaxInventoryNumber { get; set; }
        public string InventoryNumber { get; set; }
        public string ComponentInfo { get; set; }
        public string Note { get; set; }
        public string Errors { get; set; }
        public int RowNumber { get; set; }
    }
}
