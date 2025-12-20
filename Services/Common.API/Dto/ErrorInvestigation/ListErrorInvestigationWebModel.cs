using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ListErrorInvestigationWebModel
    {
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string AssigneeAccount { get; set; }
        public IEnumerable<int>? ErrorCategories { get; set; }
        public IEnumerable<ErrorInvestigationStatusType>? Statuses { get; set; }
        public double? ErrorQuantityFrom { get; set; }
        public double? ErrorQuantityTo { get; set; }
        public double? ErrorMoneyFrom { get; set; }
        public double? ErrorMoneyTo { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public IEnumerable<Guid>? InventoryIds { get; set; }
        public bool IsExportExcel { get; set; } = false;
        public string SortColumn { get; set; }
        public string SortColumnDirection { get; set; }
        public string ComponentName { get; set; }
    }
}
