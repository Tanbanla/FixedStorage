using BIVN.FixedStorage.Services.Common.API.Dto.Location;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using Inventory.API.Service.Validate;

namespace Inventory.API.Service
{
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _logger;
        private readonly InventoryContext _inventoryContext;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly RestClientFactory _restClientFactory;

        public LocationService(ILogger<LocationService> logger,
                               InventoryContext inventoryContext,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,
                               RestClientFactory restClientFactory
                            )
        {
            _logger = logger;
            _inventoryContext = inventoryContext;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _restClientFactory = restClientFactory;
        }
        public async Task<ResponseModel<IEnumerable<LocationViewModel>>> GetLocations(string departmentName)
        {
            var locations = _inventoryContext.InventoryLocations.AsNoTracking()
                                                                .Where(x => !x.IsDeleted)
                                                                .OrderBy(x => x.Name)
                                                                .AsQueryable();

            if (!string.IsNullOrEmpty(departmentName))
            {
                locations = locations.Where(x => x.DepartmentName.ToLower().Contains(departmentName.ToLower()));
            }

            if (locations == null || !locations.Any()) 
            {
                return new ResponseModel<IEnumerable<LocationViewModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Chưa có khu vực kiểm kê. Vui lòng thêm mới khu vực để quản lý."
                };
            }

            //Phần phân quyền thao tác khu vực, khi gọi đến phần này, nếu khu vực đã được gán thì bỏ qua khu vực này
            var assignedLocations = await _inventoryContext.AccountLocations.AsNoTracking()
                                                                            .Select(x => x.LocationId).ToListAsync();

            var filtered = locations.AsEnumerable()
                                    .Select(x => new LocationViewModel
                                    {
                                        Id = x.Id,
                                        LocationName = x.Name,
                                        CreateAt = x.CreatedAt,
                                        CreateBy = x.CreatedBy,
                                        DepartmentName = x.DepartmentName,
                                        FactoryNames = x.FactoryName,
                                        UpdateAt = x.UpdatedAt.HasValue ? x.UpdatedAt.Value : null,
                                        IsLocationAssigned = assignedLocations?.Contains(x.Id) == true ? true : false
                                    });

            return new ResponseModel<IEnumerable<LocationViewModel>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách khu vực kiểm kê.",
                Data = filtered
            };
        }
        public async Task<ResponseModel<bool>> AddLocation(CreateLocationDto createLocationDto)
        {
            if(createLocationDto == null || !createLocationDto.FactoryNames.Any())
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Dữ liệu không hợp lệ."
                };
            }

            //Kiểm tra tên khu vực đã tồn tại
            var existLocationName = await _inventoryContext.InventoryLocations.Where(x => !x.IsDeleted)
                                                                              .CountAsync(x => x.Name.ToLower() == createLocationDto.Name.Trim().ToLower());
            if(existLocationName > 0)
            {
                return new ResponseModel<bool>
                {
                    Code = (int)HttpStatusCodes.ExistLocatioName,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ExistLocatioName)
                };
            }

            InventoryLocation locationEntity = new InventoryLocation
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedBy = createLocationDto.userId.ToString(),
                Name = createLocationDto.Name,
                DepartmentName = createLocationDto.DepartmentName,
                FactoryName = string.Join(", ", createLocationDto.FactoryNames.DistinctBy(x => x.ToLower())),
            };

            _inventoryContext.InventoryLocations.Add(locationEntity);
            await _inventoryContext.SaveChangesAsync();

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Thêm mới khu vực kiểm kê thành công."
            };
        }
        public async Task<ResponseModel<bool>> UpdateLocation(UpdateLocationDto updateLocationDto)
        {
            if (updateLocationDto == null || !updateLocationDto.FactoryNames.Any())
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Dữ liệu không hợp lệ."
                };
            }

            var canEditLocationResult = await CanEditLocation(updateLocationDto.Id);
            if (!canEditLocationResult.Data)
                return canEditLocationResult;

            var updateEntity = await _inventoryContext.InventoryLocations.FirstOrDefaultAsync(x => x.Id == updateLocationDto.Id);

            var executionStrategy = _inventoryContext.Database.CreateExecutionStrategy();
            var executeResult = await executionStrategy.ExecuteAsync<bool>(async () =>
            {
                using (var transaction = await _inventoryContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        //Nếu chỉnh sửa khu vực
                        await LongTaskForUpdateLocation(updateEntity, updateLocationDto);

                        //Update entity
                        updateEntity.UpdatedAt = DateTime.Now;
                        updateEntity.UpdatedBy = updateLocationDto.UpdateBy.ToString();
                        updateEntity.Name = updateLocationDto.Name;
                        updateEntity.DepartmentName = updateLocationDto.DepartmentName;
                        updateEntity.FactoryName = string.Join(", ", updateLocationDto.FactoryNames.DistinctBy(x => x.ToLower()));


                        await _inventoryContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await transaction.DisposeAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogHttpContext(_httpContext, ex.Message);
                        return false;
                    }

                }
            });

            if (!executeResult)
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Data = false,
                    Message = "Chỉnh sửa khu vực kiểm kê thất bại."
                };

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Chỉnh sửa khu vực kiểm kê thành công."
            };
        }

        private async Task LongTaskForUpdateLocation(InventoryLocation inventoryLocation, UpdateLocationDto updateModel)
        {
            //Nếu tên khu vực thay đổi => cập nhật các bảng có liên quan thành giá trị mới
            if (!string.Equals(inventoryLocation.Name, updateModel.Name, StringComparison.OrdinalIgnoreCase))
            {
                _inventoryContext.AuditTargets.Where(x => x.LocationName == inventoryLocation.Name)
                           .BatchUpdate(p => p.LocationName, updateModel.Name);

                _inventoryContext.ReportingLocations.Where(x => x.LocationName == inventoryLocation.Name)
                            .BatchUpdate(p => p.LocationName, updateModel.Name);

                _inventoryContext.InventoryDocs.Where(x => x.LocationName == inventoryLocation.Name)
                            .BatchUpdate(p => p.LocationName, updateModel.Name, 4000);
            }
            //Nếu tên phòng ban thay đổi => cập nhật các bảng có liên quan thành giá trị mới
            if (!string.Equals(inventoryLocation.DepartmentName, updateModel.DepartmentName, StringComparison.OrdinalIgnoreCase))
            {
                _inventoryContext.AuditTargets.Where(x => x.DepartmentName == inventoryLocation.DepartmentName)
                            .BatchUpdate(p => p.DepartmentName, updateModel.DepartmentName);

                _inventoryContext.ReportingDepartments.Where(x => x.DepartmentName == inventoryLocation.DepartmentName)
                            .BatchUpdate(p => p.DepartmentName, updateModel.DepartmentName);

                _inventoryContext.InventoryDocs.Where(x => x.DepartmentName == inventoryLocation.DepartmentName)
                            .BatchUpdate(p => p.DepartmentName, updateModel.DepartmentName, 4000);
            }
        }

        

        public async Task<ResponseModel<bool>> DeleteLocation(Guid locationId, Guid userId)
        {
            var anyAssinged = await _inventoryContext.AccountLocations.AsNoTracking()
                                                                    .AnyAsync(x => x.LocationId == locationId);

            //Nếu khu vực muốn xóa đang gán cho người thao tác nào đó thì báo lỗi
            if(anyAssinged)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = "Bạn không thể xóa khu vực này do đã gán người thao tác."
                };
            }

            var deleteLocationEntity = await _inventoryContext.InventoryLocations.Where(x => x.Id == locationId).FirstOrDefaultAsync();
            if(deleteLocationEntity == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = false,
                    Message = "Không tìm thấy khu vực kiểm kê."
                };
            }

            //Soft delete
            deleteLocationEntity.IsDeleted = true;
            deleteLocationEntity.DeletedAt = DateTime.Now;
            deleteLocationEntity.DeletedBy = userId.ToString();

            //Xóa khu vực ở bảng quan hệ
            var deleteRelationshipLocation = await _inventoryContext.AccountLocations.Where(x => x.LocationId == locationId).ToListAsync();
            _inventoryContext.RemoveRange(deleteRelationshipLocation);

            await _inventoryContext.SaveChangesAsync();

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Xóa khu vực kiểm kê thành công."
            };
        }

        public async Task<ResponseModel<LocationViewModel>> LocationDetail(Guid locationId)
        {
            var location = await _inventoryContext.InventoryLocations.FirstOrDefaultAsync(x => x.Id == locationId);
            if (location == null)
            {
                return new ResponseModel<LocationViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy khu vực kiểm kê."
                };
            }

            var viewModel = new LocationViewModel
            {
                Id = location.Id,
                LocationName = location.Name,
                DepartmentName = location.DepartmentName,
                FactoryNames = location.FactoryName,
                CreateAt = location.CreatedAt,
                UpdateAt = location.UpdatedAt,
                CreateBy = location.CreatedBy,
            };

            return new ResponseModel<LocationViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = viewModel
            };
        }

        public async Task<ResponseModel<ResultSet<IEnumerable<InventoryActorInfoViewModel>>>> AssignmentActorList()
        {
            //Dùng chung với chức năng export nên mặc định lấy hết danh sách

            var users = await GetInternalUsers();
            if (!users.Any())
            {
                return new ResponseModel<ResultSet<IEnumerable<InventoryActorInfoViewModel>>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy người dùng nào."
                };
            }

            //Lấy tất cả tài khoản kiểm kê
            var inventoryAccounts = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                    .Select(x => new
                                                    {
                                                        x.Id,
                                                        x.UserId,
                                                        x.UserName,
                                                        x.RoleType
                                                    }).ToDictionaryAsync(x => x.UserId, x => x);

            var locationsAssignedInventory = _inventoryContext.AccountLocations.AsNoTracking()
                                                                .Include(x => x.InventoryAccount)
                                                              .Where(x => x.InventoryAccount.RoleType.HasValue &&  x.InventoryAccount.RoleType.Value == InventoryAccountRoleType.Inventory)
                                                                .AsSplitQuery()
                                                              .AsEnumerable()
                                                              .DistinctBy(x => x.Id)
                                                              .ToDictionary(x => x.Id, x => new
                                                              {
                                                                  x.Id,
                                                                  UserId = x.InventoryAccount.UserId
                                                              });

            var locationAssignedAudit = _inventoryContext.AccountLocations.AsNoTracking().Include(x => x.InventoryAccount)
                                                        .Where(x => x.InventoryAccount.RoleType.HasValue && x.InventoryAccount.RoleType.Value == InventoryAccountRoleType.Audit)
                                                        .AsSplitQuery()
                                                        .AsEnumerable()
                                                        .DistinctBy(x => x.Id)
                                                         .ToDictionary(x => x.Id, x => new
                                                         {
                                                             x.Id,
                                                             UserId = x.InventoryAccount.UserId,
                                                         });


            //var locations = await (from l in _inventoryContext.InventoryLocations.AsNoTracking().Where(x => !x.IsDeleted)
            //                    let isAssignedInventory = locationsAssignedInventory.ContainsKey(l.Id)
            //                    let isAssignedAudit = locationAssignedAudit.ContainsKey(l.Id)
            //                    select new LocationAssignedViewModel
            //                    {
            //                        Id = l.Id,
            //                        Name = l.Name,
            //                        DepartmentName = l.DepartmentName,
            //                        FactoryName = l.FactoryName,
            //                        isAssignedInventory = isAssignedInventory,
            //                        isAssignedAudit = isAssignedAudit,
            //                        UserInventoryId = isAssignedInventory ? locationsAssignedInventory[l.Id].UserId : null,
            //                        UserAuditId = isAssignedAudit ? locationAssignedAudit[l.Id].UserId : null
            //                    }).ToListAsync();


            var inventoryLocations = await _inventoryContext.InventoryLocations.AsNoTracking().Where(x => !x.IsDeleted)
                                            .Select(x => new AccountLocationViewModel
                                            {
                                                LocationId = x.Id,
                                                DepartmentName = x.DepartmentName,
                                                FactoryNames = x.FactoryName,
                                                LocationName = x.Name
                                            }).ToListAsync();

            var locations = await (from al in _inventoryContext.AccountLocations.AsNoTracking()
                                   join l in _inventoryContext.InventoryLocations.AsNoTracking().Where(x => !x.IsDeleted) on al.LocationId equals l.Id into lGroup
                                   from l in lGroup.DefaultIfEmpty()

                                   let isAssignedInventory = locationsAssignedInventory.ContainsKey(al.Id)
                                   let isAssignedAudit = locationAssignedAudit.ContainsKey(al.Id)

                                   select new LocationAssignedViewModel
                                   {
                                       Id = l.Id,
                                       Name = l.Name,
                                       DepartmentName = l.DepartmentName,
                                       FactoryName = l.FactoryName,
                                       isAssignedInventory = isAssignedInventory,
                                       isAssignedAudit = isAssignedAudit,
                                       UserInventoryId = isAssignedInventory ? locationsAssignedInventory[al.Id].UserId : null,
                                       UserAuditId = isAssignedAudit ? locationAssignedAudit[al.Id].UserId : null
                                   }).ToListAsync();


            var result = users.Where(x => (x.AccountType == AccountType.TaiKhoanChung || 
                                            x.AccountType == AccountType.TaiKhoanGiamSat) 
                                        && x.Status == (int)UserStatus.Active)
                            .Select(x => new InventoryActorInfoViewModel
                            {
                                UserId = x.Id,
                                UserName = x.UserName,
                                AllLocations = locations,
                                Locations = inventoryLocations,
                                RoleType = inventoryAccounts.ContainsKey(x.Id) ? inventoryAccounts[x.Id].RoleType.HasValue ? (int)inventoryAccounts[x.Id].RoleType.Value : null : null,
                                AccountType = x.AccountType
                            })
                            .OrderBy(x => x.UserName);

            if(result == null || !result.Any())
            {
                return new ResponseModel<ResultSet<IEnumerable<InventoryActorInfoViewModel>>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = new ResultSet<IEnumerable<InventoryActorInfoViewModel>>()
                    {
                        Data = Enumerable.Empty<InventoryActorInfoViewModel>(),
                        TotalRecords = 0
                    }
                };
            }

            ResultSet<IEnumerable<InventoryActorInfoViewModel>> resultSet = new();
            //resultSet.TotalRecords = result.Count();
            resultSet.Data = result;

            return new ResponseModel<ResultSet<IEnumerable<InventoryActorInfoViewModel>>>
            {
                Code = StatusCodes.Status200OK,
                Data = resultSet,
            };
        }

        public async Task<ResponseModel<bool>> ChangeRole(Guid userId, int? roleType, Guid actorId)
        {
            var accountEntity = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserId == userId);
            InventoryAccountRoleType? roleTypeValue = roleType.HasValue ? (InventoryAccountRoleType)Enum.ToObject(typeof(InventoryAccountRoleType), roleType.Value) : null;

            var identityUsers = await GetInternalUsers();

            //Nếu thay đổi vai trò  thì xóa hết các khu vực, chọn lại từ đầu
            if (accountEntity != null &&
                accountEntity.RoleType != roleTypeValue)
            {
                var relatedLocations = _inventoryContext.AccountLocations.Where(x => x.AccountId == accountEntity.Id).ToList();
                _inventoryContext.AccountLocations.RemoveRange(relatedLocations);
                await _inventoryContext.SaveChangesAsync();
            }

            var user = identityUsers?.FirstOrDefault(x => x.Id == userId);
            //Nếu không tìm thấy thì thêm mới
            if (accountEntity == null)
            {
                InventoryAccount newInventoryAcc = new InventoryAccount
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actorId.ToString(),
                    RoleType = roleTypeValue,
                    UserId = userId,
                };
                
                if(user != null)
                {
                    newInventoryAcc.UserName = user.UserName;
                }

                _inventoryContext.InventoryAccounts.Add(newInventoryAcc);
                await _inventoryContext.SaveChangesAsync();

            }else if (accountEntity != null)
            {
                //Mỗi lần cập nhật tài khoản kiểm kê thì cập nhật cả UserName mới nhất từ bên identity
                accountEntity.UserName = user?.UserName ?? accountEntity.UserName;
                accountEntity.RoleType  = roleTypeValue;
                accountEntity.UpdatedAt = DateTime.Now;
                accountEntity.UpdatedBy = actorId.ToString();

                _inventoryContext.InventoryAccounts.Update(accountEntity);
                await _inventoryContext.SaveChangesAsync();
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Chọn vai trò cho người thao tác thành công."
            };
        }

        public async Task<ResponseModel<bool>> ChangeLocation(Guid userId, List<Guid> locationsIds, Guid actorId)
        {
            var accountEntity = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserId == userId);
            if (accountEntity == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tìm thấy thông tin tài khoản."
                };
            }

            //Mỗi lần cập nhật tài khoản kiểm kê thì cập nhật cả UserName mới nhất từ bên identity
            var identityUsers = await GetInternalUsers();
            var identityUser = identityUsers?.FirstOrDefault(x => x.Id == userId);
            if (identityUser != null)
            {
                accountEntity.UserName = identityUser.UserName;
            }

            //Nếu reset khu vực thì xóa relationship
            if (locationsIds == null || !locationsIds.Any())
            {
                var relatedLocations = _inventoryContext.AccountLocations.Where(x => x.AccountId == accountEntity.Id).ToList();
                _inventoryContext.AccountLocations.RemoveRange(relatedLocations);
                await _inventoryContext.SaveChangesAsync();
            }
            else
            {
                List<AccountLocation> accountLocations = new();
                var existLocations = await _inventoryContext.AccountLocations.Where(x => x.AccountId == accountEntity.Id).ToListAsync();

                var relatedLocations = _inventoryContext.AccountLocations.Where(x => x.AccountId == accountEntity.Id).ToList();
                _inventoryContext.AccountLocations.RemoveRange(relatedLocations);
                await _inventoryContext.SaveChangesAsync();

                foreach (var locationId in locationsIds)
                {
                    var accountLocation = new AccountLocation
                    {
                        Id = Guid.NewGuid(),
                        AccountId = accountEntity.Id,
                        LocationId = locationId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = actorId.ToString(),
                    };
                    accountLocations.Add(accountLocation);
                }

                _inventoryContext.AccountLocations.AddRange(accountLocations);
                await _inventoryContext.SaveChangesAsync();
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = true,
                Message = "Chọn khu vực cho người thao tác thành công."
            };
        }

        private async Task<IEnumerable<ListUserModel>> GetInternalUsers()
        {
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _restClientFactory.IdentityClient().GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            return responseModel?.Data ?? Enumerable.Empty<ListUserModel>();
        }

        public async Task<ResponseModel<byte[]>> ExportAssignment()
        {
            var actorList = await AssignmentActorList();
            var result = actorList.Data.Data;

            if (result == null || !result.Any())
            {
                return new ResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phân quyền thao tác kiểm kê");

                int STTIndex = InventoryAssignExcel.ColIndexByName(InventoryAssignExcel.STT) + 1;
                int ActorIndex = InventoryAssignExcel.ColIndexByName(InventoryAssignExcel.Actor) + 1;
                int RoleIndex = InventoryAssignExcel.ColIndexByName(InventoryAssignExcel.Role) + 1;
                int LocationIndex = InventoryAssignExcel.ColIndexByName(InventoryAssignExcel.Location) + 1;
                int FactoryIndex = InventoryAssignExcel.ColIndexByName(InventoryAssignExcel.Factory) + 1;
                int DepartmentIndex = InventoryAssignExcel.ColIndexByName(InventoryAssignExcel.Department) + 1;

                // Đặt tiêu đề cho cột
                worksheet.Cells[1, STTIndex].Value = InventoryAssignExcel.STT;
                worksheet.Cells[1, ActorIndex].Value = InventoryAssignExcel.Actor;
                worksheet.Cells[1, RoleIndex].Value = InventoryAssignExcel.Role;
                worksheet.Cells[1, LocationIndex].Value = InventoryAssignExcel.Location;
                worksheet.Cells[1, FactoryIndex].Value = InventoryAssignExcel.Factory;
                worksheet.Cells[1, DepartmentIndex].Value = InventoryAssignExcel.Department;

                // Đặt kiểu và màu cho tiêu đề
                using (var range = worksheet.Cells[1, STTIndex, 1, DepartmentIndex])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.None;
                }

                Dictionary<string, string> RoleDict = new Dictionary<string, string>
                {
                    { "0", "Kiểm kê" },
                    { "1", "Giám sát" },
                    { "2", "Xúc tiến" },
                    { "3", "Xúc tiến - Người phụ trách" },
                    { "4", "Xúc tiến - Người quản lý" },
                    //{ "3", "Điều tra sai số"}
                };

                // Điền dữ liệu vào Excel
                for (int i = 0; i < result.Count(); i++)
                {
                    var item = result.ElementAtOrDefault(i);
                    string locationNames = string.Empty; 
                    var factoryNames = string.Empty;
                    string departmentNames = string.Empty;

                    if (item.RoleType == (int)InventoryAccountRoleType.Inventory)
                    {
                        locationNames = string.Join(", ", item.AllLocations.Where(x => x.UserInventoryId == item.UserId).Select(x => x.Name).Distinct().ToList());
                        factoryNames = string.Join(", ", item.AllLocations.Where(x => x.UserInventoryId == item.UserId).Select(x => x.FactoryName).Distinct().ToList());
                        departmentNames = string.Join(", ", item.AllLocations.Where(x => x.UserInventoryId == item.UserId).Select(x => x.DepartmentName).Distinct().ToList());
                    }else if (item.RoleType == (int)InventoryAccountRoleType.Audit)
                    {
                        locationNames = string.Join(", ", item.AllLocations.Where(x => x.UserAuditId == item.UserId).Select(x => x.Name).Distinct().ToList());
                        factoryNames = string.Join(", ", item.AllLocations.Where(x => x.UserAuditId == item.UserId).Select(x => x.FactoryName).Distinct().ToList());
                        departmentNames = string.Join(", ", item.AllLocations.Where(x => x.UserAuditId == item.UserId).Select(x => x.DepartmentName).Distinct().ToList());
                    }else if(item.RoleType == (int)InventoryAccountRoleType.Promotion)
                    {
                        locationNames = "Tất cả";
                        factoryNames = "Tất cả";
                        departmentNames = "Tất cả";
                    }

                    int stt = i + 1;
                    worksheet.Cells[i + 2, STTIndex].Value = stt;
                    worksheet.Cells[i + 2, ActorIndex].Value = item.UserName;
                    worksheet.Cells[i + 2, RoleIndex].Value = item.RoleType != null ? RoleDict[item.RoleType.ToString()] : string.Empty;
                    worksheet.Cells[i + 2, LocationIndex].Value = locationNames;
                    worksheet.Cells[i + 2, FactoryIndex].Value = factoryNames;
                    worksheet.Cells[i + 2, DepartmentIndex].Value = departmentNames;
                }

                // Lưu file Excel
                var stream = new MemoryStream();
                package.SaveAs(stream);

                return new ResponseModel<byte[]>
                {
                    Code = StatusCodes.Status200OK,
                    Data = stream.ToArray(),
                };
            }
        }
        public async Task<ResponseModel> GetDeparments()
        {
            var currUserModel = _httpContext.UserFromContext();

            var allLocations = _inventoryContext.InventoryLocations.AsNoTracking()
                                                        .Where(x => !x.IsDeleted)
                                                        .AsEnumerable()
                                                        .OrderBy(x => x.Name)
                                                        .Select(x => new DepartmentInventoryDto
                                                        {
                                                            DepartmentName = x.DepartmentName,
                                                            LocationName = x.Name
                                                        });

            var canViewAll = (currUserModel.AccountType == nameof(AccountType.TaiKhoanRieng) &&
                                        (currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_ALL_INVENTORY) ||
                                            currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_CURRENT_INVENTORY) ||
                                            currUserModel.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY) ||
                                            currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_REPORT)))
                                        || _httpContext.IsPromoter();

            if (!canViewAll)
            {
                var departmentNamesByRole = _inventoryContext.AccountLocations.AsNoTracking()
                                                               .Include(x => x.InventoryLocation)
                                                               .Include(x => x.InventoryAccount)
                                                               .AsEnumerable()
                                                               .Where(x => !x.InventoryLocation.IsDeleted &&
                                                                            x.InventoryAccount.UserId == Guid.Parse(currUserModel.UserId))
                                                               .Select(x => x.InventoryLocation.DepartmentName);

                allLocations = allLocations.Where(x => departmentNamesByRole.Contains(x.DepartmentName));
            }

            var result = allLocations.ToList();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                //Danh sách dropdown các màn tìm kiếm, giám sát có nhiều khu vực, kiểm kê chỉ có 1 khu vực
                Data = result.DistinctBy(x => x.DepartmentName),
                Message = "Danh sách phòng ban."
            };
        }

        public async Task<ResponseModel> GetLocationByDepartments(LocationByDepartmentDto departments)
        {
            var currUserModel = _httpContext.UserFromContext();

            if (departments.Departments.Count == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = new List<DepartmentInventoryDto>(),
                    Message = "Không tìm thấy dữ liệu phù hợp"
                };
            }

            var allLocations = _inventoryContext.InventoryLocations.AsNoTracking()
                                                        .Where(x => !x.IsDeleted)
                                                        .AsEnumerable()
                                                        .OrderBy(x => x.Name)
                                                        .Select(x => new DepartmentInventoryDto
                                                        {
                                                            DepartmentName = x.DepartmentName,
                                                            LocationName = x.Name
                                                        });

            //Nếu không phải tài khoản riêng hoặc xúc tiến thì lọc phòng ban phân quyền
            var canViewAll = (currUserModel.AccountType == nameof(AccountType.TaiKhoanRieng) &&
                                     (currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_ALL_INVENTORY) ||
                                         currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_CURRENT_INVENTORY) ||
                                         currUserModel.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY) ||
                                         currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_REPORT)))
                                     || _httpContext.IsPromoter();

            if (!canViewAll)
            {
                var departmentNamesByRole = _inventoryContext.AccountLocations.AsNoTracking()
                                                               .Include(x => x.InventoryLocation)
                                                               .Include(x => x.InventoryAccount)
                                                               .AsEnumerable()
                                                               .Where(x => !x.InventoryLocation.IsDeleted &&
                                                                            x.InventoryAccount.UserId == Guid.Parse(currUserModel.UserId))
                                                               .Select(x => x.InventoryLocation.Name);

                allLocations = allLocations.Where(x => departmentNamesByRole.Contains(x.LocationName));
            }
            else
            {
                allLocations = allLocations.Where(x => departments.Departments.Contains(x.DepartmentName));
            }

            var result = allLocations.Select(x => new DepartmentInventoryDto
            {
                DepartmentName = x.DepartmentName,
                LocationName = x.LocationName
            }).ToList();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                //Danh sách dropdown các màn tìm kiếm, giám sát có nhiều khu vực, kiểm kê chỉ có 1 khu vực
                Data = result.DistinctBy(x => x.LocationName),
                Message = "Danh sách khu vực."
            };
        }

        public async Task<ResponseModel<bool>> CanEditLocation(Guid locationId)
        {
            var afterInventoryDateResult = await new AfterInventoryDaysCheck(commonAPIConstant.AppSettings.EditLocationScheduleDays).Validate(_httpContext);
            var isAssigned = await new LocationOrDepartmentAssignedDocCheck(locationId).Validate(_httpContext);

            if (afterInventoryDateResult.Data)
            {
                if (isAssigned.Data)
                {
                    return new ResponseModel<bool>
                    {
                        Code = (int)HttpStatusCodes.UpdateLocationTakeLongTime,
                        Data = true,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.UpdateLocationTakeLongTime)
                    };
                }

                return new ResponseModel<bool> { Code = StatusCodes.Status200OK, Data = true };
            }else
            {
                if (isAssigned.Data)
                {
                    var message = isAssigned.Message + afterInventoryDateResult.Message;
                    afterInventoryDateResult.Message = message;
                    return afterInventoryDateResult;
                }else if (!isAssigned.Data)
                {
                    return new ResponseModel<bool> { Code = StatusCodes.Status200OK, Data = true };
                }
            }

            return afterInventoryDateResult;
        }

        public async Task<ResponseModel<IEnumerable<AuditorByLocationsDto>>> GetAuditorByLocations(AuditorByLocationModel locations)
        {
            var currUserModel = _httpContext.UserFromContext();

            if (!locations.Locations.Any())
            {
                return new ResponseModel<IEnumerable<AuditorByLocationsDto>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp"
                };
            }
            var allAuditors = from ia in _inventoryContext.InventoryAccounts.AsNoTracking()
                            join al in _inventoryContext.AccountLocations.AsNoTracking() on ia.Id equals al.AccountId
                            join il in _inventoryContext.InventoryLocations.AsNoTracking() on al.LocationId equals il.Id
                            where !il.IsDeleted
                            orderby ia.UserName
                            select new AuditorByLocationsDto
                            {
                                AuditorName = ia.UserName,
                                DepartmentName = il.DepartmentName,
                                LocationName = il.Name
                            };


            //Nếu không phải tài khoản riêng hoặc xúc tiến thì lọc phòng ban phân quyền
            var canViewAll = (currUserModel.AccountType == nameof(AccountType.TaiKhoanRieng) &&
                                     (currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_ALL_INVENTORY) ||
                                         currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_CURRENT_INVENTORY) ||
                                         currUserModel.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY) ||
                                         currUserModel.IsGrant(commonAPIConstant.Permissions.VIEW_REPORT)))
                                     || _httpContext.IsPromoter();

            if (!canViewAll)
            {
                var locationNamesByRole = _inventoryContext.AccountLocations.AsNoTracking()
                                                               .Include(x => x.InventoryLocation)
                                                               .Include(x => x.InventoryAccount)
                                                               .AsEnumerable()
                                                               .Where(x => !x.InventoryLocation.IsDeleted &&
                                                                            x.InventoryAccount.UserId == Guid.Parse(currUserModel.UserId))
                                                               .Select(x => x.InventoryLocation.Name);

                allAuditors = allAuditors.Where(x => locationNamesByRole.Contains(x.LocationName));
            }
            else
            {
                allAuditors = allAuditors.Where(x => locations.Locations.Contains(x.LocationName));
            }

            var items = allAuditors.ToList().DistinctBy(x => x.AuditorName);

            return new ResponseModel<IEnumerable<AuditorByLocationsDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = items,
                Message = "Danh sách người giám sát."
            };
        }
    }
}
