namespace BIVN.FixedStorage.Services.Common.API.Dto.Inventory
{
    public class CheckIsHightLightDocTypeCDto
    {
        public string InventoryId { get; set; }
        public string DocId { get; set; }
        public List<string> Ids { get; set; }
    }
}
