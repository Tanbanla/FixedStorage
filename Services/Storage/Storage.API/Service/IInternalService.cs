namespace Storage.API.Service
{
    public interface IInternalService
    {
        Task<ResponseModel<IEnumerable<FactoryInfoModel>>> GetFactories();
        Task<ResponseModel<IEnumerable<string>>> GetLayouts();
    }
}
