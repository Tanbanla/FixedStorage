namespace WebApp.Application.Services.DTO
{
    public class CreateNewDepartmentDto
    {
#nullable enable
        public string? DepartmentId { get; set; }
#nullable disable
        [Required(ErrorMessage = "Vui lòng nhập Id người dùng")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phòng ban.")]
        [Display(Name = "Tên phòng ban")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "Vui lòng chọn trưởng phòng")]
        [Display(Name = "Trưởng phòng")]
        public string ManagerId { get; set; }
    }

    public class PreventSpecialCharacters : RegularExpressionAttribute
    {
        public PreventSpecialCharacters()
            : base("")
        {
        }
    }
}
