namespace BIVN.FixedStorage.Services.Storage.API.Service
{
    public interface IFactoryService
    {
        Task<ResponseModel<IEnumerable<FactoryInfoModel>>> FactoryListAsync();
    }
}
