using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ListErrorInvestigationHistoryWebModel
    {
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string AssigneeAccount { get; set; }
        public IEnumerable<int>? ErrorCategories { get; set; } = Enumerable.Empty<int>();
        public IEnumerable<Guid>? InventoryIds { get; set; } = Enumerable.Empty<Guid>();
        public double? ErrorQuantityFrom { get; set; }
        public double? ErrorQuantityTo { get; set; }
        public double? ErrorMoneyFrom { get; set; }
        public double? ErrorMoneyTo { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsExportExcel { get; set; } = false;
        public IEnumerable<ErrorType>? ErrorTypes { get; set; } = Enumerable.Empty<ErrorType>();
    }
}
