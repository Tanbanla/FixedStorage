namespace BIVN.FixedStorage.Services.Common.API.Dto.Location
{
    public class UpdateLocationDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên khu vực.")]
        [MaxLength(50, ErrorMessage = "Tối đa {0} ký tự.")]
        public string Name { get; set; }
        public List<string> FactoryNames { get; set; } = new();
        [Required(ErrorMessage = "Vui lòng nhập tên phòng ban.")]
        [MaxLength(20, ErrorMessage = "Tối đa {0} ký tự.")]
        public string DepartmentName { get; set; }
        public Guid UpdateBy { get; set; }
    }
}
