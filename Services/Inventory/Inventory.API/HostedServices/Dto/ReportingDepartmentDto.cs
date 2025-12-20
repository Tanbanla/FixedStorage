namespace Inventory.API.HostedServices.Dto
{
    public class ReportingDepartmentDto : ReportingDto
    {
        public string DepartmentName { get; set; }
        public int TotalInventory { get; set; }
        public int TotalConfirm { get; set; }
    }
}
