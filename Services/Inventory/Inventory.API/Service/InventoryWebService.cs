using System.Linq;
using BIVN.FixedStorage.Services.Common.API;
using BIVN.FixedStorage.Services.Common.API.Dto;
using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using BIVN.FixedStorage.Services.Common.API.Dto.QRCode;
using BIVN.FixedStorage.Services.Common.API.Dto.Role;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using Inventory.API.Infrastructure.Entity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Inventory.API.Service
{
    public class InventoryWebService : IInventoryWebService
    {
        private readonly ILogger<InventoryWebService> _logger;
        private readonly IRestClient _resClient;
        private readonly HttpContext _httpContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RestClientFactory _restClientFactory;
        private readonly RestClient _identityRestClient;
        private readonly IDataAggregationService _dataAggregationService;
        //private readonly IDatabaseFactoryService _databaseFactory;
        private readonly IConfiguration _configuration;
        private readonly InventoryContext _inventoryContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IDbContextFactory<InventoryContext> _dbContextFactory;

        private InventoryDocStatus[] ExcludeDocStatus = new InventoryDocStatus[]
        {
            InventoryDocStatus.NotReceiveYet,
            InventoryDocStatus.NoInventory
        };

        public InventoryWebService(InventoryContext inventoryContext,
                                  IHttpContextAccessor httpContextAccessor,
                                  RestClientFactory restClientFactory,
                                  IConfiguration configuration,
                                  ILogger<InventoryWebService> logger,
                                  IServiceScopeFactory serviceProviderFactory,
                                  IWebHostEnvironment webHostEnvironment,
                                  IDataAggregationService dataAggregationService,
                                  IBackgroundTaskQueue backgroundTaskQueue,
                                  IDbContextFactory<InventoryContext> dbContextFactory
                            )
        {
            _inventoryContext = inventoryContext;
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceProviderFactory;
            _httpContext = httpContextAccessor.HttpContext;
            _webHostEnvironment = webHostEnvironment;
            _restClientFactory = restClientFactory;
            _identityRestClient = _restClientFactory.IdentityClient();
            _dataAggregationService = dataAggregationService;
            _backgroundTaskQueue = backgroundTaskQueue;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<ResponseModel<IEnumerable<ListInventoryModel>>> ListInventory(ListInventoryDto listInventory)
        {
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            var result = (from i in _inventoryContext.Inventories.AsEnumerable()
                          join u in users on i.CreatedBy?.ToLower() ?? string.Empty equals u.Id.ToString().ToLower() into T1Group
                          from ui in T1Group.DefaultIfEmpty()
                          orderby i.CreatedAt descending
                          select new ListInventoryModel
                          {
                              InventoryId = i.Id,
                              InventoryName = i.Name,
                              InventoryDate = i.InventoryDate,
                              AuditFailPercentage = i.AuditFailPercentage,
                              Status = (int)i.InventoryStatus,
                              CreateAt = i.CreatedAt,
                              FullName = ui?.FullName ?? string.Empty
                          });

            //TH1: Nếu tài khoản chung với vai trò xúc tiến thì hiện lấy hết
            //TH2: Các trường hợp khác TH1 thì check quyền là Xem đợt kiểm kê hiện tại thì lấy các bản ghi khác trạng thái hoàn thành
            var checkIsValidAllInventory = _httpContext.User.IsGrant(Constants.Permissions.VIEW_ALL_INVENTORY);
            var checkIsValidCurrentInventory = _httpContext.User.IsGrant(Constants.Permissions.VIEW_CURRENT_INVENTORY);
            var checkAccountTypeAndRole = _httpContext.UserFromContext();
            if (!(checkAccountTypeAndRole.AccountType == nameof(AccountType.TaiKhoanChung) && checkAccountTypeAndRole.InventoryLoggedInfo.InventoryRoleType == 2))
            {
                if (checkIsValidAllInventory == false && checkIsValidCurrentInventory)
                {
                    result = result.Where(x => x.Status != (int)InventoryStatus.Finish);
                }
            }

            if (result.Count() == 0)
            {
                return new ResponseModel<IEnumerable<ListInventoryModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.Inventory.NotFoundInventoryList,
                    Data = result
                };
            }

            Func<ListInventoryModel, bool> conditionClause = (x) =>
            {
                bool isValidCreateBy = true;
                bool isValidDateRange = true;
                bool isValidStatus = true;

                //Tim kiem theo ten nguoi tao:
                if (!listInventory.CreatedBy.IsNullOrEmpty())
                {
                    isValidCreateBy = string.IsNullOrEmpty(x.FullName) ? false : x.FullName.ToLower().Contains(listInventory.CreatedBy);
                }

                //Lọc điều kiện theo 3 case: Có cả bắt đầu và kết thúc, chỉ có bắt đầu, chỉ có kết thúc
                if (listInventory.InventoryDateStart != null && listInventory.InventoryDateEnd != null)
                {
                    isValidDateRange = x.InventoryDate.Value.Date >= listInventory.InventoryDateStart.Value.Date && x.InventoryDate.Value.Date <= listInventory.InventoryDateEnd.Value.Date;
                }
                else if (listInventory.InventoryDateStart != null)
                {
                    isValidDateRange = x.InventoryDate.Value.Date >= listInventory.InventoryDateStart.Value.Date;
                }
                else if (listInventory.InventoryDateEnd != null)
                {
                    isValidDateRange = x.InventoryDate.Value.Date <= listInventory.InventoryDateEnd.Value.Date;
                }

                //Lọc điều kiện theo trạng thái
                if (listInventory.Statuses?.Any() == true)
                {
                    var convertedStatuses = listInventory.Statuses.Select(x => int.Parse(x));
                    isValidStatus = convertedStatuses.Contains(x.Status);
                }

                var aggregateCondition = isValidCreateBy && isValidDateRange && isValidStatus;
                return aggregateCondition;

            };

            var data = result?.Where(conditionClause);

            return new ResponseModel<IEnumerable<ListInventoryModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = data
            };
        }

        public async Task<ResponseModel> CreateInventory(CreateInventoryDto createInventoryDto)
        {
            //Check tồn tại đợt kiểm kê trạng thái chưa hoàn thành thì báo lỗi:
            var checkExistInventoryStatus = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.InventoryStatus != InventoryStatus.Finish);

            if (checkExistInventoryStatus != null)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.CheckInventoryStatusNotFinish,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.CheckInventoryStatusNotFinish),
                };
            }

            const string dateFormat = Constants.DayMonthYearFormat;
            DateTime newInventoryDate = DateTime.Now;
            if (!string.IsNullOrEmpty(createInventoryDto.InventoryDate))
            {
                if (DateTime.TryParseExact(createInventoryDto.InventoryDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateFrom))
                {
                    newInventoryDate = parsedDateFrom;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                }
            }

            var newInventory = new BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Inventory
            {
                Id = Guid.NewGuid(),
                InventoryDate = newInventoryDate,
                AuditFailPercentage = createInventoryDto.AuditFailPercentage,
                InventoryStatus = InventoryStatus.NotYet,
                CreatedAt = DateTime.Now,
                CreatedBy = createInventoryDto.UserId,
                Name = newInventoryDate.ToString("yyyyMMdd")
            };

            _inventoryContext.Inventories.Add(newInventory);
            await _inventoryContext.SaveChangesAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = Constants.ResponseMessages.Inventory.CreateSuccess
            };
        }

        public async Task<ResponseModel<IEnumerable<ListInventoryModel>>> ListInventoryToExport(ListInventoryDto listInventory)
        {
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            var result = (from i in _inventoryContext.Inventories.AsEnumerable()
                          join u in users on i.CreatedBy?.ToLower() ?? string.Empty equals u.Id.ToString().ToLower() into T1Group
                          from ui in T1Group.DefaultIfEmpty()
                          orderby i.CreatedAt descending
                          select new ListInventoryModel
                          {
                              InventoryName = i.Name,
                              InventoryDate = i.InventoryDate,
                              AuditFailPercentage = i.AuditFailPercentage,
                              Status = (int)i.InventoryStatus,
                              CreateAt = i.CreatedAt,
                              FullName = ui?.FullName ?? string.Empty
                          });

            if (result.Count() == 0)
            {
                return new ResponseModel<IEnumerable<ListInventoryModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.Inventory.NotFoundInventoryList,
                    Data = result
                };
            }

            Func<ListInventoryModel, bool> conditionClause = (x) =>
            {
                bool isValidCreateBy = true;
                bool isValidDateRange = true;
                bool isValidStatus = true;

                //Tim kiem theo ten nguoi tao:
                if (!listInventory.CreatedBy.IsNullOrEmpty())
                {
                    isValidCreateBy = string.IsNullOrEmpty(x.FullName) ? false : x.FullName.ToLower().Contains(listInventory.CreatedBy.ToLower());
                }

                //Lọc điều kiện theo 3 case: Có cả bắt đầu và kết thúc, chỉ có bắt đầu, chỉ có kết thúc
                if (listInventory.InventoryDateStart != null && listInventory.InventoryDateEnd != null)
                {
                    isValidDateRange = x.InventoryDate.Value.Date >= listInventory.InventoryDateStart.Value.Date && x.InventoryDate.Value.Date <= listInventory.InventoryDateEnd.Value.Date;
                }
                else if (listInventory.InventoryDateStart != null)
                {
                    isValidDateRange = x.InventoryDate.Value.Date >= listInventory.InventoryDateStart.Value.Date;
                }
                else if (listInventory.InventoryDateEnd != null)
                {
                    isValidDateRange = x.InventoryDate.Value.Date <= listInventory.InventoryDateEnd.Value.Date;
                }

                //Lọc điều kiện theo trạng thái
                if (listInventory.Statuses?.Any() == true)
                {
                    var convertedStatuses = listInventory.Statuses.Select(x => int.Parse(x));
                    isValidStatus = convertedStatuses.Contains(x.Status);
                }

                var aggregateCondition = isValidCreateBy && isValidDateRange && isValidStatus;
                return aggregateCondition;

            };

            var data = result?.Where(conditionClause);

            return new ResponseModel<IEnumerable<ListInventoryModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = data
            };
        }

        public async Task<ResponseModel> UpdateStatusInventory(string inventoryId, InventoryStatus status, string userId)
        {
            var getInvetoryById = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower());
            if (getInvetoryById == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.Inventory.NotFound,
                };
            }

            //Check trang thai dot kiem ke la Hoan Thanh ==> ko cho doi trang thai nua:
            if (getInvetoryById.InventoryStatus == InventoryStatus.Finish)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.CheckInventoryStatusIsFinish,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.CheckInventoryStatusIsFinish),
                };
            }

            //Check đã tới ngày kiểm kê hay không:
            if (DateTime.Now.Date < getInvetoryById.InventoryDate.Date && status == InventoryStatus.Finish)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.NotYetInventoryDate),
                };
            }

            getInvetoryById.InventoryStatus = status;
            getInvetoryById.UpdatedAt = DateTime.Now;
            getInvetoryById.UpdatedBy = userId;

            await _inventoryContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = Constants.ResponseMessages.Inventory.UpdateStateSuccess
            };
        }

        public async Task<ResponseModel> GetInventoryDetail(string inventoryId)
        {
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            var result = (from i in _inventoryContext.Inventories.AsEnumerable()
                          where i.Id.ToString().ToLower() == inventoryId.ToLower()
                          select new InventoryDetailModel
                          {
                              InventoryDate = i.InventoryDate.ToString(Constants.DayMonthYearFormat),
                              AuditFailPercentage = i.AuditFailPercentage,
                              InventoryName = i.Name,
                              Status = (int)i.InventoryStatus,
                              CreatedAt = i.CreatedAt != null ? i.CreatedAt.ToString(Constants.DefaultDateFormat) : string.Empty,
                              CreatedBy = string.IsNullOrEmpty(i.CreatedBy) ? string.Empty : i.CreatedBy,
                              UpdatedAt = i.UpdatedAt != null ? i.UpdatedAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                              UpdatedBy = string.IsNullOrEmpty(i.UpdatedBy) ? string.Empty : i.UpdatedBy,
                              ForceAggregateAt = i.ForceAggregateAt,
                              IsLocked = i.IsLocked == null ? false : i.IsLocked.Value
                          }).FirstOrDefault();

            if (result == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.Inventory.NotFoundDetail,
                };
            }

            result.CreatedBy = string.IsNullOrEmpty(result.CreatedBy) ? string.Empty : users.FirstOrDefault(x => x.Id.ToString().ToLower() == result.CreatedBy.ToLower())?.FullName;
            result.UpdatedBy = string.IsNullOrEmpty(result.UpdatedBy) ? string.Empty : users.FirstOrDefault(x => x.Id.ToString().ToLower() == result.UpdatedBy.ToLower())?.FullName;

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = result
            };
        }

        public async Task<ResponseModel> UpdateInventoryDetail(UpdateInventoryDetail updateInventoryDetail, string inventoryId)
        {
            var getInventoryDetail = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == inventoryId.ToLower());

            if (getInventoryDetail == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.Inventory.NotFound,
                };
            }

            //Check đã tới ngày kiểm kê hay không:
            if (DateTime.Now.Date < getInventoryDetail.InventoryDate.Date && updateInventoryDetail.Status == InventoryStatus.Finish)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.NotYetInventoryDate,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.NotYetInventoryDate),
                };

            }

            getInventoryDetail.InventoryDate = updateInventoryDetail.InventoryDate.Value;
            getInventoryDetail.AuditFailPercentage = updateInventoryDetail.AuditFailPercentage;
            getInventoryDetail.InventoryStatus = updateInventoryDetail.Status;
            getInventoryDetail.UpdatedAt = DateTime.Now;
            getInventoryDetail.UpdatedBy = updateInventoryDetail.UserId;
            getInventoryDetail.Name = updateInventoryDetail.InventoryDate.Value.ToString("yyyyMMdd");
            getInventoryDetail.IsLocked = updateInventoryDetail.IsLocked;

            await _inventoryContext.SaveChangesAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = Constants.ResponseMessages.Inventory.UpdateInfoSuccess,
            };
        }

        public async Task<ResponseModel> UpdateToReceivedDoc(List<Guid> docIds)
        {
            if (!docIds.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                };
            }

            var inventory = await _inventoryContext.Inventories.AsNoTracking()
                                                              .OrderByDescending(x => x.InventoryDate.Date)
                                                              .LastOrDefaultAsync(x => x.InventoryStatus != Infrastructure.Entity.Enums.InventoryStatus.Finish);

            var updateIdDict = docIds.ToHashSet();

            if (!updateIdDict.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage,
                };
            }

            var currUserId = _httpContext.CurrentUserId();
            var currUser = _httpContext.UserFromContext();
            var updateStatusEntities = _inventoryContext.InventoryDocs.Where(x => updateIdDict.Contains(x.Id));

            if (!updateStatusEntities.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.NotFound
                };
            }

            var docAHasQuantity = updateStatusEntities.Where(x => x.DocType == InventoryDocType.A && x.Quantity > 0);
            var remainingDocs = updateStatusEntities.Where(x => !(x.DocType == InventoryDocType.A && x.Quantity > 0));

            try
            {
                var executionStrategy = _inventoryContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var dbContextTransaction = _inventoryContext.Database.BeginTransaction();

                    //TH1: Đối với các phiếu A đã có giá trị trường Quantity thì sẽ chuyển sang trạng thái “ Đã xác nhận “
                    await docAHasQuantity.ExecuteUpdateAsync(t => t.SetProperty(b => b.Status, x => InventoryDocStatus.Confirmed)
                                                            .SetProperty(b => b.InventoryAt, x => DateTime.Now)
                                                            .SetProperty(b => b.ReceiveAt, x => DateTime.Now)
                                                            .SetProperty(b => b.ConfirmAt, x => DateTime.Now)
                                                            .SetProperty(b => b.ReceiveBy, x => Guid.Parse(currUserId))
                                                            .SetProperty(b => b.InventoryBy, currUser.UserCode)
                                                            .SetProperty(b => b.ConfirmBy, currUser.UserCode)
                                                        );

                    //TH2 :  Đối với các phiếu khác  sẽ đổi sang trạng thái Chưa kiểm kê 
                    await remainingDocs.ExecuteUpdateAsync(t => t.SetProperty(b => b.Status, x => InventoryDocStatus.NotInventoryYet)
                                                            .SetProperty(b => b.InventoryAt, x => DateTime.Now)
                                                            .SetProperty(b => b.InventoryBy, x => currUser.UserCode)
                                                            .SetProperty(b => b.UpdatedBy, x => currUser.UserCode)
                                                            .SetProperty(b => b.ReceiveAt, x => DateTime.Now)
                                                            .SetProperty(b => b.ReceiveBy, x => Guid.Parse(currUserId)
                                                    ));

                    //Nếu có phiếu chuyển trạng thái xác nhận thì chạy Background để tính toán lại
                    if ((docAHasQuantity?.Count() ?? 0) > 0)
                    {
                        var updateModel = new InventoryDocSubmitDto { DocType = InventoryDocType.A, InventoryId = inventory.Id };
                        await _dataAggregationService.UpdateDataFromInventoryDoc(updateModel);
                    }

                    await dbContextTransaction.CommitAsync();
                    await dbContextTransaction.DisposeAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(_httpContext, ex.Message);

                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống khi thực hiện cập nhật trạng thái phiếu."
                };
            }

            var successCount = docAHasQuantity.Count() + remainingDocs.Count();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = $"Đã tiếp nhận {successCount} phiếu thành công."
            };
        }

        public async Task<InventoryResponseModel<IEnumerable<ListInventoryDocumentModel>>> GetInventoryDocument(ListInventoryDocumentDto listInventoryDocument, string inventoryId)
        {
            //Call internall API Get users:
            var request = new RestRequest(Constants.Endpoint.Internal.getUsers);
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);
            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);

            var usersDict = responseModel?.Data?.ToDictionary(x => x.Id.ToString().ToLower(), x => x);
            //var inventoryAccUserName = await _inventoryContext.InventoryAccounts.AsNoTracking().ToDictionaryAsync(x => x.UserId, x => x.UserName);

            var userFilteredByParams = string.IsNullOrEmpty(listInventoryDocument.AssigneeAccount) ?
                                                            usersDict :
                                                            usersDict.Where(x => x.Value.UserName.ToLower().Contains(listInventoryDocument.AssigneeAccount.ToLower())).ToDictionary(x => x.Key, x => x.Value);

            //Call internal API get roles:
            var currUserId = _httpContext.CurrentUserId();
            var currUser = _httpContext.UserFromContext();

            var requestRoles = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
            requestRoles.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            requestRoles.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var roles = await _identityRestClient.GetAsync(requestRoles);

            var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

            //Nếu có quyền tạo phiếu theo phòng ban, chỉ hiển thị những phiếu có phòng ban (thuộc tài khoản phân phát) ứng những phòng ban đang được chọn trên quyền này(CREATE_DOCUMENT_BY_DEPARTMENT)
            var roleClaimTypes = rolesModel?.Data.Where(x => x.UserName == currUser.Username && x.ClaimType == Constants.Roles.CREATE_DOCUMENT_BY_DEPARTMENT && !string.IsNullOrEmpty(x.ClaimValue)).Select(x => x.Department);

            var result = (from id in _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.IsDeleted != true)
                              //join i in _inventoryContext.Inventories.AsNoTracking() on id.InventoryId equals i.Id into T2Group
                              //from T2 in T2Group.DefaultIfEmpty()
                              //join ic in _inventoryContext.InventoryAccounts.AsNoTracking() on id.AssignedAccountId equals ic.UserId into T3Group
                              //from T3 in T3Group.DefaultIfEmpty()

                          where id.InventoryId.Value == Guid.Parse(inventoryId)
                          let fiveNumberDocCode = string.IsNullOrEmpty(id.DocCode) ? -1 : Convert.ToInt32(id.DocCode.Substring(id.DocCode.Length - 5))
                          //let createByFromFixStock = string.IsNullOrEmpty(id.CreatedBy) ? string.Empty : usersDict.ContainsKey(id.CreatedBy) ?
                          //                                                      usersDict[id.CreatedBy].FullName : string.Empty
                          //let assigneeAccountFromFixStock = string.IsNullOrEmpty(id.AssignedAccountId.ToString()) ? string.Empty : usersDict.ContainsKey(id.AssignedAccountId.ToString().ToLower()) ?
                          //                                                    usersDict[id.AssignedAccountId.ToString().ToLower()].UserName : string.Empty


                          select new ListInventoryDocumentModel
                          {
                              Id = id.Id,
                              AssigneeAccount = id.AssignedAccountId.HasValue ? id.AssignedAccountId.Value.ToString() : string.Empty,
                              InventoryId = id.InventoryId.HasValue ? id.InventoryId.Value : default,
                              DocCode = !string.IsNullOrEmpty(id.DocCode) ? id.DocCode : string.Empty,
                              FiveNumberFromDocCode = fiveNumberDocCode,
                              DocType = (int)id.DocType,
                              Plant = !string.IsNullOrEmpty(id.Plant) ? id.Plant : string.Empty,
                              WHLoc = !string.IsNullOrEmpty(id.WareHouseLocation) ? id.WareHouseLocation : string.Empty,
                              ComponentCode = !string.IsNullOrEmpty(id.ComponentCode) ? id.ComponentCode : string.Empty,
                              ModelCode = !string.IsNullOrEmpty(id.ModelCode) ? id.ModelCode : string.Empty,
                              StageName = !string.IsNullOrEmpty(id.StageName) ? id.StageName : string.Empty,
                              ComponentName = id.DocType == InventoryDocType.C ? id.StageName : id.ComponentName,
                              Quantity = id.Quantity,
                              Position = !string.IsNullOrEmpty(id.PositionCode) ? id.PositionCode : string.Empty,
                              SaleOrderNo = !string.IsNullOrEmpty(id.SalesOrderNo) ? id.SalesOrderNo : string.Empty,
                              Department = !string.IsNullOrEmpty(id.DepartmentName) ? id.DepartmentName : string.Empty,
                              Location = !string.IsNullOrEmpty(id.LocationName) ? id.LocationName : string.Empty,
                              StockType = !string.IsNullOrEmpty(id.StockType) ? id.StockType : string.Empty,
                              SpecialStock = !string.IsNullOrEmpty(id.SpecialStock) ? id.SpecialStock : string.Empty,
                              SaleOrderList = !string.IsNullOrEmpty(id.SaleOrderList) ? id.SaleOrderList : string.Empty,
                              AssemblyLoc = !string.IsNullOrEmpty(id.AssemblyLocation) ? id.AssemblyLocation : string.Empty,
                              VendorCode = !string.IsNullOrEmpty(id.VendorCode) ? id.VendorCode : string.Empty,
                              PhysInv = !string.IsNullOrEmpty(id.PhysInv) ? id.PhysInv : string.Empty,
                              ProOrderNo = !string.IsNullOrEmpty(id.ProductOrderNo) ? id.ProductOrderNo : string.Empty,
                              FiscalYear = id.FiscalYear.HasValue ? id.FiscalYear.Value : default,
                              Item = !string.IsNullOrEmpty(id.Item) ? id.Item : string.Empty,
                              PlantedCount = !string.IsNullOrEmpty(id.PlannedCountDate) ? id.PlannedCountDate : string.Empty,
                              ColumnC = !string.IsNullOrEmpty(id.ColumnC) ? id.ColumnC : string.Empty,
                              ColumnN = !string.IsNullOrEmpty(id.ColumnN) ? id.ColumnN : string.Empty,
                              ColumnO = !string.IsNullOrEmpty(id.ColumnO) ? id.ColumnO : string.Empty,
                              ColumnP = !string.IsNullOrEmpty(id.ColumnP) ? id.ColumnP : string.Empty,
                              ColumnQ = !string.IsNullOrEmpty(id.ColumnQ) ? id.ColumnQ : string.Empty,
                              ColumnR = !string.IsNullOrEmpty(id.ColumnR) ? id.ColumnR : string.Empty,
                              ColumnS = !string.IsNullOrEmpty(id.ColumnS) ? id.ColumnS : string.Empty,
                              Note = !string.IsNullOrEmpty(id.Note) ? id.Note : string.Empty,
                              CreatedBy = string.IsNullOrEmpty(id.CreatedBy) ? string.Empty : id.CreatedBy,
                              CreatedAt = id.CreatedAt.ToString(Constants.DefaultDateFormat),
                              SAPInventoryNo = !string.IsNullOrEmpty(id.SapInventoryNo) ? id.SapInventoryNo : string.Empty,
                          });

            //Kiểm tra xem hệ thống có phiếu C hay không:
            bool checkIsExistDocTypeC = result.Any(x => x.DocType == (int)InventoryDocType.C);

            var condition = PredicateBuilder.New<ListInventoryDocumentModel>(true);
            if (!string.IsNullOrEmpty(listInventoryDocument.Plant))
            {
                condition = condition.And(x => x.Plant.ToLower().Contains(listInventoryDocument.Plant.ToLower()));
            }
            if (!string.IsNullOrEmpty(listInventoryDocument.WHLoc))
            {
                condition = condition.And(x => x.WHLoc.ToLower().Contains(listInventoryDocument.WHLoc.ToLower()));
            }
            //Tim kiem theo Ma Linh Kien:
            if (!string.IsNullOrEmpty(listInventoryDocument.ComponentCode))
            {
                condition = condition.And(x => x.ComponentCode.ToLower().Contains(listInventoryDocument.ComponentCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(listInventoryDocument.ModelCode))
            {
                condition = condition.And(x => x.ModelCode.ToLower().Contains(listInventoryDocument.ModelCode.ToLower()));
            }

            //Tim kiem theo Loai Phieu:
            if (listInventoryDocument.DocTypes != null && listInventoryDocument.DocTypes.Any())
            {
                condition = condition.And(x => listInventoryDocument.DocTypes.Select(x => int.Parse(x)).Contains(x.DocType));
            }

            if (currUser.RoleName != DefaultAccount.RoleName)
            {
                //Tim kiem theo danh sach phong ban duoc phan quyen tao phieu:
                if (roleClaimTypes.Any())
                {
                    condition = condition.And(x => roleClaimTypes.Contains(x.Department));
                }
            }

            //Tim kiem theo Phong Ban:
            if (listInventoryDocument?.Departments != null && listInventoryDocument.Departments.Any())
            {
                var checkHasDepartmentNull = listInventoryDocument.Departments.Any(x => string.IsNullOrEmpty(x));
                if (checkHasDepartmentNull)
                {
                    condition = condition.And(x => string.IsNullOrEmpty(x.Department) || listInventoryDocument.Departments.Contains(x.Department));
                }
                else
                {
                    condition = condition.And(x => listInventoryDocument.Departments.Contains(x.Department));
                }
            }

            //Tim kiem theo Khu vuc:
            if (listInventoryDocument?.Locations != null && listInventoryDocument.Locations.Any())
            {
                var checkHasLocationNull = listInventoryDocument.Locations.Any(x => string.IsNullOrEmpty(x));
                if (checkHasLocationNull)
                {
                    condition = condition.And(x => string.IsNullOrEmpty(x.Location) || listInventoryDocument.Locations.Contains(x.Location));
                }
                else
                {
                    condition = condition.And(x => listInventoryDocument.Locations.Contains(x.Location));
                }
            }

            //Tim kiem theo dieu kien so phieu

            if (!string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocument.DocNumberTo))
            {
                condition = condition.And(x => x.FiveNumberFromDocCode != -1 &&
                                    x.FiveNumberFromDocCode >= Convert.ToInt32(listInventoryDocument.DocNumberFrom) &&
                                    x.FiveNumberFromDocCode <= Convert.ToInt32(listInventoryDocument.DocNumberTo));
            }

            //Tim kiem theo Tai Khoan Phan Phat:
            if (!string.IsNullOrEmpty(listInventoryDocument.AssigneeAccount))
            {
                condition = condition.And(x => userFilteredByParams.Select(u => u.Key.ToLower()).Contains(x.AssigneeAccount.ToLower()));
            }

            var query = result.Where(condition);

            var totalRecords = query.Count();
            var itemsFromQuery = new List<ListInventoryDocumentModel>();
            if (listInventoryDocument.IsExport)
            {
                itemsFromQuery = query.OrderBy(x => x.DocCode).ToList();
            }
            else
            {
                itemsFromQuery = query.OrderBy(x => x.DocCode).ThenBy(x => x.ModelCode).Skip(listInventoryDocument.Skip).Take(listInventoryDocument.Take).ToList();
            }

            var items = itemsFromQuery
                            .AsEnumerable()
                            .Select(x =>
                            {
                                x.CreatedBy = string.IsNullOrEmpty(x.CreatedBy) ? string.Empty :
                                                usersDict.ContainsKey(x.CreatedBy.ToLower()) ? usersDict[x.CreatedBy.ToLower()].FullName : x.CreatedBy;

                                x.AssigneeAccount = string.IsNullOrEmpty(x.AssigneeAccount) ? string.Empty :
                                               usersDict.ContainsKey(x.AssigneeAccount.ToLower()) ? usersDict[x.AssigneeAccount.ToLower()].UserName : string.Empty;

                                return x;
                            })
                            .ToList();

            return new InventoryResponseModel<IEnumerable<ListInventoryDocumentModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = items,
                IsExistDocTypeC = checkIsExistDocTypeC,
                TotalRecords = totalRecords,
                Message = "Danh sách phiếu kiểm kê.",
            };
        }

        public async Task<ResponseModel<IEnumerable<Guid>>> GetInventoryDocumentDeleteHasFilter(ListInventoryDocumentDeleteDto listInventoryDocument, string inventoryId)
        {
            //Call internall API Get users:
            var request = new RestRequest(Constants.Endpoint.Internal.getUsers);
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);
            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);

            var usersDict = responseModel?.Data?.ToDictionary(x => x.Id.ToString().ToLower(), x => x);
            //var inventoryAccUserName = await _inventoryContext.InventoryAccounts.AsNoTracking().ToDictionaryAsync(x => x.UserId, x => x.UserName);

            var userFilteredByParams = string.IsNullOrEmpty(listInventoryDocument.AssigneeAccount) ?
                                                            usersDict :
                                                            usersDict.Where(x => x.Value.UserName.ToLower().Contains(listInventoryDocument.AssigneeAccount.ToLower())).ToDictionary(x => x.Key, x => x.Value);

            //Call internal API get roles:
            var currUserId = _httpContext.CurrentUserId();
            var currUser = _httpContext.UserFromContext();

            var requestRoles = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
            requestRoles.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            requestRoles.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var roles = await _identityRestClient.GetAsync(requestRoles);

            var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

            //Nếu có quyền tạo phiếu theo phòng ban, chỉ hiển thị những phiếu có phòng ban (thuộc tài khoản phân phát) ứng những phòng ban đang được chọn trên quyền này(CREATE_DOCUMENT_BY_DEPARTMENT)
            var roleClaimTypes = rolesModel?.Data.Where(x => x.UserName == currUser.Username && x.ClaimType == Constants.Roles.CREATE_DOCUMENT_BY_DEPARTMENT && !string.IsNullOrEmpty(x.ClaimValue)).Select(x => x.Department);

            var result = (from id in _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.IsDeleted != true)
                              //join i in _inventoryContext.Inventories.AsNoTracking() on id.InventoryId equals i.Id into T2Group
                              //from T2 in T2Group.DefaultIfEmpty()
                              //join ic in _inventoryContext.InventoryAccounts.AsNoTracking() on id.AssignedAccountId equals ic.UserId into T3Group
                              //from T3 in T3Group.DefaultIfEmpty()

                          where id.InventoryId.Value == Guid.Parse(inventoryId)
                          let fiveNumberDocCode = string.IsNullOrEmpty(id.DocCode) ? -1 : Convert.ToInt32(id.DocCode.Substring(id.DocCode.Length - 5))
                          //let createByFromFixStock = string.IsNullOrEmpty(id.CreatedBy) ? string.Empty : usersDict.ContainsKey(id.CreatedBy) ?
                          //                                                      usersDict[id.CreatedBy].FullName : string.Empty
                          //let assigneeAccountFromFixStock = string.IsNullOrEmpty(id.AssignedAccountId.ToString()) ? string.Empty : usersDict.ContainsKey(id.AssignedAccountId.ToString().ToLower()) ?
                          //                                                    usersDict[id.AssignedAccountId.ToString().ToLower()].UserName : string.Empty


                          select new ListInventoryDocumentModel
                          {
                              Id = id.Id,
                              AssigneeAccount = id.AssignedAccountId.HasValue ? id.AssignedAccountId.Value.ToString() : string.Empty,
                              InventoryId = id.InventoryId.HasValue ? id.InventoryId.Value : default,
                              DocCode = !string.IsNullOrEmpty(id.DocCode) ? id.DocCode : string.Empty,
                              FiveNumberFromDocCode = fiveNumberDocCode,
                              DocType = (int)id.DocType,
                              Plant = !string.IsNullOrEmpty(id.Plant) ? id.Plant : string.Empty,
                              WHLoc = !string.IsNullOrEmpty(id.WareHouseLocation) ? id.WareHouseLocation : string.Empty,
                              ComponentCode = !string.IsNullOrEmpty(id.ComponentCode) ? id.ComponentCode : string.Empty,
                              ModelCode = !string.IsNullOrEmpty(id.ModelCode) ? id.ModelCode : string.Empty,
                              StageName = !string.IsNullOrEmpty(id.StageName) ? id.StageName : string.Empty,
                              ComponentName = !string.IsNullOrEmpty(id.ComponentName) ? id.ComponentName : string.Empty,
                              Quantity = id.Quantity,
                              Position = !string.IsNullOrEmpty(id.PositionCode) ? id.PositionCode : string.Empty,
                              SaleOrderNo = !string.IsNullOrEmpty(id.SalesOrderNo) ? id.SalesOrderNo : string.Empty,
                              Department = !string.IsNullOrEmpty(id.DepartmentName) ? id.DepartmentName : string.Empty,
                              Location = !string.IsNullOrEmpty(id.LocationName) ? id.LocationName : string.Empty,
                              StockType = !string.IsNullOrEmpty(id.StockType) ? id.StockType : string.Empty,
                              SpecialStock = !string.IsNullOrEmpty(id.SpecialStock) ? id.SpecialStock : string.Empty,
                              SaleOrderList = !string.IsNullOrEmpty(id.SaleOrderList) ? id.SaleOrderList : string.Empty,
                              AssemblyLoc = !string.IsNullOrEmpty(id.AssemblyLocation) ? id.AssemblyLocation : string.Empty,
                              VendorCode = !string.IsNullOrEmpty(id.VendorCode) ? id.VendorCode : string.Empty,
                              PhysInv = !string.IsNullOrEmpty(id.PhysInv) ? id.PhysInv : string.Empty,
                              ProOrderNo = !string.IsNullOrEmpty(id.ProductOrderNo) ? id.ProductOrderNo : string.Empty,
                              FiscalYear = id.FiscalYear.HasValue ? id.FiscalYear.Value : default,
                              Item = !string.IsNullOrEmpty(id.Item) ? id.Item : string.Empty,
                              PlantedCount = !string.IsNullOrEmpty(id.PlannedCountDate) ? id.PlannedCountDate : string.Empty,
                              ColumnC = !string.IsNullOrEmpty(id.ColumnC) ? id.ColumnC : string.Empty,
                              ColumnN = !string.IsNullOrEmpty(id.ColumnN) ? id.ColumnN : string.Empty,
                              ColumnO = !string.IsNullOrEmpty(id.ColumnO) ? id.ColumnO : string.Empty,
                              ColumnP = !string.IsNullOrEmpty(id.ColumnP) ? id.ColumnP : string.Empty,
                              ColumnQ = !string.IsNullOrEmpty(id.ColumnQ) ? id.ColumnQ : string.Empty,
                              ColumnR = !string.IsNullOrEmpty(id.ColumnR) ? id.ColumnR : string.Empty,
                              ColumnS = !string.IsNullOrEmpty(id.ColumnS) ? id.ColumnS : string.Empty,
                              Note = !string.IsNullOrEmpty(id.Note) ? id.Note : string.Empty,
                              CreatedBy = string.IsNullOrEmpty(id.CreatedBy) ? string.Empty : id.CreatedBy,
                              CreatedAt = id.CreatedAt.ToString(Constants.DefaultDateFormat),
                              SAPInventoryNo = !string.IsNullOrEmpty(id.SapInventoryNo) ? id.SapInventoryNo : string.Empty,
                          });

            //Kiểm tra xem hệ thống có phiếu C hay không:
            bool checkIsExistDocTypeC = result.Any(x => x.DocType == (int)InventoryDocType.C);

            var condition = PredicateBuilder.New<ListInventoryDocumentModel>(true);
            if (!string.IsNullOrEmpty(listInventoryDocument.Plant))
            {
                condition = condition.And(x => x.Plant.ToLower().Contains(listInventoryDocument.Plant.ToLower()));
            }
            if (!string.IsNullOrEmpty(listInventoryDocument.WHLoc))
            {
                condition = condition.And(x => x.WHLoc.ToLower().Contains(listInventoryDocument.WHLoc.ToLower()));
            }
            //Tim kiem theo Ma Linh Kien:
            if (!string.IsNullOrEmpty(listInventoryDocument.ComponentCode))
            {
                condition = condition.And(x => x.ComponentCode.ToLower().Contains(listInventoryDocument.ComponentCode.ToLower()));
            }

            if (!string.IsNullOrEmpty(listInventoryDocument.ModelCode))
            {
                condition = condition.And(x => x.ModelCode.ToLower().Contains(listInventoryDocument.ModelCode.ToLower()));
            }

            //Tim kiem theo Loai Phieu:
            if (listInventoryDocument.DocTypes != null && listInventoryDocument.DocTypes.Any())
            {
                condition = condition.And(x => listInventoryDocument.DocTypes.Select(x => int.Parse(x)).Contains(x.DocType));
            }

            //Tim kiem theo Phong Ban:
            if (listInventoryDocument?.Departments != null && listInventoryDocument.Departments.Any())
            {
                var checkHasDepartmentNull = listInventoryDocument.Departments.Any(x => string.IsNullOrEmpty(x));
                if (checkHasDepartmentNull)
                {
                    condition = condition.And(x => string.IsNullOrEmpty(x.Department) || listInventoryDocument.Departments.Contains(x.Department));
                }
                else
                {
                    condition = condition.And(x => listInventoryDocument.Departments.Contains(x.Department));
                }
            }

            //Tim kiem theo danh sach phong ban duoc phan quyen tao phieu:
            if (roleClaimTypes.Any())
            {
                condition = condition.And(x => roleClaimTypes.Contains(x.Department));
            }

            //Tim kiem theo Khu vuc:
            if (listInventoryDocument?.Locations != null && listInventoryDocument.Locations.Any())
            {
                var checkHasLocationNull = listInventoryDocument.Locations.Any(x => string.IsNullOrEmpty(x));
                if (checkHasLocationNull)
                {
                    condition = condition.And(x => string.IsNullOrEmpty(x.Location) || listInventoryDocument.Locations.Contains(x.Location));
                }
                else
                {
                    condition = condition.And(x => listInventoryDocument.Locations.Contains(x.Location));
                }
            }

            //Tim kiem theo dieu kien so phieu

            if (!string.IsNullOrEmpty(listInventoryDocument.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocument.DocNumberTo))
            {
                condition = condition.And(x => x.FiveNumberFromDocCode != -1 &&
                                    x.FiveNumberFromDocCode >= Convert.ToInt32(listInventoryDocument.DocNumberFrom) &&
                                    x.FiveNumberFromDocCode <= Convert.ToInt32(listInventoryDocument.DocNumberTo));
            }

            //Tim kiem theo Tai Khoan Phan Phat:
            if (!string.IsNullOrEmpty(listInventoryDocument.AssigneeAccount))
            {
                condition = condition.And(x => userFilteredByParams.Select(u => u.Key.ToLower()).Contains(x.AssigneeAccount.ToLower()));
            }

            var query = result.Where(condition);

            var totalRecords = query.Count();
            var itemsFromQuery = new List<ListInventoryDocumentModel>();

            itemsFromQuery = query.ToList();

            return new ResponseModel<IEnumerable<Guid>>
            {
                Code = StatusCodes.Status200OK,
                Data = itemsFromQuery.Select(x => x.Id),
                Message = "Danh sách phiếu kiểm kê.",
            };
        }

        private string GetEnumDisplayName(Enum value)
        {
            var displayAttribute = (DisplayAttribute)value
                .GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault();

            return displayAttribute?.Name ?? value.ToString();
        }

        public async Task<ResponseModel> DownloadUploadDocStatusFileTemplate()
        {
            var currUserId = _httpContext.CurrentUserId();
            var currentDate = DateTime.Now.Date;

            var currAccount = _httpContext.UserFromContext();
            //Là tài khoản riêng có quyền chỉnh sửa kiểm kê

            var canGetAllLocations = currAccount.AccountType == nameof(AccountType.TaiKhoanRieng) ||
                               _httpContext.IsPromoter();

            var inventory = currAccount.InventoryLoggedInfo?.InventoryModel;
            if (inventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không thực hiện được do không có đợt kiểm kê nào. Vui lòng thử lại sau."
                };
            }

            var docs = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventory.InventoryId);
            if (docs?.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Tải biểu mẫu không thành công do chưa có phiếu nào được tạo trong đợt kiểm kê hiện tại. Vui lòng thử lại sau."
                };
            }

            List<string> Locations = new();
            if (canGetAllLocations)
            {
                Locations = _inventoryContext.InventoryLocations.AsNoTracking().Where(x => !x.IsDeleted)?.Select(x => x.Name)?.Distinct()?.ToList();
            }
            else
            {
                //Tên các khu vực mà người dùng được phân phát
                var accountLocations = _inventoryContext.AccountLocations
                                                              .AsNoTracking()
                                                              .Include(x => x.InventoryAccount)
                                                              .Include(x => x.InventoryLocation)
                                                              .Where(x => !x.InventoryLocation.IsDeleted && x.InventoryAccount.UserId == Guid.Parse(currAccount.UserId))
                                                              .AsSplitQuery();

                Locations = accountLocations?.Select(x => x.InventoryLocation.Name)?.Distinct()?.ToList();
            }

            if (!Locations.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Tải biểu mẫu không thành công do tài khoản của bạn chưa được phân phát khu vực nào. Vui lòng thử lại sau.",
                };
            }
            var docsAssigned = new List<UploadDocStatusFileDto>();

            var docsAssignedQuery = docs.Where(x => x.InventoryId.Value == inventory.InventoryId &&
                                                x.Status == InventoryDocStatus.NotInventoryYet &&
                                                (string.IsNullOrEmpty(x.LocationName) || Locations.Contains(x.LocationName) == true))
                                    .AsEnumerable()
                                    .Select(x => new UploadDocStatusFileDto
                                    {
                                        LocationName = x.LocationName,
                                        DocCode = x.DocCode,
                                        ComponentCode = x.ComponentCode,
                                        ModelCode = x.ModelCode,
                                        Plant = x.Plant,
                                        WareHouseLocation = x.WareHouseLocation,
                                        Status = (int)x.Status,
                                        CreatedBy = x.CreatedBy
                                    }).ToList();
            if (currAccount.RoleName.Contains(Constants.InventoryRoleName.AdministratorRoleName))
            {
                docsAssigned = docsAssignedQuery;
            }
            else
            {
                docsAssigned = docsAssignedQuery.Where(x => x.CreatedBy == currUserId.ToString()).ToList();
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phiếu chưa kiểm kê");

                // Đặt tiêu đề cho cột
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Khu vực";
                worksheet.Cells[1, 3].Value = "Mã phiếu";
                worksheet.Cells[1, 4].Value = "Mã linh kiện";
                worksheet.Cells[1, 5].Value = "Model code";
                worksheet.Cells[1, 6].Value = "Plant";
                worksheet.Cells[1, 7].Value = "WH.Loc";
                worksheet.Cells[1, 8].Value = "Trạng thái";


                // Đặt kiểu và màu cho tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.None;
                }

                //Trường hợp không có phiếu ở trạng thái Chưa kiểm kê thì tải về file trống

                if (docsAssigned?.Count() > 0)
                {
                    int batchSize = 5000;
                    var totalCount = docsAssigned.Count();

                    var batches = Enumerable.Range(0, (totalCount + batchSize - 1) / batchSize)
                                  .Select(i => docsAssigned.Skip(i * batchSize).Take(batchSize));

                    Parallel.ForEach(batches, (batch, state, index) =>
                    {
                        var startRow = (int)index * batchSize + 2; // Bắt đầu từ hàng thứ 2
                        var range = worksheet.Cells[startRow, 1, startRow + batch.Count() - 1, 8];
                        int stt = (int)index * batchSize + 1; // Số thứ tự bắt đầu từ 1

                        int startExcelRow = (int)index * batchSize + 2;
                        foreach (var item in batch)
                        {
                            range[startExcelRow, 1].Value = stt;
                            range[startExcelRow, 2].Value = item?.LocationName ?? string.Empty;
                            range[startExcelRow, 3].Value = item?.DocCode ?? string.Empty;
                            range[startExcelRow, 4].Value = item?.ComponentCode ?? string.Empty;
                            range[startExcelRow, 5].Value = item?.ModelCode ?? string.Empty;
                            range[startExcelRow, 6].Value = item?.Plant ?? string.Empty;
                            range[startExcelRow, 7].Value = item?.WareHouseLocation ?? string.Empty;

                            if (item?.Status != null)
                            {
                                range[startExcelRow, 8].Value = EnumHelper<InventoryDocStatus>.GetDisplayValue((InventoryDocStatus)item.Status);
                            }
                            else
                            {
                                range[startExcelRow, 8].Value = string.Empty;
                            }

                            stt++;
                            startExcelRow++;
                        }
                    });
                }

                // Lưu file Excel
                var stream = new MemoryStream();
                package.SaveAs(stream);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = stream.ToArray(),
                    Message = "Biểu mẫu upload trạng thái."
                };
            }
        }

        public async Task<ImportResponseModel<byte[]>> UploadChangeDocStatus(IFormFile file)
        {
            var currUserId = _httpContext.CurrentUserId();
            //Lấy ra đợt kiểm kê hiện tại
            var currentDate = DateTime.Now.Date;
            var inventory = await _inventoryContext.Inventories.AsNoTracking()
                                                               .OrderBy(x => x.InventoryStatus)
                                                               .FirstOrDefaultAsync(x => x.InventoryStatus != Infrastructure.Entity.Enums.InventoryStatus.Finish);

            var currAccount = _httpContext.UserFromContext();
            //Là tài khoản riêng có quyền chỉnh sửa kiểm kê
            var isValidPrivateAcc = currAccount.AccountType == nameof(AccountType.TaiKhoanRieng) && _httpContext.User.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY);
            //Là tài khoản chung có quyền chỉnh sửa kiểm kê
            var isCommonAcc = currAccount.AccountType == nameof(AccountType.TaiKhoanChung) && _httpContext.User.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY);
            var isPromotion = currAccount?.InventoryLoggedInfo?.InventoryRoleType == (int)InventoryAccountRoleType.Promotion;

            //Ngày kiểm kê hợp lệ hợp lệ:
            var inventoryCanPerformAction = _httpContext.IsInCurrentInventory();

            if (!inventoryCanPerformAction)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Không được thay đổi trạng thái phiếu khi quá ngày kiểm kê."
                };
            }

            //Check quyền có hợp lệ:
            var canPerformAction = currAccount.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY) || _httpContext.IsPromoter();

            if (!canPerformAction)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Không có quyền thực hiện."
                };
            }

            if (inventory == null)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không thực hiện được do không có đợt kiểm kê nào. Vui lòng thử lại sau."
                };
            }

            List<string> Locations = new();
            var canViewAllLocation = currAccount.AccountType == nameof(AccountType.TaiKhoanRieng) || _httpContext.IsPromoter();
            if (canViewAllLocation)
            {
                Locations = _inventoryContext.InventoryLocations.AsNoTracking().Where(x => !x.IsDeleted)?.Select(x => x.Name)?.Distinct()?.ToList();
            }
            else
            {
                //Tên các khu vực mà người dùng được phân phát
                var accountLocations = _inventoryContext.AccountLocations.AsNoTracking()
                                                              .Include(x => x.InventoryAccount)
                                                              .Include(x => x.InventoryLocation)
                                                              .Where(x => !x.InventoryLocation.IsDeleted && x.InventoryAccount.UserId == Guid.Parse(currAccount.UserId))
                                                              .AsSplitQuery();

                Locations = accountLocations?.Select(x => x.InventoryLocation.Name)?.Distinct()?.ToList();
            }

            if (!Locations.Any())
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Tài khoản của bạn chưa được phân phát khu vực nào. Vui lòng thử lại sau.",
                };
            }

            var docsAssigned = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                                    .Where(x => x.InventoryId.Value == inventory.Id &&
                                                                    (string.IsNullOrEmpty(x.LocationName) || Locations.Contains(x.LocationName) == true))
                                                                    .ToListAsync();
            if (!docsAssigned.Any())
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không thực hiện được do tài khoản của bạn chưa được phân phát phiếu trong đợt kiểm kê. Vui lòng thử lại sau.",
                };
            }

            List<UploadDocStatusValueModel> items = new List<UploadDocStatusValueModel>();
            //Đọc file, tổng hợp dữ liệu và đánh dấu validate
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

                        //get header column
                        var headerColumns = new UploadDocStatusHeaderIndexModel
                        {
                            STT = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.STT),
                            LocationName = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.LocationName),
                            ComponentCode = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.ComponentCode),
                            DocCode = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.DocCode),
                            DocStatus = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.DocStatus),
                            ModelCode = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.ModelCode),
                            Plant = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.Plant),
                            WHLoc = sourceSheet.GetColumnIndex(Constants.UploadDocStateExcelHeaderName.WHLoc),
                        };

                        int[] headers = new int[] {headerColumns.STT, headerColumns.LocationName, headerColumns.ComponentCode,
                                                   headerColumns.DocCode, headerColumns.DocStatus, headerColumns.ModelCode,
                                                   headerColumns.Plant, headerColumns.WHLoc};

                        if (headers.Any(x => x == -1))
                        {
                            return new ImportResponseModel<byte[]>
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "File upload không đúng định dạng. Vui lòng thử lại.",
                            };
                        }

                        foreach (var row in rows)
                        {
                            //Bỏ qua các dòng null
                            var isEmptyRow = sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.Columns].All(x => string.IsNullOrEmpty(x.Value?.ToString()));
                            if (isEmptyRow) continue;

                            var item = new UploadDocStatusValueModel();
                            item.STT = sourceSheet.Cells[row, headerColumns.STT].Value?.ToString()?.Trim() ?? string.Empty;
                            item.LocationName = sourceSheet.Cells[row, headerColumns.LocationName].Value?.ToString()?.Trim() ?? string.Empty;
                            item.ComponentCode = sourceSheet.Cells[row, headerColumns.ComponentCode].Value?.ToString()?.Trim() ?? string.Empty;
                            item.DocCode = sourceSheet.Cells[row, headerColumns.DocCode].Value?.ToString()?.Trim() ?? string.Empty;
                            item.DocStatus = sourceSheet.Cells[row, headerColumns.DocStatus].Value?.ToString()?.Trim() ?? string.Empty;
                            item.ModelCode = sourceSheet.Cells[row, headerColumns.ModelCode].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Plant = sourceSheet.Cells[row, headerColumns.Plant].Value?.ToString()?.Trim() ?? string.Empty;
                            item.WHLoc = sourceSheet.Cells[row, headerColumns.WHLoc].Value?.ToString()?.Trim() ?? string.Empty;

                            items.Add(item);
                        }

                        var duplicateDocCodeInFile = items.GroupBy(x => x.DocCode)
                                                          .Where(x => x.Count() > 1)
                                                          .Select(x => x.Key)
                                                          .ToHashSet();

                        //Validate STT
                        foreach (var item in items)
                        {
                            ValidateDocCode(item, duplicateDocCodeInFile, docsAssigned, currAccount);
                            ValidateStatus(item, docsAssigned);
                        }
                    }
                }
            }

            if (!items.Any())
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "File không có dữ liệu để cập nhật."
                };
            }

            var failRowItems = items.Where(x => !x.ErrModel.IsValid);
            MemoryStream stream = new MemoryStream();
            //Ghi lỗi vào file excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Lỗi upload trạng thái");

                int STTIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.STT) + 1;
                int LocationNameIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.LocationName) + 1;
                int ComponentCodeIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.ComponentCode) + 1;
                int DocCodeIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.DocCode) + 1;
                int DocStatusIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.DocStatus) + 1;
                int ModelCodeIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.ModelCode) + 1;
                int PlantIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.Plant) + 1;
                int WHLocIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.WHLoc) + 1;
                int ErrorContentIndex = UploadDocStatusExcel.ExportHeaders.IndexOf(UploadDocStatusExcel.ErrorContent) + 1;

                // Đặt tiêu đề cho cột
                worksheet.Cells[1, STTIndex].Value = UploadDocStatusExcel.STT;
                worksheet.Cells[1, LocationNameIndex].Value = UploadDocStatusExcel.LocationName;
                worksheet.Cells[1, ComponentCodeIndex].Value = UploadDocStatusExcel.ComponentCode;
                worksheet.Cells[1, DocCodeIndex].Value = UploadDocStatusExcel.DocCode;
                worksheet.Cells[1, DocStatusIndex].Value = UploadDocStatusExcel.DocStatus;
                worksheet.Cells[1, ModelCodeIndex].Value = UploadDocStatusExcel.ModelCode;
                worksheet.Cells[1, PlantIndex].Value = UploadDocStatusExcel.Plant;
                worksheet.Cells[1, WHLocIndex].Value = UploadDocStatusExcel.WHLoc;
                worksheet.Cells[1, ErrorContentIndex].Value = UploadDocStatusExcel.ErrorContent;

                // Đặt kiểu và màu cho tiêu đề
                using (var range = worksheet.Cells[1, STTIndex, 1, ErrorContentIndex])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.None;
                }

                // Điền dữ liệu vào Excel
                for (int i = 0; i < failRowItems.Count(); i++)
                {
                    var item = failRowItems.ElementAtOrDefault(i);
                    int stt = i + 1;
                    worksheet.Cells[i + 2, STTIndex].Value = stt;
                    worksheet.Cells[i + 2, LocationNameIndex].Value = item.LocationName;
                    worksheet.Cells[i + 2, ComponentCodeIndex].Value = item.ComponentCode;
                    worksheet.Cells[i + 2, DocCodeIndex].Value = item.DocCode;
                    worksheet.Cells[i + 2, DocStatusIndex].Value = item.DocStatus;
                    worksheet.Cells[i + 2, ModelCodeIndex].Value = item.ModelCode;
                    worksheet.Cells[i + 2, PlantIndex].Value = item.Plant;
                    worksheet.Cells[i + 2, WHLocIndex].Value = item.WHLoc;
                    //Tổng hợp message lỗi
                    var errMessage = item.ErrModel.Values.SelectMany(x => x.Errors)
                                                         .Select(x => x.ErrorMessage)
                                                         .Distinct();
                    worksheet.Cells[i + 2, ErrorContentIndex].Value = string.Join("\n", errMessage);

                    using (var errorRange = worksheet.Cells[i + 2, ErrorContentIndex, i + 2, ErrorContentIndex])
                    {
                        errorRange.Style.Font.Color.SetColor(Color.Red);
                        errorRange.Style.Fill.PatternType = ExcelFillStyle.None;
                    }
                }

                // Lưu file Excel
                package.SaveAs(stream);
            }

            //Cập nhật trạng thái
            var validRowItems = items.Where(x => x.ErrModel.IsValid);
            if (validRowItems.Any())
            {
                var strategy = _inventoryContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = _inventoryContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var validRowsDocCodes = validRowItems.Select(x => x.DocCode).ToList();

                            //Dùng mã phiếu như duy nhất để tìm ra các phiếu để cập nhật trạng thái
                            //Không cập nhật các phiếu trùng mã phiếu
                            var updateEntities = _inventoryContext.InventoryDocs
                                                                    .Where(x => validRowsDocCodes.Contains(x.DocCode));

                            //Update trạng thái phiếu A
                            await updateEntities.ExecuteUpdateAsync(t => t.SetProperty(b => b.Status, x => InventoryDocStatus.Confirmed)
                                                                                        .SetProperty(b => b.ConfirmAt, x => DateTime.Now)
                                                                                        .SetProperty(b => b.ConfirmBy, currAccount.UserCode)
                                                                                        .SetProperty(p => p.InventoryAt, DateTime.Now)
                                                                                        .SetProperty(p => p.InventoryBy, currAccount.UserCode)

                                                                                    );

                            //Nếu có phiếu chuyển trạng thái xác nhận thì chạy Background để tính toán lại
                            var updateModel = new InventoryDocSubmitDto { DocType = InventoryDocType.A, InventoryId = inventory.Id };
                            await _dataAggregationService.UpdateDataFromInventoryDoc(updateModel);

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Có lỗi khi thực hiện gọi background tính toán lại số lượng phiếu A.");
                            _logger.LogError(ex.Message);

                            transaction.Rollback();
                        }
                        await transaction.DisposeAsync();
                    }
                });
            }

            var successCount = validRowItems.Count();
            var failCount = items.Except(validRowItems).Count();

            return new ImportResponseModel<byte[]>
            {
                Code = StatusCodes.Status200OK,
                Data = stream.ToArray(),
                SuccessCount = successCount,
                FailCount = failCount,
                Message = "Upload trạng thái thành công."
            };
        }

        public bool ValidateDocCode(UploadDocStatusValueModel item, HashSet<string> duplicateDocCodeInFile, List<InventoryDoc> assingedDocs, ValidateTokenResultDto currUser)
        {
            var docCodePattern = new Regex(Constants.RegexPattern.DocCodeRegex);

            //Validate mã phiếu
            if (string.IsNullOrEmpty(item.DocCode))
            {
                item.ErrModel.TryAddModelError(nameof(item.DocCode), "Mã phiếu không được để trống.");
                return false;
            }
            if (!docCodePattern.IsMatch(item.DocCode))
            {
                item.ErrModel.TryAddModelError(nameof(item.DocCode), "Mã phiếu không đúng định dạng.");
                return false;
            }
            if (item.DocCode.Length < 10 || item.DocCode.Length > 10)
            {
                item.ErrModel.TryAddModelError(nameof(item.DocCode), "Mã phiếu không đúng định dạng.");
                return false;
            }
            if (duplicateDocCodeInFile?.Contains(item.DocCode) == true)
            {
                item.ErrModel.TryAddModelError(nameof(item.DocCode), "Mã phiếu đang bị trùng.");
                return false;
            }
            if (!assingedDocs.Any(x => x.DocCode == item.DocCode))
            {
                item.ErrModel.TryAddModelError(nameof(item.DocCode), "Mã phiếu này không được phân phát trong đợt kiểm kê hiện tại.");
                return false;
            }

            var docWithDocCode = assingedDocs.FirstOrDefault(x => x.DocCode == item.DocCode);
            if (docWithDocCode == null)
            {
                item.ErrModel.TryAddModelError(nameof(item.DocCode), "Không tìm thấy mã phiếu trong hệ thống.");
                return false;
            }
            else if (docWithDocCode.Status != InventoryDocStatus.NotInventoryYet)
            {
                item.ErrModel.TryAddModelError(nameof(item.DocCode), "Mã phiếu này không thuộc trạng thái Chưa kiểm kê.");
                return false;
            }

            if (!currUser.RoleName.ToLower().Contains(Constants.InventoryRoleName.AdministratorRoleName.ToLower()))
            {
                if (!string.IsNullOrEmpty(docWithDocCode.CreatedBy) && docWithDocCode.CreatedBy != currUser.UserId)
                {
                    item.ErrModel.TryAddModelError(nameof(item.DocCode), "Bạn không có quyền cập nhật trạng thái phiếu.");
                    return false;
                }
            }
            return true;
        }
        public bool ValidateStatus(UploadDocStatusValueModel item, List<InventoryDoc> assingedDocs)
        {
            string allowValue = "Đã xác nhận";

            if (string.IsNullOrEmpty(item.DocStatus))
            {
                item.ErrModel.TryAddModelError("Status", "Trạng thái không được để trống.");
                return false;
            }
            if (item.DocStatus.ToString().ToLower() != allowValue.ToLower())
            {
                item.ErrModel.TryAddModelError("Status", "Trạng thái chỉ nhận giá trị là Đã xác nhận.");
                return false;
            }

            return true;
        }

        public async Task<ResponseModel<DocumentDetailWebModel>> InventoryDocDetail(string docId, string searchTerm = "")
        {
            if (string.IsNullOrEmpty(docId) || !Guid.TryParse(docId, out _))
            {
                return new ResponseModel<DocumentDetailWebModel>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Id không hợp lệ"
                };
            }
            //Call internal API get users:
            var request = new RestRequest(Constants.Endpoint.Internal.getUsers);
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data.Select(x => new CompactUserViewModel { UserId = x.Id, UserName = x.UserName, Code = x.Code, DepartmentName = x.DepartmentName });


            var inventoryAcc = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                        .Select(x => new
                                                        {
                                                            x.Id,
                                                            x.UserId,
                                                            x.UserName
                                                        }).ToDictionaryAsync(x => x.UserId, x => x);

            var doc = await _inventoryContext.InventoryDocs.AsNoTracking()
                                                            .Include(x => x.Inventory).AsNoTracking()
                                                            .Include(x => x.DocHistories).AsNoTracking()
                                                            .Include(x => x.DocOutputs).AsNoTracking()
                                                            .Include(x => x.DocTypeCDetails).AsNoTracking()
                                                            .FirstOrDefaultAsync(x => x.Id == Guid.Parse(docId));

            if (doc == null)
            {
                return new ResponseModel<DocumentDetailWebModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            var detailViewModel = new DocumentDetailWebModel();
            detailViewModel.InventoryId = doc.Inventory.Id;
            detailViewModel.DocumentId = doc.Id;
            detailViewModel.InventoryName = doc.Inventory.Name;
            detailViewModel.InventoryDate = doc.Inventory.InventoryDate;

            detailViewModel.ComponentCode = doc.ComponentCode;
            detailViewModel.ComponentName = doc.DocType == InventoryDocType.C ? doc.StageName : doc.ComponentName;
            detailViewModel.DocCode = doc.DocCode;
            detailViewModel.DocType = (int)doc.DocType;
            detailViewModel.Status = (int)doc.Status;
            detailViewModel.PositionCode = doc.PositionCode;
            detailViewModel.AssemblyLoc = doc.AssemblyLocation;
            detailViewModel.LocationName = doc.LocationName;
            detailViewModel.DepartmentName = doc.DepartmentName;
            detailViewModel.Quantity = doc?.Quantity == 0 && (doc.Status == InventoryDocStatus.NotReceiveYet ||
                                                                doc.Status == InventoryDocStatus.NoInventory ||
                                                                doc.Status == InventoryDocStatus.NotInventoryYet) ? null : doc?.Quantity;
            detailViewModel.Plant = doc.Plant;
            detailViewModel.WHLoc = doc.WareHouseLocation;

            detailViewModel.Note = doc.Note;
            detailViewModel.VendorCode = doc.VendorCode;
            detailViewModel.ColumnQ = doc.ColumnQ;
            detailViewModel.ColumnN = doc.ColumnN;
            detailViewModel.ColumnR = doc.ColumnR;
            detailViewModel.ColumnS = doc.ColumnS;
            detailViewModel.ColumnO = doc.ColumnO;
            detailViewModel.ColumnC = doc.ColumnC;
            detailViewModel.ColumnP = doc.ColumnP;

            detailViewModel.SaleOrderList = doc.SaleOrderList;
            detailViewModel.SaleOrderNo = doc.SalesOrderNo;
            detailViewModel.PhysInv = doc.PhysInv;
            detailViewModel.FiscalYear = doc.FiscalYear;
            detailViewModel.Item = doc.Item;
            detailViewModel.PlannedCountDate = doc.PlannedCountDate;
            detailViewModel.SapInventoryNo = doc.SapInventoryNo;
            detailViewModel.StockType = doc.StockType;
            detailViewModel.SpecialStock = doc.SpecialStock;
            detailViewModel.ModelCode = doc.ModelCode;

            detailViewModel.Investigator = doc.Investigator;
            detailViewModel.InvestigateTime = doc.InvestigateTime;
            detailViewModel.ReasonInvestigator = doc.ReasonInvestigator;

            //Last updated image
            var lastDocHistory = doc.DocHistories.OrderByDescending(x => x.CreatedAt.Date).Where(x => !string.IsNullOrEmpty(x.EvicenceImg)).FirstOrDefault();
            if (!string.IsNullOrEmpty(lastDocHistory?.EvicenceImg))
            {
                detailViewModel.EnvicenceImage = lastDocHistory.EvicenceImg;
                detailViewModel.EnvicenceImageTitle = Path.GetFileName(lastDocHistory.EvicenceImg);
            }

            detailViewModel.AssigneeAccount = doc.AssignedAccountId.HasValue && inventoryAcc.ContainsKey(doc.AssignedAccountId.Value) ? inventoryAcc[doc.AssignedAccountId.Value].UserName : string.Empty;

            detailViewModel.CreatedAt = doc.CreatedAt;
            if (!string.IsNullOrEmpty(doc.CreatedBy) && Guid.TryParse(doc.CreatedBy, out Guid convertedCreateBy))
            {
                detailViewModel.CreatedBy = inventoryAcc.ContainsKey(convertedCreateBy) ? inventoryAcc[convertedCreateBy].UserName : string.Empty;
            }
            detailViewModel.InventoryAt = doc.InventoryAt;
            detailViewModel.InventoryBy = doc.InventoryBy;

            detailViewModel.ConfirmedAt = doc.ConfirmAt;
            detailViewModel.ConfirmedBy = doc.ConfirmBy;
            detailViewModel.AuditedAt = doc.AuditAt;
            detailViewModel.AuditedBy = !string.IsNullOrEmpty(doc.AuditBy)
                                             ? users.Where(u => u.Code == doc.AuditBy)
                                                     .Select(u => u.Code + "-" + u.DepartmentName)
                                                     .FirstOrDefault() ?? doc.AuditBy
                                             : string.Empty;

            detailViewModel.ReceivedAt = doc.ReceiveAt;
            if (doc.ReceiveBy.HasValue)
            {
                detailViewModel.ReceivedBy = inventoryAcc.ContainsKey(doc.ReceiveBy.Value) ? inventoryAcc[doc.ReceiveBy.Value].UserName : string.Empty;
            }

            //Histories
            detailViewModel.DocHistories = doc.DocHistories.OrderByDescending(x => x.CreatedAt.Date)
                                                           .Select(x => new DocHistoriesModel
                                                           {
                                                               Id = x.Id,
                                                               CreatedAt = x.CreatedAt,
                                                               CreatedBy = !(x.Status == InventoryDocStatus.AuditPassed || x.Status == InventoryDocStatus.AuditFailed)
                                                                           ? x.CreatedBy
                                                                           : (!string.IsNullOrEmpty(x.CreatedBy)
                                                                                    ? users.Where(u => u.Code == x.CreatedBy)
                                                                                           .Select(u => u.Code + "-" + u.DepartmentName)
                                                                                           .FirstOrDefault() ?? x.CreatedBy
                                                                                    : string.Empty),
                                                               Action = (int)x.Action,
                                                               Status = (int)x.Status,
                                                               InventoryDocId = x.InventoryDocId.Value,
                                                               ChangeLogModel = new ChangeLogModel
                                                               {
                                                                   NewQuantity = x.NewQuantity,
                                                                   OldQuantity = x.OldQuantity,
                                                                   NewStatus = (int)x.NewStatus,
                                                                   OldStatus = (int)x.OldStatus
                                                               }
                                                           });

            //Doc Ouputs fall all documents
            detailViewModel.DocComponentABEs = doc.DocOutputs.OrderBy(x => x.CreatedAt).Select(x => new DocComponentABE
            {
                Id = x.Id,
                QuantityOfBom = x.QuantityOfBom,
                QuantityPerBom = x.QuantityPerBom
            });

            //Components for C
            if (doc.DocType == InventoryDocType.C)
            {
                detailViewModel.DocComponentCs = doc.DocTypeCDetails.Select(x => new DocComponentC
                {
                    Id = x.Id,
                    ComponentCode = x.ComponentCode,
                    QuantityOfBom = x.QuantityOfBom,
                    QuantityPerBom = x.QuantityPerBom
                });

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    detailViewModel.DocComponentCs = detailViewModel.DocComponentCs.Where(x => x.ComponentCode.Contains(searchTerm));
                }
            }

            return new ResponseModel<DocumentDetailWebModel>
            {
                Code = StatusCodes.Status200OK,
                Data = detailViewModel,
            };
        }

        public async Task<ResponseModel<IEnumerable<ListInventoryModel>>> DropdownInventories()
        {
            var currUser = _httpContext.UserFromContext();
            var inventoryAccount = _httpContext.InventoryInfo();

            var inventoryNames = _inventoryContext.Inventories.AsNoTracking()
                                                               .OrderBy(x => x.InventoryStatus)
                                                               .AsEnumerable()
                                                               .Select(x => new ListInventoryModel
                                                               {
                                                                   InventoryId = x.Id,
                                                                   InventoryName = x.Name
                                                               });


            if (currUser.IsGrant(commonAPIConstant.Permissions.VIEW_ALL_INVENTORY) == false && currUser.IsGrant(commonAPIConstant.Permissions.VIEW_CURRENT_INVENTORY))
            {
                inventoryNames = inventoryNames.Where(x => x.Status != (int)InventoryStatus.Finish).Take(1);
            }

            if (!inventoryNames.Any())
            {
                return new ResponseModel<IEnumerable<ListInventoryModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp"
                };
            }

            return new ResponseModel<IEnumerable<ListInventoryModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = inventoryNames
            };
        }

        public async Task<ResponseModel<InventoryDocsResultSet<List<ListInventoryDocumentFullModel>>>> GetInventoryDocumentFull(ListInventoryDocumentFullDto listInventoryDocumentFull)
        {
            //Call internal API get users:
            var request = new RestRequest(Constants.Endpoint.Internal.getUsers);
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data.Select(x => new CompactUserViewModel { UserId = x.Id, UserName = x.UserName, Code = x.Code, DepartmentName = x.DepartmentName });
            var inventoryUsers = await _inventoryContext.InventoryAccounts.AsNoTracking().Select(x => new CompactUserViewModel { UserId = x.UserId, UserName = x.UserName }).ToListAsync();
            var mergedUsers = users.Concat(inventoryUsers).DistinctBy(x => x.UserId);

            var usersDict = mergedUsers.ToDictionary(x => x.UserId.ToString().ToLower(), x => x.UserName);

            var userFilteredByParams = string.IsNullOrEmpty(listInventoryDocumentFull.AssigneeAccount) ?
                                                            mergedUsers.ToList() :
                                                            mergedUsers.Where(x => string.Equals(x.UserName, listInventoryDocumentFull.AssigneeAccount, StringComparison.OrdinalIgnoreCase)).ToList();

            var quantityByStatuses = new InventoryDocStatus[]
            {
                InventoryDocStatus.NotReceiveYet,
                InventoryDocStatus.NoInventory,
                InventoryDocStatus.NotInventoryYet,
            };

            //Call internal API get roles:
            var currUserId = _httpContext.CurrentUserId();
            var currUser = _httpContext.UserFromContext();
            var requestRoles = new RestRequest(Constants.Endpoint.Internal.Get_All_Roles_Department);
            requestRoles.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            requestRoles.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var roles = await _identityRestClient.GetAsync(requestRoles);

            var rolesModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>>(roles.Content ?? string.Empty);

            //Nếu có quyền tạo phiếu theo phòng ban, chỉ hiển thị những phiếu có phòng ban (thuộc tài khoản phân phát) ứng những phòng ban đang được chọn trên quyền này(CREATE_DOCUMENT_BY_DEPARTMENT)
            var roleClaimTypes = rolesModel?.Data.Where(x => x.UserName == currUser.Username && x.ClaimType == Constants.Roles.CREATE_DOCUMENT_BY_DEPARTMENT && !string.IsNullOrEmpty(x.ClaimValue)).Select(x => x.Department.ToLower());


            //Role là người giám sát: Hiển thị thông tin tài khoản giám sát: Account + Phòng ban
            var result = (from id in _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.IsDeleted != true)
                          join inventory in _inventoryContext.Inventories.AsNoTracking() on id.InventoryId equals inventory.Id into t1Group
                          from t1 in t1Group.DefaultIfEmpty()

                          select new ListInventoryDocumentFullModel
                          {
                              Id = id.Id,
                              InventoryName = !string.IsNullOrEmpty(t1.Name) ? t1.Name : string.Empty,
                              DocType = (int)id.DocType,
                              Department = !string.IsNullOrEmpty(id.DepartmentName) ? id.DepartmentName : string.Empty,
                              Location = !string.IsNullOrEmpty(id.LocationName) ? id.LocationName : string.Empty,
                              DocCode = !string.IsNullOrEmpty(id.DocCode) ? id.DocCode : string.Empty,
                              Plant = !string.IsNullOrEmpty(id.Plant) ? id.Plant : string.Empty,
                              WHLoc = !string.IsNullOrEmpty(id.WareHouseLocation) ? id.WareHouseLocation : string.Empty,
                              ComponentCode = !string.IsNullOrEmpty(id.ComponentCode) ? id.ComponentCode : string.Empty,
                              ModelCode = !string.IsNullOrEmpty(id.ModelCode) ? id.ModelCode : string.Empty,
                              ComponentName = id.DocType == InventoryDocType.C ? id.StageName : id.ComponentName,
                              Quantity = id.Quantity == 0 && quantityByStatuses.Contains(id.Status) ? null : id.Quantity,
                              Position = !string.IsNullOrEmpty(id.PositionCode) ? id.PositionCode : string.Empty,
                              Status = (int)id.Status,
                              StockType = !string.IsNullOrEmpty(id.StockType) ? id.StockType : string.Empty,
                              SpecialStock = !string.IsNullOrEmpty(id.SpecialStock) ? id.SpecialStock : string.Empty,
                              SaleOrderNo = !string.IsNullOrEmpty(id.SalesOrderNo) ? id.SalesOrderNo : string.Empty,
                              SaleOrderList = !string.IsNullOrEmpty(id.SaleOrderList) ? id.SaleOrderList : string.Empty,
                              AssigneeAccount = id.AssignedAccountId.HasValue ? id.AssignedAccountId.Value.ToString() : string.Empty,
                              ReceiveBy = id.ReceiveBy.HasValue ? id.ReceiveBy.Value.ToString() : string.Empty,
                              ReceiveAt = id.ReceiveAt.HasValue ? id.ReceiveAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                              InventoryBy = !string.IsNullOrEmpty(id.InventoryBy) ? id.InventoryBy : string.Empty,
                              InventoryAt = id.InventoryAt.HasValue ? id.InventoryAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                              ConfirmBy = !string.IsNullOrEmpty(id.ConfirmBy) ? id.ConfirmBy : string.Empty,
                              ConfirmAt = id.ConfirmAt.HasValue ? id.ConfirmAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                              AuditAt = id.AuditAt.HasValue ? id.AuditAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                              AuditBy = !string.IsNullOrEmpty(id.AuditBy) ? id.AuditBy : string.Empty,
                              SapInventoryNo = !string.IsNullOrEmpty(id.SapInventoryNo) ? id.SapInventoryNo : string.Empty,
                              AssemblyLoc = !string.IsNullOrEmpty(id.AssemblyLocation) ? id.AssemblyLocation : string.Empty,
                              VendorCode = !string.IsNullOrEmpty(id.VendorCode) ? id.VendorCode : string.Empty,
                              PhysInv = !string.IsNullOrEmpty(id.PhysInv) ? id.PhysInv : string.Empty,
                              FiscalYear = id.FiscalYear,
                              Item = !string.IsNullOrEmpty(id.Item) ? id.Item : string.Empty,
                              PlannedCountDate = !string.IsNullOrEmpty(id.PlannedCountDate) ? id.PlannedCountDate : string.Empty,
                              ColumnC = !string.IsNullOrEmpty(id.ColumnC) ? id.ColumnC : string.Empty,
                              ColumnN = !string.IsNullOrEmpty(id.ColumnN) ? id.ColumnN : string.Empty,
                              ColumnO = !string.IsNullOrEmpty(id.ColumnO) ? id.ColumnO : string.Empty,
                              ColumnP = !string.IsNullOrEmpty(id.ColumnP) ? id.ColumnP : string.Empty,
                              ColumnQ = !string.IsNullOrEmpty(id.ColumnQ) ? id.ColumnQ : string.Empty,
                              ColumnR = !string.IsNullOrEmpty(id.ColumnR) ? id.ColumnR : string.Empty,
                              ColumnS = !string.IsNullOrEmpty(id.ColumnS) ? id.ColumnS : string.Empty,
                              CreatedBy = !string.IsNullOrEmpty(id.CreatedBy) ? id.CreatedBy.ToString() : string.Empty,
                              CreatedAt = id.CreatedAt.ToString(Constants.DefaultDateFormat),
                              OrderByCreatedAt = id.CreatedAt,
                              FiveNumberDocCode = string.IsNullOrEmpty(id.DocCode) ? null : Convert.ToInt32(id.DocCode.Substring(id.DocCode.Length - 5))
                          });

            List<string> Departments = new();
            List<string> Locations = new();

            var condition = PredicateBuilder.New<ListInventoryDocumentFullModel>(true);

            //Nếu tài khoản riêng hoặc xúc tiến thì được xem toàn bộ khu vực
            var isPromotion = currUser.InventoryLoggedInfo?.InventoryRoleType == (int)InventoryAccountRoleType.Promotion;

            if (isPromotion || currUser.AccountType == nameof(AccountType.TaiKhoanRieng))
            {
                Departments = await _inventoryContext.InventoryLocations.AsNoTracking().Where(x => !x.IsDeleted).Select(x => x.DepartmentName).Distinct().ToListAsync();
                Locations = await _inventoryContext.InventoryLocations.AsNoTracking().Where(x => !x.IsDeleted).Select(x => x.Name).Distinct().ToListAsync();
            }
            else
            {
                var accountLocations = _inventoryContext.AccountLocations.Include(x => x.InventoryAccount)
                                                            .Include(x => x.InventoryLocation)
                                                            .AsNoTracking()
                                                            .Where(x => !x.InventoryLocation.IsDeleted &&
                                                            x.InventoryAccount.UserId == Guid.Parse(currUser.UserId));


                Departments = await accountLocations?.Select(x => x.InventoryLocation.DepartmentName)?.Distinct()?.ToListAsync();
                Locations = await accountLocations?.Select(x => x.InventoryLocation.Name)?.Distinct()?.ToListAsync();
            }

            //Mặc định phân quyền theo phòng ban người dùng
            //và một số phiếu có trạng thái không kiểm kê, ko có khu vực vẫn hiển thị ra
            condition = condition.And(x => Locations.Contains(x.Location) || string.IsNullOrEmpty(x.Location));
            condition = condition.And(x => Departments.Contains(x.Department) || string.IsNullOrEmpty(x.Department));

            //Tim kiem theo Khu vuc:
            if (listInventoryDocumentFull.Locations.Any())
            {
                if (listInventoryDocumentFull.Locations.Contains(""))
                {
                    listInventoryDocumentFull.Locations.Add(null);
                }
                condition = condition.And(x => listInventoryDocumentFull.Locations.Contains(x.Location));
            }

            //Tim kiem theo Phong Ban:
            if (listInventoryDocumentFull.Departments.Any())
            {
                if (listInventoryDocumentFull.Departments.Contains(""))
                {
                    listInventoryDocumentFull.Departments.Add(null);
                }
                condition = condition.And(x => listInventoryDocumentFull.Departments.Contains(x.Department));
            }

            //Tim kiem theo Loai Phieu:
            if (listInventoryDocumentFull.DocTypes.Any())
            {
                condition = condition.And(x => listInventoryDocumentFull.DocTypes.Select(x => int.Parse(x)).Contains(x.DocType));
            }

            //Tim kiem theo Trang Thai:
            if (listInventoryDocumentFull.Statuses.Any())
            {
                var noInventoryNumber = (int)InventoryDocStatus.NoInventory;

                if (isPromotion || currUser.AccountType == nameof(AccountType.TaiKhoanRieng))
                {
                    //listInventoryDocumentFull.Statuses.Add(noInventoryNumber.ToString());
                }
                else
                {
                    listInventoryDocumentFull.Statuses.Remove(noInventoryNumber.ToString());
                }

                condition = condition.And(x => listInventoryDocumentFull.Statuses.Select(x => int.Parse(x)).Contains(x.Status));
            }

            //Tim kiem theo Dot Kiem Ke:
            if (listInventoryDocumentFull.InventoryNames.Any())
            {
                condition = condition.And(x => listInventoryDocumentFull.InventoryNames.Contains(x.InventoryName));
            }

            //Tim kiem theo Plant
            if (!string.IsNullOrEmpty(listInventoryDocumentFull.Plant))
            {
                condition = condition.And(x => x.Plant.ToLower().Contains(listInventoryDocumentFull.Plant.ToLower()));
            }

            //Tim kiem theo WHLoc:
            if (!string.IsNullOrEmpty(listInventoryDocumentFull.WHLoc))
            {
                condition = condition.And(x => x.WHLoc.ToLower().Contains(listInventoryDocumentFull.WHLoc.ToLower()));

            }

            //Tim kiem theo Model Code:
            if (!string.IsNullOrEmpty(listInventoryDocumentFull.ModelCode))
            {
                condition = condition.And((x => x.ModelCode.ToLower().Contains(listInventoryDocumentFull.ModelCode.ToLower())));
            }
            //Tim kiem theo ComponentCode:
            if (!string.IsNullOrEmpty(listInventoryDocumentFull.ComponentCode))
            {
                condition = condition.And(x => x.ComponentCode.ToLower().Contains(listInventoryDocumentFull.ComponentCode.ToLower()));
            }

            //Tim kiem theo Tai Khoan Phan Phat:
            if (!string.IsNullOrEmpty(listInventoryDocumentFull.AssigneeAccount))
            {
                condition = condition.And(x => userFilteredByParams.Any() && userFilteredByParams.Select(u => u.UserId.ToString().ToLower()).Contains(x.AssigneeAccount.ToLower()));
            }

            //Tim kiem theo dieu kien so phieu
            if (!string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberFrom) && !string.IsNullOrEmpty(listInventoryDocumentFull.DocNumberTo))
            {
                condition = condition.And(x => x.FiveNumberDocCode.HasValue ? x.FiveNumberDocCode >= int.Parse(listInventoryDocumentFull.DocNumberFrom) &&
                                                                              x.FiveNumberDocCode <= int.Parse(listInventoryDocumentFull.DocNumberTo)
                                                                            : false);
            }

            //Tim kiem theo danh sach phong ban duoc phan quyen tao phieu:
            //if (roleClaimTypes.Any())
            //{
            //    condition = condition.And(x => roleClaimTypes.Contains(x.Department.ToLower()));
            //}

            var resultSet = new InventoryDocsResultSet<List<ListInventoryDocumentFullModel>>();
            var query = result.Where(condition);
            resultSet.TotalRecords = await query.CountAsync();
            resultSet.DocsNotReceiveCount = await result.Where(condition).Where(x => x.Status == (int)InventoryDocStatus.NotReceiveYet).CountAsync();

            var itemsFromQuery = new List<ListInventoryDocumentFullModel>();
            if (listInventoryDocumentFull.IsGetAllForExport)
            {
                //if (currUser.Username.ToLower().Contains(Constants.InventoryUserName.AdministratorUserName))
                //{
                itemsFromQuery = await query.OrderByDescending(x => x.OrderByCreatedAt).ToListAsync();
                //}
                //else
                //{
                //    itemsFromQuery = query.AsEnumerable().Where(x => x.CreatedBy == currUserId.ToString() && x.Status == (int)InventoryDocStatus.NotInventoryYet).OrderByDescending(x => x.OrderByCreatedAt).ToList();
                //}
            }
            else
            {
                itemsFromQuery = await query
                                       ?.OrderByDescending(x => x.OrderByCreatedAt)
                                       ?.Skip(listInventoryDocumentFull.Skip)
                                       ?.Take(listInventoryDocumentFull.Take)
                                       ?.ToListAsync();
            }

            var items = itemsFromQuery
                            ?.Select(x =>
                            {
                                x.CreatedBy = string.IsNullOrEmpty(x.CreatedBy) ? string.Empty :
                                                usersDict?.ContainsKey(x.CreatedBy.ToLower()) == true ? usersDict[x.CreatedBy.ToLower()] : x.CreatedBy;

                                x.ReceiveBy = string.IsNullOrEmpty(x.ReceiveBy) ? string.Empty :
                                                usersDict?.ContainsKey(x.ReceiveBy.ToLower()) == true ? usersDict[x.ReceiveBy.ToLower()] : x.ReceiveBy;

                                x.AssigneeAccount = string.IsNullOrEmpty(x.AssigneeAccount) ? string.Empty :
                                               usersDict?.ContainsKey(x.AssigneeAccount.ToLower()) == true ? usersDict[x.AssigneeAccount.ToLower()] : string.Empty;
                                // Cập nhật trường AuditBy theo logic yêu cầu
                                x.AuditBy = !string.IsNullOrEmpty(x.AuditBy)
                                            ? users.Where(u => u.Code == x.AuditBy)
                                                    .Select(u => u.Code + "-" + u.DepartmentName)
                                                    .FirstOrDefault() ?? x.AuditBy
                                            : string.Empty;
                                return x;
                            })
                            ?.ToList();

            resultSet.Data = items;
            return new ResponseModel<InventoryDocsResultSet<List<ListInventoryDocumentFullModel>>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách phiếu kiểm kê ở nhiều đợt.",
                Data = resultSet
            };
        }

        public async Task<ResponseModel> DeleteInventorys(string inventoryId, List<string> docIds)
        {
            //Xóa DocOutput và DocTypeCDetail:
            var getDeleteDocOutPuts = await _inventoryContext.DocOutputs.Where(x => docIds.Contains(x.InventoryDocId.ToString().ToLower())
                                                                            && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();

            if (getDeleteDocOutPuts.Any())
            {
                _inventoryContext.DocOutputs.RemoveRange(getDeleteDocOutPuts);
            }

            var getDeleteDocTypeCDetails = await _inventoryContext.DocTypeCDetails.Where(x => docIds.Contains(x.InventoryDocId.ToString().ToLower())
                                                                            && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();

            if (getDeleteDocTypeCDetails.Any())
            {
                _inventoryContext.DocTypeCDetails.RemoveRange(getDeleteDocTypeCDetails);
            }

            //Xoa DocHistory, HistoryOutput, HistoryTypeCDetail:
            var getHistoryIds = await _inventoryContext.DocHistories.Where(x => docIds.Contains(x.InventoryDocId.ToString().ToLower())
                                                                             && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).Select(x => x.Id).ToListAsync();

            var getHistoryDocOutputs = await _inventoryContext.HistoryOutputs.Where(x => getHistoryIds.Contains(x.DocHistoryId.Value)
                                                                                    && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();
            if (getHistoryDocOutputs.Any())
            {
                _inventoryContext.HistoryOutputs.RemoveRange(getHistoryDocOutputs);
            }

            var getHistoryTypeCDetails = await _inventoryContext.HistoryTypeCDetails.Where(x => getHistoryIds.Contains(x.HistoryId.Value)
                                                                                    && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();

            if (getHistoryTypeCDetails.Any())
            {
                _inventoryContext.HistoryTypeCDetails.RemoveRange(getHistoryTypeCDetails);
            }

            var getDocHistories = await _inventoryContext.DocHistories.Where(x => docIds.Contains(x.InventoryDocId.ToString().ToLower())
                                                                             && x.InventoryId.ToString().ToLower() == inventoryId.ToLower()).ToListAsync();
            if (getDocHistories.Any())
            {
                _inventoryContext.DocHistories.RemoveRange(getDocHistories);
            }

            //Xóa InventoryDocs:
            var getDeleteInventories = await _inventoryContext.InventoryDocs.Where(x => docIds.Contains(x.Id.ToString().ToLower())).ToListAsync();

            if (getDeleteInventories.Any())
            {
                _inventoryContext.InventoryDocs.RemoveRange(getDeleteInventories);

            }

            await _inventoryContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Xóa phiếu kiểm kê thành công.",
            };
        }

        public async Task<ResponseModel<GetDetailInventoryDocumentModel>> GetDetailInventory(string inventoryId, string docId)
        {
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            var usersDict = users.ToDictionary(x => x.Id.ToString().ToLower(), x => x);

            var result = (from id in _inventoryContext.InventoryDocs
                              //join i in _inventoryContext.Inventories on id.InventoryId equals i.Id into T2Group
                              //from T2 in T2Group.DefaultIfEmpty()
                              //join ic in _inventoryContext.InventoryAccounts on id.AssignedAccountId equals ic.UserId into T3Group
                              //from T3 in T3Group.DefaultIfEmpty()
                          where id.InventoryId.ToString().ToLower() == inventoryId.ToLower() && id.Id.ToString().ToLower() == docId.ToLower()
                          select new GetDetailInventoryDocumentModel
                          {
                              DocType = (int)id.DocType,
                              DocCode = id.DocCode,
                              Plant = id.Plant,
                              WHLoc = id.WareHouseLocation,
                              ComponentCode = id.ComponentCode,
                              ModelCode = id.ModelCode,
                              ComponentName = id.ComponentName,
                              Quantity = id.Quantity,
                              Position = id.PositionCode,
                              SaleOrderNo = id.SalesOrderNo,
                              Department = id.DepartmentName,
                              Location = id.LocationName,
                              AssigneeAccount = id.AssignedAccountId.Value.ToString(),
                              StockType = id.StockType,
                              SpecialStock = id.SpecialStock,
                              SaleOrderList = id.SaleOrderList,
                              PhysInv = id.PhysInv,
                              FiscalYear = id.FiscalYear,
                              PlantedCount = id.PlannedCountDate,
                              ColumnC = id.ColumnC,
                              ColumnN = id.ColumnN,
                              ColumnO = id.ColumnO,
                              ColumnP = id.ColumnP,
                              ColumnQ = id.ColumnQ,
                              ColumnR = id.ColumnR,
                              ColumnS = id.ColumnS,
                              Note = id.Note,
                              CreatedBy = id.CreatedBy,
                              CreatedAt = id.CreatedAt.ToString(Constants.DefaultDateFormat),
                              StageName = id.StageName,
                              AssemblyLoc = id.AssemblyLocation,
                              VendorCode = id.VendorCode,
                              ProOrderNo = id.ProductOrderNo,
                              Item = id.Item,
                          }).FirstOrDefault();

            if (result == null)
            {
                return new ResponseModel<GetDetailInventoryDocumentModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy chi tiết phiếu kiểm kê.",
                };
            }
            //usersDict?.ContainsKey(x.AssigneeAccount.ToLower()) == true ? usersDict[x.AssigneeAccount.ToLower()] : string.Empty;
            result.AssigneeAccount = string.IsNullOrEmpty(result.AssigneeAccount) ? string.Empty
                                        : usersDict.ContainsKey(result.AssigneeAccount.ToLower()) ? usersDict[result.AssigneeAccount.ToLower()].UserName : string.Empty;

            result.CreatedBy = string.IsNullOrEmpty(result.CreatedBy) ? string.Empty
                                        : usersDict.ContainsKey(result.CreatedBy.ToLower()) ? usersDict[result.CreatedBy.ToLower()].FullName : result.CreatedBy;

            return new ResponseModel<GetDetailInventoryDocumentModel>
            {
                Code = StatusCodes.Status200OK,
                Message = "Chi tiết phiếu kiểm kê.",
                Data = result
            };

        }

        public async Task<ResponseModel<IEnumerable<GetDocTypeCDetail>>> GetDocumentTypeC(string inventoryId, string docId, string componentCode)
        {
            var getDocTypeCDetails = _inventoryContext.DocTypeCDetails.Where(x => x.InventoryId.ToString().ToLower() == inventoryId.ToLower()
                                                                            && x.InventoryDocId.ToString().ToLower() == docId.ToLower()).Select(x => new GetDocTypeCDetail
                                                                            {
                                                                                ComponentCode = string.IsNullOrEmpty(x.ComponentCode) ? x.ModelCode : x.ComponentCode,
                                                                                ModelCode = x.ModelCode,
                                                                                QuantityOfBom = x.QuantityOfBom,
                                                                                QuantityPerBom = x.QuantityPerBom,
                                                                                No = x.No
                                                                            });
            if (!string.IsNullOrEmpty(componentCode))
            {
                getDocTypeCDetails = getDocTypeCDetails.Where(x => !string.IsNullOrEmpty(x.ComponentCode) ? x.ComponentCode.Contains(componentCode) : x.ModelCode.Contains(componentCode));
            }

            return new ResponseModel<IEnumerable<GetDocTypeCDetail>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách chi tiết phiếu C.",
                Data = await getDocTypeCDetails.OrderBy(x => x.No).ToListAsync()
            };
        }

        public async Task<ResponseModel<ResultSet<List<DocumentResultViewModel>>>> DocumentResults(DocumentResultListFilterModel filterModel)
        {
            if (filterModel == null)
            {
                return new ResponseModel<ResultSet<List<DocumentResultViewModel>>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu tìm kiếm không hợp lệ."
                };
            }

            var accounts = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                                    .Select(x => new
                                                                    {
                                                                        x.UserId,
                                                                        x.UserName
                                                                    })
                                                                    .ToDictionaryAsync(x => x.UserId.ToString().ToLower(), x => x);

            var query = (from doc in _inventoryContext.InventoryDocs.AsNoTracking()
                         join docCChild in _inventoryContext.DocTypeCComponents.AsNoTracking()
                                        on doc.Id equals docCChild.InventoryDocId into t1Group
                         from t1 in t1Group.DefaultIfEmpty()
                         where doc.InventoryId == filterModel.InventoryId
                         let fiveNumberDocCode = string.IsNullOrEmpty(doc.DocCode) ? -1 : Convert.ToInt32(doc.DocCode.Substring(doc.DocCode.Length - 5))

                         select new DocumentResultViewModel
                         {
                             Id = doc.Id,
                             //t1 != null tức phiếu này là phiếu C, lấy component code từ DocTypeCComponents vì InventoryDocs sẽ không có
                             ComponentCode = t1 != null ? t1.ComponentCode : doc.ComponentCode,
                             ModelCode = doc.ModelCode,
                             Plant = t1 != null ? t1.Plant : doc.Plant,
                             WHLoc = t1 != null ? t1.WarehouseLocation : doc.WareHouseLocation,
                             DocCode = doc.DocCode,
                             FiveNumberDocCode = fiveNumberDocCode,
                             //Trường này để phục vụ order theo mã phiếu nhưng vẫn câu query ở dạng lazy loading
                             //ConvertedDocCode = convertedDocCodeOrderby,
                             DocType = (int)doc.DocType,
                             Quantity = t1 != null ? t1.TotalQuantity : doc.Quantity,
                             TotalQuantity = doc.TotalQuantity,
                             AccountQuantity = doc.AccountQuantity,
                             ErrorQuantity = doc.ErrorQuantity,
                             UnitPrice = doc.UnitPrice.HasValue ? doc.UnitPrice.Value : default,
                             StockType = doc.StockType,
                             SpecialStock = doc.SpecialStock,
                             SaleOrderNo = doc.SalesOrderNo,
                             PhysInv = doc.PhysInv,
                             ProductOrderNo = doc.ProductOrderNo,
                             InventoryBy = doc.InventoryBy,
                             ConfirmedBy = doc.ConfirmBy,
                             InventoryAt = doc.InventoryAt.HasValue ? doc.InventoryAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                             ConfirmedAt = doc.ConfirmAt.HasValue ? doc.ConfirmAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                             AuditBy = doc.AuditBy,
                             AuditAt = doc.AuditAt.HasValue ? doc.AuditAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                             Position = doc.PositionCode,
                             AssemblyLoc = doc.AssemblyLocation,
                             VendorCode = doc.VendorCode,
                             SaleOrderList = doc.SaleOrderList,
                             ComponentName = doc.ComponentName,
                             No = doc.No ?? string.Empty,
                             ErrorMoney = doc.ErrorMoney.HasValue ? doc.ErrorMoney.Value : default,
                             CSAP = doc.CSAP,
                             KSAP = doc.KSAP,
                             MSAP = doc.MSAP,
                             OSAP = doc.OSAP,
                             ErrorMoneyAbs = doc.ErrorMoney.HasValue ? Math.Abs(doc.ErrorMoney.Value) : default,
                             ErrorQuantityAbs = Math.Abs(doc.ErrorQuantity)
                         });

            var predicate = PredicateBuilder.New<DocumentResultViewModel>(true);
            if (!string.IsNullOrEmpty(filterModel.Plant))
            {
                predicate = predicate.And(x => string.IsNullOrEmpty(x.Plant) ? false : x.Plant.Contains(filterModel.Plant.Trim()));
            }
            if (!string.IsNullOrEmpty(filterModel.WHLoc))
            {
                predicate = predicate.And(x => string.IsNullOrEmpty(x.WHLoc) ? false : x.WHLoc.Contains(filterModel.WHLoc.Trim()));
            }
            if (filterModel.DocTypes != null || filterModel.DocTypes.Any())
            {
                predicate = predicate.And(x => filterModel.DocTypes.Select(x => int.Parse(x)).Contains(x.DocType));
            }
            if (!string.IsNullOrEmpty(filterModel.ComponentCode))
            {
                predicate = predicate.And(x => string.IsNullOrEmpty(x.ComponentCode) ? false : x.ComponentCode.Contains(filterModel.ComponentCode.Trim()));
            }
            if (!string.IsNullOrEmpty(filterModel.ModelCode))
            {
                predicate = predicate.And(x => string.IsNullOrEmpty(x.ModelCode) ? false : x.ModelCode.Contains(filterModel.ModelCode.Trim()));
            }

            if (!string.IsNullOrEmpty(filterModel.DocNumberFrom) && !string.IsNullOrEmpty(filterModel.DocNumberTo))
            {
                int convertedDocCodeFrom = Convert.ToInt32(filterModel.DocNumberFrom);
                int convertedDocCodeTo = Convert.ToInt32(filterModel.DocNumberTo);

                predicate = predicate.And(x => x.FiveNumberDocCode != -1 && (x.FiveNumberDocCode >= convertedDocCodeFrom && x.FiveNumberDocCode <= convertedDocCodeTo));
            }

            var filterItems = query.Where(predicate);

            ResultSet<List<DocumentResultViewModel>> resultSet = new ResultSet<List<DocumentResultViewModel>>();

            resultSet.TotalRecords = await filterItems.CountAsync();

            if (filterModel.IsAllForExport)
            {
                //Tổng hợp kết quả khi export chỉ lấy các phiếu A
                int docTypeA = (int)InventoryDocType.A;

                //Sorting By ComponentCode, ErrorQuantity, ErrorMoneyAbs:
                if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ComponentCode)
                {
                    if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                    {
                        resultSet.Data = await filterItems.Where(x => x.DocType == docTypeA)
                                                  .OrderBy(x => x.ComponentCode).ToListAsync();
                    }
                    else
                    {
                        resultSet.Data = await filterItems.Where(x => x.DocType == docTypeA)
                                                  .OrderByDescending(x => x.ComponentCode).ToListAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ErrorQuantity)
                {
                    if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                    {
                        resultSet.Data = await filterItems.Where(x => x.DocType == docTypeA)
                                                  .OrderBy(x => x.ErrorQuantity).ToListAsync();
                    }
                    else
                    {
                        resultSet.Data = await filterItems.Where(x => x.DocType == docTypeA)
                                                  .OrderByDescending(x => x.ErrorQuantity).ToListAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ErrorMoneyAbs)
                {
                    if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                    {
                        resultSet.Data = await filterItems.Where(x => x.DocType == docTypeA)
                                                  .OrderBy(x => x.ErrorMoneyAbs).ToListAsync();
                    }
                    else
                    {
                        resultSet.Data = await filterItems.Where(x => x.DocType == docTypeA)
                                                  .OrderByDescending(x => x.ErrorMoneyAbs).ToListAsync();
                    }
                }
                else
                {
                    resultSet.Data = await filterItems.Where(x => x.DocType == docTypeA)
                                                      .OrderBy(x => x.ComponentCode)
                                                      .ThenBy(x => x.DocType).ToListAsync();
                }

            }
            else
            {
                //Sorting By ComponentCode, ErrorQuantity, ErrorMoneyAbs:
                if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ComponentCode)
                {
                    if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                    {
                        resultSet.Data = await filterItems.OrderBy(x => x.ComponentCode).Skip(filterModel.Skip).Take(filterModel.Take).ToListAsync();
                    }
                    else
                    {
                        resultSet.Data = await filterItems.OrderByDescending(x => x.ComponentCode).Skip(filterModel.Skip).Take(filterModel.Take).ToListAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ErrorQuantity)
                {
                    if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                    {
                        resultSet.Data = await filterItems.OrderBy(x => x.ErrorQuantity).Skip(filterModel.Skip).Take(filterModel.Take).ToListAsync();
                    }
                    else
                    {
                        resultSet.Data = await filterItems.OrderByDescending(x => x.ErrorQuantity).Skip(filterModel.Skip).Take(filterModel.Take).ToListAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ErrorMoneyAbs)
                {
                    if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                    {
                        resultSet.Data = await filterItems.OrderBy(x => x.ErrorMoneyAbs).Skip(filterModel.Skip).Take(filterModel.Take).ToListAsync();
                    }
                    else
                    {
                        resultSet.Data = await filterItems.OrderByDescending(x => x.ErrorMoneyAbs).Skip(filterModel.Skip).Take(filterModel.Take).ToListAsync();
                    }
                }
                else
                {
                    resultSet.Data = await filterItems.OrderBy(x => x.ComponentCode).ThenBy(x => x.DocType).Skip(filterModel.Skip).Take(filterModel.Take).ToListAsync();
                }

            }

            return new ResponseModel<ResultSet<List<DocumentResultViewModel>>>
            {
                Code = StatusCodes.Status200OK,
                Data = resultSet
            };
        }

        public async Task<ResponseModel<IEnumerable<DocumentResultViewModel>>> DocumentResultsToExport(DocumentResultListFilterModel filterModel)
        {
            if (filterModel == null)
            {
                return new ResponseModel<IEnumerable<DocumentResultViewModel>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu tìm kiếm không hợp lệ."
                };
            }

            var accounts = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                                    .Select(x => new
                                                                    {
                                                                        x.UserId,
                                                                        x.UserName
                                                                    })
                                                                    .ToDictionaryAsync(x => x.UserId.ToString().ToLower(), x => x);

            var query = (from doc in _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == filterModel.InventoryId)
                         join docCChild in _inventoryContext.DocTypeCComponents.AsNoTracking()
                                        on doc.ModelCode equals docCChild.MainModelCode into t1Group
                         from t1 in t1Group.DefaultIfEmpty()
                         where doc.DocType == InventoryDocType.A
                         select new DocumentResultViewModel
                         {
                             Id = doc.Id,
                             //t1 != null tức phiếu này là phiếu C, lấy component code từ DocTypeCComponents vì InventoryDocs sẽ không có
                             ComponentCode = t1 != null ? t1.ComponentCode : doc.ComponentCode,
                             ModelCode = doc.ModelCode,
                             Plant = t1 != null ? t1.Plant : doc.Plant,
                             WHLoc = t1 != null ? t1.WarehouseLocation : doc.WareHouseLocation,
                             Quantity = t1 != null ? t1.TotalQuantity : doc.Quantity,
                             TotalQuantity = doc.TotalQuantity,
                             AccountQuantity = doc.AccountQuantity,
                             ErrorQuantity = doc.ErrorQuantity,
                             UnitPrice = doc.UnitPrice.HasValue ? doc.UnitPrice.Value : default,
                             DocCode = doc.DocCode,
                             StockType = doc.StockType,
                             SpecialStock = doc.SpecialStock,
                             SaleOrderNo = doc.SalesOrderNo,
                             PhysInv = doc.PhysInv,
                             ProductOrderNo = doc.ProductOrderNo,
                             InventoryBy = doc.InventoryBy,
                             InventoryAt = doc.InventoryAt.HasValue ? doc.InventoryAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                             ConfirmedBy = doc.ConfirmBy,
                             ConfirmedAt = doc.ConfirmAt.HasValue ? doc.ConfirmAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                             AuditBy = doc.AuditBy,
                             AuditAt = doc.AuditAt.HasValue ? doc.AuditAt.Value.ToString(Constants.DefaultDateFormat) : string.Empty,
                             Position = doc.PositionCode,
                             AssemblyLoc = doc.AssemblyLocation,
                             VendorCode = doc.VendorCode,
                             SaleOrderList = doc.SaleOrderList,
                             DocType = (int)doc.DocType,
                             ComponentName = doc.ComponentName,

                             ErrorMoney = doc.ErrorMoney.HasValue ? doc.ErrorMoney.Value : default,
                             CSAP = doc.CSAP,
                             KSAP = doc.KSAP,
                             MSAP = doc.MSAP,
                             OSAP = doc.OSAP,
                             PlannedCountDate = doc.PlannedCountDate,

                             FiscalYear = doc.FiscalYear,
                             Item = doc.Item
                         });


            Func<DocumentResultViewModel, bool> condition = (x) =>
            {
                bool validPlant = true;
                bool validWHLoc = true;
                bool validDocType = true;
                bool validDocCode = true;
                bool validComponentCode = true;
                bool validModelCode = true;

                //Plant
                if (!string.IsNullOrEmpty(filterModel.Plant))
                {
                    validPlant = x.Plant?.Contains(filterModel.Plant.Trim()) ?? false;
                }

                //WHLoc
                if (!string.IsNullOrEmpty(filterModel.WHLoc))
                {
                    validWHLoc = x.WHLoc?.Contains(filterModel.WHLoc.Trim()) ?? false;
                }

                //Doctypes
                if (filterModel.DocTypes != null && filterModel.DocTypes.Any())
                {
                    validDocType = filterModel.DocTypes.Contains(x.DocType.ToString());
                }
                //DocCode
                if (!string.IsNullOrEmpty(filterModel.DocNumberFrom) && !string.IsNullOrEmpty(filterModel.DocNumberTo))
                {
                    var convertedDocCodeFrom = int.Parse(filterModel.DocNumberFrom.Trim());
                    var convertedDocCodeTo = int.Parse(filterModel.DocNumberTo.Trim());

                    validDocCode = !string.IsNullOrEmpty(x.DocCode) && int.TryParse(x.DocCode.Substring(x.DocCode.Length - 5), out int docCodeCondition) ?
                                    docCodeCondition >= convertedDocCodeFrom && docCodeCondition <= convertedDocCodeTo : false;
                }
                //ComponentCode
                if (!string.IsNullOrEmpty(filterModel.ComponentCode))
                {
                    validComponentCode = x.ComponentCode?.Contains(filterModel.ComponentCode.Trim()) ?? false;
                }
                if (!string.IsNullOrEmpty(filterModel.ModelCode))
                {
                    validModelCode = x.ModelCode?.Contains(filterModel.ModelCode.Trim()) ?? false;
                }

                return validPlant && validWHLoc && validDocType && validDocCode && validComponentCode && validModelCode;
            };

            var filterResult = query.Where(condition);

            IEnumerable<DocumentResultViewModel> results = Enumerable.Empty<DocumentResultViewModel>();

            //Sorting By ComponentCode, ErrorQuantity, ErrorMoneyAbs:
            if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ComponentCode)
            {
                if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                {
                    results = filterResult.OrderBy(x => x.ComponentCode).AsEnumerable();
                }
                else
                {
                    results = filterResult.OrderByDescending(x => x.ComponentCode).AsEnumerable();
                }
            }
            else if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ErrorQuantity)
            {
                if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                {
                    results = filterResult.OrderBy(x => x.ErrorQuantity).AsEnumerable();
                }
                else
                {
                    results = filterResult.OrderByDescending(x => x.ErrorQuantity).AsEnumerable();
                }
            }
            else if (!string.IsNullOrEmpty(filterModel.OrderColumn) && filterModel.OrderColumn == Constants.DocumentResult.ErrorMoneyAbs)
            {
                if (filterModel.OrderColumnDirection == Constants.DocumentResult.OrderByAsc)
                {
                    results = filterResult.OrderBy(x => x.ErrorMoneyAbs).AsEnumerable();
                }
                else
                {
                    results = filterResult.OrderByDescending(x => x.ErrorMoneyAbs).AsEnumerable();
                }
            }
            else
            {
                results = filterResult.OrderBy(x => x.DocCode).AsEnumerable();
            }

            return new ResponseModel<IEnumerable<DocumentResultViewModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = results
            };

        }

        public async Task<ResponseModel<byte[]>> ExportDocumentResultExcel(DocumentResultListFilterModel filterModel)
        {
            var docResultList = await DocumentResults(filterModel);
            var totalItemCount = docResultList.Data?.Data?.Count ?? default;

            //Nếu không có dữ liệu
            if (totalItemCount == 0)
            {
                return new ResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp để xuất file."
                };
            }

            var items = docResultList.Data.Data;
            Byte[] bytes;
            //Ghi lỗi vào file excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách tổng hợp kết quả");

                var STTIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.STT);
                var ComponentCodeIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ComponentCode);
                var ModelCodeIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ModelCode);
                var PlantIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.Plant);
                var WHLocIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.WHLoc);
                var QuantityIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.Quantity);
                var TotalQuantityIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.TotalQuantity);
                var AccountIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.Account);
                var ErrorQuantityIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ErrorQuantity);
                var ErrorMoneyIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ErrorMoney);
                var UnitPriceIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.UnitPrice);
                var DocCodeIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.DocCode);
                var StockTypesIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.StockTypes);
                var SpecialStockIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.SpecialStock);
                var SaleOrderNoIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.SaleOrderNo);
                var PhysInvIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.PhysInv);
                var ProOrderNoIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ProOrderNo);
                var InventoryByIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.InventoryBy);
                var InventoryAtIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.InventoryAt);
                var ConfirmByIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ConfirmBy);
                var ConfirmAtIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ConfirmAt);
                var AuditByIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.AuditBy);
                var AuditAtIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.AuditAt);
                var ComponentNameIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.ComponentName);
                var PositionIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.Position);
                var AssemblyLocIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.AssemblyLoc);
                var VendorCodeIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.VendorCode);
                var SaleOrderListIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.SaleOrderList);
                var NoIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.No);
                var CSAPIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.CSAP);
                var KSAPIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.KSAP);
                var MSAPIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.MSAP);
                var OSAPIndex = DocumentResultExcel.GetColumnIndex(DocumentResultExcel.OSAP);

                // Đặt tiêu đề cho cột
                worksheet.Cells[1, STTIndex].Value = DocumentResultExcel.STT;
                worksheet.Cells[1, ComponentCodeIndex].Value = DocumentResultExcel.ComponentCode;
                worksheet.Cells[1, ModelCodeIndex].Value = DocumentResultExcel.ModelCode;
                worksheet.Cells[1, PlantIndex].Value = DocumentResultExcel.Plant;
                worksheet.Cells[1, WHLocIndex].Value = DocumentResultExcel.WHLoc;
                worksheet.Cells[1, QuantityIndex].Value = DocumentResultExcel.Quantity;
                worksheet.Cells[1, TotalQuantityIndex].Value = DocumentResultExcel.TotalQuantity;
                worksheet.Cells[1, AccountIndex].Value = DocumentResultExcel.Account;
                worksheet.Cells[1, ErrorQuantityIndex].Value = DocumentResultExcel.ErrorQuantity;
                worksheet.Cells[1, ErrorMoneyIndex].Value = DocumentResultExcel.ErrorMoney;
                worksheet.Cells[1, UnitPriceIndex].Value = DocumentResultExcel.UnitPrice;
                worksheet.Cells[1, DocCodeIndex].Value = DocumentResultExcel.DocCode;
                worksheet.Cells[1, StockTypesIndex].Value = DocumentResultExcel.StockTypes;
                worksheet.Cells[1, SpecialStockIndex].Value = DocumentResultExcel.SpecialStock;
                worksheet.Cells[1, SaleOrderNoIndex].Value = DocumentResultExcel.SaleOrderNo;
                worksheet.Cells[1, PhysInvIndex].Value = DocumentResultExcel.PhysInv;
                worksheet.Cells[1, ProOrderNoIndex].Value = DocumentResultExcel.ProOrderNo;
                worksheet.Cells[1, InventoryByIndex].Value = DocumentResultExcel.InventoryBy;
                worksheet.Cells[1, InventoryAtIndex].Value = DocumentResultExcel.InventoryAt;
                worksheet.Cells[1, ConfirmByIndex].Value = DocumentResultExcel.ConfirmBy;
                worksheet.Cells[1, ConfirmAtIndex].Value = DocumentResultExcel.ConfirmAt;
                worksheet.Cells[1, AuditByIndex].Value = DocumentResultExcel.AuditBy;
                worksheet.Cells[1, AuditAtIndex].Value = DocumentResultExcel.AuditAt;
                worksheet.Cells[1, ComponentNameIndex].Value = DocumentResultExcel.ComponentName;
                worksheet.Cells[1, PositionIndex].Value = DocumentResultExcel.Position;
                worksheet.Cells[1, AssemblyLocIndex].Value = DocumentResultExcel.AssemblyLoc;
                worksheet.Cells[1, VendorCodeIndex].Value = DocumentResultExcel.VendorCode;
                worksheet.Cells[1, SaleOrderListIndex].Value = DocumentResultExcel.SaleOrderList;
                worksheet.Cells[1, NoIndex].Value = DocumentResultExcel.No;
                worksheet.Cells[1, CSAPIndex].Value = DocumentResultExcel.CSAP;
                worksheet.Cells[1, KSAPIndex].Value = DocumentResultExcel.KSAP;
                worksheet.Cells[1, MSAPIndex].Value = DocumentResultExcel.MSAP;
                worksheet.Cells[1, OSAPIndex].Value = DocumentResultExcel.OSAP;

                // Đặt kiểu và màu cho tiêu đề
                using (var range = worksheet.Cells[1, DocumentResultExcel.GetColumnIndex(DocumentResultExcel.STT), 1, DocumentResultExcel.GetColumnIndex(DocumentResultExcel.OSAP)])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.None;
                }

                int batchSize = 2000;
                var totalCount = items.Count();

                var batches = Enumerable.Range(0, (totalCount + batchSize - 1) / batchSize)
                              .Select(i => items.Skip(i * batchSize).Take(batchSize));

                Parallel.ForEach(batches, (batch, state, index) =>
                {
                    var startRow = (int)index * batchSize + 2;

                    using (var range = worksheet.Cells[startRow, 1, startRow + batch.Count() - 1, DocumentResultExcel.GetColumnIndex(DocumentResultExcel.OSAP)])
                    {
                        int stt = (int)index * batchSize + 1;
                        int startExcelRow = (int)index * batchSize + 2;

                        foreach (var item in batch)
                        {
                            range[startExcelRow, STTIndex].Value = stt;
                            range[startExcelRow, ComponentCodeIndex].Value = item.ComponentCode;
                            range[startExcelRow, ModelCodeIndex].Value = item.ModelCode;
                            range[startExcelRow, PlantIndex].Value = item.Plant;
                            range[startExcelRow, WHLocIndex].Value = item.WHLoc;
                            range[startExcelRow, QuantityIndex].Value = item.Quantity?.ToDisplayValue();
                            range[startExcelRow, TotalQuantityIndex].Value = item.TotalQuantity.ToDisplayValue();
                            range[startExcelRow, AccountIndex].Value = item.AccountQuantity.ToDisplayValue();
                            range[startExcelRow, ErrorQuantityIndex].Value = item.ErrorQuantity.ToDisplayValue();
                            range[startExcelRow, ErrorMoneyIndex].Value = item.ErrorMoney;
                            range[startExcelRow, UnitPriceIndex].Value = item.UnitPrice.ToDisplayValue();
                            range[startExcelRow, DocCodeIndex].Value = item.DocCode;
                            range[startExcelRow, StockTypesIndex].Value = item.StockType;
                            range[startExcelRow, SpecialStockIndex].Value = item.SpecialStock;
                            range[startExcelRow, SaleOrderNoIndex].Value = item.SaleOrderNo;
                            range[startExcelRow, PhysInvIndex].Value = item.PhysInv;
                            range[startExcelRow, ProOrderNoIndex].Value = item.ProductOrderNo;
                            range[startExcelRow, InventoryByIndex].Value = item.InventoryBy;
                            range[startExcelRow, InventoryAtIndex].Value = item.InventoryAt;
                            range[startExcelRow, ConfirmByIndex].Value = item.ConfirmedBy;
                            range[startExcelRow, ConfirmAtIndex].Value = item.ConfirmAt;
                            range[startExcelRow, AuditByIndex].Value = item.AuditBy;
                            range[startExcelRow, AuditAtIndex].Value = item.AuditAt;
                            range[startExcelRow, ComponentNameIndex].Value = item.ComponentName;
                            range[startExcelRow, PositionIndex].Value = item.Position;
                            range[startExcelRow, AssemblyLocIndex].Value = item.AssemblyLoc;
                            range[startExcelRow, VendorCodeIndex].Value = item.VendorCode;
                            range[startExcelRow, SaleOrderListIndex].Value = item.SaleOrderList;
                            range[startExcelRow, NoIndex].Value = item.No;
                            range[startExcelRow, CSAPIndex].Value = item.CSAP;
                            range[startExcelRow, KSAPIndex].Value = item.KSAP;
                            range[startExcelRow, MSAPIndex].Value = item.MSAP;
                            range[startExcelRow, OSAPIndex].Value = item.OSAP;

                            stt++;
                            startExcelRow++;
                        }
                    }
                });

                bytes = package.GetAsByteArray();
            }

            return new ResponseModel<byte[]>
            {
                Code = StatusCodes.Status200OK,
                Data = bytes,
                Message = "Xuất file tổng hợp kết quả thành công."
            };
        }


        public async Task<ResponseModel> ExportTreeGroups(Guid inventoryId, string machineModel, string machineType = null)
        {
            var modelCodeRegex = new Regex(Constants.RegexPattern.ModelCodeRegex);
            var mainLineRegex = new Regex(Constants.RegexPattern.MainLineGrpRegex);
            var shareGrpRegex = new Regex(Constants.RegexPattern.ShareGrpRegex);
            var shareGrpByModelRegex = new Regex(Constants.RegexPattern.ShareGrpByModelRegex);
            var finishGrpRegex = new Regex(Constants.RegexPattern.FinishGrpRegex);

            //get data from DocTypeCUnit & InventoryDoc

            var modelCodeStageNames = _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryId && x.ModelCode.Contains(machineModel)).Select(x => new { x.ModelCode, x.StageName });

            var compUnitquery = await (from invDocTypeC in _inventoryContext.InventoryDocs.AsNoTracking()
                                       join docTypeCDetail in _inventoryContext.DocTypeCDetails.AsNoTracking() on invDocTypeC.Id equals docTypeCDetail.InventoryDocId
                                       join stagName in modelCodeStageNames on docTypeCDetail.ModelCode equals stagName.ModelCode

                                       where invDocTypeC.InventoryId == inventoryId
                                       && !string.IsNullOrEmpty(docTypeCDetail.ModelCode) && docTypeCDetail.ModelCode.Contains(machineModel)

                                       select new ModelCodeGroupDto
                                       {
                                           MainModelCode = docTypeCDetail.ModelCode,
                                           StageName = stagName.StageName,
                                           UnitModelCode = string.Empty,
                                           DirectParent = docTypeCDetail.DirectParent
                                       }).Distinct().ToListAsync();

            var listModelCode = compUnitquery.Select(x => x.MainModelCode).ToList();

            var standAlone = await (from invDocTypeC in _inventoryContext.InventoryDocs.AsNoTracking()
                                    join docTypeCDetail in _inventoryContext.DocTypeCDetails.AsNoTracking() on invDocTypeC.Id equals docTypeCDetail.InventoryDocId


                                    where invDocTypeC.InventoryId == inventoryId && string.IsNullOrEmpty(docTypeCDetail.ModelCode) && docTypeCDetail.DirectParent.Contains(machineModel) && (string.IsNullOrEmpty(machineType) || docTypeCDetail.DirectParent.Contains(machineType))
                                    && !listModelCode.Contains(docTypeCDetail.DirectParent)
                                    select new ModelCodeGroupDto
                                    {
                                        MainModelCode = docTypeCDetail.DirectParent,
                                        StageName = invDocTypeC.StageName,
                                        UnitModelCode = string.Empty,
                                        DirectParent = docTypeCDetail.DirectParent
                                    }).Distinct().ToListAsync();

            { }
            //var modelCodes = _inventoryContext.DocTypeCDetails.AsNoTracking().Where(d => d.InventoryId == inventoryId
            //    && !string.IsNullOrEmpty(d.DirectParent) && !string.IsNullOrEmpty(d.ModelCode)).Select(d => new KeyValuePair<string, string>(d.ModelCode, d.DirectParent)).ToList();
            //var removeWrongCompInvDocItem = compInvDocQuery.Select(x =>
            //  {

            //      if (!string.IsNullOrEmpty(x.UnitModelCode) && modelCodes.Any(m => m.Key == x.UnitModelCode && m.Value == x.MainModelCode))
            //      {
            //          return x;
            //      }
            //      else if (string.IsNullOrEmpty(x.UnitModelCode))
            //      {
            //          return x;
            //      }

            //      return null;

            //  }).ToList();

            //union into 1 list
            //var unGroupDatas = compUnitquery.Union(standAlone).ToList();

            //group and select into list TreeGroupDto
            var groupedByModelCodeDatas = compUnitquery.Concat(standAlone)
             .Where(x =>

                                     modelCodeRegex.IsMatch(x.DirectParent)
                                     && (string.IsNullOrEmpty(machineType) || modelCodeRegex.Match(x.DirectParent).Groups[2].Value == machineType)
                                    )
             .GroupBy(x => x.MainModelCode)
            .Select(x =>
            {
                var modelCodeMatch = modelCodeRegex.Match(x.Key);
                var treeGroupDto = new TreeGroupDto
                {
                    MachineModel = modelCodeMatch.Groups[1].Value,
                    Line = modelCodeMatch.Groups[3].Value,
                    ModelStage = modelCodeMatch.Groups[4].Value,
                    ModelCode = x.Key,

                    ModelStageNumber = modelCodeMatch.Groups[5].Value,
                    StageName = x.FirstOrDefault(g => g.MainModelCode == x.Key).StageName,

                    AttachmentModelCodes = compUnitquery.Where(g => g.DirectParent == x.Key).Select(g => $"{g.MainModelCode}|({g.StageName})").Distinct().ToList()
                };
                return treeGroupDto;
            }).OrderBy(x => x.ModelCode).ThenBy(x => x.Line).ThenByDescending(x => x.ModelStageNumber)
             .ToList();

            if (groupedByModelCodeDatas.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu."
                };
            }

            //group by MachineMode and Line for each excel sheet
            var groupedByMachineModelLineModelCodeDatas = groupedByModelCodeDatas.GroupBy(x => new { x.MachineModel, x.Line, x.ModelCode });

            //get the longest attachment for columns dimension in excel
            var maxCol = groupedByModelCodeDatas.Max(x => x.AttachmentModelCodes.Count());

            using (var excelPackage = new ExcelPackage())
            {
                //add sheets base on machine model and line
                var sheetNames = groupedByMachineModelLineModelCodeDatas.Where(x => x.Key.Line != "0" && x.Key.MachineModel == machineModel && (string.IsNullOrEmpty(machineType) || x.Key.ModelCode.Contains(machineType))).Select(x => new
                {
                    SheetName = $"{x.Key.MachineModel} - Line {x.Key.Line}",
                }).Distinct();

                foreach (var item in sheetNames)
                {
                    excelPackage.Workbook.Worksheets.Add(item.SheetName);
                }
                foreach (var sheet in excelPackage.Workbook.Worksheets)
                {
                    //var attachmentData = new List<Dictionary<string, object>>();
                    //get data 
                    var sheetData = groupedByModelCodeDatas.Where(x => sheet.Name == $"{x.MachineModel} - Line {x.Line}" || (sheet.Name.Contains(x.MachineModel) && x.Line == "0")).Select(x =>
                    {
                        var data = new Dictionary<string, object>()
                        {
                            { "MainModelCode", $"{x.ModelCode}|({x.StageName})"}


                        };
                        var listAttachment = x.AttachmentModelCodes.OrderByDescending(x => x).ToList();

                        for (int i = 0; i < listAttachment.Count; i++)
                        {
                            data.Add($"AttachModelCode{i}", listAttachment[i]);
                        }

                        var listSharedAttachment = listAttachment.Where(x => shareGrpRegex.IsMatch(x) || shareGrpByModelRegex.IsMatch(x)).ToList();
                        //var shareData = new Dictionary<string, object>();
                        //for (int i = 0; i < listSharedAttachment.Count; i++)
                        //{
                        //    shareData.Add($"AttachModelCode{i}", listSharedAttachment[i]);
                        //}
                        //attachmentData.Add(shareData);

                        //foreach (var item in listAttachment)
                        //{
                        //    data.Add($"AttachModelCode{listAttachment.IndexOf(item)}", item);
                        //}
                        return data;
                    });
                    //sheetData = sheetData.Concat(attachmentData.Where(x=>x.Any()));
                    var displayData = sheetData.Select(x => x.Values.ToList());

                    var data = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(sheetData));
                    //sheetData.Select(x => x.Values).ToList();
                    sheet.Cells.LoadFromDataTable(data, true);
                    sheet.DefaultRowHeight = 35;
                    sheet.DefaultColWidth = 15;


                    sheet.InsertColumn(1, 1);
                    sheet.InsertRow(1, 1);

                    sheet.Cells[2, 1].Value = Constants.TreeGroupColumn.No;
                    sheet.Cells[2, 2].Value = Constants.TreeGroupColumn.MainModelCode;
                    foreach (var item in sheet.Cells)
                    {
                        if (item.Value is string)
                        {
                            item.Value = ((string)item.Value).Replace("|", ("" + ((char)13) + ((char)10)));
                            item.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            item.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            item.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            if (mainLineRegex.IsMatch((string)item.Value))
                            {
                                item.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                item.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(1, 180, 198, 231));
                            }
                            else
                            //if (shareGrpRegex.IsMatch((string)item.Value))
                            {
                                item.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                item.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(1, 252, 228, 214));
                            }

                        }
                    }
                    ////set title
                    sheet.Row(1).Style.Font.Size = 18;
                    sheet.Row(1).Style.Font.Bold = true;
                    sheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    sheet.Row(2).Style.Font.Bold = true;
                    sheet.Row(1).Height = sheet.DefaultRowHeight;
                    sheet.Row(2).Height = sheet.DefaultRowHeight;
                    sheet.Row(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[1, 1].Value = $"Model {sheet.Name}";


                    for (int i = 1; i <= sheet.Dimension.Columns; i++)
                    {
                        //sheet.Column(i).Style.Font.Size = 12;
                        //sheet.Column(i).Style.Font.Bold = true;
                        sheet.Column(i).Width = sheet.DefaultColWidth;
                        if (i == 1)
                        {
                            sheet.Column(i).Width = 5;
                        }
                        if (i > 2)
                        {
                            sheet.Cells[2, i].Value = string.Format(Constants.TreeGroupColumn.AttachModelCode, $"{Environment.NewLine}{i - 2}");

                            sheet.Cells[2, i].Style.WrapText = true;
                        }
                        sheet.Cells[2, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[2, i].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(1, 217, 225, 242));

                    }
                    for (int i = 3; i <= sheet.Dimension.Rows; i++)
                    {
                        sheet.Cells[i, 1].Value = i - 2;
                        sheet.Cells[i, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        sheet.Cells[i, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet.Row(i).CustomHeight = true;
                        sheet.Row(i).Height = sheet.DefaultRowHeight;
                        sheet.Row(i).Style.Font.Bold = true;
                        sheet.Row(i).Style.Font.Size = 12;
                        sheet.Row(i).Style.WrapText = true;

                    }

                }
                var result = new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = excelPackage.GetAsByteArray(),
                };
                return result;
            }

            //return new ResponseModel
            //{
            //    Code = StatusCodes.Status200OK,
            //    Data = excelData,
            //};

        }
        private List<string> GetAttachGroups()
        {
            return null;
        }
        private List<string> GetAttachments(Regex mainLineRegex, Regex shareGrpRegex, Regex shareGroupByModelRegex, Regex finishGrpRegex, List<ModelCodeGroupDto> data, string mainModelCode, List<KeyValuePair<string, string>> modelCodes)
        {
            //for finish group

            if (finishGrpRegex.IsMatch(mainModelCode))
            {
                var finishStageNumber = Convert.ToInt32(finishGrpRegex.Match(mainModelCode).Groups[5].Value);
                if (finishStageNumber == 2)
                {
                    var finishAttachGroups = data.Where(x =>
                                                             //x.MainModelCode == mainModelCode
                                                             // && 
                                                             (

                                                             (x.DirectParent == mainModelCode && (shareGrpRegex.IsMatch(x.UnitModelCode) || shareGroupByModelRegex.IsMatch(x.UnitModelCode)))
                                                             || (x.MainModelCode == mainModelCode && finishGrpRegex.IsMatch(x.UnitModelCode) && Convert.ToInt32(finishGrpRegex.Match(x.UnitModelCode).Groups[5].Value) + 1 == finishStageNumber)
                                                             //|| (shareGrpRegex.IsMatch(x.UnitModelCode))
                                                             ))
                                                            .Select(x => new { x.DirectParent, x.UnitModelCode, x.StageName, Index = mainLineRegex.IsMatch(x.UnitModelCode) ? 0 : 1 }).OrderBy(x => x.Index).ToList();
                    return finishAttachGroups.Where(x => !x.UnitModelCode.Contains(mainModelCode)).Select(x =>
                    {

                        if (!string.IsNullOrEmpty(x.UnitModelCode) && modelCodes.Any(m => m.Key == x.UnitModelCode && m.Value == x.DirectParent))
                        {
                            return x;
                        }
                        else if (string.IsNullOrEmpty(x.UnitModelCode) || string.IsNullOrEmpty(x.DirectParent))
                        {
                            return x;
                        }

                        return null;


                    }).Where(x => x != null).Select(x => $"{x.UnitModelCode}|({x.StageName})").Distinct().ToList();
                }
                else
                {
                    var maxStageNumber = data.Where(x => x.MainModelCode == mainModelCode && mainLineRegex.IsMatch(x.UnitModelCode)).Max(x => Convert.ToInt32(mainLineRegex.Match(x.UnitModelCode).Groups[5].Value));
                    var finishAttachGroups = data.Where(x =>

                                                             (

                                                              (x.MainModelCode == mainModelCode && mainLineRegex.IsMatch(x.UnitModelCode) && Convert.ToInt32(mainLineRegex.Match(x.UnitModelCode).Groups[5].Value) == maxStageNumber
                                                             )
                                                             || (x.DirectParent == mainModelCode && (shareGrpRegex.IsMatch(x.UnitModelCode) || shareGroupByModelRegex.IsMatch(x.UnitModelCode)))

                                                             ))
                                                            .Select(x => new { x.DirectParent, x.UnitModelCode, x.StageName, Index = mainLineRegex.IsMatch(x.UnitModelCode) ? 0 : 1 }).OrderBy(x => x.Index).ToList();
                    return finishAttachGroups.Where(x => !x.UnitModelCode.Contains(mainModelCode)).Select(x =>
                    {

                        if (!string.IsNullOrEmpty(x.UnitModelCode) && modelCodes.Any(m => m.Key == x.UnitModelCode && m.Value == x.DirectParent))
                        {
                            return x;
                        }
                        else if (string.IsNullOrEmpty(x.UnitModelCode) || string.IsNullOrEmpty(x.DirectParent))
                        {
                            return x;
                        }

                        return null;


                    }).Where(x => x != null).Select(x => $"{x.UnitModelCode}|({x.StageName})").Distinct().ToList();
                }
            }
            else
            {

                var mainModelMatch = mainLineRegex.IsMatch(mainModelCode) ? mainLineRegex.Match(mainModelCode) : shareGrpRegex.IsMatch(mainModelCode) ? shareGrpRegex.Match(mainModelCode) : shareGroupByModelRegex.Match(mainModelCode);
                var stageNumber = Convert.ToInt32(mainModelMatch.Groups[5].Value);

                var orderedGroups = data.Where(x =>
                //x.MainModelCode == mainModelCode
                //&& 
                ((x.DirectParent == mainModelCode && (shareGrpRegex.IsMatch(x.UnitModelCode) || shareGroupByModelRegex.IsMatch(x.UnitModelCode)))
                || (x.DirectParent == mainModelCode && (shareGroupByModelRegex.IsMatch(x.MainModelCode)))
                || (x.MainModelCode == mainModelCode && mainLineRegex.IsMatch(x.UnitModelCode) && Convert.ToInt32(mainLineRegex.Match(x.UnitModelCode).Groups[5].Value) + 1 == stageNumber)
                //|| (shareGrpRegex.IsMatch(x.UnitModelCode))
                )).Select(x => new { Code = shareGroupByModelRegex.IsMatch(x.MainModelCode) ? x.MainModelCode : x.UnitModelCode, x.StageName, Index = mainLineRegex.IsMatch(x.UnitModelCode) ? 0 : 1 }).OrderBy(x => x.Index);

                var attachGroups = orderedGroups.Select(x => $"{x.Code}|({x.StageName})").Distinct().ToList();
                return attachGroups.Where(x => !x.Contains(mainModelCode)).ToList();
            }
        }

        public async Task<ResponseModel<TreeGroupFilterDto>> GetTreeGroupFilters()
        {
            var modelRegex = new Regex(Constants.RegexPattern.ModelCodeRegex);
            var modelCodes = await _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.DocType == InventoryDocType.C)
                .Select(x => x.ModelCode).Distinct().ToListAsync();
            if (modelCodes?.Count > 0)
            {
                var machineModels = modelCodes
                    .Where(x => modelRegex.IsMatch(x))
                    .Select(x => modelRegex.Match(x).Groups[1].Value)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                var machineTypes = modelCodes.Where(x => modelRegex.IsMatch(x)).Select(x => modelRegex.Match(x).Groups[2].Value).Distinct().ToList();

                var result = new ResponseModel<TreeGroupFilterDto>
                {
                    Code = StatusCodes.Status200OK,
                    Data = new TreeGroupFilterDto
                    {
                        MachineModels = machineModels,
                        MachineTypes = machineTypes
                    }
                };
                return result;
            }
            return new ResponseModel<TreeGroupFilterDto>
            {
                Code = StatusCodes.Status204NoContent,
                Data = null
            };
        }

        public async Task<ResponseModel<ResultSet<IEnumerable<DocComponentC>>>> DocCComponents(Guid documentId, int skip, int take, string search)
        {
            var componentsC = _inventoryContext.DocTypeCDetails.AsNoTracking()
                                                                      .Where(x => x.InventoryDocId.Value == documentId)
                                                                      .Select(x => new DocComponentC
                                                                      {
                                                                          Id = x.Id,
                                                                          InventoryId = x.InventoryId,
                                                                          InventoryDocId = x.InventoryDocId.Value,
                                                                          ComponentCode = x.ComponentCode,
                                                                          ModelCode = x.ModelCode,
                                                                          QuantityOfBom = x.QuantityOfBom,
                                                                          QuantityPerBom = x.QuantityPerBom,
                                                                          IsHighLight = x.isHighlight,
                                                                          No = x.No
                                                                      });

            if (!string.IsNullOrEmpty(search.Trim()))
            {
                componentsC = componentsC.Where(x => !string.IsNullOrEmpty(x.ComponentCode) && x.ComponentCode.ToLower().Contains(search.ToLower()) ||
                                                    !string.IsNullOrEmpty(x.ModelCode) && x.ModelCode.ToLower().Contains(search.ToLower()));
            }

            var totalRecords = await componentsC.CountAsync();
            var result = await componentsC.OrderBy(x => x.No).Skip(skip).Take(take).ToListAsync();

            return new ResponseModel<ResultSet<IEnumerable<DocComponentC>>>
            {
                Code = StatusCodes.Status200OK,
                Data = new ResultSet<IEnumerable<DocComponentC>>
                {
                    Data = result,
                    TotalRecords = totalRecords
                },
            };
        }

        public async Task<ResponseModel> UpdateAllReceiveDoc(List<Guid> excludeIds)
        {
            var currInventoryAccount = _httpContext.UserFromContext();
            var inventory = currInventoryAccount?.InventoryLoggedInfo?.InventoryModel;

            if (inventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy đợt kiểm kê."
                };
            }


            var notInventoryYetDocs = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventory.InventoryId &&
                                                                                   x.Status == InventoryDocStatus.NotReceiveYet &&
                                                                                   !excludeIds.Contains(x.Id));

            if (notInventoryYetDocs?.Count() == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy phiếu chưa tiếp nhận."
                };
            }

            bool isInventoryAccount = currInventoryAccount.InventoryLoggedInfo.HasRoleType && (currInventoryAccount?.InventoryLoggedInfo?.InventoryRoleType == (int)InventoryAccountRoleType.Inventory);
            if (isInventoryAccount)
            {
                notInventoryYetDocs = notInventoryYetDocs.Where(x => x.AssignedAccountId == Guid.Parse(currInventoryAccount.UserId));
            }

            var docsCount = notInventoryYetDocs?.Count();
            if (docsCount == 0)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy phiếu chưa tiếp nhận được phân phát cho bạn."
                };
            }

            var currUser = _httpContext.UserFromContext();

            var docAHasQuantity = notInventoryYetDocs.Where(x => x.DocType == InventoryDocType.A && x.Quantity > 0);
            var remainingDocs = notInventoryYetDocs.Where(x => !(x.DocType == InventoryDocType.A && x.Quantity > 0));

            //TH1: Đối với các phiếu A đã có giá trị trường Quantity thì sẽ chuyển sang trạng thái “ Đã xác nhận “
            await docAHasQuantity.ExecuteUpdateAsync(t => t.SetProperty(b => b.Status, x => InventoryDocStatus.Confirmed)
                                                    .SetProperty(b => b.InventoryAt, x => DateTime.Now)
                                                    .SetProperty(b => b.ReceiveAt, x => DateTime.Now)
                                                    .SetProperty(b => b.ConfirmAt, x => DateTime.Now)
                                                    .SetProperty(b => b.ReceiveBy, x => Guid.Parse(currUser.UserId))
                                                    .SetProperty(b => b.InventoryBy, currUser.UserCode)
                                                    .SetProperty(b => b.ConfirmBy, currUser.UserCode));

            await remainingDocs.ExecuteUpdateAsync(t => t.SetProperty(b => b.Status, x => InventoryDocStatus.NotInventoryYet)
                                                                            .SetProperty(b => b.ReceiveAt, x => DateTime.Now)
                                                                            .SetProperty(b => b.ReceiveBy, x => Guid.Parse(currInventoryAccount.UserId))
                                                                        );

            //Nếu có phiếu chuyển trạng thái xác nhận thì chạy Background để tính toán lại
            if (docAHasQuantity?.Count() > 0)
            {
                var updateModel = new InventoryDocSubmitDto { DocType = InventoryDocType.A, InventoryId = currInventoryAccount.InventoryLoggedInfo.InventoryModel.InventoryId };
                await _dataAggregationService.UpdateDataFromInventoryDoc(updateModel);
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = $"Tiếp nhận {docsCount.Value} phiếu thành công."
            };
        }

        public async Task<ResponseModel<bool>> CheckAnyDocAssigned()
        {
            var currUser = _httpContext.UserFromContext();

            var rightPermission = (currUser.AccountType == nameof(AccountType.TaiKhoanRieng) && currUser.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY)) ||
                                           _httpContext.IsPromoter();

            var inventory = currUser?.InventoryLoggedInfo?.InventoryModel;
            if (inventory == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy đợt kiểm kê hiện tại."
                };
            }

            var docs = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventory.InventoryId);
            if (docs?.Count() == 0)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không thực hiện được do chưa có phiếu nào được tạo trong đợt kiểm kê hiện tại. Vui lòng thử lại sau."
                };
            }

            docs = docs.Where(x => x.Status == InventoryDocStatus.NotInventoryYet);
            if (docs?.Count() == 0)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy phiếu có trạng thái chưa kiểm kê trong đợt kiểm kê hiện tại. Vui lòng thử lại sau."
                };
            }

            bool isInventoryAccount = currUser.InventoryLoggedInfo.HasRoleType && (currUser.InventoryLoggedInfo?.InventoryRoleType == (int)InventoryAccountRoleType.Inventory);
            if (isInventoryAccount)
            {
                var docsAssigned = docs.Where(x => x.AssignedAccountId == Guid.Parse(currUser.UserId));

                if (docsAssigned?.Count() == 0)
                {
                    return new ResponseModel<bool>
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Không thực hiện được do tài khoản của bạn chưa được phân phát phiếu trong đợt kiểm kê hiện tại. Vui lòng thử lại sau."
                    };
                }
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
            };
        }

        public async Task<ResponseModel<bool>> CheckDownloadDocTemplate()
        {
            var currUser = _httpContext.UserFromContext();
            var inventory = currUser?.InventoryLoggedInfo?.InventoryModel;

            var rightPermission = (currUser.AccountType == nameof(AccountType.TaiKhoanRieng) && currUser.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY)) ||
                                           _httpContext.IsPromoter();

            //Nếu tài khoản riêng hoặc xúc tiến thì không cần check
            if (rightPermission)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status200OK,
                };
            }

            if (inventory == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy đợt kiểm kê hiện tại."
                };
            }

            var docs = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventory.InventoryId);
            if (docs?.Count() == 0)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Tải biểu mẫu không thành công do chưa có phiếu nào được tạo trong đợt kiểm kê hiện tại. Vui lòng thử lại sau."
                };
            }

            docs = docs.Where(x => x.Status == InventoryDocStatus.NotInventoryYet);
            if (docs?.Count() == 0)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Tải biểu mẫu không thành công do không tìm thấy phiếu có trạng thái chưa kiểm kê trong đợt kiểm kê hiện tại. Vui lòng thử lại sau."
                };
            }


            bool isInventoryAccount = currUser.InventoryLoggedInfo.HasRoleType && (currUser.InventoryLoggedInfo?.InventoryRoleType == (int)InventoryAccountRoleType.Inventory);
            if (isInventoryAccount)
            {
                var docsAssigned = docs.Where(x => x.AssignedAccountId == Guid.Parse(currUser.UserId));

                if (docsAssigned?.Count() == 0)
                {
                    return new ResponseModel<bool>
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Tải biểu mẫu không thành công do tài khoản của bạn chưa được phân phát phiếu trong đợt kiểm kê hiện tại. Vui lòng thử lại sau."
                    };
                }
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
            };
        }
        public async Task<ResponseModel> CheckExistDocTypeA(string inventoryId)
        {
            var existDocTypeA = await _inventoryContext.InventoryDocs.FirstOrDefaultAsync(x => x.InventoryId.ToString().ToLower() == inventoryId.ToLower() && x.DocType == InventoryDocType.A);
            if (existDocTypeA == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Vui lòng tạo phiếu A trước khi thực hiện tạo các phiếu khác."
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
            };
        }

        public async Task<ResponseModel> AggregateDocResults(Guid inventoryId, string userId)
        {
            //Validate
            if (!Guid.TryParse(inventoryId.ToString(), out _))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu không hợp lệ."
                };
            }
            var inventory = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id == inventoryId);
            var inventoryDocSubmitDto = new InventoryDocSubmitDto
            {
                InventoryId = inventoryId,
                ForceAggregate = true,
                ForceAggregateAt = DateTime.Now,
                DocType = InventoryDocType.C
            };

            //if (!inventory.ForceAggregateAt.HasValue || (inventoryDocSubmitDto.ForceAggregateAt.HasValue && inventory.ForceAggregateAt.HasValue && inventory.ForceAggregateAt.Value.AddMinutes(5) <= inventoryDocSubmitDto.ForceAggregateAt.Value))
            {
                //await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(x => SyntheticData(inventoryId, userId));
                var synteticRes = await SyntheticData(inventoryId, userId);

                if (!string.IsNullOrEmpty(synteticRes))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = synteticRes,
                    };
                }

                await _dataAggregationService.UpdateDataFromInventoryDoc(inventoryDocSubmitDto);

                inventory.ForceAggregateAt = inventoryDocSubmitDto.ForceAggregateAt.Value;
                _inventoryContext.Inventories.Update(inventory);
                await _inventoryContext.SaveChangesAsync();
                await _dataAggregationService.AddDataToErrorInvestigation(inventoryId);
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Đang tiến hành tổng hợp kết quả.",
                    Data = inventory.ForceAggregateAt
                };
            }
            //else
            //{
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status200OK,
            //        Message = "Đang tiến hành tổng hợp kết quả.",
            //        Data = inventory.ForceAggregateAt
            //    };
            //}
        }
        public async Task<ImportResponseModel<byte[]>> ImportUpdateQuantity(IFormFile file, string inventoryName)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string[] allowExtension = new[] { ".xlsx" };

            if (allowExtension.Contains(fileExtension) == false || file.Length < 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật số lượng các phiếu."
                };
            }

            var inventories = await _inventoryContext.Inventories.AsNoTracking().Select(x => new
            {
                x.Id,
                x.Name
            }).ToListAsync();
            var inventoryId = inventories?.FirstOrDefault(x => x.Name == inventoryName)?.Id;

            var currUser = _httpContext.UserFromContext();

            List<ImportUpdateQuantityExcelValueModel> items = new();
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

                        //Lấy tiêu đề từng cột dòng đầu tiên:

                        var headerValue = (object[,])sourceSheet.Cells["A1:BA1"].Value;

                        if (headerValue.Cast<string>().Any(string.IsNullOrEmpty))
                        {
                            return new ImportResponseModel<byte[]>
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật số lượng các phiếu.",
                            };
                        }

                        var ListHeaderExcel = new List<string>();

                        foreach (var item in headerValue)
                        {
                            ListHeaderExcel.Add(item.ToString());
                        }

                        //List<string> listTitleHeaderExcels = new();
                        //int totalColumnsCount = sourceSheet.Dimension.End.Column;

                        //// Lặp qua từng cột trong hàng đầu tiên và lấy giá trị của từng ô
                        //for (int col = 1; col <= totalColumnsCount; col++)
                        //{
                        //    var columnHeader = sourceSheet.Cells[1, col].Value?.ToString();
                        //    if (!string.IsNullOrEmpty(columnHeader))
                        //    {
                        //        listTitleHeaderExcels.Add(columnHeader);
                        //    }
                        //}

                        List<string> ListHeader = new List<string>
                        {
                            Constants.ImportUpdateQuantity.No,
                            Constants.ImportUpdateQuantity.DocCode,
                            Constants.ImportUpdateQuantity.Note,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                            Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom,
                        };

                        // Kiểm tra độ dài của hai danh sách
                        if (ListHeader.Count != ListHeaderExcel.Count)
                        {
                            return new ImportResponseModel<byte[]>
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật số lượng các phiếu.",
                            };
                        }

                        // Kiểm tra từng phần tử của hai danh sách
                        for (var i = 0; i < ListHeaderExcel.Count; i++)
                        {
                            if (ListHeaderExcel[i] != ListHeader[i])
                            {
                                return new ImportResponseModel<byte[]>
                                {
                                    Code = StatusCodes.Status400BadRequest,
                                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật số lượng các phiếu.",
                                };
                            }
                        }

                        foreach (var row in rows)
                        {
                            //Bỏ qua các dòng null
                            var isEmptyRow = sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.Columns].All(x => string.IsNullOrEmpty(x.Value?.ToString()));
                            if (isEmptyRow)
                            {
                                continue;
                            }
                            var item = new ImportUpdateQuantityExcelValueModel();
                            item.DocCode = sourceSheet.Cells[row, 2].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Note = sourceSheet.Cells[row, 3].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom01 = sourceSheet.Cells[row, 4].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom02 = sourceSheet.Cells[row, 5].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom03 = sourceSheet.Cells[row, 6].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom04 = sourceSheet.Cells[row, 7].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom05 = sourceSheet.Cells[row, 8].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom06 = sourceSheet.Cells[row, 9].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom07 = sourceSheet.Cells[row, 10].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom08 = sourceSheet.Cells[row, 11].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom09 = sourceSheet.Cells[row, 12].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom10 = sourceSheet.Cells[row, 13].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom11 = sourceSheet.Cells[row, 14].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom12 = sourceSheet.Cells[row, 15].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom13 = sourceSheet.Cells[row, 16].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom14 = sourceSheet.Cells[row, 17].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom15 = sourceSheet.Cells[row, 18].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom16 = sourceSheet.Cells[row, 19].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom17 = sourceSheet.Cells[row, 20].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom18 = sourceSheet.Cells[row, 21].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom19 = sourceSheet.Cells[row, 22].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom20 = sourceSheet.Cells[row, 23].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom21 = sourceSheet.Cells[row, 24].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom22 = sourceSheet.Cells[row, 25].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom23 = sourceSheet.Cells[row, 26].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom24 = sourceSheet.Cells[row, 27].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom25 = sourceSheet.Cells[row, 28].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom26 = sourceSheet.Cells[row, 29].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom27 = sourceSheet.Cells[row, 30].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom28 = sourceSheet.Cells[row, 31].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom29 = sourceSheet.Cells[row, 32].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom30 = sourceSheet.Cells[row, 33].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom31 = sourceSheet.Cells[row, 34].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom32 = sourceSheet.Cells[row, 35].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom33 = sourceSheet.Cells[row, 36].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom34 = sourceSheet.Cells[row, 37].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom35 = sourceSheet.Cells[row, 38].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom36 = sourceSheet.Cells[row, 39].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom37 = sourceSheet.Cells[row, 40].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom38 = sourceSheet.Cells[row, 41].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom39 = sourceSheet.Cells[row, 42].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom40 = sourceSheet.Cells[row, 43].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom41 = sourceSheet.Cells[row, 44].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom42 = sourceSheet.Cells[row, 45].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom43 = sourceSheet.Cells[row, 46].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom44 = sourceSheet.Cells[row, 47].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom45 = sourceSheet.Cells[row, 48].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom46 = sourceSheet.Cells[row, 49].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom47 = sourceSheet.Cells[row, 50].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom48 = sourceSheet.Cells[row, 51].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom49 = sourceSheet.Cells[row, 52].Value?.ToString()?.Trim() ?? string.Empty;
                            item.QuantityOfBom_Mutil_QuantityPerBom50 = sourceSheet.Cells[row, 53].Value?.ToString()?.Trim() ?? string.Empty;
                            items.Add(item);
                        }

                        //Validate STT

                        foreach (var item in items)
                        {
                            //validate:
                            //Các trường mã phiếu, số lượng thùng x số thùng = false => Bắn lỗi file ko đúng định dạng:
                            var validateDocCodeResult = ValidateDocCodeUpdateQuantity(item, inventoryId.Value);
                            var validateQuantityOfBom_Mutil_QuantityPerBomResult = ValidateQuantityOfBom_Mutil_QuantityPerBom(item);
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            //Ghi vào excel các bản ghi bị lỗi
            var invalidItems = items.Where(x => x.Error.IsValid == false).ToList();

            MemoryStream stream = new MemoryStream();

            if (invalidItems.Any())
            {
                //Ghi lỗi vào file excel
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lỗi khi cập nhật số lượng");

                    // Đặt tiêu đề cho cột

                    worksheet.Cells[1, 1].Value = Constants.ImportUpdateQuantity.No;
                    worksheet.Column(1).Width = 10;
                    worksheet.Cells[1, 2].Value = Constants.ImportUpdateQuantity.DocCode;
                    worksheet.Column(2).Width = 20;
                    worksheet.Cells[1, 3].Value = Constants.ImportUpdateQuantity.Note;
                    worksheet.Column(3).Width = 20;
                    worksheet.Cells[1, 4].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(4).Width = 30;
                    worksheet.Cells[1, 5].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(5).Width = 30;
                    worksheet.Cells[1, 6].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(6).Width = 30;
                    worksheet.Cells[1, 7].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(7).Width = 30;
                    worksheet.Cells[1, 8].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(8).Width = 30;
                    worksheet.Cells[1, 9].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(9).Width = 30;
                    worksheet.Cells[1, 10].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(10).Width = 30;
                    worksheet.Cells[1, 11].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(11).Width = 30;
                    worksheet.Cells[1, 12].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(12).Width = 30;
                    worksheet.Cells[1, 13].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(13).Width = 30;
                    worksheet.Cells[1, 14].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(14).Width = 30;
                    worksheet.Cells[1, 15].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(15).Width = 30;
                    worksheet.Cells[1, 16].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(16).Width = 30;
                    worksheet.Cells[1, 17].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(17).Width = 30;
                    worksheet.Cells[1, 18].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(18).Width = 30;
                    worksheet.Cells[1, 19].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(19).Width = 30;
                    worksheet.Cells[1, 20].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(20).Width = 30;
                    worksheet.Cells[1, 21].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(21).Width = 30;
                    worksheet.Cells[1, 22].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(22).Width = 30;
                    worksheet.Cells[1, 23].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(23).Width = 30;
                    worksheet.Cells[1, 24].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(24).Width = 30;
                    worksheet.Cells[1, 25].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(25).Width = 30;
                    worksheet.Cells[1, 26].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(26).Width = 30;
                    worksheet.Cells[1, 27].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(27).Width = 30;
                    worksheet.Cells[1, 28].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(28).Width = 30;

                    worksheet.Cells[1, 29].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(29).Width = 30;
                    worksheet.Cells[1, 30].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(30).Width = 30;
                    worksheet.Cells[1, 31].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(31).Width = 30;
                    worksheet.Cells[1, 32].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(32).Width = 30;
                    worksheet.Cells[1, 33].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(33).Width = 30;
                    worksheet.Cells[1, 34].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(34).Width = 30;
                    worksheet.Cells[1, 35].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(35).Width = 30;
                    worksheet.Cells[1, 36].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(36).Width = 30;
                    worksheet.Cells[1, 37].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(37).Width = 30;
                    worksheet.Cells[1, 38].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(38).Width = 30;
                    worksheet.Cells[1, 39].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(39).Width = 30;
                    worksheet.Cells[1, 40].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(40).Width = 30;
                    worksheet.Cells[1, 41].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(41).Width = 30;
                    worksheet.Cells[1, 42].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(42).Width = 30;
                    worksheet.Cells[1, 43].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(43).Width = 30;
                    worksheet.Cells[1, 44].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(44).Width = 30;
                    worksheet.Cells[1, 45].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(45).Width = 30;
                    worksheet.Cells[1, 46].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(46).Width = 30;
                    worksheet.Cells[1, 47].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(47).Width = 30;
                    worksheet.Cells[1, 48].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(48).Width = 30;
                    worksheet.Cells[1, 49].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(49).Width = 30;
                    worksheet.Cells[1, 50].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(50).Width = 30;
                    worksheet.Cells[1, 51].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(51).Width = 30;
                    worksheet.Cells[1, 52].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(52).Width = 30;
                    worksheet.Cells[1, 53].Value = Constants.ImportUpdateQuantity.QuantityOfBom_Mutil_QuantityPerBom;
                    worksheet.Column(53).Width = 30;
                    //worksheet.Cells[1, note].Value = ImportSAPExcel.Note;
                    worksheet.Cells[1, 54].Value = Constants.ImportUpdateQuantity.ErrorSummary;
                    worksheet.Column(54).Width = 20;

                    // Đặt kiểu và màu cho tiêu đề
                    using (var range = worksheet.Cells[1, 1, 1, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.None;
                    }

                    // Điền dữ liệu vào Excel
                    for (int i = 0; i < invalidItems.Count(); i++)
                    {
                        var item = invalidItems.ElementAtOrDefault(i);

                        int stt = i + 1;
                        //Tổng hợp message lỗi
                        var errMessage = item.Error.Values.SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)
                                                             .Distinct();

                        worksheet.Cells[i + 2, 1].Value = stt;
                        worksheet.Cells[i + 2, 2].Value = item.DocCode;
                        worksheet.Cells[i + 2, 3].Value = item.Note;
                        worksheet.Cells[i + 2, 4].Value = item.QuantityOfBom_Mutil_QuantityPerBom01;
                        worksheet.Cells[i + 2, 5].Value = item.QuantityOfBom_Mutil_QuantityPerBom02;
                        worksheet.Cells[i + 2, 6].Value = item.QuantityOfBom_Mutil_QuantityPerBom03;
                        worksheet.Cells[i + 2, 7].Value = item.QuantityOfBom_Mutil_QuantityPerBom04;
                        worksheet.Cells[i + 2, 8].Value = item.QuantityOfBom_Mutil_QuantityPerBom05;
                        worksheet.Cells[i + 2, 9].Value = item.QuantityOfBom_Mutil_QuantityPerBom06;
                        worksheet.Cells[i + 2, 10].Value = item.QuantityOfBom_Mutil_QuantityPerBom07;
                        worksheet.Cells[i + 2, 11].Value = item.QuantityOfBom_Mutil_QuantityPerBom08;
                        worksheet.Cells[i + 2, 12].Value = item.QuantityOfBom_Mutil_QuantityPerBom09;
                        worksheet.Cells[i + 2, 13].Value = item.QuantityOfBom_Mutil_QuantityPerBom10;
                        worksheet.Cells[i + 2, 14].Value = item.QuantityOfBom_Mutil_QuantityPerBom11;
                        worksheet.Cells[i + 2, 15].Value = item.QuantityOfBom_Mutil_QuantityPerBom12;
                        worksheet.Cells[i + 2, 16].Value = item.QuantityOfBom_Mutil_QuantityPerBom13;
                        worksheet.Cells[i + 2, 17].Value = item.QuantityOfBom_Mutil_QuantityPerBom14;
                        worksheet.Cells[i + 2, 18].Value = item.QuantityOfBom_Mutil_QuantityPerBom15;
                        worksheet.Cells[i + 2, 19].Value = item.QuantityOfBom_Mutil_QuantityPerBom16;
                        worksheet.Cells[i + 2, 20].Value = item.QuantityOfBom_Mutil_QuantityPerBom17;
                        worksheet.Cells[i + 2, 21].Value = item.QuantityOfBom_Mutil_QuantityPerBom18;
                        worksheet.Cells[i + 2, 22].Value = item.QuantityOfBom_Mutil_QuantityPerBom19;
                        worksheet.Cells[i + 2, 23].Value = item.QuantityOfBom_Mutil_QuantityPerBom20;
                        worksheet.Cells[i + 2, 24].Value = item.QuantityOfBom_Mutil_QuantityPerBom21;
                        worksheet.Cells[i + 2, 25].Value = item.QuantityOfBom_Mutil_QuantityPerBom22;
                        worksheet.Cells[i + 2, 26].Value = item.QuantityOfBom_Mutil_QuantityPerBom23;
                        worksheet.Cells[i + 2, 27].Value = item.QuantityOfBom_Mutil_QuantityPerBom24;
                        worksheet.Cells[i + 2, 28].Value = item.QuantityOfBom_Mutil_QuantityPerBom25;
                        worksheet.Cells[i + 2, 29].Value = item.QuantityOfBom_Mutil_QuantityPerBom26;
                        worksheet.Cells[i + 2, 30].Value = item.QuantityOfBom_Mutil_QuantityPerBom27;
                        worksheet.Cells[i + 2, 31].Value = item.QuantityOfBom_Mutil_QuantityPerBom28;
                        worksheet.Cells[i + 2, 32].Value = item.QuantityOfBom_Mutil_QuantityPerBom29;
                        worksheet.Cells[i + 2, 33].Value = item.QuantityOfBom_Mutil_QuantityPerBom30;
                        worksheet.Cells[i + 2, 34].Value = item.QuantityOfBom_Mutil_QuantityPerBom31;
                        worksheet.Cells[i + 2, 35].Value = item.QuantityOfBom_Mutil_QuantityPerBom32;
                        worksheet.Cells[i + 2, 36].Value = item.QuantityOfBom_Mutil_QuantityPerBom33;
                        worksheet.Cells[i + 2, 37].Value = item.QuantityOfBom_Mutil_QuantityPerBom34;
                        worksheet.Cells[i + 2, 38].Value = item.QuantityOfBom_Mutil_QuantityPerBom35;
                        worksheet.Cells[i + 2, 39].Value = item.QuantityOfBom_Mutil_QuantityPerBom36;
                        worksheet.Cells[i + 2, 40].Value = item.QuantityOfBom_Mutil_QuantityPerBom37;
                        worksheet.Cells[i + 2, 41].Value = item.QuantityOfBom_Mutil_QuantityPerBom38;
                        worksheet.Cells[i + 2, 42].Value = item.QuantityOfBom_Mutil_QuantityPerBom39;
                        worksheet.Cells[i + 2, 43].Value = item.QuantityOfBom_Mutil_QuantityPerBom40;
                        worksheet.Cells[i + 2, 44].Value = item.QuantityOfBom_Mutil_QuantityPerBom41;
                        worksheet.Cells[i + 2, 45].Value = item.QuantityOfBom_Mutil_QuantityPerBom42;
                        worksheet.Cells[i + 2, 46].Value = item.QuantityOfBom_Mutil_QuantityPerBom43;
                        worksheet.Cells[i + 2, 47].Value = item.QuantityOfBom_Mutil_QuantityPerBom44;
                        worksheet.Cells[i + 2, 48].Value = item.QuantityOfBom_Mutil_QuantityPerBom45;
                        worksheet.Cells[i + 2, 49].Value = item.QuantityOfBom_Mutil_QuantityPerBom46;
                        worksheet.Cells[i + 2, 50].Value = item.QuantityOfBom_Mutil_QuantityPerBom47;
                        worksheet.Cells[i + 2, 51].Value = item.QuantityOfBom_Mutil_QuantityPerBom48;
                        worksheet.Cells[i + 2, 52].Value = item.QuantityOfBom_Mutil_QuantityPerBom49;
                        worksheet.Cells[i + 2, 53].Value = item.QuantityOfBom_Mutil_QuantityPerBom50;
                        //worksheet.Cells[i + 2, note].Value = "";
                        worksheet.Cells[i + 2, 54].Value = string.Join("\n", errMessage);

                        using (var errorRange = worksheet.Cells[i + 2, 54, i + 2, 54])
                        {
                            errorRange.Style.Font.Color.SetColor(Color.Red);
                            errorRange.Style.Fill.PatternType = ExcelFillStyle.None;
                        }
                    }

                    // Lưu file Excel
                    package.SaveAs(stream);
                }
            }

            //Sau khi đã tổng hợp các bản ghi từ file excel và đánh dấu các lỗi validate vào từng item
            //Với những item hợp lệ:
            //1.Cập nhật DocOutputs, Quantity trong bảng InventoryDocs và thêm mới lịch sử DocHistory and HistoryOutputs
            //2. Nếu có phiếu C thì cập nhật chi tiết bảng DocTypeCDetails và chi tiết lịch sử HistoryDocTypeCDetails

            var validItems = items.Where(x => x.Error.IsValid).ToList();

            var userInfo = BIVN.FixedStorage.Services.Common.API.Utilities.UserFromContext(_httpContext);
            var strategy = _inventoryContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var dbcontext = await _dbContextFactory.CreateDbContextAsync();
                using var transaction = await dbcontext.Database.BeginTransactionAsync();
                try
                {
                    var listInvDocToUpdate = new List<InventoryDoc>();
                    var listInvDocQntChangeDto = new List<InventoryDocQuantityChangeDto>();
                    var docOutputUpdateQuantityDtos = new List<ListDocOutputUpdateQuantityDto>();
                    foreach (var item in validItems)
                    {
                        var getInventoryDoc = await dbcontext.InventoryDocs.FirstOrDefaultAsync(x => (x.InventoryId.HasValue && x.InventoryId == inventoryId) && x.DocCode.ToLower() == item.DocCode.ToLower());
                        if (getInventoryDoc == null)
                        {
                            continue;
                        }
                        //Cập nhật trường Investigator, ReasonInvestigator, InvestigateTime:
                        getInventoryDoc.ReasonInvestigator = item.Note;
                        getInventoryDoc.InvestigateTime = DateTime.Now;
                        getInventoryDoc.Investigator = currUser.Username;


                        // Xóa hết dữ liệu DocOutputs trước đó:
                        var getDocOutputs = await dbcontext.DocOutputs.Where(x => x.InventoryDocId == getInventoryDoc.Id).ToListAsync();
                        dbcontext.RemoveRange(getDocOutputs);

                        //Thêm mới dữ liệu vào DocOutputs:
                        List<string> listQuantityOfBom_Mutil_QuantityPerBom = new();
                        AddQuantitiesToList(item, listQuantityOfBom_Mutil_QuantityPerBom);

                        ListDocOutputUpdateQuantityDto listDocOutputUpdateQuantityDto = new ListDocOutputUpdateQuantityDto(getInventoryDoc.Id);

                        List<DocOutput> docs = new();
                        double oldQuantityInventoryDocSum = 0, newQuantityInventoryDocSum = 0, quantitySum = 0;

                        // Lấy số lượng cũ, số lượng mới để ghi change log lịch sử
                        oldQuantityInventoryDocSum = getInventoryDoc.Quantity;
                        foreach (var quantity in listQuantityOfBom_Mutil_QuantityPerBom)
                        {
                            var quantityCut = quantity.Split("*");

                            DocOutput_UpdateQuantity docOutput_UpdateQuantity = new DocOutput_UpdateQuantity();
                            docOutput_UpdateQuantity.QuantityPerBom = double.Parse(quantityCut[0]);
                            docOutput_UpdateQuantity.QuantityOfBom = double.Parse(quantityCut[1]);
                            listDocOutputUpdateQuantityDto.DocOutputs.Add(docOutput_UpdateQuantity);
                            docOutputUpdateQuantityDtos.Add(listDocOutputUpdateQuantityDto);
                            docs.Add(new DocOutput
                            {
                                Id = Guid.NewGuid(),
                                InventoryDocId = getInventoryDoc.Id,
                                InventoryId = getInventoryDoc.InventoryId.Value,
                                QuantityPerBom = double.Parse(quantityCut[0]),
                                QuantityOfBom = double.Parse(quantityCut[1]),
                                CreatedAt = DateTime.Now,
                                CreatedBy = userInfo.UserCode
                            });

                            quantitySum = double.Parse(quantityCut[0]) * double.Parse(quantityCut[1]);
                            newQuantityInventoryDocSum += quantitySum;
                        }
                        dbcontext.DocOutputs.AddRange(docs);

                        // Cập nhật lại số lượng:
                        getInventoryDoc.Quantity = newQuantityInventoryDocSum;

                        listInvDocQntChangeDto.Add(new InventoryDocQuantityChangeDto
                        {
                            InventoryDocId = getInventoryDoc.Id,
                            OldQuantity = oldQuantityInventoryDocSum,
                            NewQuantity = newQuantityInventoryDocSum
                        });


                        // Log success


                        listInvDocToUpdate.Add(getInventoryDoc);
                        //dbcontext.InventoryDocs.Update(getInventoryDoc);

                    }
                    dbcontext.InventoryDocs.UpdateRange(listInvDocToUpdate);
                    await dbcontext.SaveChangesAsync();

                    foreach (var getInventoryDoc in listInvDocToUpdate)
                    {


                        // Lưu Lịch sử DocHistory:
                        var getOldDocHistory = await dbcontext.DocHistories.Where(x => x.InventoryDocId == getInventoryDoc.Id && x.InventoryId == getInventoryDoc.InventoryId.Value).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                        //Lưu Lịch sử DocHistory:

                        var quantityChange = listInvDocQntChangeDto.FirstOrDefault(x => x.InventoryDocId == getInventoryDoc.Id);
                        var docOutputUpdateQuantityDto = docOutputUpdateQuantityDtos.FirstOrDefault(x => x.InventoryDocId == getInventoryDoc.Id);
                        var newDocHistory = new DocHistory
                        {
                            Id = Guid.NewGuid(),
                            InventoryId = getInventoryDoc.InventoryId.Value,
                            InventoryDocId = getInventoryDoc.Id,
                            Action = getOldDocHistory == null ? DocHistoryActionType.Inventory : getOldDocHistory.Action,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userInfo.UserCode,
                            OldQuantity = quantityChange.OldQuantity,
                            NewQuantity = quantityChange.NewQuantity,
                            OldStatus = getOldDocHistory == null ? InventoryDocStatus.NotInventoryYet : getOldDocHistory.OldStatus,
                            NewStatus = getOldDocHistory == null ? InventoryDocStatus.NotInventoryYet : getOldDocHistory.NewStatus,
                            Status = getOldDocHistory == null ? InventoryDocStatus.NotInventoryYet : getOldDocHistory.Status,
                            IsChangeCDetail = getOldDocHistory == null ? false : getOldDocHistory.IsChangeCDetail,
                            EvicenceImg = getOldDocHistory == null ? string.Empty : getOldDocHistory.EvicenceImg,
                        };

                        dbcontext.DocHistories.Add(newDocHistory);

                        // Lưu HistoryOutput:
                        if (docOutputUpdateQuantityDto != null && docOutputUpdateQuantityDto.DocOutputs.Any())
                        {
                            List<HistoryOutput> hisOuts = new();
                            foreach (var model in docOutputUpdateQuantityDto.DocOutputs)
                            {
                                hisOuts.Add(new HistoryOutput
                                {
                                    Id = Guid.NewGuid(),
                                    DocHistoryId = newDocHistory.Id,
                                    InventoryId = getInventoryDoc.InventoryId.Value,
                                    QuantityOfBom = model.QuantityOfBom,
                                    QuantityPerBom = model.QuantityPerBom,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = userInfo.UserCode
                                });
                            }
                            dbcontext.HistoryOutputs.AddRange(hisOuts);
                        }
                        //Nếu có phiếu C, Cập nhật lại DocTypeCDetails và lưu lại lịch sử HistoryTypeCDetail:
                        if (getInventoryDoc.DocType == InventoryDocType.C)
                        {
                            var getDocTypeCDetail = await dbcontext.DocTypeCDetails.Where(x => x.InventoryDocId == getInventoryDoc.Id
                                              && x.InventoryId == getInventoryDoc.InventoryId.Value).ToListAsync();

                            //Lưu lịch sử historyTypeCDetail:
                            List<HistoryTypeCDetail> hisTypeC = new();

                            foreach (var docTypeC in getDocTypeCDetail)
                            {
                                //Cập nhật DocTypeCDetail:
                                docTypeC.QuantityPerBom = quantityChange.NewQuantity * docTypeC.QuantityOfBom;
                                docTypeC.UpdatedAt = DateTime.Now;
                                docTypeC.UpdatedBy = userInfo.UserCode;

                                hisTypeC.Add(new HistoryTypeCDetail
                                {
                                    Id = Guid.NewGuid(),
                                    HistoryId = newDocHistory.Id,
                                    InventoryId = getInventoryDoc.InventoryId.Value,
                                    ComponentCode = docTypeC.ComponentCode,
                                    ModelCode = docTypeC.ModelCode,
                                    QuantityOfBom = docTypeC.QuantityOfBom,
                                    QuantityPerBom = docTypeC.QuantityPerBom,
                                    IsHighlight = docTypeC.isHighlight,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = userInfo.UserCode
                                });
                            }
                            dbcontext.HistoryTypeCDetails.AddRange(hisTypeC);

                            await dbcontext.SaveChangesAsync();

                            //Call Backgroud Job để tổng hợp lại số lượng đối với phiếu C:
                            var updateDocTotalParam = new InventoryDocSubmitDto();
                            updateDocTotalParam.InventoryId = getInventoryDoc.InventoryId.Value;
                            updateDocTotalParam.InventoryDocIds = getDocTypeCDetail.Select(x => x.InventoryDocId.Value).ToList();
                            updateDocTotalParam.ModelCodes = getDocTypeCDetail.Select(x => x.ModelCode).ToList();
                            updateDocTotalParam.DocType = InventoryDocType.C;
                            await _dataAggregationService.UpdateDataFromInventoryDoc(updateDocTotalParam);
                        }
                        else
                        {
                            await _dataAggregationService.UpdateDataFromInventoryDoc(new InventoryDocSubmitDto { DocType = InventoryDocType.A, InventoryId = getInventoryDoc.InventoryId.Value });
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Có lỗi khi thực hiện cập nhật lại số lượng: ", ex);
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            var failCount = items.Where(x => x.Error.IsValid == false).Count();
            var successCount = items.Where(x => x.Error.IsValid == true).Count();

            var fileType = Constants.FileResponse.ExcelType;
            var fileName = string.Format("DulieuCapNhatSoLuong_Fileloi_{0}", DateTime.Now.ToString(Constants.DatetimeFormat));

            return new ImportResponseModel<byte[]>
            {
                Bytes = stream.ToArray(),
                Code = StatusCodes.Status200OK,
                FailCount = failCount,
                SuccessCount = successCount,
                FileType = fileType,
                FileName = fileName,
            };
        }

        private static void AddQuantitiesToList(ImportUpdateQuantityExcelValueModel item, List<string> listQuantityOfBom_Mutil_QuantityPerBom)
        {
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom01))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom01);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom02))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom02);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom03))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom03);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom04))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom04);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom05))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom05);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom06))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom06);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom07))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom07);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom08))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom08);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom09))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom09);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom10))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom10);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom11))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom11);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom12))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom12);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom13))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom13);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom14))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom14);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom15))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom15);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom16))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom16);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom17))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom17);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom18))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom18);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom19))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom19);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom20))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom20);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom21))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom21);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom22))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom22);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom23))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom23);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom24))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom24);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom25))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom25);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom26))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom26);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom27))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom27);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom28))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom28);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom29))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom29);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom30))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom30);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom31))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom31);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom32))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom32);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom33))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom33);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom34))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom34);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom35))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom35);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom36))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom36);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom37))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom37);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom38))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom38);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom39))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom39);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom40))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom40);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom41))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom41);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom42))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom42);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom43))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom43);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom44))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom44);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom45))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom45);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom46))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom46);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom47))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom47);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom48))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom48);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom49))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom49);
            }
            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom50))
            {
                listQuantityOfBom_Mutil_QuantityPerBom.Add(item.QuantityOfBom_Mutil_QuantityPerBom50);
            }
        }

        private bool ValidateDocCodeUpdateQuantity(ImportUpdateQuantityExcelValueModel item, Guid inventoryId)
        {
            //Validate MaterialCode required:
            if (string.IsNullOrEmpty(item.DocCode))
            {
                item.Error.TryAddModelError(nameof(item.DocCode), "Vui lòng nhập mã phiếu.");
                return false;
            }

            //Từ DocCode check xem có phiếu hay không:
            var getInventoryDocByDocCode = _inventoryContext.InventoryDocs.Any(x => (x.InventoryId.HasValue && x.InventoryId == inventoryId) && x.DocCode.ToLower() == item.DocCode.ToLower());
            if (!getInventoryDocByDocCode)
            {
                item.Error.TryAddModelError(nameof(item.DocCode), $"Phiếu có mã phiếu {item.DocCode} không tồn tại trong hệ thống.");
                return false;
            }

            //Nếu phiếu ở trạng thái không kiểm kê => Bắn lỗi: Không thể cập nhật số lượng với trạng thái phiếu không kiểm kê.
            var getInventoryDocStatus = _inventoryContext.InventoryDocs.Any(x => (x.InventoryId.HasValue && x.InventoryId == inventoryId) && x.DocCode.ToLower() == item.DocCode.ToLower() && x.Status == InventoryDocStatus.NoInventory);
            if (getInventoryDocStatus)
            {
                item.Error.TryAddModelError(nameof(item.DocCode), $"Không thể cập nhật số lượng với trạng thái phiếu không kiểm kê.");
                return false;
            }
            return true;
        }

        private bool ValidateQuantityOfBom_Mutil_QuantityPerBom(ImportUpdateQuantityExcelValueModel item)
        {
            var QuantityOfBom_Mutil_QuantityPerBomPattern = new Regex(Constants.RegexPattern.QuantityOfBom_Mutil_QuantityPerBomRegex);
            var errorIndexColumns = new List<int>();

            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom01) && !QuantityOfBom_Mutil_QuantityPerBomPattern.IsMatch(item.QuantityOfBom_Mutil_QuantityPerBom01))
            {
                errorIndexColumns.Add(1);
            }

            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom02) && !QuantityOfBom_Mutil_QuantityPerBomPattern.IsMatch(item.QuantityOfBom_Mutil_QuantityPerBom02))
            {
                errorIndexColumns.Add(2);
            }

            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom03) && !QuantityOfBom_Mutil_QuantityPerBomPattern.IsMatch(item.QuantityOfBom_Mutil_QuantityPerBom03))
            {
                errorIndexColumns.Add(3);
            }

            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom04) && !QuantityOfBom_Mutil_QuantityPerBomPattern.IsMatch(item.QuantityOfBom_Mutil_QuantityPerBom04))
            {
                errorIndexColumns.Add(4);
            }

            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom05) && !QuantityOfBom_Mutil_QuantityPerBomPattern.IsMatch(item.QuantityOfBom_Mutil_QuantityPerBom05))
            {
                errorIndexColumns.Add(5);
            }

            if (!string.IsNullOrEmpty(item.QuantityOfBom_Mutil_QuantityPerBom06) && !QuantityOfBom_Mutil_QuantityPerBomPattern.IsMatch(item.QuantityOfBom_Mutil_QuantityPerBom06))
            {
                errorIndexColumns.Add(6);
            }

            if (errorIndexColumns.Any())
            {
                var message = string.Join(",", errorIndexColumns.Select(x => $"({x})"));
                string summary = $"Số lượng thùng x số thùng vị trí {message} không đúng định dạng.";
                item.Error.TryAddModelError(nameof(item.QuantityOfBom_Mutil_QuantityPerBom01), summary);
                return false;
            }
            return true;
        }

        public async Task<ResponseModel<TreeGroupQRCodeFilterDto>> GetTreeGroupQRCodeFilters()
        {
            var modelRegex = new Regex(Constants.RegexPattern.ModelCodeRegex);
            var modelCodes = await _inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.DocType == InventoryDocType.C && x.IsDeleted != true)
                .Select(x => x.ModelCode).Distinct().ToListAsync();
            if (modelCodes?.Count > 0)
            {
                var machineModels = modelCodes.Where(x => modelRegex.IsMatch(x)).Select(x => modelRegex.Match(x).Groups[1].Value).Distinct().OrderBy(x => x).ToList();
                var machineTypes = modelCodes.Where(x => modelRegex.IsMatch(x)).Select(x => modelRegex.Match(x).Groups[2].Value).Distinct().ToList();
                var lineNames = modelCodes.Where(x => modelRegex.IsMatch(x)).Select(x => modelRegex.Match(x).Groups[3].Value).Distinct().ToList();

                var result = new ResponseModel<TreeGroupQRCodeFilterDto>
                {
                    Code = StatusCodes.Status200OK,
                    Data = new TreeGroupQRCodeFilterDto
                    {
                        MachineModels = machineModels,
                        MachineTypes = machineTypes,
                        LineNames = lineNames
                    }
                };
                return result;
            }
            return new ResponseModel<TreeGroupQRCodeFilterDto>
            {
                Code = StatusCodes.Status204NoContent,
                Data = null
            };
        }

        public async Task<ResponseModel<TreeGroupInventoryErrorFilterDto>> GetTreeGroupInventoryErrorFilters()
        {
            var plants = await _inventoryContext.InventoryDocs.AsNoTracking().Select(x => x.Plant).Distinct().ToListAsync();
            var assigneeAccounts = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                    .Where(x => x.RoleType == InventoryAccountRoleType.Inventory)
                                    .Select(x => new AssigneeAccount
                                    {
                                        AssigneeAccountId = x.UserId,
                                        UserName = x.UserName
                                    }).ToListAsync();
            var result = new TreeGroupInventoryErrorFilterDto
            {
                Plants = plants,
                AssigneeAccounts = assigneeAccounts
            };

            return new ResponseModel<TreeGroupInventoryErrorFilterDto>
            {
                Code = StatusCodes.Status200OK,
                Data = result
            };
        }

        private async Task<string> SyntheticData(Guid inventoryId, string userId)
        {
            var listComponent = new List<DocTypeCComponent>();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                {
                    //delete existed DocTypeCComponents
                    await inventoryContext.DocTypeCComponents.Where(x => x.InventoryId == inventoryId).ExecuteDeleteAsync();

                    //get all DocTypeCDetails
                    var docTypeCDetails = await inventoryContext.DocTypeCDetails.AsNoTracking().Where(x => x.InventoryId == inventoryId).OrderBy(x => x.ComponentCode).ToListAsync();

                    //get all Model of Unit
                    var unitModels = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.C && x.LineName == "0" && x.IsDeleted != true)
                        .Select(x => new ModelOfUnit
                        {
                            InventoryId = inventoryId,
                            InventoryDocId = x.Id,
                            ModelCode = x.ModelCode,
                            Plant = x.Plant,
                            WareHouseLocation = x.WareHouseLocation,
                            MachineModel = x.MachineModel,
                            MachineType = x.MachineType,
                            StageName = x.StageName

                        }).OrderBy(x => x.MachineModel).ThenBy(x => x.MachineType).ThenBy(x => x.StageName).ToListAsync();

                    //generate DocTypeCComponents for Model of Unit
                    var unitResult = GenerateDocTypeCDocments(userId, ref listComponent, docTypeCDetails, unitModels);


                    var listHontais = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.C && x.LineName != "0" && x.IsDeleted == null)
                        .Select(x => new ModelOfUnit
                        {
                            InventoryId = inventoryId,
                            InventoryDocId = x.Id,
                            ModelCode = x.ModelCode,
                            Plant = x.Plant,
                            WareHouseLocation = x.WareHouseLocation,
                            MachineModel = x.MachineModel,
                            MachineType = x.MachineType,
                            LineName = x.LineName,
                            LineType = x.LineType,
                            StageName = x.StageName,

                        }).OrderBy(x => x.MachineModel).ThenBy(x => x.MachineType).ThenBy(x => x.LineName).ThenBy(x => x.LineType).ThenBy(x => x.StageName).ToListAsync();


                    //generate DocTypeCComponents for Hontais
                    var hontaiResult = GenerateDocTypeCDocments(userId, ref listComponent, docTypeCDetails, listHontais);

                    if (unitResult.Any() || hontaiResult.Any())
                    {
                        return $"{string.Join(", ", unitResult.Concat(hontaiResult))}";
                    }

                    await inventoryContext.DocTypeCComponents.AddRangeAsync(listComponent);
                    await inventoryContext.SaveChangesAsync();

                    return string.Empty;
                }
            }




        }

        private List<string> GenerateDocTypeCDocments(string userId, ref List<DocTypeCComponent> listComponent, List<DocTypeCDetail> docTypeCDetails, List<ModelOfUnit> unitModels)
        {
            var tempList = unitModels;
            while (tempList.Any())
            {
                var nextTempList = new List<ModelOfUnit>();
                foreach (var model in tempList)
                {
                    var (code, components) = GenerateDocTypeCDocument(model, docTypeCDetails, listComponent, userId);
                    if (code == StatusCodes.Status500InternalServerError)
                    {
                        nextTempList.Add(model);
                    }
                    else
                    {

                        listComponent = listComponent.Concat(components).ToList();
                    }
                }
                if (nextTempList.SequenceEqual(tempList))
                {
                    return nextTempList.Select(x => x.ModelCode).ToList();
                }
                tempList = nextTempList;

            }
            return new();
        }

        private (int code, List<DocTypeCComponent> components) GenerateDocTypeCDocument(ModelOfUnit modelOfUnit, List<DocTypeCDetail> docTypeCDetails, List<DocTypeCComponent> docTypeCComponents, string userId)
        {
            var componets = new List<DocTypeCComponent>();
            var details = docTypeCDetails.Where(x => x.InventoryDocId == modelOfUnit.InventoryDocId).ToList();
            if (!details.Any())
            {
                return (StatusCodes.Status200OK, componets);
            }
            foreach (var detail in details)
            {
                if (!string.IsNullOrEmpty(detail.ModelCode))
                {
                    var listExtracted = docTypeCComponents.Where(x => x.MainModelCode == detail.ModelCode).ToList();
                    if (!listExtracted.Any())
                    {
                        return (StatusCodes.Status500InternalServerError, new List<DocTypeCComponent>());
                    }
                    else
                    {
                        foreach (var extract in listExtracted)
                        {
                            var component = new DocTypeCComponent
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.Now,
                                CreatedBy = userId,
                                MainModelCode = modelOfUnit.ModelCode,
                                Plant = modelOfUnit.Plant,
                                WarehouseLocation = modelOfUnit.WareHouseLocation,
                                UnitModelCode = detail.ModelCode,
                                ComponentCode = extract.ComponentCode,
                                QuantityOfBOM = extract.SyntheticQuantity,
                                QuantityPerBOM = detail.QuantityPerBom,
                                TotalQuantity = extract.SyntheticQuantity * detail.QuantityPerBom,
                                SyntheticQuantity = detail.QuantityOfBom * extract.SyntheticQuantity,
                                InventoryDocId = modelOfUnit.InventoryDocId,
                                InventoryId = modelOfUnit.InventoryId
                            };
                            componets.Add(component);
                        }
                    }
                }
                else
                {
                    var component = new DocTypeCComponent
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                        MainModelCode = modelOfUnit.ModelCode,
                        Plant = modelOfUnit.Plant,
                        WarehouseLocation = modelOfUnit.WareHouseLocation,
                        UnitModelCode = null,
                        ComponentCode = detail.ComponentCode,
                        QuantityOfBOM = 1,
                        QuantityPerBOM = detail.QuantityPerBom,
                        TotalQuantity = detail.QuantityPerBom * 1,
                        SyntheticQuantity = detail.QuantityOfBom,
                        InventoryDocId = modelOfUnit.InventoryDocId,
                        InventoryId = modelOfUnit.InventoryId
                    };
                    componets.Add(component);
                }
            }
            return (StatusCodes.Status200OK, componets);
        }

        private class ModelOfUnit
        {
            public Guid InventoryDocId { get; set; }
            public string ModelCode { get; set; }
            public string Plant { get; set; }
            public string WareHouseLocation { get; set; }
            public string MachineModel { get; set; }
            public string MachineType { get; set; }
            public string StageName { get; set; }
            public Guid InventoryId { get; internal set; }
            public string LineName { get; internal set; }
            public string LineType { get; internal set; }
        }

    }
}
