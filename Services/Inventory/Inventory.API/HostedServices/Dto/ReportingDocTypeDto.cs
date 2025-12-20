namespace Inventory.API.HostedServices.Dto
{
    public class ReportingDocTypeDto : ReportingDto
    {
        public InventoryDocType DocType { get; set; }
        public int TotalInventory { get; set; }
        public int TotalConfirm { get; set; }
    }
}
