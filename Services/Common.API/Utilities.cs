using System.Runtime.CompilerServices;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using BIVN.FixedStorage.Services.Common.API.User;
using Microsoft.Extensions.Logging;
using static BIVN.FixedStorage.Services.Common.API.Constants.ImportExcelColumns;

namespace BIVN.FixedStorage.Services.Common.API
{
    public static class Utilities
    {
        public static string FormatData(this KeyValuePair<string, object> data)
        {
            if (data.Value != null && data.Value.GetType() == typeof(Guid) && data.Key == "UserId")
            {
                return ((Guid)data.Value).ToString();
            }
            else if (data.Value != null && data.Value.GetType() == typeof(int))
            {
                var value = (int)data.Value;
                if (data.Key == "Status")
                {
                    return EnumHelper<UserStatus>.GetDisplayValue((UserStatus)value);
                }
                else if (data.Key == "AccountType")
                {
                    return EnumHelper<AccountType>.GetDisplayValue((AccountType)value);
                }
            }
            else if (data.Value != null && data.Value.GetType() == typeof(DateTime))
            {
                return ((DateTime)data.Value).ToString("dd-MM-yyyy");
            }
            else if (data.Value == null)
            {
                return string.Empty;
            }
            return data.Value.ToString();
        }

        public static void SetBackgroundExcelExportEpplus(ExcelWorksheet workSheet, Color color, int fromRow, int fromCol, int toRow, int toCol)
        {
            if (toRow == 0 && toCol == 0)
            {
                workSheet.Cells[fromRow, fromCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[fromRow, fromCol].Style.Fill.BackgroundColor.SetColor(color);
                workSheet.Cells[fromRow, fromCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[fromRow, fromCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[fromRow, fromCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[fromRow, fromCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[fromRow, fromCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[fromRow, fromCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                return;
            }
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.Fill.BackgroundColor.SetColor(color);
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[fromRow, fromCol, toRow, toCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        public static int GetColumnByName(this ExcelWorksheet ws, string columnName)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            var colIndex = ws.Cells.FirstOrDefault(c => c.Value?.ToString().ToLower() == columnName.ToLower())?.Start?.Column ?? 30;
            return colIndex;
        }
        public static int GetColumnIndex(this ExcelWorksheet ws, string columnName)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            try
            {
                return ws.Cells["1:1"].FirstOrDefault(c => c.Value?.ToString()?.Trim()?.ToLower() == columnName?.Trim()?.ToLower())?.Start.Column ?? -1;
            }
            catch (Exception)
            {

                return -1;
            }

        }

        public static string TrimBetWeen(this string text)
        {
            text = text.Trim();
            text = string.Join(" ", text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return text;
        }

        public static string FiveCharacterFromDocCode(this string? docCode)
        {
            if (docCode == null)
            {
                return docCode;
            }

            return docCode.Substring(docCode.Length - 5);
        }


        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(x => x.Type == Constants.HttpContextModel.UserKey).Value;
        }
        public static bool IsGrant(this ClaimsPrincipal user, string permission)
        {
            return user.Claims.Any(x => x.Type == permission);
        }
        public static bool IsGrant(this ValidateTokenResultDto currUser, string permission)
        {
            return currUser.RoleClaims.Any(x => x.ClaimType == permission);
        }

        public static string GetCookieValue(this HttpRequest request, string key)
        {
            return request?.Cookies[key];
        }
        public static string TokenFromCookie(this HttpRequest request, string key = Constants.HttpContextModel.TokenKey)
        {
            return GetCookieValue(request, key);
        }

        public static ValidateTokenResultDto UserFromContext(this HttpContext httpContext)
        {
            return (ValidateTokenResultDto)httpContext.Items[Constants.HttpContextModel.UserKey];
        }

        public static string RoleType(this HttpContext httpContext)
        {
            var user = UserFromContext(httpContext);
            return user?.AccountType ?? string.Empty;
        }

        /// <summary>
        /// Là xúc tiến, xúc tiến - Người phụ trách, xúc tiến - người quản lý
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static bool IsPromoter(this HttpContext httpContext)
        {
            var user = UserFromContext(httpContext);
            var checkRole = user?.InventoryLoggedInfo?.InventoryRoleType;
            return checkRole == 2 || checkRole == 3 || checkRole == 4;
        }

        /// <summary>
        /// Quyền là kiểm kê, xác nhận
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static bool IsInventory(this HttpContext httpContext)
        {
            var user = UserFromContext(httpContext);
            return user?.InventoryLoggedInfo?.InventoryRoleType == 0;
        }

        /// <summary>
        /// Nếu tài khoản riêng tích vào xem tất cả đợt kiểm kê hoặc xem đợt kiểm kê hiện tại
        /// Hoặc tài khoản chung xúc tiến
        /// </summary>
        public static bool UserCanViewAllInventory(this HttpContext httpContext)
        {
            int promotionInventoryType = 2;
            var currentUser = httpContext.UserFromContext();
            var isViewAllInventoryRole = currentUser.IsGrant(Constants.Permissions.VIEW_ALL_INVENTORY);
            var isPromotionRole = currentUser.InventoryLoggedInfo?.InventoryRoleType == promotionInventoryType;

            return isViewAllInventoryRole || isPromotionRole;
        }

        public static InventoryLoggedInfo InventoryInfo(this HttpContext httpContext)
        {
            var user = UserFromContext(httpContext);
            var inventory = user?.InventoryLoggedInfo ?? default;
            return inventory;
        }

        public static bool IsInInventoryDate(this HttpContext httpcontext)
        {
            var currInventory = httpcontext.InventoryInfo()?.InventoryModel;
            if (currInventory == null) return false;

            return DateTime.Now.Date <= currInventory.InventoryDate.Date;
        }

        public static bool IsValid(this ModelStateDictionary modelstate, string key)
        {
            if (modelstate.TryGetValue(key, out var entry))
            {
                return entry.IsValid();
            }

            throw new NullReferenceException($"Không tìm thấy key: {key}");
        }

        public static bool IsValid(this ModelStateEntry model)
        {
            return model?.Errors?.Count == 0;
        }

        public static void LogHttpContext(this ILogger logger, HttpContext context, string? errorMessage)
        {
            var Request = context.Request;
            var route = string.Join(" - ", Request.RouteValues.Select(x => $"{x.Key.ToUpper()}: {x.Value.ToString().ToUpper()}"));
            var request = $"Method: {Request.Method} - Route: {Request.Path.Value} - {route}";

            logger.LogInformation(request);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                logger.LogError($"ErrorMessage: {errorMessage}");
            }
        }

        public static string DisplayChangeLogHistory(double oldQuantity, double newQuantity, int oldSatus, int newStatus, bool isChangeCDetail)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string convertOldQuantity = oldQuantity % 1 == 0 ? oldQuantity.ToString("#,##0") : oldQuantity.ToString("#,##0.00");
            string convertNewQuantity = newQuantity % 1 == 0 ? newQuantity.ToString("#,##0") : newQuantity.ToString("#,##0.00");

            if (oldQuantity == 0)
            {
                stringBuilder.AppendLine($"Nhập tổng SL: <label class='color-0D2EA0'> {convertNewQuantity} </label> <br />");
            }
            if (oldQuantity != 0 && oldQuantity != newQuantity)
            {
                stringBuilder.AppendLine($"Cập nhật tổng SL: <label class='color-0D2EA0'> {convertOldQuantity} -> {convertNewQuantity} </label> <br />");
            }
            if (isChangeCDetail)
            {
                stringBuilder.AppendLine($"Cập nhật: Số lượng trong bảng chi tiết <br />");
            }

            string statusTitle = string.Empty;
            string colorClass = string.Empty;

            Dictionary<int, string> statusName = new Dictionary<int, string>()
            {
                {0, "Chưa tiếp nhận"},
                {1, "Không kiểm kê"},
                {2, "Chưa kiểm kê"},
                {3, "Chờ xác nhận"},
                {4, "Cần chỉnh sửa"},
                {5, "Đã xác nhận"},
                {6, "Đã đạt giám sát"},
                {7, "Không đạt giám sát"},
            };

            Dictionary<int, string> colorClassName = new Dictionary<int, string>()
            {
                {0, "color-0D2EA0"},
                {1, "color-333333"},
                {2, "color-87868C"},
                {3, "color-F3A600"},
                {4, "color-ED7200"},
                {5, "color-17AE5C"},
                {6, "color-5092FC"},
                {7, "color-E60000"},
            };

            statusTitle = statusName[newStatus];
            colorClass = colorClassName[newStatus];

            if (oldSatus != newStatus)
            {
                stringBuilder.AppendLine($"Cập nhật trạng thái: <label class='{colorClass}'> {statusTitle} </label> <br />");
            }
            if (oldSatus == newStatus && oldQuantity == newQuantity && isChangeCDetail == false)
            {
                stringBuilder.AppendLine($"Cập nhật dữ liệu chi tiết phiếu <br />");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Lấy 3 chữ số hàng thập phân
        /// </summary>
        /// <param name="decimalNumber">Số ký tự hàng thập phân</param>
        /// <returns></returns>
        public static string ToDisplayValue(this double value, int decimalNumber = 3)
        {
            StringBuilder decimalChar = new StringBuilder();
            for (int i = 1; i <= decimalNumber; i++)
            {
                decimalChar.Append("#");
            }
            string converted = decimalChar.ToString();
            return value.ToString($"0.{converted}");
        }

        public static string FormatSheetName(string name)
        {
            // Loại bỏ các ký tự không hợp lệ
            string validName = Regex.Replace(name, @"[\/\\\?\*\[\]\:]", "");

            // Nếu dài hơn 31 ký tự, cắt còn 28 và thêm "..."
            if (validName.Length > 31)
            {
                return validName.Substring(0, 28) + "...";
            }

            return validName;
        }
    }
}
