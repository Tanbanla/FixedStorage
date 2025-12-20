namespace Inventory.API.HostedServices.Dto
{
    public class ReportingLocationDto : ReportingDto
    {
        public string LocationName { get; set; }
        public int TotalInventory { get; set; }
        public int TotalConfirm { get; set; }
    }
}
