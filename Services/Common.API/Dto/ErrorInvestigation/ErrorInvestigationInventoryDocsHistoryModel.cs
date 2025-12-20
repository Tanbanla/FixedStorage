using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ErrorInvestigationInventoryDocsHistoryModel
    {
        public IEnumerable<string>? InventoryNames { get; set; }
    }
}
