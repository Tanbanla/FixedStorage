using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums;

namespace BIVN.FixedStorage.Services.Inventory.API.Service.Dto.ErrorInvestigation
{
    public class ErrorInvestigationConfirmModel
    {
        [Required(ErrorMessage = "Quantity is required.")]
        public double Quantity { get; set; }
        [Required(ErrorMessage = "ErrorCategory is required.")]
        public int ErrorCategory { get; set; }
        public string ErrorDetails { get; set; }
        public IFormFile? ConfirmationImage1 { get; set; }
        public IFormFile? ConfirmationImage2 { get; set; }
        public bool IsDeleteImage1 { get; set; } = false;
        public bool IsDeleteImage2 { get; set; } = false;
        [RegularExpression(@"^[FMT][0-9]{7}$", ErrorMessage = "Mã nhân viên không đúng định dạng. Vui lòng thử lại.")]
        public string UserCode { get; set; }
    }
}
