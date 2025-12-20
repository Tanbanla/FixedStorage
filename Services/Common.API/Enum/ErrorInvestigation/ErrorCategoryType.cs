namespace BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation
{
    public enum ErrorCategoryType
    {
        [Microsoft.OpenApi.Attributes.Display("Kiểm kê sai")]
        Inventory,
        [Microsoft.OpenApi.Attributes.Display("Quy cách đóng gói")]
        Packaging,
        [Microsoft.OpenApi.Attributes.Display("Lỗi không thống kê")]
        Statistical,
        [Microsoft.OpenApi.Attributes.Display("Không rõ nguyên nhân")]
        Unknown,
        [Microsoft.OpenApi.Attributes.Display("BOM sai")]
        BOM,
        [Microsoft.OpenApi.Attributes.Display("Dùng nhầm")]
        Misuse,
        [Microsoft.OpenApi.Attributes.Display("Khác")]
        Other
    }
    public enum ErrorType
    {
        [Microsoft.OpenApi.Attributes.Display("Giữ lại")]
        Retain,
        [Microsoft.OpenApi.Attributes.Display("Chờ xác nhận")]
        AwaitConfirmation,
        [Microsoft.OpenApi.Attributes.Display("Điều chỉnh")]
        Adjustment,
        [Microsoft.OpenApi.Attributes.Display("Từ chối")]
        Cancel
    }
    public enum AdjustmentType
    {
        [Microsoft.OpenApi.Attributes.Display("Điều chỉnh")]
        Adjust,
        [Microsoft.OpenApi.Attributes.Display("Xác nhận điều chỉnh")]
        AdjustConfirm,
        [Microsoft.OpenApi.Attributes.Display("Từ chối điều chỉnh")]
        AdjustReject
    }

}
