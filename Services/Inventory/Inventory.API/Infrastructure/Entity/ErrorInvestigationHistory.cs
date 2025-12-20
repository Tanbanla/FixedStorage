using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums;

namespace BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity
{
#nullable enable
    public class ErrorInvestigationHistory:AuditEntity<Guid>
    {
        public required string ComponentCode { get; set; }
        public string? ComponentName { get; set; }
        public string? PositionCode { get; set; }
        public int AdjustmentNo { get; set; }
        public double OldValue { get; set; }
        public double NewValue { get; set; }
        public int ErrorCategory { get; set; }
        public required string ErrorDetails { get; set; }
        public Guid InvestigatorId { get; set; }
        public Guid? AdjusterId { get; set; }
        public DateTime ConfirmationTime { get; set; }
        public string? ConfirmationImage1{ get; set; }
        public string? ConfirmationImage2{ get; set; }
        public Guid ErrorInvestigationId { get; set; }
        public ErrorType? ErrorType { get; set; }
        public bool IsDelete { get; set; } = false;
        public Guid? ConfirmInvestigatorId { get; set; }
        public Guid? ApproveInvestigatorId { get; set; }
        public string? InvestigatorUserCode { get; set; }

        public virtual ErrorInvestigation ErrorInvestigation { get; set; }

    }
}
