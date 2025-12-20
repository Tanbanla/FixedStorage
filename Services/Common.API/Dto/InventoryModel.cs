namespace BIVN.FixedStorage.Services.Common.API.Dto
{
    public class InventoryModel
    {
        public Guid InventoryId { get; set; }
        public string Name { get; set; }
        public DateTime InventoryDate { get; set; }
        public int Status { get; set; }
    }
}
