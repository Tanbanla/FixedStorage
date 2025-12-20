namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class DepartmentInventoryDto
    {
        public string DepartmentName { get; set; }
        public string LocationName { get; set; }

    }

    public class AuditorByLocationsDto : DepartmentInventoryDto
    {
        public string AuditorName { get; set; }

    }

}
