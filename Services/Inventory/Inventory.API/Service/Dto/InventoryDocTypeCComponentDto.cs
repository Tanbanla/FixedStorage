namespace Inventory.API.Service.Dto
{
    public class InventoryDocTypeCComponentDto
    {
        public string ModelCode { get; set; }
        public string AttachModelCode { get; set; }
        public string ComponentCode { get; set; }
        public double QuantityOfBOM { get; set; }
        public string Plant { get; set; }
        public string WarehouseLocation { get; set; }
        public int Quantity { get; set; }
        public List<InventoryDocTypeCComponentDto> Attachment { get; set; }
    }
}
