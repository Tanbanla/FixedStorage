namespace BIVN.FixedStorage.Services.Storage.API.Service
{
    public class FactoryService : IFactoryService
    {
        private readonly ILogger<FactoryService> _logger;
        private readonly StorageContext _storageContext;
        private readonly HttpContext _httpContext;

        public FactoryService(ILogger<FactoryService> logger,
                              StorageContext storageContext,
                              IHttpContextAccessor httpContextAccessor
                            )
        {
            _logger = logger;
            _storageContext = storageContext;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ResponseModel<IEnumerable<FactoryInfoModel>>> FactoryListAsync()
        {
            var factories = _storageContext.Factories
                                .AsEnumerable()
                                .OrderBy(x => Int32.TryParse(x.Code, out int value) ? 0 : 1)
                                .ThenBy(x => x.Code)
                                .Select(x => new FactoryInfoModel
                                {
                                    Id = x.Id,
                                    Name = x.Name
                                });
            if(factories?.Any() == false)
            {
                return new ResponseModel<IEnumerable<FactoryInfoModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy nhà máy nào !"
                };
            }

            return new ResponseModel<IEnumerable<FactoryInfoModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = factories,
                Message = "Lấy danh sách nhà máy thành công"
            };
        }
    }
}
