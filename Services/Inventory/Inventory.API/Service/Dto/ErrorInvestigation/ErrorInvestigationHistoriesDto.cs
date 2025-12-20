using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums;

namespace Inventory.API.Service.Dto.ErrorInvestigation
{
    public class ErrorInvestigationHistoriesDto
    {
        public double? OldValue { get; set; }
        public double? NewValue { get; set; }
        public int Index { get; set; }
        public int ErrorCategory { get; set; }
        public string ErrorDetail { get; set; }
        public string Investigator { get; set; }
        public string InvestigationTime { get; set; }
        public string ConfirmInvestigationTime { get; set; }
        public string ConfirmationImage1 { get; set; }
        public string ConfirmationImageTitle1 { get; set; }
        public string ConfirmationImage2 { get; set; }
        public string ConfirmationImageTitle2 { get; set; }
        public string ErrorCategoryName { get; set; }
    }
}
