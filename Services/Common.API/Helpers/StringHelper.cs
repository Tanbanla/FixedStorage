namespace BIVN.FixedStorage.Services.Common.API.Helpers
{
    public static class StringHelper
    {
        #region[String]

        public static bool HasOnlyNormalVietnameseCharacters(string input)
        {
            bool isNormalCharacters = true;
            foreach (char c in input)
            {
                if (!Constants.AllowableCharacters.Vietnamese.Contains(c.ToString()))
                {
                    return isNormalCharacters = false;
                }
            }           
            return isNormalCharacters;
        }

        public static bool HasOnlyNormalEnglishCharacters(string input)
        {
            bool isNormalCharacters = true;
            foreach (var c in input)
            {
                if (!Constants.AllowableCharacters.English.Contains(c.ToString()))
                {
                    return isNormalCharacters = false;
                }
            }
            return isNormalCharacters;
        }

        public static bool HasSpecialCharacters(string input)
        {
            foreach (var specialCharacter in Constants.SpecialCharacters.List)
            {
                if (input.Contains(specialCharacter))
                {
                    return true;
                }
            }
            return false;          
        }

        public static bool IsVietnameseString(string input)
        {
            return Regex.IsMatch(input, @"\b\S*[AĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴAĂÂÁẮẤÀẰẦẢẲẨÃẴẪẠẶẬĐEÊÉẾÈỀẺỂẼỄẸỆIÍÌỈĨỊOÔƠÓỐỚÒỒỜỎỔỞÕỖỠỌỘỢUƯÚỨÙỪỦỬŨỮỤỰYÝỲỶỸỴA-Z ]+\S*\b");
        }

        public static string ToUnsignString(string input)
        {
            input = input.Trim();
            for (int i = 0x20; i < 0x30; i++)
            {
                input = input.Replace(((char)i).ToString(), " ");
            }
            input = input.Replace(".", "-");
            input = input.Replace(" ", "-");
            input = input.Replace(",", "-");
            input = input.Replace(";", "-");
            input = input.Replace(":", "-");
            input = input.Replace("  ", "-");
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string str = input.Normalize(NormalizationForm.FormD);
            string str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
            while (str2.IndexOf("?") >= 0)
            {
                str2 = str2.Remove(str2.IndexOf("?"), 1);
            }
            while (str2.Contains("--"))
            {
                str2 = str2.Replace("--", "-").ToLower();
            }
            return str2;
        }

        public static string ToFileNameString(string input)
        {
            input = input.Trim();
            for (int i = 0x20; i < 0x30; i++)
            {
                input = input.Replace(((char)i).ToString(), " ");
            }
            input = input.Replace(".", "_");
            input = input.Replace(" ", "_");
            input = input.Replace(",", "_");
            input = input.Replace(";", "_");
            input = input.Replace(":", "_");
            input = input.Replace("  ", "_");
            input = input.Replace("-", "_");
            input = input.Replace("@", "_");
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string str = input.Normalize(NormalizationForm.FormD);
            string str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
            while (str2.IndexOf("?") >= 0)
            {
                str2 = str2.Remove(str2.IndexOf("?"), 1);
            }
            while (str2.Contains("--"))
            {
                str2 = str2.Replace("--", "__").ToLower();
            }
            return str2;
        }

        public static bool IsInvalidUnsignString(string input)
        {
            string pattern = @"^Á|À|Ã|Ạ|Â|Ấ|Ầ|Ẫ|Ậ|Ă|Ắ|Ằ|Ẵ|Ặ|à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ|É|È|Ẽ|Ẹ|Ê|Ế|Ề|Ễ|Ệ|è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ|Í|Ì|Ĩ|Ị|ì|í|ị|ỉ|ĩ|Ó|Ò|Õ|Ọ|Ô|Ố|Ồ|Ỗ|Ộ|Ơ|Ớ|Ờ|Ỡ|Ợ|ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ|Ú|Ù|Ũ|Ụ|Ư|Ứ|Ừ|Ữ|Ự|ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ|Ý|Ỳ|Ỹ|Ỵ|ỳ|ý|ỵ|ỷ|ỹ|Đ|đ|\\u0300|\\u0301|\\u0303|\\u0309|\\u0323|\\u02C6|\\u0306|\\u031B$";
            Regex rgx = new Regex(pattern);
            return rgx.IsMatch(input);
        }

        /// <summary>
        /// Replace HTML template with values
        /// </summary>
        /// <param name="template">Template content HTML</param>
        /// <param name="replacements">Dictionary with key/value</param>
        /// <returns></returns>
        public static string Parse(this string template, Dictionary<string, string> replacements)
        {
            if (replacements.Count > 0)
            {
                template = replacements.Keys
                            .Aggregate(template, (current, key) => current.Replace(key, replacements[key]));
            }
            return template;
        }

        public static string NumberConvertToString(decimal number)
        {
            string s = number.ToString("#");
            string[] numberWords = new string[] { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string[] layer = new string[] { "", "nghìn", "triệu", "tỷ" };
            int i, j, unit, dozen, hundred;
            string str = " ";
            bool booAm = false;
            decimal decS = 0;
            try
            {
                decS = Convert.ToDecimal(s.ToString());
            }
            catch
            {
            }
            if (decS < 0)
            {
                decS = -decS;
                s = decS.ToString();
                booAm = true;
            }
            i = s.Length;
            if (i == 0)
                str = numberWords[0] + str;
            else
            {
                j = 0;
                while (i > 0)
                {
                    unit = Convert.ToInt32(s.Substring(i - 1, 1));
                    i--;
                    if (i > 0)
                        dozen = Convert.ToInt32(s.Substring(i - 1, 1));
                    else
                        dozen = -1;
                    i--;
                    if (i > 0)
                        hundred = Convert.ToInt32(s.Substring(i - 1, 1));
                    else
                        hundred = -1;
                    i--;
                    if ((unit > 0) || (dozen > 0) || (hundred > 0) || (j == 3))
                        str = layer[j] + str;
                    j++;
                    if (j > 3) j = 1;
                    if ((unit == 1) && (dozen > 1))
                        str = "một " + str;
                    else
                    {
                        if ((unit == 5) && (dozen > 0))
                            str = "lăm " + str;
                        else if (unit > 0)
                            str = numberWords[unit] + " " + str;
                    }
                    if (dozen < 0)
                        break;
                    else
                    {
                        if ((dozen == 0) && (unit > 0)) str = "lẻ " + str;
                        if (dozen == 1) str = "mười " + str;
                        if (dozen > 1) str = numberWords[dozen] + " mươi " + str;
                    }
                    if (hundred < 0) break;
                    else
                    {
                        if ((hundred > 0) || (dozen > 0) || (unit > 0)) str = numberWords[hundred] + " trăm " + str;
                    }
                    str = " " + str;
                }
            }
            if (booAm) str = "Âm " + str;
            return Regex.Replace(str + "đồng chẵn", pattern: @"\s+", replacement: " ").Trim();
        }

        public static string[] DateStringFormats()
        {
            string[] dateStringformats = {
                "dd/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yyyy", "dd/MM/yy", "dd/M/yy", "d/M/yy", "d/MM/yy",
                "dd-MM-yyyy", "dd-M-yyyy", "d-M-yyyy", "d-MM-yyyy", "dd-MM-yy", "dd-M-yy", "d-M-yy", "d-MM-yy",
                "yyyy-mm-dd", "d/M/yyyy",
                "dd-MM-yyyy hh:mm:ss", "dd/MM/yyyy hh:mm:ss", "dd/M/yyyy hh:mm:ss", "d/MM/yyyy hh:mm:ss", "d/M/yyyy hh:mm:ss"
            };
            return dateStringformats;
        }

        #endregion

        #region Generate random code

        const string CHARACTERS = "abcdefghijklmnopqursuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string NUMBER = "1234567890";
        const string CHAR_SPECIAL = "!@£$%^&*()#€";
        const string CHAR_UPPER = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GenerateRandomPassword()
        {
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();

            for (int i = 0; i < 6; i++)
            {
                int index = rnd.Next(CHARACTERS.Length);
                sb.Append(CHARACTERS[index]);
            }
            sb.Append(NUMBER[rnd.Next(NUMBER.Length)]);
            sb.Append(CHAR_SPECIAL[rnd.Next(CHAR_SPECIAL.Length)]);
            sb.Append(CHAR_UPPER[rnd.Next(CHAR_UPPER.Length)]);

            return sb.ToString();
        }

        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string AlphabetAndNumber = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random Random = new Random();
        /// <summary>
        /// Generate random code
        /// </summary>
        /// <returns></returns>
        public static string GenerateRandomCode()
        {
            var randomAlphabet = GenerateRandomString(Alphabet, 4);

            var randomNumber = Enumerable.Range(0, 9)
                .OrderBy(x => Random.Next())
                .Take(4);
            return $"{string.Join(string.Empty, randomNumber)}{randomAlphabet}";
            //return $"{randomAlphabet}{string.Join(string.Empty, randomNumber)}";
        }

        public static string GenerateRandomNumberCode()
        {
            //var randomAlphabet = GenerateRandomString(Alphabet, 1);

            var randomNumber = Enumerable.Range(0, 9)
                .OrderBy(x => Random.Next())
                .Take(4);
            return $"{string.Join(string.Empty, randomNumber)}";
            //return $"{randomAlphabet}{string.Join(string.Empty, randomNumber)}";
        }

        public static string GenerateExtensionOfUrl()
        {
            var randomAlphabet = GenerateRandomString(Alphabet, 2);

            var randomNumber = Enumerable.Range(0, 9)
                .OrderBy(x => Random.Next())
                .Take(3);
            return $"{randomAlphabet}-{string.Join(string.Empty, randomNumber)}";
        }

        /// <summary>
        /// Generate random string
        /// </summary>
        /// <param name="random"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomString(string random, int length)
        {
            return new string(Enumerable.Repeat(random, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region[NumberPhone]

        /// <summary>
        /// Kiểm tra là số điện thoại di động VN hợp lệ
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static bool IsValidMobilePhoneVN(string phone)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrWhiteSpace(phone) || phone.Contains(' '))
                return false;
            //string pattern = @"^(([03+[2-9]|05+[6|8|9]|07+[0|6|7|8|9]|08+[1-9]|09+[0-9]]){3})+[0-9]{7}$";
            //string pattern = @"^0[3|5|7|8|9]+[0-9]{8}$";
            string pattern = @"^[0-9]{10}$";
            return Regex.IsMatch(phone.Trim(), pattern);
        }

        /// <summary>
        /// Kiểm tra là số điện thoại hợp lệ
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrWhiteSpace(phone) || phone.Contains(' '))
                return false;
            return Regex.IsMatch(phone.Trim(), "^[0-9]*$");
        }

        //	1234567890
        //	123-456-7890
        //	123-456-7890 x1234
        //	123-456-7890 ext1234
        //	(123)-456-7890
        //  123.456.7890
        //  123 456 7890
        //public static bool IsValidNumberPhone(string str)
        //{
        //    if (Regex.IsMatch(str, @"^[0-9]{10}$")
        //        || Regex.IsMatch(str, @"^[0-9]{3}[\-]{1}[0-9]{3}[\-]{1}[0-9]{4}$")
        //        || Regex.IsMatch(str, @"^[0-9]{3}[\-]{1}[0-9]{3}[\-]{1}[0-9]{4}[\s]{1}[a-z]{1}[0-9]{4}$")
        //        || Regex.IsMatch(str, @"^[0-9]{3}[\-]{1}[0-9]{3}[\-]{1}[0-9]{4}[\s]{1}[a-z]{3}[0-9]{4}$")
        //        || Regex.IsMatch(str, @"^[\(]{1}[0-9]{3}[\)]{1}[\-]{1}[0-9]{3}[\-]{1}[0-9]{4}$")
        //        || Regex.IsMatch(str, @"^[0-9]{3}[\.]{1}[0-9]{3}[\.]{1}[0-9]{4}$")
        //        || Regex.IsMatch(str, @"^[0-9]{3}[\s]{1}[0-9]{3}[\s]{1}[0-9]{4}$"))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        public static bool IsValidDigit(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str) || str.Contains(' '))
                return false;
            return Regex.IsMatch(str.Trim(), "^[0-9]*$");
        }

        #endregion

        #region[Email]
        public static bool IsValidEmail(string str)
        {
            string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Regex check = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            return check.IsMatch(str.Trim());
        }

        /// <summary>
        /// Check input(định dạng email)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmailBySystem(string email)
        {
            var addr = new System.Net.Mail.MailAddress(email.Trim());
            return addr.Address == email;
        }

        #endregion                                

        public static Dictionary<string, string> ErrorMessages(this ModelStateDictionary modelState)
        {
            var errors = new Dictionary<string, string>();
            for (int i = 0; i < modelState.Count; i++)
            {
                var stateObj = modelState.Values.ElementAtOrDefault(i);
                var error = stateObj.Errors.FirstOrDefault();
                if (error == null) continue;

                var errorKey = modelState.Keys.ElementAtOrDefault(i);

                errors[errorKey] = error.ErrorMessage;
            }
            return errors;
        }
    }
}
