namespace BIVN.FixedStorage.Services.Storage.API.Service
{
    public class PositionService : IPositionService
    {
        private readonly StorageContext _storageContext;
        private readonly HttpContext _httpContext;
        private readonly IRestClient _restClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PositionService> _logger;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        public PositionService(StorageContext storageContext,
                              IHttpContextAccessor httpContextAccessor,
                              IRestClient restClient,
                              IConfiguration configuration,
                              ILogger<PositionService> logger,
                              IServiceScopeFactory serviceProviderFactory
            )
        {
            _storageContext = storageContext;
            _httpContext = httpContextAccessor.HttpContext;
            _restClient = restClient;
            _configuration = configuration;
            _logger = logger;
            _serviceScopeFactory = serviceProviderFactory;
        }
        // Updated logic: layout is optional and uses Dapper for querying
        public async Task<ResponseModel> GetComponentInfoAndListPosition(ComponentDto componentDto)
        {
            if (!_httpContext.TryGetFactoriesFromUserRole(out IEnumerable<RoleClaimDto> claims))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy nhà máy nào.",
                };
            }

            var existComponentCode = await _storageContext.Positions.AnyAsync(x => x.ComponentCode.Trim() == componentDto.componentCode.Trim());
            if (!existComponentCode)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ComponentNotExist,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentNotExist),
                };
            }

            var factoryIds = claims.Select(x => x.ClaimValue.ToLower()).ToList();
            if (factoryIds.Count == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy nhà máy nào.",
                };
            }

            // Build SQL using Dapper
            var sql = new StringBuilder();
            sql.Append("SELECT p.Id, p.ComponentName, p.ComponentCode, p.ComponentInfo, p.SupplierCode, p.SupplierName, p.SupplierShortName, p.InventoryNumber, p.MinInventoryNumber, p.MaxInventoryNumber, p.Note, p.PositionCode, p.FactoryId ");
            sql.Append("FROM Positions p \n");
            sql.Append("INNER JOIN Factory f ON p.FactoryId = f.Id \n");
            sql.Append("INNER JOIN Storages s ON p.StorageId = s.Id \n");
            sql.Append("WHERE p.ComponentCode = @ComponentCode AND LOWER(CONVERT(varchar(36), f.Id)) IN (");
            // create parameter list for IN clause
            var inParams = new List<string>();
            var dapperParams = new DynamicParameters();
            dapperParams.Add("ComponentCode", componentDto.componentCode);
            for (int i = 0; i < factoryIds.Count; i++)
            {
                var paramName = $"@f{i}";
                inParams.Add(paramName);
                dapperParams.Add(paramName, factoryIds[i]);
            }
            sql.Append(string.Join(",", inParams));
            sql.Append(") ");
            if (!string.IsNullOrWhiteSpace(componentDto.layout))
            {
                sql.Append("AND s.Layout = @Layout ");
                dapperParams.Add("Layout", componentDto.layout);
            }

            using var conn = _storageContext.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            var rows = await conn.QueryAsync(new CommandDefinition(sql.ToString(), dapperParams));
            var list = rows.Select(r => new
            {
                Id = (Guid)r.Id,
                ComponentName = (string)r.ComponentName,
                ComponentCode = (string)r.ComponentCode,
                ComponentInfo = (string?)r.ComponentInfo,
                SupplierCode = (string?)r.SupplierCode,
                SupplierName = (string?)r.SupplierName,
                SupplierShortName = (string?)r.SupplierShortName,
                InventoryNumber = (double?)r.InventoryNumber,
                MinInventoryNumber = (double?)r.MinInventoryNumber,
                MaxInventoryNumber = (double?)r.MaxInventoryNumber,
                Note = (string?)r.Note,
                PositionCode = (string)r.PositionCode,
                FactoryId = (Guid)r.FactoryId
            }).ToList();

            if (list.Count == 0)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.PositionCodeOutsideFactory,
                    Message = string.IsNullOrWhiteSpace(componentDto.layout)
                        ? "Không tìm thấy vị trí linh kiện theo mã linh kiện trong phạm vi phân quyền."
                        : "Vị trí cố định không thuộc nhà máy được phân quyền.",
                };
            }

            var groupedComponents = list.GroupBy(p => p.PositionCode)
                .Select(grouped => new
                {
                    PositionCode = grouped.Key,
                    ComponentDetails = grouped.Select(p => new
                    {
                        id = p.Id,
                        componentName = p.ComponentName,
                        componentCode = p.ComponentCode,
                        componentInfo = p.ComponentInfo,
                        supplierCode = p.SupplierCode,
                        supplierName = p.SupplierName,
                        supplierShortName = p.SupplierShortName,
                        inventoryNumber = p.InventoryNumber,
                        minInventoryNumber = p.MinInventoryNumber,
                        maxInventoryNumber = p.MaxInventoryNumber,
                        note = p.Note,
                        positionCode = p.PositionCode,
                        factoryId = p.FactoryId
                    })
                }).ToList();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = string.IsNullOrWhiteSpace(componentDto.layout)
                    ? "Thông tin linh kiện và danh sách vị trí linh kiện theo mã linh kiện"
                    : "Thông tin linh kiện và danh sách vị trí linh kiện",
                Data = groupedComponents
            };
        }
        public async Task<ResponseModel> GetListStorage()
        {
            if (!_httpContext.TryGetFactoriesFromUserRole(out IEnumerable<RoleClaimDto> claims))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy nhà máy nào.",
                };
            }

            var FactoryId = claims.Select(x => x.ClaimValue.ToLower()).ToList();

            var storageList = await (from s in _storageContext.Storages.AsNoTracking()
                                     join f in _storageContext.Factories.AsNoTracking() on s.FactoryId equals f.Id
                                     where (FactoryId.Contains(f.Id.ToString().ToLower()))
                                     select new StorageDto
                                     {
                                         Id = s.Id,
                                         Layout = s.Layout,
                                     }
                                     ).ToListAsync();

            if (!storageList.Any())
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotFoundCoresspondingPosition,
                    Message = "Không tìm thấy vị trí tương ứng",
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách mã khu vực",
                Data = storageList
            };
        }

        public async Task<ResponseModel<List<LayoutDto>>> GetLayoutList()
        {
            var result = new ResponseModel<List<LayoutDto>>();
            //var user = _httpContext.User.Identities.FirstOrDefault(x => x.AuthenticationType == Constants.HttpContextModel.UserKey);
            var user = (ValidateTokenResultDto)_httpContext.Items[Constants.HttpContextModel.UserKey];
            var factoryFilter = user.RoleClaims
                .Where(x => x.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY && !string.IsNullOrEmpty(x.ClaimValue))
                .Select(x => x.ClaimValue.ToUpper());
            StringBuilder factoryIdList = new StringBuilder();
            if (factoryFilter?.Any() == true)
            {
                foreach (var factoryId in factoryFilter)
                {
                    factoryIdList.Append($"'{factoryId}',");
                }
                factoryIdList.Length--;
            }

            var query = new StringBuilder();
            query.Append("SELECT FactoryId, FactoryName, Layout, CreatedAt, PositionCount, InventoryNumberCount ");
            query.Append("FROM ");
            query.Append("(SELECT * FROM (SELECT f.Id FactoryId, f.Name FactoryName, s.Id StorageId, s.Layout Layout, s.CreatedAt CreatedAt FROM Factory f LEFT JOIN Storages s ON f.Id = s.FactoryId ");
            if (factoryIdList.Length > 0)
            {
                query.Append($"WHERE f.Id IN ({factoryIdList}) ");
            }
            else if (factoryIdList.Length == 0)
            {
                query.Append($"WHERE f.Id IS NULL ");
            }
            query.Append(") o GROUP BY o.FactoryId, o.FactoryName, o.StorageId, o.Layout, o.CreatedAt) AS q0 ");

            query.Append("LEFT JOIN ");
            query.Append("(SELECT o.layout Position1Layout, count(o.position) PositionCount ");
            query.Append("FROM (SELECT p.Layout layout, p.PositionCode position FROM Positions p ");
            query.Append("WHERE PATINDEX('%' + p.Layout + '%',p.PositionCode) > 0 ");
            if (factoryIdList.Length > 0)
            {
                query.Append($"AND p.FactoryId IN ({factoryIdList}) ");
            }
            query.Append(") o GROUP BY o.layout) AS q1 ON q0.Layout = q1.Position1Layout ");

            query.Append("LEFT JOIN ");
            query.Append("(SELECT o.layout Position2Layout, count(o.position) InventoryNumberCount ");
            query.Append("FROM (SELECT p.Layout layout, p.PositionCode position FROM Positions p ");
            query.Append("WHERE p.InventoryNumber <= p.MinInventoryNumber ");
            if (factoryIdList.Length > 0)
            {
                query.Append($"AND p.FactoryId IN ({factoryIdList}) ");
            }
            query.Append(") o GROUP BY o.Layout) AS q2 ON q0.Layout = q2.Position2Layout ");
            query.Append(" WHERE Layout IS NOT NULL ");
            query.Append("ORDER BY CreatedAt ASC ");
            using var conn = _storageContext.Database.GetDbConnection();
            var layoutListResult = await conn.QueryAsync<LayoutDto>(query.ToString());
            if (layoutListResult == null)
            {
                var layoutResult = new ResponseModel<List<LayoutDto>>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Data = null
                };
                return await Task.FromResult(layoutResult);
            }

            if (layoutListResult?.Any() == false)
            {
                var layoutResult = new ResponseModel<List<LayoutDto>>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Không có bản ghi khu vực nào",
                    Data = null
                };
                return await Task.FromResult(layoutResult);
            }

            foreach (var item in layoutListResult)
            {
                if (!string.IsNullOrEmpty(item.Layout))
                {
                    switch (item.Layout.Length)
                    {
                        case 3:
                            item.FactoryName = $"F{item.Layout.Substring(0, 1)}";
                            item.ShelfName = item.Layout.Substring(1);
                            break;
                        case 4:
                            var isNumberOrCharacter = item.Layout.ElementAt(1);
                            if (char.IsNumber(isNumberOrCharacter))
                            {
                                item.FactoryName = $"F{item.Layout.Substring(0, 2)}";
                                item.ShelfName = item.Layout.Substring(2);
                            }
                            else
                            {
                                item.FactoryName = $"F{item.Layout.Substring(0, 1)}";
                                item.ShelfName = item.Layout.Substring(1);
                            }
                            break;
                        case int x when x >= 5 && x <= 8:
                            isNumberOrCharacter = item.Layout.ElementAt(1);
                            if (char.IsNumber(isNumberOrCharacter))
                            {
                                item.FactoryName = $"F{item.Layout.Substring(0, 2)}";
                                item.ShelfName = item.Layout.Substring(2);
                            }
                            else
                            {
                                item.FactoryName = $"F{item.Layout.Substring(0, 1)}";
                                var storageChar1 = item.Layout.ElementAt(1);
                                var storageChar2 = item.Layout.ElementAt(2);
                                if (storageChar1.ToString() == "T" && char.IsNumber(storageChar2) && ValidStorages.Numbers.Contains(int.Parse(storageChar2.ToString())))
                                {
                                    item.StorageName = $"{storageChar1}{storageChar2}";
                                    item.ShelfName = item.Layout.Substring(3);
                                }
                                else
                                {
                                    item.ShelfName = item.Layout.Substring(1);
                                }
                            }
                            break;
                    }
                }
                item.InventoryStatus = $"{(int)InventoryStatus.PositionNearlyOutOfStock}";
            }

            result.Data = layoutListResult.ToList();
            result.Code = StatusCodes.Status200OK;
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel> AddNewComponent(CreateComponentDto createComponentDto, string userId)
        {
            //Check 1: Vị trí cố định không thuộc nhà máy được phân quyền:

            if (!_httpContext.TryGetFactoriesFromUserRole(out IEnumerable<RoleClaimDto> claims))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy nhà máy nào.",
                };
            }
            //Danh sách Id các nhà máy đã được phân quyền:
            var FactoryId = claims.Select(x => x.ClaimValue.ToLower()).ToList();

            //Từ PositionCode(5T4-01) lấy ra Code trong Factory(Là 1 hoặc 2 chữ số đầu tiên trong PositionCode VD: 5)
            var getFactoryCode = Regex.Match(createComponentDto.PositionCode, @"^\d{1,2}");

            Guid getIdFactory = Guid.Empty;
            string getNameFactory = string.Empty;

            if (getFactoryCode.Success)
            {
                //Check tồn tại nhà máy(Từ vị trí cố định, Lấy ra code nhà máy):
                var checkExistFactoryCode = await _storageContext.Factories.FirstOrDefaultAsync(x => x.Code == getFactoryCode.Value);
                if (checkExistFactoryCode == null)
                {
                    //Thêm mới Nhà máy:
                    var newFactory = new BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity.Factory
                    {
                        Id = Guid.NewGuid(),
                        Name = $"F{getFactoryCode.Value}",
                        Code = getFactoryCode.Value,
                        Status = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };

                    getIdFactory = newFactory.Id;
                    getNameFactory = newFactory.Name;

                    await _storageContext.Factories.AddAsync(newFactory);

                }
                else
                {
                    var checkPositionCodeInsideFactory = await (from f in _storageContext.Factories.AsNoTracking()
                                                                join s in _storageContext.Storages.AsNoTracking() on f.Id equals s.FactoryId
                                                                where (f.Code.Contains(getFactoryCode.Value) && FactoryId.Contains(f.Id.ToString().ToLower()))
                                                                select new
                                                                {
                                                                    FactoryId = f.Id,
                                                                    FactoryName = f.Name,
                                                                    StorageId = s.Id,
                                                                }).FirstOrDefaultAsync();

                    if (checkPositionCodeInsideFactory == null)
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.PositionCodeOutsideFactory,
                            Message = "Vị trí cố định không thuộc nhà máy được phân quyền.",
                        };
                    }

                    getIdFactory = checkPositionCodeInsideFactory.FactoryId;
                    getNameFactory = checkPositionCodeInsideFactory.FactoryName;
                }
            }

            //Check 2: 1 mã linh kiện có nhiều vị trí cố định và có thể có nhiều mã nhà cung cấp:

            var checkExistPositionCode = await _storageContext.Positions.AsNoTracking().FirstOrDefaultAsync(x => x.PositionCode.Contains(createComponentDto.PositionCode));
            if (checkExistPositionCode != null)
            {
                if (checkExistPositionCode.ComponentCode.Contains(createComponentDto.ComponentCode))
                {
                    if (checkExistPositionCode.SupplierCode.Contains(createComponentDto.SupplierCode))
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.Supplier_Position_Component_CodeExisted,
                            Message = $"Đã tồn tại vị trí cố định {checkExistPositionCode.PositionCode} có chứa mã linh kiện {checkExistPositionCode.ComponentCode} thuộc mã nhà cung cấp {checkExistPositionCode.SupplierCode} trên hệ thống. Vui lòng kiểm tra lại.",
                        };
                    }
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.CannotMutilple_ComponentCode_BelongTo_PositionCode,
                        Message = $"Đã tồn tại vị trí cố định {checkExistPositionCode.PositionCode} chứa mã linh kiện {checkExistPositionCode.ComponentCode}. Vui lòng kiểm tra lại.",
                    };
                }
            }
            // Lấy ra thông tin để thêm linh kiện mới:

            //Lấy ra Layout(từ đầu chuỗi đến khi gặp ký tự '-' hoặc '/') từ PositionCode mà người dùng nhập vào:
            var getLayout = createComponentDto.PositionCode.Split(new char[] { '-', '/' })[0];

            var getComponent = await (from p in _storageContext.Positions.AsNoTracking()
                                      join f in _storageContext.Factories.AsNoTracking() on p.FactoryId equals f.Id
                                      join s in _storageContext.Storages.AsNoTracking() on p.StorageId equals s.Id
                                      where (s.Layout.Contains(getLayout))
                                      select new
                                      {
                                          FactoryId = f.Id,
                                          FactoryName = f.Name,
                                          Layout = p.Layout,
                                          StorageId = s.Id,
                                      }).FirstOrDefaultAsync();

            if (getComponent != null)
            {
                var newPosition = new Position
                {
                    Id = Guid.NewGuid(),
                    ComponentCode = createComponentDto.ComponentCode,
                    ComponentName = createComponentDto.ComponentName,
                    FactoryId = getComponent?.FactoryId,
                    FactoryName = getComponent?.FactoryName,
                    InventoryNumber = double.Parse(createComponentDto.InventoryNumber),
                    MinInventoryNumber = double.Parse(createComponentDto.MinInventoryNumber),
                    MaxInventoryNumber = double.Parse(createComponentDto.MaxInventoryNumber),
                    PositionCode = createComponentDto.PositionCode,
                    Layout = getComponent.Layout,
                    StorageId = getComponent.StorageId,
                    ComponentInfo = createComponentDto.ComponentInfo,
                    Note = createComponentDto.Note,
                    CreatedAt = DateTime.Now,
                    SupplierCode = createComponentDto.SupplierCode,
                    SupplierName = createComponentDto.SupplierName,
                    SupplierShortName = createComponentDto.SupplierShortName,
                    CreatedBy = userId
                };
                await _storageContext.Positions.AddAsync(newPosition);
                await _storageContext.SaveChangesAsync();
            }
            else
            {
                var checkExistLayoutStorage = await _storageContext.Storages.FirstOrDefaultAsync(x => x.Layout.Contains(getLayout));
                Guid getStorageId = Guid.Empty;
                //Insert into Storage:
                if (checkExistLayoutStorage == null)
                {
                    var newStorage = new Storage.API.Infrastructure.Entity.Storage
                    {
                        Id = Guid.NewGuid(),
                        Layout = getLayout,
                        FactoryId = getIdFactory,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };

                    getStorageId = newStorage.Id;

                    await _storageContext.Storages.AddAsync(newStorage);
                }

                //Insert into Positions:
                var newPosition = new Position
                {
                    Id = Guid.NewGuid(),
                    ComponentCode = createComponentDto.ComponentCode,
                    ComponentName = createComponentDto.ComponentName,
                    FactoryId = getIdFactory,
                    FactoryName = getNameFactory,
                    InventoryNumber = double.Parse(createComponentDto.InventoryNumber),
                    MinInventoryNumber = double.Parse(createComponentDto.MinInventoryNumber),
                    MaxInventoryNumber = double.Parse(createComponentDto.MaxInventoryNumber),
                    PositionCode = createComponentDto.PositionCode,
                    Layout = getLayout,
                    StorageId = getStorageId,
                    ComponentInfo = createComponentDto.ComponentInfo,
                    Note = createComponentDto.Note,
                    CreatedAt = DateTime.Now,
                    SupplierCode = createComponentDto.SupplierCode,
                    SupplierName = createComponentDto.SupplierName,
                    SupplierShortName = createComponentDto.SupplierShortName,
                    CreatedBy = userId
                };

                await _storageContext.Positions.AddAsync(newPosition);
                await _storageContext.SaveChangesAsync();
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Thêm mới linh kiện thành công",
                Data = null
            };
        }

        public async Task<ResponseModel> GetDetailComponent(string id)
        {
            var getDetailComponent = await _storageContext.Positions.Where(x => x.Id.ToString().ToLower().Contains(id.ToLower()))
                .Select(x => new GetDetailComponentDto
                {
                    Id = x.Id,
                    ComponentCode = x.ComponentCode,
                    ComponentName = x.ComponentName,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    SupplierShortName = x.SupplierShortName,
                    PositionCode = x.PositionCode,
                    MinInventoryNumber = x.MinInventoryNumber,
                    MaxInventoryNumber = x.MaxInventoryNumber,
                    InventoryNumber = x.InventoryNumber,
                    ComponentInfo = x.ComponentInfo,
                    Note = x.Note,
                    CreatedAt = x.CreatedAt.ToString(Constants.DefaultDateFormat),
                    CreatedName = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt.Value.ToString(Constants.DefaultDateFormat),
                    UpdatedName = x.UpdatedBy,
                }).FirstOrDefaultAsync();

            if (getDetailComponent == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy linh kiện trong hệ thống",
                    Data = null
                };
            }

            //Call Internal API Get UserInfo CreateName:

            if (!getDetailComponent.CreatedName.IsNullOrEmpty() && !getDetailComponent.UpdatedName.IsNullOrEmpty() &&
                getDetailComponent.CreatedName.Contains(getDetailComponent.UpdatedName))
            {
                var request = new RestRequest($"api/identity/user/{getDetailComponent.CreatedName}");
                request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                var response = await _restClient.GetAsync(request);

                var result = JsonConvert.DeserializeObject<ResponseModel<GetUserInfoDto>>(response?.Content ?? string.Empty);

                if (result?.Code == StatusCodes.Status200OK)
                {
                    getDetailComponent.CreatedName = result.Data.FullName;
                    getDetailComponent.UpdatedName = result.Data.FullName;
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Thông tin chi tiết linh kiện",
                    Data = getDetailComponent
                };
            }

            if (!getDetailComponent.CreatedName.IsNullOrEmpty())
            {
                var request = new RestRequest($"api/identity/user/{getDetailComponent.CreatedName}");
                request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                var response = await _restClient.GetAsync(request);

                var result = JsonConvert.DeserializeObject<ResponseModel<GetUserInfoDto>>(response?.Content ?? string.Empty);

                if (result?.Code == StatusCodes.Status200OK)
                {
                    getDetailComponent.CreatedName = result.Data.FullName;
                }
            }
            if (!getDetailComponent.UpdatedName.IsNullOrEmpty())
            {
                var request = new RestRequest($"api/identity/user/{getDetailComponent.UpdatedName}");
                request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
                request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

                var response = await _restClient.GetAsync(request);

                var result = JsonConvert.DeserializeObject<ResponseModel<GetUserInfoDto>>(response?.Content ?? string.Empty);

                if (result?.Code == StatusCodes.Status200OK)
                {
                    getDetailComponent.UpdatedName = result.Data.FullName;
                }
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Thông tin chi tiết linh kiện",
                Data = getDetailComponent
            };
        }

        public async Task<ResponseModel<PagedList<ComponentFilterItemResultDto>>> GetAllComponentsPaging(ComponentsFilterDto filterModel)
        {
            var user = (ValidateTokenResultDto)_httpContext.Items[Constants.HttpContextModel.UserKey];
            var factoryFilter = user != null ? user.RoleClaims
                .Where(x => x.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY && !string.IsNullOrEmpty(x.ClaimValue))
                .Select(x => x.ClaimValue.ToUpper()).ToList() : new List<string>();

            var query = from p in _storageContext.Positions
                        select new ComponentFilterQueryResultDto
                        {
                            Id = p.Id,
                            Layout = p.Layout,
                            FactoryId = p.FactoryId,
                            ComponentCode = p.ComponentCode,
                            ComponentName = p.ComponentName,
                            SupplierName = p.SupplierName,
                            ComponentPosition = p.PositionCode,
                            InventoryNumber = p.InventoryNumber,
                            MaxInventoryNumber = p.MaxInventoryNumber,
                            MinInventoryNumber = p.MinInventoryNumber,
                            CreatedAt = p.CreatedAt,
                            UpdatedAt = p.UpdatedAt,
                            SupplierShortName = p.SupplierShortName
                        };
            if (query?.Any() == false)
            {
                return new ResponseModel<PagedList<ComponentFilterItemResultDto>>()
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Không có bản ghi nào",
                    Data = new PagedList<ComponentFilterItemResultDto>(new List<ComponentFilterItemResultDto>(), new PagingInfo(10, 1))
                };
            }

            if (factoryFilter?.Any() == true)
            {
                query = query.Where(x => factoryFilter.Contains(x.FactoryId.ToString().ToUpper()));
            }
            else
            {
                query = query.Where(x => x.FactoryId == default(Guid));
            }
            var inventoryStatus = (InventoryStatus)Int32.Parse(filterModel.InventoryStatus);
            switch (inventoryStatus)
            {
                // Vị trí gần hết linh kiện
                // Các bản ghi có Tồn kho thực tế <= tồn kho nhỏ nhất
                case InventoryStatus.PositionNearlyOutOfStock:
                    query = query.Where(x => x.InventoryNumber <= x.MinInventoryNumber);
                    break;

                // Linh kiện mới được cập nhật
                // Các bản ghi có thời gian chỉnh sửa, tạo mới trong vòng 7 ngày gần nhất tính từ thời điểm hiện tại
                case InventoryStatus.NewComponentsHasJustUpdated:
                    query = query.Where(x => (x.CreatedAt.HasValue && EF.Functions.DateDiffDay(x.CreatedAt.Value.Date, DateTime.Now.Date) <= 7) || (x.UpdatedAt.HasValue && EF.Functions.DateDiffDay(x.UpdatedAt.Value.Date, DateTime.Now.Date) <= 7));
                    break;

                default:
                    break;
            }
            if (string.IsNullOrEmpty(filterModel.AllLayouts) && filterModel.LayoutIds?.Any() == true)
            {
                query = query.Where(x => filterModel.LayoutIds.Contains(x.Layout));
            }
            if (!string.IsNullOrEmpty(filterModel.ComponentCode))
            {
                query = query.Where(x => x.ComponentCode.ToLower().Contains(filterModel.ComponentCode.ToLower()));
            }
            if (!string.IsNullOrEmpty(filterModel.ComponentName))
            {
                query = query.Where(x => x.ComponentName.ToLower().Contains(filterModel.ComponentName.ToLower()));
            }
            if (!string.IsNullOrEmpty(filterModel.SupplierName))
            {
                query = query.Where(x => x.SupplierName.ToLower().Contains(filterModel.SupplierName.ToLower()));
            }
            if (!string.IsNullOrEmpty(filterModel.ComponentPosition))
            {
                query = query.Where(x => x.ComponentPosition.ToLower().Contains(filterModel.ComponentPosition.ToLower()));
            }
            if (filterModel.ComponentInventoryQtyStart.HasValue && filterModel.ComponentInventoryQtyEnd.HasValue)
            {
                query = query.Where(x => x.InventoryNumber.HasValue
                && x.InventoryNumber.Value >= filterModel.ComponentInventoryQtyStart.Value
                && x.InventoryNumber.Value <= filterModel.ComponentInventoryQtyEnd.Value);
            }
            else if (filterModel.ComponentInventoryQtyStart.HasValue)
            {
                query = query.Where(x => x.InventoryNumber.HasValue && x.InventoryNumber.Value >= filterModel.ComponentInventoryQtyStart.Value);
            }
            else if (filterModel.ComponentInventoryQtyEnd.HasValue)
            {
                query = query.Where(x => x.InventoryNumber.HasValue && x.InventoryNumber.Value <= filterModel.ComponentInventoryQtyEnd.Value);
            }
            filterModel.Paging.RowsCount = query.Count();

            var pagedResultModel = query.OrderBy(x => x.ComponentCode.StartsWith("0") ? 2 : 1).ThenBy(x => x.ComponentCode)
                                    .Skip(filterModel.Paging.StartRowIndex)
                                    .Take(filterModel.Paging.PageSize);


            var result = new ResponseModel<PagedList<ComponentFilterItemResultDto>>(new PagedList<ComponentFilterItemResultDto>(new List<ComponentFilterItemResultDto>(), filterModel?.Paging ?? new PagingInfo(10, 1)));
            result.Data.List = pagedResultModel.Select(x => new ComponentFilterItemResultDto
            {
                Id = x.Id,
                ComponentCode = x.ComponentCode,
                ComponentName = x.ComponentName,
                SupplierName = x.SupplierName,
                ComponentPosition = x.ComponentPosition,
                InventoryNumber = x.InventoryNumber,
                MaxInventoryNumber = x.MaxInventoryNumber,
                IsNewCreateAt = x.CreatedAt.HasValue && (DateTime.Now - x.CreatedAt.Value).TotalDays <= 7,
                IsNewUpdateAt = x.UpdatedAt.HasValue && (DateTime.Now - x.UpdatedAt.Value).TotalDays <= 7,
                SupplierShortName = x.SupplierShortName
            }).ToList();
            result.Code = StatusCodes.Status200OK;
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel<List<DropDownListItemDto>>> GetLayoutDropDownList()
        {
            var result = new ResponseModel<List<DropDownListItemDto>>();
            var user = (ValidateTokenResultDto)_httpContext.Items[Constants.HttpContextModel.UserKey];
            var factoryFilter = user.RoleClaims
                .Where(x => x.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY && !string.IsNullOrEmpty(x.ClaimValue))
                .Select(x => x.ClaimValue.ToUpper());
            StringBuilder factoryIdList = new StringBuilder();
            if (factoryFilter?.Any() == true)
            {
                foreach (var factoryId in factoryFilter)
                {
                    factoryIdList.Append($"'{factoryId}',");
                }
                factoryIdList.Length--;
            }

            var query = new StringBuilder();
            query.Append("SELECT o.layout Id, o.layout Name FROM ( ");
            query.Append("SELECT s.Layout layout, f.Id FactoryId, f.Name FactoryName FROM Factory f ");
            query.Append("LEFT JOIN Storages s ON f.Id = s.FactoryId ");
            if (factoryIdList.Length > 0)
            {
                query.Append($"WHERE f.Id IN ({factoryIdList}) ");
            }
            else if (factoryIdList.Length == 0)
            {
                query.Append($"WHERE f.Id IS NULL ");
            }
            //if (!string.IsNullOrEmpty(layout))
            //{
            //    query.Append($"AND p.Layout = '{layout}' ");
            //}
            query.Append(") o GROUP BY o.layout, o.FactoryId ");
            using var conn = _storageContext.Database.GetDbConnection();
            var layoutDropDownListResult = await conn.QueryAsync<DropDownListItemDto>(query.ToString());
            if (layoutDropDownListResult == null)
            {
                var layoutResult = new ResponseModel<List<DropDownListItemDto>>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Data = null
                };
                return await Task.FromResult(layoutResult);
            }

            if (layoutDropDownListResult?.Any() == false)
            {
                var layoutResult = new ResponseModel<List<DropDownListItemDto>>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Không có bản ghi khu vực nào",
                    Data = null
                };
                return await Task.FromResult(layoutResult);
            }

            result.Data = layoutDropDownListResult.ToList();
            result.Code = StatusCodes.Status200OK;
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel> UpdateComponent(UpdateComponentDto updateComponentDto, string componentId, string userId)
        {
            //Check 1: Vị trí cố định không thuộc nhà máy được phân quyền:

            if (!_httpContext.TryGetFactoriesFromUserRole(out IEnumerable<RoleClaimDto> claims))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy nhà máy nào.",
                };
            }
            //Danh sách Id các nhà máy đã được phân quyền:
            var FactoryId = claims.Select(x => x.ClaimValue.ToLower()).ToList();

            //Từ PositionCode(5T4-01) lấy ra Code trong Factory(Là 1 hoặc 2 chữ số đầu tiên trong PositionCode VD: 5)
            var getFactoryCode = Regex.Match(updateComponentDto.PositionCode, @"^\d{1,2}");

            Guid getIdFactory = Guid.Empty;
            string getNameFactory = string.Empty;

            if (getFactoryCode.Success)
            {

                //Check tồn tại nhà máy(Từ vị trí cố định, Lấy ra code nhà máy):
                var checkExistFactoryCode = await _storageContext.Factories.FirstOrDefaultAsync(x => x.Code == getFactoryCode.Value);
                if (checkExistFactoryCode == null)
                {
                    //Thêm mới Nhà máy:
                    var newFactory = new BIVN.FixedStorage.Services.Storage.API.Infrastructure.Entity.Factory
                    {
                        Id = Guid.NewGuid(),
                        Name = $"F{getFactoryCode.Value}",
                        Code = getFactoryCode.Value,
                        Status = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };

                    getIdFactory = newFactory.Id;
                    getNameFactory = newFactory.Name;

                    await _storageContext.Factories.AddAsync(newFactory);

                }
                else
                {
                    var checkPositionCodeInsideFactory = await (from f in _storageContext.Factories.AsNoTracking()
                                                                join s in _storageContext.Storages.AsNoTracking() on f.Id equals s.FactoryId
                                                                where (f.Code.Contains(getFactoryCode.Value) && FactoryId.Contains(f.Id.ToString().ToLower()))
                                                                select new
                                                                {
                                                                    FactoryId = f.Id,
                                                                    FactoryName = f.Name,
                                                                    StorageId = s.Id,
                                                                }).FirstOrDefaultAsync();

                    if (checkPositionCodeInsideFactory == null)
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.PositionCodeOutsideFactory,
                            Message = "Vị trí cố định không thuộc nhà máy được phân quyền.",
                        };
                    }

                    //Lấy ra Id nhà máy và tên nhà máy:
                    getIdFactory = checkPositionCodeInsideFactory.FactoryId;
                    getNameFactory = checkPositionCodeInsideFactory.FactoryName;
                }

            }

            //Check 2: Nếu tồn tại vị trí cố định, mã linh kiện, mã nhà cung cấp thì báo lỗi:

            var checkGetComponentById = await _storageContext.Positions.FirstOrDefaultAsync(x => x.Id.ToString().ToLower().Contains(componentId));
            if (!checkGetComponentById.ComponentCode.Contains(updateComponentDto.ComponentCode) || !checkGetComponentById.SupplierCode.Contains(updateComponentDto.SupplierCode) || !checkGetComponentById.PositionCode.Contains(updateComponentDto.PositionCode))
            {
                var checkExistPositionCode = await _storageContext.Positions.AsNoTracking().FirstOrDefaultAsync(x => x.PositionCode.Contains(updateComponentDto.PositionCode));
                if (checkExistPositionCode != null)
                {
                    if (checkExistPositionCode.ComponentCode.Contains(updateComponentDto.ComponentCode))
                    {
                        if (checkExistPositionCode.SupplierCode.Contains(updateComponentDto.SupplierCode))
                        {
                            return new ResponseModel
                            {
                                Code = (int)HttpStatusCodes.Supplier_Position_Component_CodeExisted,
                                Message = $"Đã tồn tại vị trí cố định {checkExistPositionCode.PositionCode} có chứa mã linh kiện {checkExistPositionCode.ComponentCode} thuộc mã nhà cung cấp {checkExistPositionCode.SupplierCode} trên hệ thống. Vui lòng kiểm tra lại.",
                            };
                        }
                    }
                    else
                    {
                        return new ResponseModel
                        {
                            Code = (int)HttpStatusCodes.CannotMutilple_ComponentCode_BelongTo_PositionCode,
                            Message = $"Đã tồn tại vị trí cố định {checkExistPositionCode.PositionCode} chứa mã linh kiện {checkExistPositionCode.ComponentCode}. Vui lòng kiểm tra lại.",
                        };
                    }
                }
            }

            //Lấy thông tin linh kiện theo componentId:
            var getComponentById = await _storageContext.Positions.FirstOrDefaultAsync(x => x.Id.ToString().ToLower().Contains(componentId.ToLower()));
            if (getComponentById == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy linh kiện",
                };
            }

            //Update Positions:
            getComponentById.ComponentCode = updateComponentDto.ComponentCode;
            getComponentById.ComponentName = updateComponentDto.ComponentName;
            getComponentById.SupplierCode = updateComponentDto.SupplierCode;
            getComponentById.SupplierName = updateComponentDto.SupplierName;
            getComponentById.SupplierShortName = updateComponentDto.SupplierShortName;
            getComponentById.PositionCode = updateComponentDto.PositionCode;
            getComponentById.InventoryNumber = double.Parse(updateComponentDto.InventoryNumber);
            getComponentById.MinInventoryNumber = double.Parse(updateComponentDto.MinInventoryNumber);
            getComponentById.MaxInventoryNumber = double.Parse(updateComponentDto.MaxInventoryNumber);
            getComponentById.ComponentInfo = updateComponentDto.ComponentInfo;
            getComponentById.Note = updateComponentDto.Note;
            getComponentById.UpdatedAt = DateTime.Now;
            getComponentById.UpdatedBy = userId;
            getComponentById.FactoryId = getIdFactory;
            getComponentById.FactoryName = getNameFactory;

            //Check Layout đã tồn tại trong Positions Join Storages:
            //Lấy ra Layout(từ đầu chuỗi đến khi gặp ký tự '-' hoặc '/') từ PositionCode mà người dùng nhập vào:
            var getLayout = updateComponentDto.PositionCode.Split(new char[] { '-', '/' })[0];

            var CheckExistLayout = await (from p in _storageContext.Positions.AsNoTracking()
                                          join f in _storageContext.Factories.AsNoTracking() on p.FactoryId equals f.Id
                                          join s in _storageContext.Storages.AsNoTracking() on p.StorageId equals s.Id
                                          where (s.Layout.Contains(getLayout))
                                          select p).FirstOrDefaultAsync();

            if (CheckExistLayout != null)
            {
                getComponentById.Layout = CheckExistLayout.Layout;
                getComponentById.StorageId = CheckExistLayout.StorageId;
            }
            else
            {
                //Insert into Storage:
                Guid getStorageId = Guid.Empty;
                var newStorage = new Storage.API.Infrastructure.Entity.Storage
                {
                    Id = Guid.NewGuid(),
                    Layout = getLayout,
                    FactoryId = getIdFactory,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };

                getStorageId = newStorage.Id;
                await _storageContext.Storages.AddAsync(newStorage);

                //Update Positions:

                getComponentById.Layout = getLayout;
                getComponentById.StorageId = getStorageId;
            }

            await _storageContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Chỉnh sửa linh kiện thành công",
                Data = null
            };
        }

        public async Task<ResponseModel> DeleteComponents(List<string> ids)
        {
            //Xóa danh sách linh kiện với ids tương ứng:
            var getComponents = await _storageContext.Positions.Where(x => (ids.Select(i => i.ToLower()).ToList()).Contains(x.Id.ToString().ToLower())).ToListAsync();

            var listStorageId = getComponents.Select(x => x.StorageId).Distinct().ToList();

            if (getComponents.Any())
            {
                _storageContext.Positions.RemoveRange(getComponents);
                await _storageContext.SaveChangesAsync();
            }

            //Check khu vực của những linh kiện vừa xóa, nếu khu vực đó mà không còn linh kiện nào thì xóa luôn khu vực đó đi:
            var storages = await (from s in _storageContext.Storages.AsNoTracking()
                                  join p in _storageContext.Positions.AsNoTracking() on s.Id equals p.StorageId into p_lefjoin
                                  from p in p_lefjoin.DefaultIfEmpty()
                                  where listStorageId.Contains(s.Id) && p == null
                                  select s).ToListAsync();

            _storageContext.Storages.RemoveRange(storages);
            await _storageContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Xóa linh kiện thành công.",
                Data = null
            };
        }

        public async Task<ResponseModel<List<ComponentFilterItemResultDto>>> GetAllComponentsToExport(ComponentsFilterToExportDto model)
        {
            var user = (ValidateTokenResultDto)_httpContext.Items[Constants.HttpContextModel.UserKey];
            var factoryFilter = user != null ? user.RoleClaims
                .Where(x => x.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY && !string.IsNullOrEmpty(x.ClaimValue))
                .Select(x => x.ClaimValue.ToUpper()).ToList() : new List<string>();

            var query = from p in _storageContext.Positions
                        select new ComponentFilterQueryResultDto
                        {
                            Id = p.Id,
                            Layout = p.Layout,
                            FactoryId = p.FactoryId,
                            ComponentCode = p.ComponentCode,
                            ComponentName = p.ComponentName,
                            SupplierCode = p.SupplierCode,
                            SupplierName = p.SupplierName,
                            ComponentPosition = p.PositionCode,
                            InventoryNumber = p.InventoryNumber,
                            MaxInventoryNumber = p.MaxInventoryNumber,
                            MinInventoryNumber = p.MinInventoryNumber,
                            CreatedAt = p.CreatedAt,
                            UpdatedAt = p.UpdatedAt,
                            SupplierShortName = p.SupplierShortName,
                            ComponentInfo = p.ComponentInfo,
                            Note = p.Note
                        };
            if (query?.Any() == false)
            {
                return new ResponseModel<List<ComponentFilterItemResultDto>>()
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Không có bản ghi nào",
                    Data = new List<ComponentFilterItemResultDto>()
                };
            }

            if (factoryFilter?.Any() == true)
            {
                query = query.Where(x => factoryFilter.Contains(x.FactoryId.ToString().ToUpper()));
            }
            var inventoryStatus = (InventoryStatus)int.Parse(model.InventoryStatus);
            switch (inventoryStatus)
            {
                // Vị trí gần hết linh kiện
                // Các bản ghi có Tồn kho thực tế <= tồn kho nhỏ nhất
                case InventoryStatus.PositionNearlyOutOfStock:
                    query = query.Where(x => x.InventoryNumber <= x.MinInventoryNumber);
                    break;
                // Linh kiện mới được cập nhật
                // Các bản ghi có thời gian chỉnh sửa, tạo mới trong vòng 7 ngày gần nhất tính từ thời điểm hiện tại
                case InventoryStatus.NewComponentsHasJustUpdated:
                    query = query.Where(x => (x.CreatedAt.HasValue && EF.Functions.DateDiffDay(x.CreatedAt.Value.Date, DateTime.Now.Date) <= 7) || (x.UpdatedAt.HasValue && EF.Functions.DateDiffDay(x.UpdatedAt.Value.Date, DateTime.Now.Date) <= 7));
                    break;
                default:
                    break;
            }
            if (string.IsNullOrEmpty(model.AllLayouts) && model.LayoutIds?.Any() == true)
            {
                query = query.Where(x => model.LayoutIds.Contains(x.Layout));
            }
            if (!string.IsNullOrEmpty(model.ComponentCode))
            {
                query = query.Where(x => x.ComponentCode.ToLower() == model.ComponentCode.ToLower());
            }
            if (!string.IsNullOrEmpty(model.ComponentName))
            {
                query = query.Where(x => x.ComponentName.ToLower() == model.ComponentName.ToLower());
            }
            if (!string.IsNullOrEmpty(model.SupplierName))
            {
                query = query.Where(x => x.SupplierName.ToLower() == model.SupplierName.ToLower());
            }
            if (!string.IsNullOrEmpty(model.ComponentPosition))
            {
                query = query.Where(x => x.ComponentPosition.ToLower() == model.ComponentPosition.ToLower());
            }
            if (model.ComponentInventoryQtyStart.HasValue && model.ComponentInventoryQtyEnd.HasValue)
            {
                query = query.Where(x => x.InventoryNumber.HasValue
                && x.InventoryNumber.Value >= model.ComponentInventoryQtyStart.Value
                && x.InventoryNumber.Value <= model.ComponentInventoryQtyEnd.Value);
            }
            else if (model.ComponentInventoryQtyStart.HasValue)
            {
                query = query.Where(x => x.InventoryNumber.HasValue && x.InventoryNumber.Value >= model.ComponentInventoryQtyStart.Value);
            }
            else if (model.ComponentInventoryQtyEnd.HasValue)
            {
                query = query.Where(x => x.InventoryNumber.HasValue && x.InventoryNumber.Value <= model.ComponentInventoryQtyEnd.Value);
            }

            var pagedResultModel = query.OrderByDescending(c => c.Id);
            var result = new ResponseModel<List<ComponentFilterItemResultDto>>(new List<ComponentFilterItemResultDto>());

            // Mapping block patched: use x.ComponentPosition instead of x.PositionCode
            result.Data = pagedResultModel.Select(x => new ComponentFilterItemResultDto
            {
                Id = x.Id,
                ComponentCode = x.ComponentCode,
                ComponentName = x.ComponentName,
                SupplierCode = x.SupplierCode,
                SupplierName = x.SupplierName,
                ComponentPosition = x.ComponentPosition,
                InventoryNumber = x.InventoryNumber,
                MaxInventoryNumber = x.MaxInventoryNumber,
                MinInventoryNumber = x.MinInventoryNumber,
                SupplierShortName = x.SupplierShortName,
                ComponentInfo = x.ComponentInfo,
                Note = x.Note,
            }).ToList();
            result.Code = StatusCodes.Status200OK;
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel> DeleteLayout(string layout)
        {
            //Xóa khu vực với id truyền xuống:
            var getLayout = await _storageContext.Storages.FirstOrDefaultAsync(x => x.Layout.ToLower().Contains(layout.ToLower()));

            if (getLayout != null)
            {
                _storageContext.Storages.RemoveRange(getLayout);
            }

            //Check nếu còn những linh kiện thuộc khu vực vừa xóa => Xóa luôn những linh kiện đó đi:
            var getComponents = await _storageContext.Positions.Where(x => x.Layout.ToLower().Contains(layout.ToLower())).ToListAsync();

            _storageContext.Positions.RemoveRange(getComponents);
            await _storageContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Xóa khu vực thành công.",
                Data = null
            };
        }

        public async Task<ResponseModel<ComponentItemImportResultDto>> ImportExcelComponentListAsync([FromForm] IFormFile file)
        {
            var result = new ResponseModel<ComponentItemImportResultDto>(StatusCodes.Status200OK, new ComponentItemImportResultDto(new List<ComponentCellDto>(), 0, 0));
            if (file == null)
            {
                result.Code = StatusCodes.Status500InternalServerError;
                result.Message = "File import không tồn tại";
                return await Task.FromResult(result);
            }
            else if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "File sai định dạng, vui lòng chọn lại file.";
                return await Task.FromResult(result);
            }

            if (file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx"))
            {
                await ImportComponentAsync(file, result);
            }

            return await Task.FromResult(result);
        }

        private string GetCellValue(ExcelWorksheet sourceSheet, int row, int headerColumnIndex)
        {
            if (headerColumnIndex == -1)
                return string.Empty;
            var value = sourceSheet.GetValue(row, headerColumnIndex);
            return value != null ? value.ToString() : string.Empty;
        }

        private bool CheckRequiredColumn(params string[] columns)
        {
            if (columns.Any(string.IsNullOrEmpty))
            {
                return false;
            }
            return true;
        }

        private List<ComponentCellDto> GetDataFromCell(IList<int> rows, ExcelWorksheet sheet, ComponentColumnIndexDto cellHeader)
        {
            //init thread safe insertList 
            var dataFromCellConcurrentBag = new ConcurrentBag<ComponentCellDto>();

            //parallel through insertList rows
            Parallel.ForEach(rows, (row) =>
            {
                var hasNo = int.TryParse(GetCellValue(sheet, row, cellHeader.NoColumnIndex), out int idx);
                ComponentCellDto componentCellDto = new ComponentCellDto
                {

                    No = hasNo ? idx : (row - 1),
                    ComponentCode = GetCellValue(sheet, row, cellHeader.ComponentCodeColumnIndex),
                    ComponentName = GetCellValue(sheet, row, cellHeader.ComponentNameColumnIndex),
                    SupplierCode = GetCellValue(sheet, row, cellHeader.SupplierCodeColumnIndex),
                    SupplierName = GetCellValue(sheet, row, cellHeader.SupplierNameColumnIndex),
                    SupplierShortName = GetCellValue(sheet, row, cellHeader.SupplierShortNameColumnIndex),
                    PositionCode = GetCellValue(sheet, row, cellHeader.PositionCodeColumnIndex),
                    MinInventoryNumber = GetCellValue(sheet, row, cellHeader.MinInventoryNumberColumnIndex),
                    MaxInventoryNumber = GetCellValue(sheet, row, cellHeader.MaxInventoryNumberColumnIndex),
                    InventoryNumber = GetCellValue(sheet, row, cellHeader.InventoryNumberColumnIndex),
                    ComponentInfo = GetCellValue(sheet, row, cellHeader.ComponentInfoColumnIndex),
                    Note = GetCellValue(sheet, row, cellHeader.NoteColumnIndex),
                    RowNumber = row

                };
                dataFromCellConcurrentBag.Add(componentCellDto);

            });
            //return insertList from posCodeConcurrentBag
            return dataFromCellConcurrentBag.ToList();
        }

        private (List<Infrastructure.Entity.Factory>, List<Infrastructure.Entity.Storage>) SetFactoriesAndStoragesEntities(Regex positionRegex, IList<ComponentCellDto> dataFromCell, string importer)
        {

            //init factory & storage insertList entity                     
            var factories = new List<Infrastructure.Entity.Factory>();
            var storages = new List<Infrastructure.Entity.Storage>();
            if (dataFromCell != null)
            {
                // get factory from positon code
                var factoryCodes = dataFromCell.Where(x => !string.IsNullOrEmpty(x.PositionCode)).Select(x => x.PositionCode.ElementAt(0).ToString()).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();

                //get storage from postion code
                var storageLayouts = dataFromCell.Select(x => GetLayout(positionRegex.Match(x.PositionCode).Value)).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();


                var notExistedFactories = factoryCodes.Where(x => !_storageContext.Factories.Select(x => x.Code).ToList().Contains(x));
                var notExistedStorages = storageLayouts.Where(x => !_storageContext.Storages.Select(x => x.Layout).Distinct().ToList().Contains(x));



                foreach (var factoryCode in notExistedFactories)
                {
                    var factory = new Infrastructure.Entity.Factory
                    {
                        Id = Guid.NewGuid(),
                        Name = $"F{factoryCode}",
                        Code = $"{factoryCode}",
                        Status = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = importer
                    };
                    factories.Add(factory);
                }

                foreach (var storageLayout in notExistedStorages)
                {
                    var factoryId = notExistedFactories.Count() > 0 ? factories.FirstOrDefault(x => x.Code == storageLayout.ElementAt(0).ToString()).Id : _storageContext.Factories.FirstOrDefault(x => x.Code == storageLayout.ElementAt(0).ToString()).Id;
                    var storage = new Infrastructure.Entity.Storage
                    {
                        Id = Guid.NewGuid(),
                        Layout = storageLayout,
                        CreatedAt = DateTime.Now,
                        FactoryId = factoryId,
                        CreatedBy = importer
                    };
                    storages.Add(storage);
                }

                //var listFactoryCode = context.Factories.Select(x => x.Code).ToList();
                //var factoriesToInsert = factories.Where(x => !listFactoryCode.Contains(x.Code)).ToList();

                //var listStorageLayout = context.Storages.Select(x => x.Layout).ToList();
                //var storagesToInsert = storages.Where(x => !listStorageLayout.Contains(x.Layout)).ToList();
                if (factories.Count > 0)
                    _storageContext.Factories.AddRange(factories);
                if (storages.Count > 0)
                    _storageContext.Storages.AddRange(storages);

                _storageContext.SaveChanges();


                return (_storageContext.Factories.Where(x => factoryCodes.Contains(x.Code)).ToList(), _storageContext.Storages.Where(x => storageLayouts.Contains(x.Layout)).ToList());
            }
            return new();
        }

        private async Task<ValidateCellDto> ValidateCellData(List<ComponentCellDto> dataFromCell, ExcelWorksheet sourceSheet, Regex positionRegex)
        {
            ValidateCellDto validateCellDto = new ValidateCellDto();
            HashSet<string> positionSupplierComponent = new();
            List<bool> validList = new();

            var components = await _storageContext.Positions.Where(x => !string.IsNullOrEmpty(x.PositionCode)).Select(x => new { x.PositionCode, x.ComponentCode, x.SupplierCode }).ToListAsync();
            var dataCells = dataFromCell.Select(x => new { x.PositionCode, x.ComponentCode, x.SupplierCode });

            foreach (var item in dataFromCell)
            {
                var errorsStrBuilder = new StringBuilder();
                bool isValid = true;
                // position code
                if (!string.IsNullOrEmpty(item.PositionCode))
                {
                    if (item.PositionCode.Length > 20)
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Vị trí cố định tối đa 20 ký tự. ");
                    }
                    if (!positionRegex.IsMatch(item.PositionCode))
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Vị trí cố định không đúng. ");

                    }
                }
                else
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu vị trí cố định. ");
                }

                // component code
                if (!string.IsNullOrEmpty(item.ComponentCode))
                {
                    if (!StringHelper.HasOnlyNormalEnglishCharacters(item.ComponentCode))
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Mã linh kiện không đúng. ");
                    }
                    else if (item.ComponentCode.Length != 9)
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Mã linh kiện tối đa 9 ký tự. ");
                    }

                }
                else
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu mã linh kiện. ");
                }

                // component name
                if (!string.IsNullOrEmpty(item.ComponentName))
                {
                    if (item.ComponentName.Length > 150)
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Tên linh kiện tối đa 150 ký tự. ");
                    }
                }
                else
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu tên linh kiện. ");
                }

                // supplier code
                if (!string.IsNullOrEmpty(item.SupplierCode))
                {
                    if (item.SupplierCode.Length > 50)
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Mã nhà cung cấp tối đa 50 ký tự. ");
                    }

                }
                else
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu mã nhà cung cấp. ");
                }

                // supplier name
                if (!string.IsNullOrEmpty(item.SupplierName))
                {
                    if (item.SupplierName.Length > 250)
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Tên nhà cung cấp tối đa 250 ký tự. ");
                    }
                }
                else
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu tên nhà cung cấp. ");
                }

                // supplier short name
                if (!string.IsNullOrEmpty(item.SupplierShortName))
                {
                    if (item.SupplierShortName.Length > 250)
                    {
                        isValid = false;
                        errorsStrBuilder.Append("Tên nhà cung cấp rút gọn tối đa 250 ký tự. ");
                    }
                }
                else
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu tên nhà cung cấp rút gọn. ");
                }

                // same position code with different component code existed in database
                if (components?.Count > 0 && components.Any(x => x.PositionCode == item.PositionCode && x.ComponentCode != item.ComponentCode))
                {
                    var existedPosCom = components.FirstOrDefault(x => x.PositionCode == item.PositionCode && x.ComponentCode != item.ComponentCode);
                    isValid = false;
                    errorsStrBuilder.Append($"Đã tồn tại vị trí cố định {existedPosCom.PositionCode} chứa mã linh kiện {existedPosCom.ComponentCode}.Vui lòng kiểm tra lại. ");
                }
                // same position code with different componentcode existed in file
                else if (dataCells.Any(x => x.PositionCode.ToLower() == item.PositionCode.ToLower() && x.ComponentCode.ToLower() != item.ComponentCode.ToLower()))
                {
                    var existedPosCom = dataCells.FirstOrDefault(x => x.PositionCode.ToLower() == item.PositionCode.ToLower() && x.ComponentCode.ToLower() != item.ComponentCode.ToLower());
                    isValid = false;
                    errorsStrBuilder.Append($"Đã tồn tại vị trí cố định {existedPosCom.PositionCode} chứa mã linh kiện {existedPosCom.ComponentCode}.Vui lòng kiểm tra lại. ");
                }


                // min inventory number
                if (string.IsNullOrEmpty(item.MinInventoryNumber))
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu tồn kho nhỏ nhất. ");
                }
                else if (!double.TryParse(item.MinInventoryNumber, out var _))
                {
                    isValid = false;
                    errorsStrBuilder.Append("Tồn kho nhỏ nhất không đúng định dạng. ");
                }
                else if (item.MinInventoryNumber.Length > 8)
                {
                    isValid = false;
                    errorsStrBuilder.Append("Số lượng tồn kho nhỏ nhất tối đa 8 ký tự. ");
                }

                // actual inventory number
                if (string.IsNullOrEmpty(item.InventoryNumber))
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu tồn kho thực tế. ");
                }
                else if (!double.TryParse(item.InventoryNumber, out var _))
                {
                    isValid = false;
                    errorsStrBuilder.Append("Tồn kho thực tế không đúng định dạng. ");
                }
                else if (item.InventoryNumber.Length > 8)
                {
                    isValid = false;
                    errorsStrBuilder.Append("Số lượng tồn kho thực tế tối đa 8 ký tự. ");
                }

                // max inventory number
                if (string.IsNullOrEmpty(item.MaxInventoryNumber))
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thiếu tồn kho lớn nhất. ");
                }
                else if (!double.TryParse(item.MaxInventoryNumber, out var _))
                {
                    isValid = false;
                    errorsStrBuilder.Append("Tồn kho lớn nhất không đúng định dạng. ");
                }
                else if (item.MaxInventoryNumber.Length > 8)
                {
                    isValid = false;
                    errorsStrBuilder.Append("Số lượng tồn kho lớn nhất tối đa 8 ký tự. ");
                }

                // min inventory number > max inventory number
                if (!string.IsNullOrEmpty(item.MinInventoryNumber) && double.TryParse(item.MinInventoryNumber, out var _minNum) && !string.IsNullOrEmpty(item.MaxInventoryNumber) && double.TryParse(item.MaxInventoryNumber, out var _maxNum) && _minNum > _maxNum)
                {
                    isValid = false;
                    errorsStrBuilder.Append("Tồn kho nhỏ nhất lớn hơn tồn kho lớn nhất. ");
                }

                // actual inventory number > max inventory number
                if (!string.IsNullOrEmpty(item.InventoryNumber) && double.TryParse(item.InventoryNumber, out var _actualNum) && !string.IsNullOrEmpty(item.MaxInventoryNumber) && double.TryParse(item.MaxInventoryNumber, out var _maxIntNum) && _actualNum > _maxIntNum)
                {
                    isValid = false;
                    errorsStrBuilder.Append("Tồn kho thực tế lớn hơn tồn kho lớn nhất. ");
                }

                // component info
                if (!string.IsNullOrEmpty(item.ComponentInfo) && item.ComponentInfo.Length > 250)
                {
                    isValid = false;
                    errorsStrBuilder.Append("Thông tin linh kiện tối đa 250 ký tự. ");
                }

                // note
                if (!string.IsNullOrEmpty(item.Note) && item.Note.Length > 50)
                {
                    isValid = false;
                    errorsStrBuilder.Append("Ghi chú tối đa 50 ký tự. ");
                }

                if (!isValid)
                {
                    validList.Add(isValid);
                    validateCellDto.FailCount++;
                    item.Errors = errorsStrBuilder.ToString();
                    errorsStrBuilder.Clear();
                }
            }

            if (validList.Any(x => x == false))
            {
                validateCellDto.IsValid = false;
            }

            return validateCellDto;
        }

        private (List<Position>, List<Position>) SetPositionsToEntities(Regex positionRegex, ExcelWorksheet sourceSheet, (List<Infrastructure.Entity.Factory>, List<Infrastructure.Entity.Storage>) factoriesAndStorages, List<ComponentCellDto> dataFromCell, string importer)
        {
            HashSet<string> posCodes = new();
            posCodes = _storageContext.Positions.Select(x => x.PositionCode).ToHashSet();
            var datacePosCode = dataFromCell.Where(d => string.IsNullOrEmpty(d.Errors)).Select(x => x.PositionCode).ToHashSet();


            var existedPosition = _storageContext.Positions.AsNoTracking().Where(x => datacePosCode.Contains(x.PositionCode)).ToList();

            var postionConcurrentBag = new ConcurrentBag<Position>();
            var postionExistedConcurrentBag = new ConcurrentBag<List<Position>>
            {
                existedPosition
            };
            Parallel.ForEach(dataFromCell, item =>
            {

                if (string.IsNullOrEmpty(item.Errors))
                {

                    var factoryName = factoriesAndStorages.Item1.FirstOrDefault(x => item.PositionCode.First().ToString() == x.Code)?.Name;
                    var factoryId = factoriesAndStorages.Item1.FirstOrDefault(x => item.PositionCode.First().ToString() == x.Code)?.Id;
                    var layout = factoriesAndStorages.Item2.FirstOrDefault(x => item.PositionCode.Contains(x.Layout))?.Layout;
                    var storageId = factoriesAndStorages.Item2.FirstOrDefault(x => item.PositionCode.Contains(x.Layout)).Id;

                    double.TryParse(item.MinInventoryNumber, out var minInventoryNumber);
                    double.TryParse(item.MaxInventoryNumber, out var maxInventoryNumber);
                    double.TryParse(item.InventoryNumber, out var inventoryNumber);

                    var position = new Position
                    {
                        Id = Guid.NewGuid(),
                        ComponentCode = item.ComponentCode,
                        ComponentName = item.ComponentName,
                        SupplierCode = item.SupplierCode,
                        SupplierName = item.SupplierName,
                        PositionCode = item.PositionCode,
                        MinInventoryNumber = minInventoryNumber,
                        MaxInventoryNumber = maxInventoryNumber,
                        InventoryNumber = inventoryNumber,
                        ComponentInfo = item.ComponentInfo,
                        Note = item.Note,
                        FactoryName = factoryName,
                        CreatedAt = DateTime.Now,
                        StorageId = storageId,
                        CreatedBy = importer,
                        Layout = layout,
                        FactoryId = factoryId,
                        SupplierShortName = item.SupplierShortName
                    };


                    postionConcurrentBag.Add(position);
                }

            });
            return (postionExistedConcurrentBag.SelectMany(x => x).ToList(), postionConcurrentBag.ToList());
        }
        private void InsertToDb((List<Position>, List<Position>) positions, (List<Infrastructure.Entity.Factory>, List<Infrastructure.Entity.Storage>) factoriesAndStorages, ResponseModel<ComponentItemImportResultDto> result)
        {
            //parallel insert
            var listExisted = new List<IEnumerable<Position>>();
            var list = new List<IEnumerable<Position>>();

            for (int i = 1; i <= 100; i++)
            {
                if (positions.Item1.Count < 15_000)
                    listExisted.Add(positions.Item1.Skip((i - 1) * 150).Take(150));
                else if (positions.Item1.Count < 25_000)
                    listExisted.Add(positions.Item1.Skip((i - 1) * 250).Take(250));
                else if (positions.Item1.Count < 50_000)
                    listExisted.Add(positions.Item1.Skip((i - 1) * 500).Take(500));
            }

            for (int i = 1; i <= 100; i++)
            {
                if (positions.Item2.Count < 15_000)
                    list.Add(positions.Item2.Skip((i - 1) * 150).Take(150));
                else if (positions.Item2.Count < 25_000)
                    list.Add(positions.Item2.Skip((i - 1) * 250).Take(250));
                else if (positions.Item2.Count < 50_000)
                    list.Add(positions.Item2.Skip((i - 1) * 500).Take(500));
            }


            Parallel.ForEach(listExisted, async position =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var context = scope.ServiceProvider.GetRequiredService<StorageContext>();
                    {
                        context.Positions.RemoveRange(position);
                        await context.SaveChangesAsync();


                    }
                }
            });


            Parallel.ForEach(list, async position =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var context = scope.ServiceProvider.GetRequiredService<StorageContext>();
                    {
                        await context.Positions.AddRangeAsync(position);
                        await context.SaveChangesAsync();


                    }
                }
            });

            result.Data.SuccessCount = positions.Item2.Count();
        }
        private async Task ImportComponentAsync(IFormFile file, ResponseModel<ComponentItemImportResultDto> result)
        {
            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    int totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    if (sourceSheet != null)
                    {
                        sourceSheet.Cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sourceSheet.Cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        sourceSheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                        Regex positionCodeRegex = new Regex(@RegexPattern.PositionCodeRegex);


                        //get header column
                        ComponentColumnIndexDto cellHeader = GetHeaderFromCell(sourceSheet);

                        //get data from cell
                        var dataFromCell = GetDataFromCell(rows, sourceSheet, cellHeader);

                        //validate cell data
                        if (cellHeader.ComponentNameColumnIndex == -1 || cellHeader.ComponentCodeColumnIndex == -1 ||
                          cellHeader.SupplierCodeColumnIndex == -1 || cellHeader.SupplierNameColumnIndex == -1 || cellHeader.SupplierShortNameColumnIndex == -1 ||
                          cellHeader.PositionCodeColumnIndex == -1 || cellHeader.MinInventoryNumberColumnIndex == -1 || cellHeader.MaxInventoryNumberColumnIndex == -1 ||
                          cellHeader.InventoryNumberColumnIndex == -1)
                        {
                            result.Code = (int)HttpStatusCodes.InvalidFileExcel;
                            result.Message = "File không đúng định dạng. Vui lòng thử lại.";
                            return;
                        }
                        if (dataFromCell.Count == 0)
                        {
                            result.Code = (int)HttpStatusCodes.InvalidFileExcel;
                            result.Message = "File không đúng định dạng. Vui lòng thử lại.";
                            return;
                        }

                        var validateCellData = await ValidateCellData(dataFromCell, sourceSheet, positionCodeRegex);


                        // set factories & storages entity
                        var factoriesAndStorages = SetFactoriesAndStoragesEntities(positionCodeRegex, dataFromCell, _httpContext.CurrentUser().UserId);

                        //set data to position entities
                        var positions = SetPositionsToEntities(positionCodeRegex, sourceSheet, factoriesAndStorages, dataFromCell, _httpContext.CurrentUser().UserId); // fixed variable name

                        //insert to database
                        InsertToDb(positions, factoriesAndStorages, result);

                        //return excel result

                        //result.Data.SuccessCount = validateCellData.SuccessCount;
                        result.Data.FailCount = validateCellData.FailCount;

                        if (!validateCellData.IsValid)
                        {
                            result.Data.FailedImportComponents = dataFromCell.Where(x => !string.IsNullOrEmpty(x.Errors)).ToList();
                        }


                    }
                }
            }
        }


        private ComponentColumnIndexDto GetHeaderFromCell(ExcelWorksheet sourceSheet)
        {
            var componentColumnIndexes = new ComponentColumnIndexDto
            {
                NoColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.No),
                ComponentCodeColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.ComponentCode),
                ComponentNameColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.ComponentName),
                SupplierCodeColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.SupplierCode),
                SupplierNameColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.SupplierName),
                SupplierShortNameColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.SupplierShortName),
                PositionCodeColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.PositionCode),
                MinInventoryNumberColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.MinInventoryNumber),
                MaxInventoryNumberColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.MaxInventoryNumber),
                InventoryNumberColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.InventoryNumber),
                ComponentInfoColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.ComponentInfo),
                NoteColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.Note),
                ErrorsColumnIndex = sourceSheet.GetColumnByName(Constants.ColumnComponentImport.Errors)
            };
            return componentColumnIndexes;
        }

        private string GetLayout(string positionCode)
        {
            var storageLayout = string.Empty;
            if (positionCode.Contains("-"))
            {
                var getStorageLayout = positionCode.Split("-").FirstOrDefault();
                storageLayout = getStorageLayout.Contains("/") ? getStorageLayout.Split("/").FirstOrDefault() : getStorageLayout;
            }
            else if (positionCode.Contains("/"))
            {
                storageLayout = positionCode.Split("/").FirstOrDefault();
            }
            else
            {
                storageLayout = positionCode;
            }
            return storageLayout;
        }

        public async Task<ValidateFilterDto<ComponentsFilterErrorDto>> ValidateFilterModelGetFilterComponents(ComponentsFilterDto model)
        {
            var result = new ValidateFilterDto<ComponentsFilterErrorDto> { Data = new ComponentsFilterErrorDto(), IsInvalid = false };
            if (model.ComponentInventoryQtyStart.HasValue && model.ComponentInventoryQtyEnd.HasValue && model.ComponentInventoryQtyStart.Value > model.ComponentInventoryQtyEnd.Value)
            {
                result.Code = StatusCodes.Status400BadRequest;
                result.Message = "Sai định dạng, vui lòng nhập lại.";
                result.IsInvalid = true;
            }

            if (string.IsNullOrEmpty(model.InventoryStatus))
            {
                result.Data.InventoryStatus = "Vui lòng nhập trạng thái tồn kho.";
                result.IsInvalid = true;
            }
            else if ((int.TryParse(model.InventoryStatus, out var _validStt) && _validStt < default(int)) || !int.TryParse(model.InventoryStatus, out var _invalidStt))
            {
                result.Data.InventoryStatus = "Trạng thái tồn kho không đúng định dạng.";
                result.IsInvalid = true;
            }
            else if (!((InventoryStatus[])Enum.GetValues(typeof(InventoryStatus))).Any(x => x == (InventoryStatus)int.Parse(model.InventoryStatus)))
            {
                result.Data.InventoryStatus = "Trạng thái tồn kho không hợp lệ";
                result.IsInvalid = true;
            }
            return await Task.FromResult(result);
        }

        public async Task<ResponseModel<List<DropDownListItemDto>>> GetInventoryStatusDropDownList()
        {
            var result = new ResponseModel<List<DropDownListItemDto>>(new List<DropDownListItemDto>());
            foreach (var inventoryStatusItem in (InventoryStatus[])Enum.GetValues(typeof(InventoryStatus)))
            {
                result.Data.Add(new DropDownListItemDto { Id = ((int)inventoryStatusItem).ToString(), Name = EnumHelper<InventoryStatus>.GetDisplayValue(inventoryStatusItem) });
            }
            result.Code = StatusCodes.Status200OK;
            return await Task.FromResult(result);
        }
    }
}
