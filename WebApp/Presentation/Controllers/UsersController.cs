using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using @webappConstants = BIVN.FixedStorage.Services.Common.API.Constants;

namespace WebApp.Presentation.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IRestClient _restClient;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public UsersController(ILogger<UsersController> logger,
                              IRestClient restClient,
                              IUserService userService,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration
                              )
        {
            _logger = logger;
            _restClient = restClient;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        [RouteDisplayValue(RouteDisplay.LIST_USER)]
        [Route("list-users")]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> Index()
        {
            var token = Request.TokenFromCookie();

            try
            {

                //Phong Ban:
                var reqDepartment = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Department);
                reqDepartment.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                var departments = Enumerable.Empty<DepartmentDto>();
                //Call API DS Phong Ban:
                var resultListDepartment = await _restClient.GetAsync(reqDepartment);
                var getDepartments = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<DepartmentDto>>>(resultListDepartment?.Content)?.Data;

                var departmentPermission = User.Claims.Where(x => x.Type == @webappConstants.Permissions.DEPARTMENT_DATA_INQUIRY).Select(x => x.Value.ToLower());
                if (departmentPermission?.Any() == true)
                {
                    departments = getDepartments?.Where(x => departmentPermission.Contains(x.Id.ToLower()));
                }

                ViewBag.Departments = departments;
                var defaultCultures = new List<CultureInfo>()
                {
                    new CultureInfo("vi-VN"),
                    new CultureInfo("en-US"),
                };

                CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures);
                var cultureItems = cinfo.Where(x => defaultCultures.Contains(x))
                    .Select(c => new SelectListItem { Value = c.Name, Text = c.DisplayName })
                    .ToList();
                ViewBag.Cultures = cultureItems;
            }
            catch (Exception ex)
            {

                _logger.LogError("Lỗi khi get API danh sách phòng ban");
                _logger.LogError($"Error:{ex.Message}-{ex.InnerException?.Message}");
            }

            try
            {
                //Nhom Quyen:
                var reqRole = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Role);
                reqRole.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                //Call API DS Nhom Quyen:
                var resultListRole = await _restClient.GetAsync(reqRole);
                var convertedResultListRole = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<RoleInfoModel>>>(resultListRole?.Content)?.Data;
                ViewBag.Roles = convertedResultListRole;
            }
            catch (Exception ex)
            {

                _logger.LogError("Lỗi khi get API nhóm quyền");
                _logger.LogError($"Error:{ex.Message}-{ex.InnerException?.Message}");
            }

            try
            {
                //DS các trạng thái::
                var reqStatus = new RestRequest(commonAPIConstant.Endpoint.api_Identity_Status);
                reqStatus.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, token);

                //Call API DS Trạng Thái:
                var resultListStatus = await _restClient.GetAsync(reqStatus);
                var convertedresultListStatus = JsonConvert.DeserializeObject<ResponseModel<IList<string>>>(resultListStatus?.Content)?.Data;
                ViewBag.Status = convertedresultListStatus;
            }
            catch (Exception ex)
            {

                _logger.LogError("Lỗi khi get API danh sách trạng thái");
                _logger.LogError($"Error:{ex.Message}-{ex.InnerException?.Message}");
            }

            return View();
        }

        [HttpGet(commonAPIConstant.Endpoint.WebApp_Create_User)]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> CreateUser()
        {
            try
            {
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Create_User);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                var response = await _restClient.GetAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<CreateUserDto>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                else if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<CreateUserDto>(result.Code, result.Message, result.Data));
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                //using var responseBody = _httpContextAccessor.HttpContext.Response.Body;                
                //var response = System.Text.Json.JsonSerializer.Deserialize<ResponseModel>(responseBody, JsonDefaults.CamelCasing);
                return Unauthorized(new ResponseModel { Code = StatusCodes.Status401Unauthorized, Message = "Không có quyền truy cập" });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.WebApp_Create_User)]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> CreateUserAsync([FromForm] CreateUserDto model)
        {
            try
            {
                var check = _userService.ValidateCreateUserInput(model);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<CreateUserErrorDto>(check.Code.Value, check.Message, check.Data));
                var request = new RestRequest(BIVN.FixedStorage.Services.Common.API.Constants.Endpoint.API_Identity_Create_User);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[BIVN.FixedStorage.Services.Common.API.Constants.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<CreateUserErrorDto>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                else if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<CreateUserErrorDto>(result.Code, result.Message, result.Data));
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.WebApp_Get_User_Detail)]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> GetUserDetail(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(id) && !Guid.TryParse(id, out var _))
                {
                    return BadRequest(new ResponseModel<string>(StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ", id));
                }
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Get_User_Detail);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                request.AddParameter("id", id);
                var response = await _restClient.GetAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<UserDetailDto>>(response?.Content ?? string.Empty);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                else if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<UserDetailDto>(result.Code, result.Message, result.Data));
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPut(commonAPIConstant.Endpoint.WebApp_Update_User)]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> UpdateUserAsync(UpdateUserDto model)
        {
            try
            {
                var check = await _userService.ValidateUpdateUserInputAsync(model);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<UpdateUserErrorDto>(check.Code.Value, check.Message, check.Data));
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Update_User);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                model.UpdatedBy = _httpContextAccessor.HttpContext.User.FindFirstValue("id");
                request.AddBody(model);
                var response = await _restClient.ExecutePutAsync(request);
                if (string.IsNullOrEmpty(response.Content))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                if (response?.IsSuccessful == false && !string.IsNullOrEmpty(response.Content))
                {
                    var failResult = JsonConvert.DeserializeObject<ResponseModel<UpdateUserErrorDto>>(response.Content);
                    return BadRequest(failResult);
                }

                var result = JsonConvert.DeserializeObject<ResponseModel<UpdateUserErrorDto>>(response.Content);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                else if (result.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(new ResponseModel<UpdateUserErrorDto>(result.Code, result.Message, result.Data));
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.WebApp_Export_User_List)]
        [Authorize(commonAPIConstant.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> ExportUserListAsync(UserListExportFilterDto model)
        {
            try
            {
                //var check = await _userService.ValidateExportUserListFilterAsync(model);
                //if (check.IsInvalid)
                //    return BadRequest(new ResponseModel<UserListExportErrorResultDto>(check.Code.Value, check.Message, check.Data));
                var request = new RestRequest(commonAPIConstant.Endpoint.API_Identity_Get_Filter_User_List_Export);
                var token = _httpContextAccessor.HttpContext.Request.Cookies[commonAPIConstant.UserClaims.Token];
                request.AddHeader(commonAPIConstant.HttpContextModel.AuthorizationKey, $"{token}");
                request.AddBody(model);
                var response = await _restClient.PostAsync(request);
                if (response?.IsSuccessful == false)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                if (string.IsNullOrEmpty(response?.Content))
                {
                    return new JsonResult(new ResponseModel { Code = (int)response.StatusCode, Message = "Không có bản ghi nào" });
                }
                var result = JsonConvert.DeserializeObject<ResponseModel<List<FilterListUserDto>>>(response?.Content);
                if (result == null)
                {
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                }
                else if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<List<FilterListUserDto>>(result.Code, result.Message, result.Data));
                }
                if (result.Data?.Any() == false)
                {
                    return new JsonResult(new ResponseModel<List<FilterListUserDto>>(result.Code, result.Message, result.Data));
                }

                var exportResult = await _userService.ExportFilteredUserListAsync(result.Data);
                if (exportResult == null)
                    return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
                return File(exportResult, commonAPIConstant.FileResponse.ExcelType, $"DanhSachNguoiDung_{DateTime.Now.ToString(commonAPIConstant.DatetimeFormat)}.xlsx");
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
