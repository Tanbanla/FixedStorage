namespace BIVN.FixedStorage.Services.Storage.API
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
                var erroKeyStyle = errorKey.ToCamelCase();

                errors[erroKeyStyle] = error.ErrorMessage;
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


        public static string ToCamelCase(this string str)
        {
            var words = str.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = Regex.Replace(words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)",
                m =>
                {
                    return m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value;
                });
            var tailWords = words.Skip(1)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .ToArray();
            return $"{leadWord}{string.Join(string.Empty, tailWords)}";
        }

        public static Dictionary<string, string> InputStorageExcelHeaderNames = new Dictionary<string, string>
        {
            { nameof(ImportInputStorageDto.BwinOutputCode), "Mã chỉ thị xuất kho" },
            { nameof(ImportInputStorageDto.ComponentCode), "Mã linh kiện" },
            { nameof(ImportInputStorageDto.ComponentName), "Tên linh kiện" },
            { nameof(ImportInputStorageDto.SupplierCode), "Mã nhà cung cấp" },
            { nameof(ImportInputStorageDto.SupplierName), "Tên nhà cung cấp" },
            { nameof(ImportInputStorageDto.Quantity), "Số lượng" },
            { nameof(ImportInputStorageDto.PositionCode), "Vị trí" },
            { nameof(ImportInputStorageDto.SupplierShortName), "Tên nhà cung cấp rút gọn" }
        };

        public static int GetNumberLength(this int number)
        {
            int myLen = 0;
            while (number > 0)
            {
                number /= 10;
                myLen++;
            }
            return myLen;
        }

        public static int GetColumnByName(this ExcelWorksheet ws, string columnName)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            var range = ws.Cells["1:1"].ToList();
            var index = range.FirstOrDefault(c => c?.Text?.Trim()?.Contains(columnName) == true)?.Start?.Column ?? -1 ;
            return index;
        }

        //public static bool CompareStringsIgnoringHiddenCharacters(this string str1, string str2)
        //{
        //    // Loại bỏ các ký tự ẩn hoặc không hiển thị bằng cách sử dụng biểu thức chính quy
        //    string pattern = @"\p{C}"; // Xóa các ký tự Unicode Control Category
        //    string cleanStr1 = Regex.Replace(str1, pattern, string.Empty);
        //    string cleanStr2 = Regex.Replace(str2, pattern, string.Empty);

        //    // So sánh chuỗi sau khi đã loại bỏ các ký tự ẩn
        //    return string.Equals(cleanStr1, cleanStr2, StringComparison.OrdinalIgnoreCase); // Không phân biệt chữ hoa/thường
        //}
    }
}
