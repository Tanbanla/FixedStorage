namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class HistoryOutputModel
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid HistoryId { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
    }
}
