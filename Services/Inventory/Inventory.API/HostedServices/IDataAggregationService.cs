namespace Inventory.API.HostedServices
{
    public interface IDataAggregationService
    {
        Task AddDataToDocTypeCComponent(List<InventoryDocTypeCSheetDto> dataFromSheets, string importer, Guid inventoryId);
        Task UpdateDataFromInventoryDoc(InventoryDocSubmitDto inventoryDocSubmitDto);
        Task AddDataToErrorInvestigation(Guid inventoryId);
    }
}
