namespace Inventory.API.Service.Dto
{
    public class SwaggerDocABEListResponseModel
    {
        public InventoryDocViewModel InventoryDoc {get;}
        public List<DocComponentABE> Components { get;}
        public List<DocHistoriesModel> Histories { get;}
    }
}
