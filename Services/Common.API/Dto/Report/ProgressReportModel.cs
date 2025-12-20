namespace BIVN.FixedStorage.Services.Common.API.Dto.Report
{
    public class ProgressReportModel
    {
        public int TotalDoc { get; set; }
        public int TotalTodo { get; set; }
        public int TotalInventory { get; set; }
        public int TotalConfirm { get; set; }
        public int DocType { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
    }
    public class ProgressReportResult
    {
        public List<ProgressReportModel> ProgressReportDocTypes { get; set; }
        public List<ProgressReportModel> ProgressReportDepartments { get; set; }
        public List<ProgressReportModel> ProgressReportLocations { get; set; }
    }
}
