using System.Linq.Expressions;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using Microsoft.EntityFrameworkCore.Query;

namespace BIVN.FixedStorage.Services.Inventory.API
{
    public static class Utilities
    {
        public static Dictionary<string, string> ErrorMessages(this ModelStateDictionary modelState)
        {
            var errors = new Dictionary<string, string>();
            for (int i = 0; i < modelState.Count; i++)
            {
                var stateObj = modelState.Values.ElementAtOrDefault(i);
                var error = stateObj.Errors.FirstOrDefault();
                if (error == null) continue;

                var errorKey = modelState.Keys.ElementAtOrDefault(i);
                errorKey = errorKey.ToCamelCase();
                errors[errorKey] = error.ErrorMessage;
            }
            return errors;
        }

        public static string ErrorTextMessages(this ModelStateDictionary modelState)
        {
            var errors = "";
            for (int i = 0; i < modelState.Count; i++)
            {
                var stateObj = modelState.Values.ElementAtOrDefault(i);
                var error = stateObj.Errors.FirstOrDefault();
                if (error == null) continue;

                errors = error.ErrorMessage;
                break;
            }
            return errors;
        }
        public static string ToCamelCase(this string s)
        {
            var x = s.Replace("_", "");
            if (x.Length == 0) return "null";
            x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
            return char.ToLower(x[0]) + x.Substring(1);
        }

        public static ValidateTokenResultDto? CurrentUser(this HttpContext context)
        {
            ValidateTokenResultDto user = (ValidateTokenResultDto)context.Items[Constants.HttpContextModel.UserKey] ?? null;
            return user;
        }

        public static string CurrentUserId(this HttpContext context)
        {
            var currentUser = context.CurrentUser();
            return currentUser?.UserId ?? null;
        }

        //public static List<Task<List<T>>> InsertToReportTable<T>(this List<T> TList,CaptureTimeType captureTimeType) where T : class
        //{

        //    Task.WhenAll(Task.FromResult(TList)).ContinueWith();
        //}

        public static string DisplayChangeLog(double oldQuantity, double newQuantity, int oldSatus, int newStatus, bool isChangeCDetail)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string convertOldQuantity = oldQuantity % 1 == 0 ? oldQuantity.ToString("#,##0") : oldQuantity.ToString("#,##0.00");
            string convertNewQuantity = newQuantity % 1 == 0 ? newQuantity.ToString("#,##0") : newQuantity.ToString("#,##0.00");

            if (oldQuantity == 0)
            {
                stringBuilder.AppendLine($"Nhập tổng SL: {convertNewQuantity}.");
            }
            if (oldQuantity != 0 && oldQuantity != newQuantity)
            {
                stringBuilder.AppendLine($"Cập nhật tổng SL: {convertOldQuantity} -> {convertNewQuantity}.");
            }
            if (isChangeCDetail)
            {
                stringBuilder.AppendLine($"Cập nhật: Số lượng trong bảng chi tiết.");
            }

            string statusTitle = EnumHelper<InventoryDocStatus>.GetDisplayValue((InventoryDocStatus)newStatus);

            if (oldSatus != newStatus)
            {
                stringBuilder.AppendLine($"Cập nhật trạng thái: {statusTitle}.");
            }
            if (oldSatus == newStatus && oldQuantity == newQuantity && isChangeCDetail == false)
            {
                stringBuilder.AppendLine($"Cập nhật dữ liệu chi tiết phiếu.");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Kiểm tra ngày kiểm kê đã quá hạn chưa
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsInCurrentInventory(this HttpContext context)
        {
            var currInventoryDate = context.InventoryInfo()?.InventoryModel?.InventoryDate;
            var isInInventory = currInventoryDate.HasValue && currInventoryDate.Value.Subtract(DateTime.Now).Days >= 0;
            return isInInventory;
        }

        public static void BatchUpdate<T>(
                    this IQueryable<T> query,
                        Expression<Func<T, string>> expKey,
                        object value,
                        int batchSize = 1000) where T : class
        {
            var totalCount = query.Count();
            var batches = Enumerable.Range(0, (totalCount + batchSize - 1) / batchSize).Select(i => query.Skip(i * batchSize).Take(batchSize));

            var memberExpression = (MemberExpression)expKey.Body;
            string property = memberExpression.Member.Name;

            if(totalCount < batchSize)
            {
                query.ExecuteUpdate(set => set.SetProperty(p => EF.Property<object>(p, property), p => value));
            }
            else
            {
                Parallel.ForEach(batches, (items) =>
                {
                    items.ExecuteUpdate(set => set.SetProperty(p => EF.Property<object>(p, property), p => value));
                });
            }
        }

        public static void BatchUpdateMultiColumn<T>(
                    this IQueryable<T> query,
                        Dictionary<string, object> keyValues,
                        int batchSize = 1000) where T : class
        {
            var totalCount = query.Count();
            var batches = Enumerable.Range(0, (totalCount + batchSize - 1) / batchSize).Select(i => query.Skip(i * batchSize).Take(batchSize));

            Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expr = _ => _;
            foreach (var item in keyValues)
            {
                Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> newExpr = sett => sett.SetProperty(p => EF.Property<object>(p, item.Key), p => item.Value);
                expr = AppendSetProperty<T>(expr, newExpr);
            }

            Parallel.ForEach(batches, (items) =>
            {
                items.ExecuteUpdate(expr);
            });
        }

        public static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> AppendSetProperty<TEntity>(
                        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> left,
                        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> right)
        {
            var replace = new ReplacingExpressionVisitor(right.Parameters, new[] { left.Body });
            var combined = replace.Visit(right.Body);
            return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(combined, left.Parameters);
        }
    }
}
