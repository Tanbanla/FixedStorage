using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Enum
{
    public enum GeneralSettingType
    {
        [Microsoft.OpenApi.Attributes.Display("Phân loại sai số")]
        ErrorCategory,
        [Microsoft.OpenApi.Attributes.Display("Tỷ lệ điều tra")]
        InvestigationPercent
    }
}
