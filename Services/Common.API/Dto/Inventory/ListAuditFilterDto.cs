namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class ListAuditFilterDto
    {
        public Guid InventoryId { get; set; }
        public Guid AccountId { get; set; }

        //Mặc định là lọc tất cả = -1
        public string DepartmentName { get; set; } = "-1";
        public string LocationName { get; set; } = "-1";
        public string ComponentCode { get; set; } = "-1";
    }
}
