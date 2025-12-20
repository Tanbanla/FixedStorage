#nullable enable
namespace Inventory.API.HostedServices.Dto
{
    public class ReportingAuditDto : ReportingDto
    {
        public string? LocationtName { get; set; }
        public int TotalPass { get; set; }
        public int TotalFail { get; set; }
        public string? DepartmentName { get; set; }
        public string? AuditorName { get; set; }
        public ReportingAuditType Type { get; set; }
    }
}
