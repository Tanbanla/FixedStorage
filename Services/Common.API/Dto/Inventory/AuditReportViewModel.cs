using System;
using System.Reflection.Metadata.Ecma335;
using BIVN.FixedStorage.Services.Common.API.Dto.Report;
using BIVN.FixedStorage.Services.Common.API.Enum;

namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ProgressReportModelAudit
    {
        public int TotalDoc { get; set; }
        public int TotalTodo { get; set; }
        //public int DocType { get; set; }
        public string LocationName { get; set; }
        public int TotalPass { get; set; }
        public int TotalFail { get; set; }
    }              

    public class AuditReportViewModel
    {
        public List<ProgressReportModelAudit> ProgressReportLocations { get; set; }
    }


    public class AuditReportDto
    {
        public int TotalDoc { get; set; }
        public int TotalTodo { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public int TotalPass { get; set; }
        public int TotalFail { get; set; }
        public ReportingAuditType ReportingAuditType { get; set; }
    }

}
