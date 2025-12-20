namespace Inventory.API.Service.Dto
{
    public class AggregateDocTypeCDetailDto
    {
        public Guid InventoryId { get; set; }
        public Guid? InventoryDocId { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
        public string? ComponentCode { get; set; }
        public string? ModelCode { get; set; }
        public string DirectParent { get; set; }

    }
}
