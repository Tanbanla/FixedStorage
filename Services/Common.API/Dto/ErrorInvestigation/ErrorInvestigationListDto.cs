using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;

namespace BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation
{
    public class ErrorInvestigationListDto
    {
        public Guid ErrorInvestigationId { get; set; }
        public string ComponentCode { get; set; }
        public double? Quantity { get; set; }
        public double? ErrorMoneyAbs { get; set; }
        public ErrorInvestigationStatusType? Status { get; set; }
        public string PositionCode { get; set; }
        public string ComponentName { get; set; }

    }
}
