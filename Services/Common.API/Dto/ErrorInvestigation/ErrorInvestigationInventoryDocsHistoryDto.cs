using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ErrorInvestigationInventoryDocsHistoryDto
    {
        public int Index { get; set; }
        public int InvestigatingCount { get; set; }
        public string InventoryName { get; set; }
        public double? OldValue { get; set; }
        public double? NewValue { get; set; }
        public int ErrorCategory { get; set; }
        public string ErrorDetails { get; set; }
        public string Investigator { get; set; }
        public string InvestigationDatetime { get; set; }
        public string ConfirmInvestigationDatetime { get; set; }
        public string InvestigationImage1 { get; set; }
        public string InvestigationImage2 { get; set; }
        public string ErrorCategoryName { get; set; }
        public Guid InvestigatorId { get; set; }
    }
}
