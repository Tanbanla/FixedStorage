namespace BIVN.FixedStorage.Inventory.Inventory.API.HostedServices.Dto
{
    public class InventoryDocSubmitDto
    {
        public Guid InventoryId { get; set; }
        public List<Guid> InventoryDocIds { get; set; } = new();
        public List<string> ModelCodes { get; set; } = new();
        public InventoryDocType DocType { get; set; }
        public bool? ForceAggregate { get; set; }
        public DateTime? ForceAggregateAt { get; set; }
    }
}
