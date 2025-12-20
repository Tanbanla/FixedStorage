using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums;

namespace Inventory.API.Service.Dto.ErrorInvestigation
{
    public class ErrorInvestigationConfirmedViewDetailDto
    {
        public double? ErrorQuantity { get; set; }
        public int? ErrorCategory { get; set; }
        public string ErrorDetails { get; set; }
        public string ConfirmationImage1 { get; set; }
        public string ConfirmationImageTitle1 { get; set; }
        public string ConfirmationImage2 { get; set; }
        public string ConfirmationImageTitle2 { get; set; }
        public ErrorInvestigationStatusType Status { get; set; }
    }
}
