namespace Inventory.API.Service
{
    /// <summary> 
    /// API web
    /// </summary>
    public interface IInventoryHistoryService
    {

        Task<ResponseModel<InventoryHistoryDetailViewModel>> Detail(InventoryHistoryDetailFilterModel filterModel);
    }
}
