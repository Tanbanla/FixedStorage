namespace BIVN.FixedStorage.Services.Storage.API.Service.Dto
{
    public class CreateComponentDto
    {
        /// <summary>
        /// Mã linh kiện
        /// </summary>
        [Required(ErrorMessage = "Yêu cầu cung cấp mã linh kiện.")]
        [MaxLength(9,ErrorMessage = "Mã linh kiện tối đa 9 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Mã linh kiện chỉ được chứa chữ cái và số.")]
        public string ComponentCode { get; set; }
        /// <summary>
        /// Tên linh kiện
        /// </summary>

        [Required(ErrorMessage = "Yêu cầu cung cấp tên linh kiện.")]
        [MaxLength(150,ErrorMessage = "Tên linh kiện tối đa 150 ký tự.")]
        public string ComponentName { get; set; }
        /// <summary>
        /// Tên nhà cung cấp
        /// </summary>
        /// 
        [Required(ErrorMessage = "Yêu cầu cung cấp tên nhà cung cấp.")]
        [MaxLength(250, ErrorMessage = "Tên nhà cung cấp tối đa 250 ký tự.")]
        public string SupplierName { get; set; }
        /// <summary>
        /// Tên nhà cung cấp rút gọn
        /// </summary>
        /// 
        [Required(ErrorMessage = "Yêu cầu cung cấp tên nhà cung cấp rút gọn.")]
        [MaxLength(250, ErrorMessage = "Tên nhà cung cấp rút gọn tối đa 250 ký tự.")]
        public string SupplierShortName { get; set; }
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        /// 
        [Required(ErrorMessage = "Yêu cầu cung cấp mã nhà cung cấp.")]
        [MaxLength(50, ErrorMessage = "Mã nhà cung cấp tối đa 50 ký tự.")]
        public string SupplierCode { get; set; }
        /// <summary>
        /// Vị trí cố định
        /// </summary>
        /// 
        [Required(ErrorMessage = "Yêu cầu cung cấp vị trí cố định.")]
        [MaxLength(20, ErrorMessage = "Vị trí cố định tối đa 20 ký tự.")]
        [RegularExpression(@RegexPattern.PositionCodeRegex, ErrorMessage = "Vị trí cố định không đúng định dạng. Vui lòng nhập lại.")]
        public string PositionCode { get; set; }
        /// <summary>
        /// Tồn kho thực tế
        /// </summary>
        /// 
        [Required(ErrorMessage = "Yêu cầu cung cấp tồn kho thực tế.")]
        [MaxLength(8, ErrorMessage = "Tồn kho thực tế tối đa 8 ký tự.")]
        [RegularExpression(@"^[0-9.]*$", ErrorMessage = "Tồn kho thực tế chỉ được nhập số.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Tồn kho thực tế phải lớn hơn 0")]
        public string InventoryNumber { get; set; }
        /// <summary>
        /// Tồn kho nhỏ nhất
        /// </summary>
        /// 
        [Required(ErrorMessage = "Yêu cầu cung cấp tồn kho nhỏ nhất.")]
        [MaxLength(8, ErrorMessage = "Tồn kho nhỏ nhất tối đa 8 ký tự.")]
        [RegularExpression(@"^[0-9.]*$", ErrorMessage = "Tồn kho nhỏ nhất chỉ được nhập số.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Tồn kho nhỏ nhất phải lớn hơn 0")]
        public string MinInventoryNumber { get; set; }
        /// <summary>
        /// Tồn kho lớn nhất
        /// </summary>
        /// 
        [Required(ErrorMessage = "Yêu cầu cung cấp tồn kho lớn nhất.")]
        [MaxLength(8, ErrorMessage = "Tồn kho lớn nhất tối đa 8 ký tự.")]
        [RegularExpression(@"^[0-9.]*$", ErrorMessage = "Tồn kho lớn nhất chỉ được nhập số.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Tồn kho lớn nhất phải lớn hơn 0")]
        public string MaxInventoryNumber { get; set; }
        /// <summary>
        /// Thông tin linh kiện
        /// </summary>
        public string ComponentInfo { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }
    }
}
