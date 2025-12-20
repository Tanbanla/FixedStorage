namespace BIVN.FixedStorage.Services.Common.API.Dto.Department
{
    public class CreateDepartmentDto
    {

#nullable enable
        public string? DepartmentId { get; set; }
        /// <summary>
        /// Lưu trưởng phòng
        /// </summary>
        public string? ManagerId { get; set; }
#nullable disable


        /// <summary>
        /// Lưu người tạo
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập User Id")]
        public string UserId { get; set; }

        /// <summary>
        /// Lưu tên phòng ban
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên phòng ban.")]
        public string Name { get; set; }


    }
}
