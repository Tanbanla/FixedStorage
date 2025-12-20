namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class LocationByDepartmentDto
    {
        public List<string>? Departments { get; set; } = new();
    }

    public class AuditorByLocationModel
    {
        public List<string>? Locations { get; set; } = new();
    }

}
