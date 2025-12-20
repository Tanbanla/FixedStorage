namespace BIVN.FixedStorage.Services.Common.API.Dto.Location
{
    public class LocationViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string LocationName { get; set; }
        public string FactoryNames { get; set; }
        public string DepartmentName { get; set; }

        public DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsLocationAssigned { get; set; } = false;

        public bool IsAssignedInventory { get; set; } = false;
        public bool IsAssignedAudit { get; set; } = false;
    }
}
