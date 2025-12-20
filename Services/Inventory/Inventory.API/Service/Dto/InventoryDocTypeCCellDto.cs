namespace Inventory.API.Service.Dto
{
    public class InventoryDocTypeCCellDto
    {
        public required string No { get; set; }
        public required string Plant { get; set; }
        public required string WareHouseLocation { get; set; }
        public required string ModelCode { get; set; }
        public required string MaterialCode { get; set; }
        public required string BOMUseQty { get; set; }
        public required string StageName { get; set; }
        public required string Assignee { get; set; }
        public required int RowNumber { get; set; }
        public string SheetName { get; set; }
    }
}
