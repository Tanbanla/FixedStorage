namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class InternalDepartmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ManagerId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
