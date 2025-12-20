namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class DepartmentDto
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public required string ManagerId { get; set; }

#nullable enable
        public string? ManagerName { get; set; }
#nullable disable
        
        public DateTime CreateAt { get; set; }
        public string CreateBy { get; set; }
        public int Members { get; set; }
    }
}
