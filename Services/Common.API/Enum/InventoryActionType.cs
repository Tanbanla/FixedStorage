namespace BIVN.FixedStorage.Services.Common.API.Enum
{
    public enum InventoryActionType
    {
        [Display(Name = "Kiểm kê")]
        Inventory = 0,
        [Display(Name = "Xác nhận kiểm kê")]
        ConfirmInventory = 1,
        [Display(Name = "Giám sát")]
        Audit = 2,
    }

    public enum ReportingAuditType
    {
        [Microsoft.OpenApi.Attributes.Display("Báo cáo giám sát cố định")]
        FixedReportingAudit,
        [Microsoft.OpenApi.Attributes.Display("Báo cáo giám sát tự do")]
        FreeReportingAudit
    }

}
