namespace Inventory.API.HostedServices.Dto
{
    public class ReportingDto
    {
        public Guid InventoryId { get; set; }
        public int TotalDoc { get; set; }
        public int TotalTodo { get; set; }
        public InventoryDocStatus CaptureTimeType { get; set; }
    }
}
