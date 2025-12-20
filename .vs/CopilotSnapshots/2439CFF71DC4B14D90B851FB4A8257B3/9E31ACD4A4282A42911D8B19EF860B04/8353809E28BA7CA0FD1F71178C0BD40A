using System;
using BIVN.FixedStorage.Services.Common.API;
using LinqKit;

namespace Storage.API.Service
{
    public class HistoryService : IHistoryService
    {
        private readonly ILogger<HistoryService> _logger;
        private readonly StorageContext _storageContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRestClient _restClient;
        private readonly IConfiguration _configuration;

        public HistoryService(ILogger<HistoryService> logger,
                              StorageContext storageContext,
                              IHttpContextAccessor httpContextAccessor,
                              IRestClient restClient,
                              IConfiguration configuration
                            )
        {
            _logger = logger;
            _storageContext = storageContext;
            _httpContextAccessor = httpContextAccessor;
            _restClient = restClient;
            _configuration = configuration;
        }

        private async Task<IEnumerable<RoleClaimDto>> UserPermissions(string userId)
        {
            var req = new RestRequest(Constants.Endpoint.Internal.absolute + $"/users/roles/{userId}");
            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var result = await _restClient.ExecuteGetAsync(req);
            var convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<IEnumerable<RoleClaimDto>>>(result.Content, JsonDefaults.CamelCaseOtions);
            return convertedResult?.Data;
        }

        private async Task<IEnumerable<InternalUserDto>> GetInternalUsers()
        {
            var req = new RestRequest(Constants.Endpoint.Internal.absolute + "/users");
            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var result = await _restClient.ExecuteGetAsync(req);
            var convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<IEnumerable<InternalUserDto>>>(result.Content, JsonDefaults.CamelCaseOtions);
            return convertedResult.Data;
        }

        private async Task<IEnumerable<InternalDepartmentDto>> GetInternalDepartments()
        {
            var req = new RestRequest(Constants.Endpoint.Internal.absolute + "/departments");
            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var result = await _restClient.ExecuteGetAsync(req);
            var convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<IEnumerable<InternalDepartmentDto>>>(result.Content, JsonDefaults.CamelCaseOtions);
            return convertedResult.Data;
        }

        public async Task<ResponseModel<HistoryPagedList>> GetHistories(HistoryFilterModel historyFilterModel)
        {
            if (historyFilterModel == null)
            {
                return new ResponseModel<HistoryPagedList>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu không hợp lệ"
                };
            }

            var users = await GetInternalUsers();
            var userPermissions = await UserPermissions(historyFilterModel.UserId);
            //Lưu ý, bình thường danh sách departments sẽ lọc các phòng ban bị xóa
            //Nhưng riêng phần lịch sử, khi người dùng không chọn gì trên UI thì sẽ hiển thị cả những phòng ban đã bị xóa
            var departments = await GetInternalDepartments();

            var usersDictionary = users.ToDictionary(x => x.Id.ToLower());
            var departmentsDictionary = departments.ToDictionary(x => x.Id.ToLower(), x => x.Name);

            var query = (from h in _storageContext.PositionHistories.AsNoTracking()
                         let convertedCreateBy = h.CreatedBy.ToLower()
                         let convertedDepartmentId = h.DepartmentId.ToString().ToLower()
                         let user = usersDictionary.ContainsKey(convertedCreateBy) ? usersDictionary[convertedCreateBy] : null
                         let department = departmentsDictionary.ContainsKey(convertedDepartmentId) ? departmentsDictionary[convertedDepartmentId] : string.Empty

                         select new HistoryResultModel
                         {
                             Id = h.Id.ToString(),
                             CreateDate = h.CreatedAt,
                             ActivityType = h.PositionHistoryType.HasValue ? (int)h.PositionHistoryType.Value : -1,
                             ComponentCode = h.ComponentCode ?? string.Empty,
                             PositionCode = h.PositionCode ?? string.Empty,
                             Quantity = h.Quantity.HasValue ? h.Quantity.Value : 0,
                             InventoryNumber = h.InventoryNumber.HasValue ? h.InventoryNumber.Value : 0,
                             Note = string.IsNullOrEmpty(h.Note) ? string.Empty : h.Note,
                             Layout = h.Layout,
                             UserName = user != null ? user.Name : string.Empty,
                             UserCode = (!string.IsNullOrEmpty(h.EmployeeCode)) ? h.EmployeeCode : (user != null ? user.Code : string.Empty),
                             DepartmentName = department,

                             FactoryId = h.FactoryId.HasValue ? h.FactoryId.Value : default,
                             DepartmentId = h.DepartmentId,
                             CreateBy = h.CreatedBy.ToLower()
                         });

            IEnumerable<string> userViewDepartmentIds;
            //Ngoài quyền xem phòng ban và phòng ban được chọn, + thêm các phòng ban đã bị xóa mềm để hiển thị lịch sử
            if (historyFilterModel.isAllDepartments || historyFilterModel.Departments.Count == 0)
            {
                userViewDepartmentIds = userPermissions?.Where(x => x.ClaimType == Constants.Permissions.DEPARTMENT_DATA_INQUIRY)
                                                        .Select(x => x.ClaimValue.ToLower());

                var deletedDepartment = departments.Where(x => x.IsDeleted.HasValue && x.IsDeleted.Value == true).Select(x => x.Id.ToLower()).ToList();

                userViewDepartmentIds = userViewDepartmentIds.Concat(deletedDepartment).Distinct().Select(x => x.ToUpper()).ToList();
            }
            else
            {
                userViewDepartmentIds = historyFilterModel.Departments.Select(x => x.ToUpper()).ToList();
            }

            var userViewFactoryIds = userPermissions?.Where(x => x.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY)
                                                        .Select(x => x.ClaimValue.ToUpper()).ToList()
                                        ?? historyFilterModel.Factories;

            //Nếu không có quyền xem nhà máy hay phòng ban nào => không tìm thấy dữ liệu phù hợp
            if (userViewFactoryIds?.Any() == false)
            {
                return new ResponseModel<HistoryPagedList>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            var condition = PredicateBuilder.New<HistoryResultModel>(true);

            if (userViewFactoryIds?.Any() == true)
            {
                condition = condition.And(x => userViewFactoryIds.Select(x => Guid.Parse(x)).ToList().Contains(x.FactoryId));
            }

            //Tìm kiếm theo danh sách phòng ban
            if (userViewDepartmentIds?.Any() == true)
            {
                condition = condition.And(x => userViewDepartmentIds.Select(x => Guid.Parse(x)).ToList().Contains(x.DepartmentId));
            }

            if (!string.IsNullOrEmpty(historyFilterModel.UserName))
            {
                var userIdByNames = users.Where(x => x.Name.ToLower().Contains(historyFilterModel.UserName.ToLower())).Select(x => x.Id.ToLower());

                if (userIdByNames.Any())
                {
                    condition = condition.And(x => userIdByNames.ToList().Contains(x.CreateBy));
                }
                else
                {
                    condition = condition.And(x => string.IsNullOrEmpty(x.CreateBy));
                }

            }

            //Tìm theo khoảng số lượng
            if (historyFilterModel.QuantityFrom >= 0 && historyFilterModel.QuantityTo >= 0)
            {
                condition = condition.And(x => x.Quantity >= historyFilterModel.QuantityFrom && x.Quantity <= historyFilterModel.QuantityTo);
            }
            else if (historyFilterModel.QuantityFrom >= 0)
            {
                condition = condition.And(x => x.Quantity >= historyFilterModel.QuantityFrom);
            }
            else if (historyFilterModel.QuantityTo >= 0)
            {
                condition = condition.And(x => x.Quantity <= historyFilterModel.QuantityTo);
            }
            //Tìm theo mã linh kiện
            if (!string.IsNullOrEmpty(historyFilterModel.ComponentCode))
            {
                condition = condition.And(x => x.ComponentCode.ToLower().Contains(historyFilterModel.ComponentCode.ToLower()));
            }
            //Tìm theo vị trí cố định
            if (!string.IsNullOrEmpty(historyFilterModel.PositionCode))
            {
                condition = condition.And(x => x.PositionCode.ToLower().Contains(historyFilterModel.PositionCode.ToLower()));
            }

            //Tìm kiếm theo loại nghiệp vụ
            if (historyFilterModel.Types?.Any() == true)
            {
                condition = condition.And(x => historyFilterModel.Types.Select(x => int.Parse(x)).Contains(x.ActivityType));
            }

            //Tìm kiếm theo khu vực
            //Lưu ý, khu vực có thể bị xóa nhưng màn hình lịch sử khi chọn dropdown là tất cả thì hiển thị cả những khu vực đã bị xóa lên
            if (historyFilterModel.isAllLayouts == false && historyFilterModel.Layouts.Any())
            {
                condition = condition.And(x => historyFilterModel.Layouts.Contains(x.Layout));
            }

            //Tìm kiếm theo khoảng thời gian
            if (historyFilterModel.dateFrom != null && historyFilterModel.dateTo != null)
            {
                condition = condition.And(x => x.CreateDate.Date >= historyFilterModel.dateFrom.Value.Date && x.CreateDate.Date <= historyFilterModel.dateTo.Value.Date);
            }
            else if (historyFilterModel.dateFrom != null)
            {
                condition = condition.And(x => x.CreateDate.Date >= historyFilterModel.dateFrom.Value.Date);
            }
            else if (historyFilterModel.dateTo != null)
            {
                condition = condition.And(x => x.CreateDate.Date <= historyFilterModel.dateTo.Value.Date);
            }


            var rootQuery = query.Where(condition)
                                .OrderByDescending(x => x.CreateDate);

            HistoryPagedList pagedList = new HistoryPagedList();
            await pagedList.CreateAsync(rootQuery, historyFilterModel.Skip, historyFilterModel.PageSize, isGetAll: historyFilterModel.IsGetAll);

            return new ResponseModel<HistoryPagedList>
            {
                Code = StatusCodes.Status200OK,
                Data = pagedList
            };
        }

        public async Task<ResponseModel<HistoryDetailModel>> GetHistoryDetail(string userId, string historyId)
        {
            var users = await GetInternalUsers();
            var userPermissions = await UserPermissions(userId);
            var departments = await GetInternalDepartments();

            var historyDetail = await _storageContext.PositionHistories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == historyId.ToLower());

            if (historyDetail == null)
            {
                return new ResponseModel<HistoryDetailModel> { Code = StatusCodes.Status404NotFound, Message = "Không tìm thấy dữ liệu" };
            }

            var detailModel = new HistoryDetailModel();
            detailModel.UserName = users.FirstOrDefault(x => x.Id.ToLower() == historyDetail.CreatedBy.ToLower())?.Name ?? string.Empty;
            detailModel.CreateDate = historyDetail.CreatedAt;
            detailModel.DepartmentName = departments.FirstOrDefault(x => x.Id.ToLower() == historyDetail.DepartmentId.ToString().ToLower())?.Name ?? string.Empty;
            detailModel.Type = historyDetail.PositionHistoryType.HasValue ? (int)historyDetail.PositionHistoryType.Value : -1;
            detailModel.ComponentCode = historyDetail?.ComponentCode ?? string.Empty;
            detailModel.ComponentName = historyDetail?.ComponentName ?? string.Empty;
            detailModel.SupplierCode = historyDetail?.SupplierCode ?? string.Empty;
            detailModel.SupplierName = historyDetail?.SupplierName ?? string.Empty;
            detailModel.SupplierShortName = historyDetail?.SupplierShortName ?? string.Empty;
            detailModel.PositionCode = historyDetail?.PositionCode ?? string.Empty;
            detailModel.Quantity = historyDetail.Quantity.HasValue ? historyDetail.Quantity.Value : 0;
            detailModel.InventoryNumber = historyDetail.InventoryNumber.HasValue ? historyDetail.InventoryNumber.Value : 0;
            detailModel.Note = historyDetail?.Note ?? string.Empty;

            return new ResponseModel<HistoryDetailModel>
            {
                Code = StatusCodes.Status200OK,
                Data = detailModel,
            };
        }
        public async Task<ResponseModel<ResultSet<IEnumerable<HistoryInOutExportResultDto>>>> GetHistoriesToExportExcel(HistoryInOutExportDto historyFilterModel)
        {
            if (historyFilterModel == null)
            {
                return new ResponseModel<ResultSet<IEnumerable<HistoryInOutExportResultDto>>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu không hợp lệ"
                };
            }

            var users = await GetInternalUsers();
            var userPermissions = await UserPermissions(historyFilterModel.UserId);
            var departments = await GetInternalDepartments();

            var usersDictionary = users.ToDictionary(x => x.Id.ToLower());
            var departmentsDictionary = departments.ToDictionary(x => x.Id.ToLower());

            var query = (from h in _storageContext.PositionHistories.AsNoTracking()

                         let convertedCreateBy = h.CreatedBy.ToLower()
                         let convertedDepartmentId = h.DepartmentId.ToString().ToLower()
                         let user = usersDictionary.ContainsKey(convertedCreateBy) ? usersDictionary[convertedCreateBy] : null
                         let department = departmentsDictionary.ContainsKey(convertedDepartmentId) ? departmentsDictionary[convertedDepartmentId] : null
                         select new HistoryInOutExportResultDto
                         {
                             Id = h.Id.ToString(),
                             CreateDate = h.CreatedAt,
                             ActivityType = h.PositionHistoryType.HasValue ? (int)h.PositionHistoryType.Value : -1,
                             ComponentCode = h.ComponentCode ?? string.Empty,
                             PositionCode = h.PositionCode ?? string.Empty,
                             Quantity = h.Quantity.HasValue ? h.Quantity.Value : 0,
                             InventoryNumber = h.InventoryNumber.HasValue ? h.InventoryNumber.Value : 0,
                             Note = string.IsNullOrEmpty(h.Note) ? string.Empty : h.Note,
                             FactoryId = h.FactoryId.ToString().ToLower(),
                             Layout = h.Layout ?? string.Empty,
                             DepartmentId = h.DepartmentId.ToString(),
                             UserName = user != null ? user.Name : string.Empty,
                             UserCode = (!string.IsNullOrEmpty(h.EmployeeCode)) ? h.EmployeeCode : (user != null ? user.Code : string.Empty),
                             DepartmentName = department != null ? department.Name : string.Empty,
                         });

            IEnumerable<string> userViewDepartmentIds;
            //Ngoài quyền xem phòng ban và phòng ban được chọn, + thêm các phòng ban đã bị xóa mềm để hiển thị lịch sử
            if (historyFilterModel.isAllDepartments || historyFilterModel?.Departments?.Count == 0)
            {
                userViewDepartmentIds = userPermissions?.Where(x => x.ClaimType == Constants.Permissions.DEPARTMENT_DATA_INQUIRY)
                                                        .Select(x => x.ClaimValue.ToLower());
                var deletedDepartment = departments.Where(x => x.IsDeleted.HasValue && x.IsDeleted.Value == true).Select(x => x.Id.ToLower()).ToList();
                userViewDepartmentIds = userViewDepartmentIds.Concat(deletedDepartment);
            }
            else
            {
                userViewDepartmentIds = historyFilterModel.Departments;
            }

            var userViewFactoryIds = userPermissions?.Where(x => x.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY)
                                                        .Select(x => x.ClaimValue.ToLower())
                                        ?? historyFilterModel.Factories;

            //Nếu không có quyền xem nhà máy hay phòng ban nào => không tìm thấy dữ liệu phù hợp
            if (userViewFactoryIds?.Any() == false)
            {
                return new ResponseModel<ResultSet<IEnumerable<HistoryInOutExportResultDto>>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            if (userViewFactoryIds?.Any() == true)
            {
                query = query.Where(x => string.IsNullOrEmpty(x.FactoryId) ? false : userViewFactoryIds.Contains(x.FactoryId.ToLower()));
            }


            Func<HistoryInOutExportResultDto, bool> conditionClause = (x) =>
            {
                bool validUserName = true;
                bool validQuantityRange = true;
                bool validComponentCode = true;
                bool validPositionCode = true;
                bool validType = true;
                bool validFactory = true;
                bool validDepartment = true;
                bool validLayout = true;
                bool validDateRange = true;

                //Tìm theo tên người dùng
                if (!string.IsNullOrEmpty(historyFilterModel.UserName))
                {
                    validUserName = string.IsNullOrEmpty(x.UserName) ? false : x.UserName.ToLower().Contains(historyFilterModel.UserName.ToLower());
                }

                //Tìm theo khoảng số lượng
                if (historyFilterModel.QuantityFrom >= 0 && historyFilterModel.QuantityTo >= 0)
                {
                    validQuantityRange = x.Quantity >= historyFilterModel.QuantityFrom && x.Quantity <= historyFilterModel.QuantityTo;
                }
                else if (historyFilterModel.QuantityFrom >= 0)
                {
                    validQuantityRange = x.Quantity >= historyFilterModel.QuantityFrom;
                }
                else if (historyFilterModel.QuantityTo >= 0)
                {
                    validQuantityRange = x.Quantity <= historyFilterModel.QuantityTo;
                }
                //Tìm theo mã linh kiện
                if (!string.IsNullOrEmpty(historyFilterModel.ComponentCode))
                {
                    validComponentCode = string.IsNullOrEmpty(x.ComponentCode) ? false : x.ComponentCode.ToLower().Contains(historyFilterModel.ComponentCode.ToLower());
                }
                //Tìm theo vị trí cố định
                if (!string.IsNullOrEmpty(historyFilterModel.PositionCode))
                {
                    validPositionCode = string.IsNullOrEmpty(x.PositionCode) ? false : x.PositionCode.ToLower().Contains(historyFilterModel.PositionCode.ToLower());
                }

                //Tìm kiếm theo loại nghiệp vụ
                if (historyFilterModel.Types?.Any() == true)
                {
                    validType = historyFilterModel.Types.Contains(x.ActivityType.ToString());
                }

                //Tìm kiếm theo danh sách nhà máy
                if (userViewFactoryIds?.Any() == true)
                {
                    validFactory = string.IsNullOrEmpty(x.FactoryId) ? false : userViewFactoryIds.Select(x => x.ToLower()).Contains(x.FactoryId.ToLower());
                }
                //Tìm kiếm theo danh sách phòng ban
                if (userViewDepartmentIds?.Any() == true)
                {
                    validDepartment = string.IsNullOrEmpty(x.DepartmentId) ? false : userViewDepartmentIds.Select(x => x.ToLower()).Contains(x.DepartmentId.ToLower());
                }
                //Tìm kiếm theo khu vực
                //Lưu ý, khu vực có thể bị xóa nhưng màn hình lịch sử khi chọn dropdown là tất cả thì hiển thị cả những khu vực đã bị xóa lên
                if (historyFilterModel.isAllLayouts == false && historyFilterModel.Layouts.Any())
                {
                    validLayout = string.IsNullOrEmpty(x.Layout) ? false : historyFilterModel.Layouts.Contains(x.Layout);
                }

                //Tìm kiếm theo khoảng thời gian
                if (historyFilterModel.dateFrom != null && historyFilterModel.dateTo != null)
                {
                    validDateRange = x.CreateDate.Date >= historyFilterModel.dateFrom.Value.Date && x.CreateDate.Date <= historyFilterModel.dateTo.Value.Date;
                }
                else if (historyFilterModel.dateFrom != null)
                {
                    validDateRange = x.CreateDate.Date >= historyFilterModel.dateFrom.Value.Date;
                }
                else if (historyFilterModel.dateTo != null)
                {
                    validDateRange = x.CreateDate.Date <= historyFilterModel.dateTo.Value.Date;
                }

                return validUserName && validQuantityRange && validComponentCode && validPositionCode && validType && validFactory && validDepartment && validLayout && validDateRange;
            };


            ResultSet<IEnumerable<HistoryInOutExportResultDto>> resultSet = new();
            query = query.OrderByDescending(x => x.CreateDate);
            var records = query?.Where(conditionClause)?.AsEnumerable();
            resultSet.Data = records ?? default;

            return new ResponseModel<ResultSet<IEnumerable<HistoryInOutExportResultDto>>>
            {
                Code = StatusCodes.Status200OK,
                Data = resultSet
            };
        }

    }
}
