using BIVN.FixedStorage.Services.Common.API.Dto.Report;

namespace Inventory.API.Service
{
    public interface IReportService
    {
        Task<ResponseModel<ProgressReportResult>> ProgressReport(ProgressReportDto progressReport);
        Task<ResponseModel<AuditReportViewModel>> AggregateAuditReport(ProgressReportDto progressReport);
        Task<ResponseModel<IEnumerable<AuditReportDto>>> AuditReports(AuditReportModel auditReportModel);
    }
}
