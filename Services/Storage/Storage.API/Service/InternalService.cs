namespace Storage.API.Service
{
    public class InternalService : IInternalService
    {
        private readonly ILogger<InternalService> _logger;
        private readonly StorageContext _storageContext;

        public InternalService(ILogger<InternalService> logger,
                               StorageContext storageContext
                            )
        {
            _logger = logger;
            _storageContext = storageContext;
        }
        public async Task<ResponseModel<IEnumerable<FactoryInfoModel>>> GetFactories()
        {
            var factories = await _storageContext.Factories.AsNoTracking()
                                    .Select(x => new FactoryInfoModel
                                    {
                                        Id = x.Id,
                                        Name = x.Name
                                    })
                                    .ToListAsync();
            if (factories?.Any() == false)
            {
                return new ResponseModel<IEnumerable<FactoryInfoModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu nhà máy."
                };
            }

            return new ResponseModel<IEnumerable<FactoryInfoModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = factories,
                Message = "Danh sách dữ liệu nhà máy."
            };
        }

        public async Task<ResponseModel<IEnumerable<string>>> GetLayouts()
        {
            var layouts = await _storageContext.Storages.AsNoTracking()
                                    .Select(x => x.Layout)
                                    .ToListAsync();

            if (layouts?.Any() == false)
            {
                return new ResponseModel<IEnumerable<string>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu khu vực."
                };
            }

            return new ResponseModel<IEnumerable<string>>
            {
                Code = StatusCodes.Status200OK,
                Data = layouts,
                Message = "Danh sách khu vực."
            };
        }
    }
}
