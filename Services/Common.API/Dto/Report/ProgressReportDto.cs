using BIVN.FixedStorage.Services.Common.API.Enum;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Report
{
    public class ProgressReportDto
    {
        public string InventoryId { get; set; }
        public int CaptureTimeType { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<int> DocTypes { get; set; } = new();

        //public string IsCheckAllDepartment { get; set; }
        //public string IsCheckAllLocation { get; set; }

    }

    public class AuditReportModel
    {
        public Guid InventoryId { get; set; }
        public AuditReportType AuditReportType { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<string> Auditors { get; set; } = new();
        public List<ReportingAuditType> ReportingAuditTypes { get; set; } = new();
    }

    public enum AuditReportType
    {
        Department,
        Location,
        Auditor
    }
}
