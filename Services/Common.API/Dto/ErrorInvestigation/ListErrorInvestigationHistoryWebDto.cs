using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ListErrorInvestigationHistoryWebDto
    {
        public Guid ErrorInvestigationId { get; set; }
        public Guid ErrorInvestigationHistoryId { get; set; }
        public string InventoryName { get; set; }
        public Guid InventoryId { get; set; }
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string Position { get; set; }
        public double? TotalQuantity { get; set; }
        public double? AccountQuantity { get; set; }
        public double? ErrorQuantity { get; set; }
        public double? ErrorMoney { get; set; }
        //public double? UnitPrice { get; set; }
        //public double? ErrorQuantityAbs { get; set; }
        //public double? ErrorMoneyAbs { get; set; }
        public string AssigneeAccount { get; set; }
        public double? InvestigationQuantity { get; set; }
        public int? ErrorCategory { get; set; }
        public string ErrorDetail { get; set; }
        public string Investigator { get; set; }
        public string InvestigationDateTime { get; set; }
        //public double? InvestigationTotal { get; set; }
        public ErrorInvestigationStatusType Status { get; set; }
        public string InvestigationHistoryCount { get; set; }
        public string NoteDocumentTypeA { get; set; }
        public ErrorType? ErrorType { get; set; }
        public string ConfirmInvestigator { get; set; }
        public string ApproveInvestigator { get; set; }
        public string ErrorCategoryName { get; set; }
        public Guid InvestigatorId { get; set; }

    }
}
