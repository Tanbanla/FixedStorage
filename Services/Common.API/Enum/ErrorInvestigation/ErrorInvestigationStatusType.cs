using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation
{
    public enum ErrorInvestigationStatusType
    {
        [Microsoft.OpenApi.Attributes.Display("Chưa điều tra")]
        NotYetInvestigated = 0,
        [Microsoft.OpenApi.Attributes.Display("Đang điều tra")]
        UnderInvestigation = 1,
        [Microsoft.OpenApi.Attributes.Display("Đã điều tra")]
        Investigated = 2,
    }
}
