using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation
{
    public enum ErrorInvestigationConfirmType
    {
        [Display(Name = "Xác nhận điều tra")]
        ErrorInvestigationConfirm = 0,
        [Display(Name = "Cập nhật điều tra")]
        ErrorInvestigationUpdate = 1,
    }
}
