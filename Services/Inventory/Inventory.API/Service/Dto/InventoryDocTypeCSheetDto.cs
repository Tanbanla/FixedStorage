namespace Inventory.API.Service.Dto
{
    public class InventoryDocTypeCSheetDto
    {
        public string Plant { get; set; }
        //public string WarehouseLocation { get; set; }
        public string SheetName { get; set; }
        public string ModelCode { get; set; }
        
        public List<InventoryDocTypeCCellDto> Rows { get; set; }
    }
}
