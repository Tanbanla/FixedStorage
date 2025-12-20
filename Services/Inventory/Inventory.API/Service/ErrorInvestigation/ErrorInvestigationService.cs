
using System;
using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums;
using BIVN.FixedStorage.Services.Inventory.API.Service.Dto.ErrorInvestigation;
using Dapper;
using Inventory.API.Service.Dto.ErrorInvestigation;
using static Azure.Core.HttpHeader;

namespace Inventory.API.Service.ErrorInvestigation
{
    public partial class ErrorInvestigationService : IErrorInvestigationService
    {
        private readonly ILogger<ErrorInvestigationService> _logger;
        private readonly IRestClient _resClient;
        private readonly HttpContext _httpContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RestClientFactory _restClientFactory;
        //private readonly IDatabaseFactoryService _databaseFactory;
        private readonly IConfiguration _configuration;
        private readonly InventoryContext _inventoryContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDataAggregationService _dataAggregationService;
        private readonly IMemoryCache _memoryCache;
        private readonly IInventoryWebService _inventoryService;

        private readonly RestClient _identityRestClient;

        private InventoryDocStatus[] ExcludeDocStatus = new InventoryDocStatus[]
        {
            InventoryDocStatus.NotReceiveYet,
            InventoryDocStatus.NoInventory
        };
        public ErrorInvestigationService(InventoryContext inventoryContext,
                                         IHttpContextAccessor httpContextAccessor,
                                         IRestClient restClient,
                                         IConfiguration configuration,
                                         ILogger<ErrorInvestigationService> logger,
                                         IServiceScopeFactory serviceProviderFactory,
                                         IWebHostEnvironment webHostEnvironment,
                                         RestClientFactory restClientFactory,
                                         IDataAggregationService dataAggregationService,
                                         IInventoryWebService inventoryService
                                        )
        {
            _inventoryContext = inventoryContext;
            _logger = logger;
            _configuration = configuration;
            _resClient = restClient;
            _serviceScopeFactory = serviceProviderFactory;
            _httpContext = httpContextAccessor.HttpContext;
            _webHostEnvironment = webHostEnvironment;
            _restClientFactory = restClientFactory;
            _identityRestClient = _restClientFactory.IdentityClient();
            _dataAggregationService = dataAggregationService;
            _inventoryService = inventoryService;
        }

        public async Task<ResponseModel> CheckValidInventoryRole(Guid accountId, params int[] accessTos)
        {
            var account = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                                   .FirstOrDefaultAsync(x => x.UserId == accountId);

            if (account == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = commonAPIConstant.ResponseMessages.UnAuthorized
                };
            }

            var roleType = (int)account.RoleType;
            if (accessTos.Contains(roleType))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK
                };
            }
            else
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = commonAPIConstant.ResponseMessages.UnAuthorized
                };
            }
        }

        public async Task<ResponseModel> CheckValidInventoryDate(Guid inventoryId)
        {
            var inventory = await _inventoryContext.Inventories.AsNoTracking()
                                                               .FirstOrDefaultAsync(x => x.Id == inventoryId);
            if (inventory == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy đợt kiểm kê."
                };
            }

            var currentDate = DateTime.Now.Date;
            if (currentDate <= inventory.InventoryDate.Date)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.CannotErrorInvestigation,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.CannotErrorInvestigation)
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK
            };
        }

        public async Task<ResponseModel<IEnumerable<ErrorInvestigationListDto>>> ErrorInvestigationList(Guid inventoryId, ErrorInvestigationStatusType? status, string componentCode,int pageSize = 20, int pageNum = 1)
        {
            var user = _httpContext.UserFromContext();
            var userId = Guid.Parse(user.UserId);
            var isAdminRole = user.RoleName;

            string assignedAccountCondition = isAdminRole == Constants.DefaultAccount.RoleName
                ? ""
                : "AND doc.AssignedAccount = @UserId";

            //Lấy ra danh sách linh kiện điều tra sai số:
            var sqlQuery = $@"
                            SELECT 
                                ei.Id AS ErrorInvestigationId,
                                ei.ComponentCode,
                                ei.ComponentName,
                                doc.PositionCode,
                                ROUND(doc.ErrorQuantity, 3) AS Quantity,
                                ei.Status,
                                ROUND(ABS(doc.ErrorQuantity * doc.UnitPrice), 3) AS ErrorMoneyAbs
                            FROM ErrorInvestigations ei
                            LEFT JOIN ErrorInvestigationInventoryDocs doc
                                ON ei.Id = doc.ErrorInvestigationId
                            WHERE ei.InventoryId = @InventoryId 
                                AND doc.ErrorQuantity != 0
                                AND doc.DocType = @DocType
                                {assignedAccountCondition}
                            ORDER BY ABS(doc.ErrorMoney) DESC";

            var parameters = new
            {
                InventoryId = inventoryId,
                DocType = (int)InventoryDocType.A,
                UserId = userId
            };
            using var connection = _inventoryContext.Database.GetDbConnection();
            var errorInvestigationListQuery = (await connection.QueryAsync<ErrorInvestigationListDto>(sqlQuery, parameters)).AsQueryable();

            if (errorInvestigationListQuery == null || !errorInvestigationListQuery.Any())
            {
                return new ResponseModel<IEnumerable<ErrorInvestigationListDto>>
                {
                    Code = (int)HttpStatusCodes.ErrorInvestigationNotFound,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorInvestigationNotFound)
                };
            }

            var predictBuilder = PredicateBuilder.New<ErrorInvestigationListDto>(true);
            //Tim kiem theo ma linh kien:
            if (!string.IsNullOrEmpty(componentCode))
            {
                predictBuilder = predictBuilder.And(x => x.ComponentCode.Contains(componentCode));
            }
            //Tim kiem theo status:
            if (status.HasValue)
            {
                predictBuilder = predictBuilder.And(x => x.Status == status.Value);
            }

            var errorInvestigationList = errorInvestigationListQuery.Where(predictBuilder);
            var result = errorInvestigationList
                                    .Skip((pageNum - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            return new ResponseModel<IEnumerable<ErrorInvestigationListDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = result
            };
        }

        public async Task<ResponseModel> UpdateStatusErrorInvestigation(Guid inventoryId, string componentCode)
        {
            var errorInvestigation = await _inventoryContext.ErrorInvestigations.FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode);
            if(errorInvestigation == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.ErrorInvestigationNotFound
                };
            }
            if(errorInvestigation.Status == ErrorInvestigationStatusType.Investigated)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ErrorQuantityStatusInvestigated,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorQuantityStatusInvestigated)
                };
            }

            errorInvestigation.Status = ErrorInvestigationStatusType.NotYetInvestigated;
            errorInvestigation.InvestigatorId = null;
            errorInvestigation.UpdatedBy = null;
            errorInvestigation.UpdatedAt = null;
            _inventoryContext.ErrorInvestigations.Update(errorInvestigation);

            await _inventoryContext.SaveChangesAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = Constants.ResponseMessages.UpdateStatusErrorInvestigationSuccessfully
            };
        }

        public string UploadImage(IFormFile File, string InventoryName, string ComponentCode)
        {
            string filePath = null;
            if (File != null)
            {
                try
                {
                    var path = InitTypeDocumentPath(InitRootPath(), Path.Combine("images", "ErrorInvestigation"));
                    var userDirectoryPath = InitTypeDocumentPath(path, InventoryName);

                    var uniqueTicks = DateTime.UtcNow.Ticks;
                    var newFile = $"{ComponentCode}_{DateTime.Now.ToString(Constants.DatetimeFormat)}_{uniqueTicks}{Path.GetExtension(File.FileName)}";

                    filePath = Path.Combine(userDirectoryPath, newFile);
                    _logger.LogError($"Lưu ảnh filepath: {filePath}");
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        File.CopyTo(stream);
                        stream.Flush();
                    }

                    filePath = Path.Combine("images", "ErrorInvestigation", InventoryName, newFile);
                    _logger.LogError($"Lưu ảnh fullpath: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Lỗi khi thực hiện lưu ảnh");
                    _logger.LogError(filePath);
                    _logger.LogError(ex.Message);
                    return filePath;
                }

            }
            return filePath;
        }

        public string InitRootPath()
        {
            var rootFolder = _configuration["UploadPath"];
            return rootFolder;
        }

        public string InitTypeDocumentPath(string previousPath, string typeDocument)
        {
            var path = Path.Combine(previousPath, typeDocument);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public async Task<ResponseModel> ConfirmErrorInvestigation(Guid inventoryId, string componentCode, ErrorInvestigationConfirmType type, ErrorInvestigationConfirmModel model)
        {
            //Check Không được điều chỉnh số lượng cùng dấu với số lượng sai số:
            var getCurrentErrorQuantity = await _inventoryContext.ErrorInvestigations.AsNoTracking()
                                        .Include(x => x.ErrorInvestigationInventoryDocs)
                                        .Where(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode && x.ErrorInvestigationInventoryDocs.Any(doc => doc.ErrorQuantity != 0 && doc.DocType == InventoryDocType.A))
                                        .Select(x => x.ErrorInvestigationInventoryDocs.Where(doc => doc.DocType == InventoryDocType.A).OrderByDescending(doc => Math.Abs(doc.ErrorQuantity ?? 0)).Select(doc => doc.ErrorQuantity).FirstOrDefault())
                                        .FirstOrDefaultAsync();

            if (getCurrentErrorQuantity.HasValue)
            {
                // Kiểm tra nếu cả hai có cùng dấu (cả đều âm hoặc đều dương)
                if ((getCurrentErrorQuantity < 0 && model.Quantity < 0) || (getCurrentErrorQuantity > 0 && model.Quantity > 0))
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.ErrorQuantityTheSameSignErrorInvestigation,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorQuantityTheSameSignErrorInvestigation)
                    };
                }

                // Số lượng điều chỉnh không được lớn hơn số lượng chênh lệch:
                if(Math.Abs(model.Quantity) > Math.Abs((double)getCurrentErrorQuantity))
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.AdjustmentQuantityCannotGreaterThanErrorQuantity,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.AdjustmentQuantityCannotGreaterThanErrorQuantity)
                    };
                }
            }

            //Update Status Error Investigation:
            var errorInvestigation = await _inventoryContext.ErrorInvestigations.FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode);
            if (errorInvestigation == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.ErrorInvestigationNotFound
                };
            }

            if(type == ErrorInvestigationConfirmType.ErrorInvestigationConfirm)
            {
                errorInvestigation.Status = ErrorInvestigationStatusType.Investigated;
            }
            //errorInvestigation.InvestigatorId = _httpContext.UserFromContext().InventoryLoggedInfo.AccountId;
            //errorInvestigation.UpdatedBy = (_httpContext.UserFromContext().InventoryLoggedInfo.AccountId).ToString();
            //errorInvestigation.UpdatedAt = DateTime.Now;
            errorInvestigation.CurrentInvestigatorId = Guid.Parse(_httpContext.UserFromContext()?.UserId);
            errorInvestigation.InvestigatorId = Guid.Parse(_httpContext.UserFromContext()?.UserId);
            errorInvestigation.InvestigatorUserCode = model.UserCode ?? _httpContext.UserFromContext().UserCode;
            _inventoryContext.ErrorInvestigations.Update(errorInvestigation);

            //Update ErrorQuantity in ErrorInvestigationDocs:
            var errorInvestigationDocs = await _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking().OrderByDescending(x => x.ErrorQuantity).FirstOrDefaultAsync(x => x.ErrorInvestigationId == errorInvestigation.Id && x.DocType == InventoryDocType.A);
            if (errorInvestigationDocs == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.ErrorInvestigationDocTypeANotFound
                };
            }
            var oldErrorQuantity = errorInvestigationDocs.ErrorQuantity ?? 0;
            errorInvestigationDocs.ErrorQuantity = errorInvestigationDocs.ErrorQuantity.HasValue ? model.Quantity + errorInvestigationDocs.ErrorQuantity : model.Quantity;
            errorInvestigationDocs.ErrorMoney = errorInvestigationDocs.ErrorQuantity * errorInvestigationDocs.UnitPrice;
            _inventoryContext.ErrorInvestigationInventoryDocs.Update(errorInvestigationDocs);

            //Get ErrorInvestigationHistory Images:
            var errorInvestigationHistoryImages = await _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                    .Where(x => x.ErrorInvestigationId == errorInvestigation.Id)
                                                    .OrderByDescending(x => x.CreatedAt)
                                                    .Select(x => new { x.ConfirmationImage1, x.ConfirmationImage2 })
                                                    .FirstOrDefaultAsync();
            //Add new error investigation history:
            var inventoryName = _httpContext.UserFromContext().InventoryLoggedInfo.InventoryModel.Name;
            var errorInvestigationHistory = new ErrorInvestigationHistory
            {
                Id = Guid.NewGuid(),
                ErrorInvestigationId = errorInvestigation.Id,
                NewValue = errorInvestigationDocs.ErrorQuantity.Value,
                OldValue = oldErrorQuantity,
                ConfirmationTime = DateTime.Now,
                ErrorCategory = model.ErrorCategory,
                ComponentCode = componentCode,
                ErrorDetails = model.ErrorDetails,
                CreatedAt = DateTime.Now,
                CreatedBy = (_httpContext.UserFromContext().InventoryLoggedInfo.AccountId).ToString(),
                ConfirmationImage1 = (model.ConfirmationImage1 != null && model.ConfirmationImage1.Length > 0) ? UploadImage(model.ConfirmationImage1, inventoryName, componentCode) : (model.IsDeleteImage1 ? string.Empty : errorInvestigationHistoryImages?.ConfirmationImage1),
                ConfirmationImage2 = (model.ConfirmationImage2 != null && model.ConfirmationImage2.Length > 0) ? UploadImage(model.ConfirmationImage2, inventoryName, componentCode) : (model.IsDeleteImage2 ? string.Empty : errorInvestigationHistoryImages?.ConfirmationImage2),
                InvestigatorId = _httpContext.UserFromContext().InventoryLoggedInfo.AccountId,
                ComponentName = errorInvestigation.ComponentName ?? string.Empty,
                PositionCode = errorInvestigationDocs.PositionCode ?? string.Empty,
                ErrorType = ErrorType.Retain,
                InvestigatorUserCode = model.UserCode ?? _httpContext.UserFromContext().UserCode,
            };
            _inventoryContext.ErrorInvestigationHistories.Add(errorInvestigationHistory);

            await _inventoryContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = Constants.ResponseMessages.ConfirmInvestigationSuccessfully
            };
        }

        public async Task<ResponseModel<ErrorInvestigationDocumentListDto>> ErrorInvestigationDocumentList(string? userCode, Guid inventoryId, string componentCode)
        {
            var errorInvestigation = await _inventoryContext.ErrorInvestigations.FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode);
            if (errorInvestigation == null)
            {
                return new ResponseModel<ErrorInvestigationDocumentListDto>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.ErrorInvestigationNotFound
                };
            }

            //CurrentUserCode:
            var currentUserCode = _httpContext.UserFromContext().UserCode;

            //Lấy ra danh sách người dùng:
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            //CurrentErrorInvestigationUserCode:
            //var currentErrorInvestigationUserCode = users?.Where(x => x.Id == errorInvestigation.CurrentInvestigatorId).Select(x => x.Code).FirstOrDefault();


            //if (errorInvestigation.Status == ErrorInvestigationStatusType.UnderInvestigation && 
            //    (currentUserCode != currentErrorInvestigationUserCode) && 
            //    (errorInvestigation.UpdatedAt.HasValue && (DateTime.Now - errorInvestigation.UpdatedAt.Value).TotalMinutes <= 30) )
            //{
            //    return new ResponseModel<ErrorInvestigationDocumentListDto>
            //    {
            //        Code = (int)HttpStatusCodes.ComponentErrorInvestigating,
            //        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentErrorInvestigating)
            //    };
            //}

            var currentInvestigating = Guid.Parse(_httpContext.UserFromContext()?.UserId);
            if (errorInvestigation.Status != ErrorInvestigationStatusType.Investigated)
            {
                errorInvestigation.Status = ErrorInvestigationStatusType.UnderInvestigation;
                
            }
            errorInvestigation.UpdatedBy = currentInvestigating.ToString();
            errorInvestigation.UpdatedAt = DateTime.Now;
            errorInvestigation.CurrentInvestigatorId = currentInvestigating;
            errorInvestigation.InvestigatorId = currentInvestigating;
            errorInvestigation.InvestigatorUserCode = userCode ?? _httpContext.UserFromContext().UserCode;
            _inventoryContext.ErrorInvestigations.Update(errorInvestigation);
            await _inventoryContext.SaveChangesAsync();

            var getErrorInvestigationDocuments = await _inventoryContext.ErrorInvestigations.AsNoTracking()
                                                        .Include(x => x.ErrorInvestigationInventoryDocs)
                                                        .Where(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode)
                                                        .Select(x => new ErrorInvestigationDocumentListDto
                                                        {
                                                            ComponentCode = x.ComponentCode,
                                                            ComponentName = x.ComponentName,
                                                            Status = x.Status,
                                                            Position = x.ErrorInvestigationInventoryDocs
                                                                            .Where(doc => doc.DocType == InventoryDocType.A)
                                                                            .Select(doc => doc.PositionCode)
                                                                            .FirstOrDefault(),
                                                            ErrorQuantity = Math.Round(x.ErrorInvestigationInventoryDocs
                                                                            .Where(doc => doc.DocType == InventoryDocType.A)
                                                                            .Select(doc => doc.ErrorQuantity)
                                                                            .FirstOrDefault() ?? 0.000, 3),
                                                            ErrorMonyAbs = Math.Round(x.ErrorInvestigationInventoryDocs
                                                                            .Where(doc => doc.DocType == InventoryDocType.A)
                                                                            .Select(doc => Math.Abs(doc.ErrorQuantity * doc.UnitPrice ?? 0))
                                                                            .FirstOrDefault(), 3),
                                                            DocumentList = x.ErrorInvestigationInventoryDocs
                                                                            .Select(doc => new DocumentList
                                                                            {
                                                                                DocId = doc.InventoryDocId,
                                                                                AccountQuantity = Math.Round(doc.AccountQuantity ?? 0, 3),
                                                                                DocCode = doc.DocCode,
                                                                                BOM = doc.BOM
                                                                            }).ToList()
                                                        }).FirstOrDefaultAsync();

            if (getErrorInvestigationDocuments != null && getErrorInvestigationDocuments.DocumentList.Any())
            {
                var docIds = getErrorInvestigationDocuments.DocumentList.Select(d => d.DocId).ToList();

                var inventoryDocs = await _inventoryContext.InventoryDocs.AsNoTracking()
                                            .Where(doc => docIds.Contains(doc.Id))
                                            .Select(doc => new { doc.Id, doc.Quantity })
                                            .ToListAsync();

                foreach (var document in getErrorInvestigationDocuments.DocumentList)
                {
                    var matchingDoc = inventoryDocs.FirstOrDefault(d => d.Id == document.DocId);
                    if (matchingDoc != null)
                    {
                        document.AccountQuantity = Math.Round(matchingDoc.Quantity, 3);
                    }
                }
            }
            return new ResponseModel<ErrorInvestigationDocumentListDto>
            {
                Code = StatusCodes.Status200OK,
                Data = getErrorInvestigationDocuments,
                Message = Constants.ResponseMessages.ErrorInvestigationDocumentListSuccessfully
            };
        }

        public async Task<ResponseModel> ErrorInvestigationConfirmedViewDetail(Guid inventoryId, string componentCode)
        {
            var getErrorInvestigation = await _inventoryContext.ErrorInvestigations.AsNoTracking()
                                                    .Where(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode)
                                                    .FirstOrDefaultAsync();
            //Check ErrorInvestigation not found:
            if (getErrorInvestigation == null)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ErrorInvestigationNotFound,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorInvestigationNotFound)
                };
            }
            //Check ErrorInvestigation Status
            if (getErrorInvestigation.Status != ErrorInvestigationStatusType.Investigated)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ErrorQuantityStatusDifferInvestigated,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorQuantityStatusDifferInvestigated),
                    Data = getErrorInvestigation.Status
                };
            }

            var errorInvestigationConfirmedViewDetail = await _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                            .Where(x => x.ErrorInvestigationId == getErrorInvestigation.Id)
                                                            .OrderByDescending(x => x.CreatedAt)
                                                            .Select(x => new ErrorInvestigationConfirmedViewDetailDto
                                                            {
                                                                Status = getErrorInvestigation.Status,
                                                                ErrorCategory = x.ErrorCategory,
                                                                ErrorDetails = x.ErrorDetails,
                                                                ConfirmationImage1 = x.ConfirmationImage1,
                                                                ConfirmationImageTitle1 = Path.GetFileName(x.ConfirmationImage1 ?? string.Empty),
                                                                ConfirmationImage2 = x.ConfirmationImage2,
                                                                ConfirmationImageTitle2 = Path.GetFileName(x.ConfirmationImage2 ?? string.Empty),
                                                                ErrorQuantity = Math.Round(x.NewValue - x.OldValue, 3)
                                                            }).FirstOrDefaultAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = errorInvestigationConfirmedViewDetail,
                Message = Constants.ResponseMessages.ErrorInvestigationConfirmedViewDetailSuccessfully
            };
        }

        public string GetUserName(Guid userId)
        {
            var userName = _inventoryContext.InventoryAccounts.AsNoTracking().FirstOrDefault(x => x.UserId == userId);
            return userName != null ? userName.UserName : string.Empty;
        }

        public async Task<ResponseModel> ErrorInvestigationHistories(Guid inventoryId, string componentCode)
        {
            var getErrorInvestigation = await _inventoryContext.ErrorInvestigations.AsNoTracking()
                                                        .Where(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode)
                                                        .FirstOrDefaultAsync();
            //Check ErrorInvestigation not found:
            if (getErrorInvestigation == null)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ErrorInvestigationNotFound,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorInvestigationNotFound)
                };
            }

            var errorInvestigationHistories = await _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                    .Where(x => x.ErrorInvestigationId == getErrorInvestigation.Id && !x.IsDelete)
                                                    .OrderByDescending(x => x.CreatedAt)
                                                    .ToListAsync();
            if(errorInvestigationHistories == null || !errorInvestigationHistories.Any())
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ErrorInvestigationHistoryNotFound,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorInvestigationHistoryNotFound)
                };
            }

            //Lấy ra danh sách người dùng:
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            //var investigatorName = string.IsNullOrEmpty(getErrorInvestigation.UpdatedBy) ? string.Empty : GetUserName(Guid.Parse(getErrorInvestigation.UpdatedBy));
            var investigationTime = getErrorInvestigation.UpdatedAt?.ToString("HH:mm dd/MM/yyyy") ?? string.Empty;

            var errorCategoryNames = await _inventoryContext.GeneralSettings.AsNoTracking()
                                                    .Where(x => x.Type == GeneralSettingType.ErrorCategory && x.InventoryId == inventoryId)
                                                    .Select(x => new { 
                                                        ErrorCategory = x.Key1,
                                                        ErrorCategoryName = x.Value1
                                                    }).ToListAsync();

            var result = errorInvestigationHistories
                                                    .Select((history, index) => new ErrorInvestigationHistoriesDto
                                                    {
                                                        OldValue = history.OldValue,
                                                        NewValue = history.NewValue,
                                                        ErrorCategory = history.ErrorCategory,
                                                        ErrorDetail = history.ErrorDetails,
                                                        Investigator = string.IsNullOrEmpty(history?.InvestigatorUserCode) ? users.FirstOrDefault(ia => ia.Id == history?.InvestigatorId)?.Code : history?.InvestigatorUserCode,
                                                        InvestigationTime = investigationTime,
                                                        ConfirmInvestigationTime = history.ConfirmationTime.ToString("HH:mm dd/MM/yyyy"),
                                                        ConfirmationImage1 = history.ConfirmationImage1,
                                                        ConfirmationImage2 = history.ConfirmationImage2,
                                                        ConfirmationImageTitle1 = Path.GetFileName(history.ConfirmationImage1 ?? string.Empty),
                                                        ConfirmationImageTitle2 = Path.GetFileName(history.ConfirmationImage2 ?? string.Empty),
                                                        Index = errorInvestigationHistories.Count - index,
                                                        ErrorCategoryName = errorCategoryNames.FirstOrDefault(x => x.ErrorCategory == history.ErrorCategory.ToString())?.ErrorCategoryName ?? string.Empty
                                                    }).ToList();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = result,
                Message = Constants.ResponseMessages.ErrorInvestigationHistoriesSuccessfully
            };
        }





    }
}
