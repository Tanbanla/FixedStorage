namespace BIVN.FixedStorage.Services.Common.API
{
    // System.Net.HttpStatusCode (enum)
    // Microsoft.AspNetCore.Http.StatusCodes (static class)
    // EnumHelper<HttpStatusCodesExtension>.GetDisplayValue(item.ComponentNotExist);
    // EnumHelper<Status>.GetValueFromName
    public enum HttpStatusCodes
    {
        // Custom
        [Display(Name = "Khóa do không cập nhật mật khẩu")]
        LockByNotUpdatePassword = 10,

        [Display(Name = "Khóa do không tương tác")]
        LockByUnactive = 11,

        [Display(Name = "Khóa bởi Admin")]
        LockByAdmin = 12,

        [Display(Name = "Tài khoản không tồn tại")]
        TheAccountIsNotExisted = 13,

        [Display(Name = "Mật khẩu Tài khoản không đúng")]
        ThePasswordIsNotCorrect = 14,

        [Display(Name = "Tài khoản đã được đăng nhập ở máy khác")]
        ThisAccountHasBeenLoggedIntoOtherDeviceBefore = 15,

        [Display(Name = "Tài khoản đã bị xóa")]
        ThisAccountHasBeenDeleted = 16,

        [Display(Name = "Token hết hạn sử dụng")]
        TokenHasExpired = 17,

        [Display(Name = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.")]
        TokenIsInvalid = 18,

        [Display(Name = "Chữ ký của Token không hợp lệ")]
        TokenSignatureIsInvalid = 19,

        [Display(Name = "Mã nhân viên đã tồn tại. Vui lòng nhập lại.")]
        CodeUserExisted = 20,

        [Display(Name = "Mã linh kiện không tồn tại trong hệ thống")]
        ComponentNotExist = 50,

        [Display(Name = "NotFoundCoresspondingPosition")]
        NotFoundCoresspondingPosition = 51,

        [Display(Name = "QuantityPositionNotEnough")]
        QuantityPositionNotEnough = 52,

        [Display(Name = "Mã linh kiện đã tồn tại trong hệ thống")]
        ExistedPositionCode = 55,
        [Display(Name = "Vị trí cố định không thuộc nhà máy được phân quyền")]
        PositionCodeOutsideFactory = 56,
        [Display(Name = "Mã linh kiện hoặc tên linh kiện đã tồn tại trên hệ thống")]
        ComponentCodeOrComponentNameExisted = 57,
        [Display(Name = "Mã nhà cung cấp hoặc tên nhà cung cấp đã tồn tại trên hệ thống")]
        SupplierCodeOrSupplierNameExisted = 58,
        [Display(Name = "Mã nhà cung cấp, vị trí cố định và mã linh kiện đã tồn tại trên hệ thống")]
        Supplier_Position_Component_CodeExisted = 59,
        [Display(Name = "Quyền truy cập của tài khoản đã thay đổi")]
        RoleChanged60 = 60,
        [Display(Name = "Tồn kho nhỏ nhất đang lớn hơn tồn kho lớn nhất, vui lòng kiểm tra lại.")]
        MinInventeryNumber_IsNotGreater_MaxInventeryNumber = 61,
        [Display(Name = "Tồn kho thực tế đang lớn hơn tồn kho lớn nhất, vui lòng kiểm tra lại.")]
        InventeryNumber_IsNotGreater_MaxInventeryNumber = 62,
        [Display(Name = "Đã tồn tại vị trí cố định chứa mã linh kiện, vui lòng kiểm tra lại.")]
        CannotMutilple_ComponentCode_BelongTo_PositionCode = 63,

        [Display(Name = "Tài khoản không tồn tại quyền.")]
        HasNotAnyRoles = 21,

        [Display(Name = "Tài khoản không tồn tại quyền chi tiết.")]
        HasNotAnyRolePermissions = 22,

        [Display(Name = "Tạo Token không thành công.")]
        GenerateTokenFailed = 23,

        [Display(Name = "Client Id không hợp lệ.")]
        InvalidClientId = 24,

        [Display(Name = "Client Secret không hợp lệ.")]
        InvalidClientSecret = 25,

        [Display(Name = "Thông tin tài khoản của bạn đã thay đổi. Vui lòng đăng nhập lại để cập nhật thông tin mới nhất.")]
        InvalidSecurityStampAfterChangePassword = 26,

        [Display(Name = "File không đúng định dạng. Vui lòng thử lại.")]
        InvalidFileExcel = 70,
        [Display(Name = "Bạn không thể thay đổi trạng thái của đợt kiểm kê do chưa tới thời gian kiểm kê.")]
        NotYetInventoryDate = 64,
        [Display(Name = "Tài khoản chưa được assign vào phiếu kiểm kê.")]
        NotAssigneeAccountId = 65,
        [Display(Name = "Trạng thái phiếu kiểm kê không đúng. Vui lòng thử lại sau.")]
        InvalidStatusInventoryDoc = 66,

        [Display(Name = "Mã linh kiện không tồn tại. Vui lòng thử lại.")]
        InventoryNotFoundComponentCode = 80,
        [Display(Name = "Mã linh kiện này không nằm trong danh sách thực hiện kiểm kê của bạn. Vui lòng thử lại.")]
        ComponentNotAssigned = 81,
        [Display(Name = "Mã linh kiện này chưa được thực hiện kiểm kê. Vui lòng thử lại.")]
        ComponentNotInventoryYet = 83,
        [Display(Name = "Công đoạn này chưa được thực hiện kiểm kê. Vui lòng thử lại.")]
        DocumentNotInventoriedYet = 84,
        [Display(Name = "Mật khẩu cũ không đúng")]
        InvalidOldPassword = 85,
        [Display(Name = "Mật khẩu mới không được trùng mật với mật khẩu hiện tại")]
        OldPasswordDuplicateWithNewPassword = 86,
        [Display(Name = "Mã linh kiện này chưa được thực hiện xác nhận kiểm kê. Vui lòng thử lại.")]
        DocumentNotConfirmInventory = 87,

        [Display(Name = "Mã linh kiện này không nằm trong danh sách thực hiện giám sát kiểm kê của bạn. Vui lòng thử lại.")]
        ComponentNotInYourAudit = 74,
        [Display(Name = "Mã linh kiện này chưa được thực hiện xác nhận kiểm kê. Vui lòng thử lại.")]
        NotConfirmBeforeAudit = 75,

        [Display(Name = "Mã linh kiện này không nằm trong danh sách giám sát. Vui lòng thử lại.")]
        ComponentNotInAuditTarget = 76,

        [Display(Name = "Mã linh kiện này không thuộc khu vực giám sát. Vui lòng thử lại.")]
        ComponentNotInLocation = 77,

        [Display(Name = "Hiện đang có đợt kiểm kê chưa hoàn thành. Vui lòng không thêm đợt kiểm kê mới.")]
        CheckInventoryStatusNotFinish = 90,

        [Display(Name = "Bạn không thể thay đổi trạng thái của đợt kiểm kê đã hoàn thành.")]
        CheckInventoryStatusIsFinish = 91,

        [Display(Name = "Vui lòng tạo phiếu A trước khi thực hiện tạo các phiếu khác.")]
        CheckExistDoctypeA = 95,


        UpdateAuditInfoNotExistInDocA = 141,

        [Display(Name = "Khu vực đã tồn tại.Vui lòng thử lại.")]
        ExistLocatioName = 40,

        [Display(Name = "Không tìm thấy phiếu A trên hệ thống.")]
        NotExistDocTypeA = 41,

        [Display(Name = "Tài khoản chung chưa được gán vai trò. Vui lòng liên hệ quản lý để được gán vai trò.")]
        NotAssignInventoryRole = 43,

        [Display(Name = "Tài khoản đăng nhập chưa được gán khu vực giám sát. Vui lòng liên hệ quản lý để gán khu vực giám sát cho tài khoản này.")]
        AuditAccountNotAssignLocation = 44,

        [Display(Name = "Tài khoản đăng nhập chưa được gán vai trò thao tác. Vui lòng liên hệ quản lý để gán vai trò thao tác cho tài khoản này.")]
        AuditAccountNotAssignType = 46,

        [Display(Name = "Bạn không thể thay đổi tên khu vực vì khu vực đã được gán cho phiếu.")]
        LocationAssignedToDocument = 42,

        [Display(Name = "Hệ thống cần thời gian để cập nhật toàn bộ phiếu liên quan tới khu vực.")]
        UpdateLocationTakeLongTime = 45,

        [Display(Name = "Đợt kiểm kê đã bị khóa.")]
        IsLockedInventory = 96,
        [Display(Name = "Mã linh kiện không có trên hệ thống. Vui lòng thử lại.")]
        ScanDocBNotFound = 100,

        [Display(Name = "Không tìm thấy danh sách linh kiện điều tra sai số.")]
        ErrorInvestigationNotFound = 101,
        [Display(Name = "Đang trong thời gian kiểm kê. Không được thực hiện điều tra sai số. Vui lòng thử lại sau.")]
        CannotErrorInvestigation = 102,
        [Display(Name = "Không được điều chỉnh số lượng cùng dấu với số lượng sai số.")]
        ErrorQuantityTheSameSignErrorInvestigation = 103,
        [Display(Name = "Điều chỉnh sai số đang có trạng thái khác trạng thái đã điều tra.")]
        ErrorQuantityStatusDifferInvestigated = 104,
        [Display(Name = "Điều chỉnh sai số đang có trạng thái là đã điều tra")]
        ErrorQuantityStatusInvestigated = 105,
        [Display(Name = "Linh kiện đang được điều tra sai số.")]
        ComponentErrorInvestigating = 106,
        [Display(Name = "Số lượng điều chỉnh không được lớn hơn số lượng chênh lệch.")]
        AdjustmentQuantityCannotGreaterThanErrorQuantity = 107,
        [Display(Name = "Linh kiện chưa có lịch sử điều tra. Vui lòng tiến hành điều tra sai số.")]
        ErrorInvestigationHistoryNotFound = 108,
        [Display(Name = "Chỉ điều chỉnh lịch sử điều tra với trạng thái Giữ lại.")]
        UpdateErrorTypesIsRemain = 109,
        [Display(Name = "Chỉ xác nhận điều chỉnh lịch sử điều tra với trạng thái Chờ xác nhận và Từ chối.")]
        UpdateErrorTypesIsAwaitConfirmOrCancel = 110,
        [Display(Name = "Đã tồn tại tên phân loại trên hệ thống.")]
        ExistedErrorCategoryName = 111,
        [Display(Name = "Mã linh kiện này đã được giám sát. Không được thực hiện kiển kê và xác nhận lại.")]
        ComponentCodeIsAudited = 112,
        [Display(Name = "Mã linh kiện này chưa được kiểm kê hoặc đã được giám sát. Vui lòng thử lại.")]
        ComponentCodeIsNotInventoryYetOrAudited = 113,
    }
}
