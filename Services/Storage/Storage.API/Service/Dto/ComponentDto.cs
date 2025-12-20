namespace BIVN.FixedStorage.Services.Storage.API.Service.Dto
{
    public class ComponentDto
    {
        // Layout is now optional; if null the search will be performed by component code only.
        [MaxLength(50, ErrorMessage = "yêu cầu nhập tối đa 50 ký tự")]
        public string? layout { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mã linh kiện")]
        [MaxLength(50, ErrorMessage = "yêu cầu nhập tối đa 50 ký tự")]
        public string componentCode { get; set; }
    }
}
