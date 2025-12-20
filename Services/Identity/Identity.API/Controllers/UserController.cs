namespace BIVN.FixedStorage.Identity.API.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<IIdentityService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UserController(IIdentityService identityService
            , ILogger<IIdentityService> logger
            , IConfiguration configuration
            , IWebHostEnvironment environment
            , IUserService userService
            , IHttpContextAccessor httpContextAccessor
            , IWebHostEnvironment hostingEnvironment
            )
        {
            _identityService = identityService;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            _userService = userService;
            this._httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPut]
        [Route("api/identity/update/user")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> UpdateUserInfo([FromForm] UpdateUserInfoDto updateUserInfo)
        {
        if (updateUserInfo.file == null || updateUserInfo.file.Length == 0)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tìm thấy file ảnh"
                });
            }

            if (!IsImageFile(updateUserInfo.file))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "File không phải là hình ảnh"
                });
            }

            if (updateUserInfo.file.Length > 5 * 1024 * 1024) // 5MB
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Ảnh quá dung lượng cho phép. Vui lòng chọn lại"
                });
            }

            if (updateUserInfo.userId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy id người dùng"
                });
            }
            var result = await _userService.UpdateUserInfo(updateUserInfo);

            return Ok(result);
        }

        private bool IsImageFile(IFormFile file)
        {
            if (file == null)
                return false;

            // Danh sách các phần mở rộng phổ biến của tệp hình ảnh
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", "webp", "svg", "tiff" };

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            // Kiểm tra xem phần mở rộng của tệp có trong danh sách cho phép không
            return allowedExtensions.Contains(fileExtension);
        }

        [HttpGet("api/identity/user/{userId}")]
        [InternalService]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy id người dùng"
                });
            }
            var result = await _userService.GetUserInfo(userId);

            return Ok(result);
        }

        [HttpPost(Constants.Endpoint.API_Identity_Get_Filter_User_List)]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> FilterUser()
        {

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var userName = Request.Form["UserName"].FirstOrDefault();
            var fullName = Request.Form["FullName"].FirstOrDefault();
            var code = Request.Form["Code"].FirstOrDefault();
            var allDepartments = Request.Form["AllDepartments"].FirstOrDefault();
            var allRoles = Request.Form["AllRoles"].FirstOrDefault();
            var allStatus = Request.Form["AllStatus"].FirstOrDefault();
            var departmentIds = Request.Form["DepartmentIds[]"].ToList();
            var roleIds = Request.Form["RoleIds[]"].ToList();
            var status = Request.Form["Status[]"].ToList();
            var allAccountType = Request.Form["AllAccountType"].FirstOrDefault();
            var accountTypes = Request.Form["AccountTypes[]"].ToList();

            var filterUseModal = new FilterUseModel();
            filterUseModal.UserName = userName;
            filterUseModal.FullName = fullName;
            filterUseModal.Code = code;
            filterUseModal.DepartmentIds= departmentIds;
            filterUseModal.RoleIds = roleIds;
            filterUseModal.Status = status;
            filterUseModal.AllDepartments = allDepartments;
            filterUseModal.AllRoles = allRoles;
            filterUseModal.AllStatus = allStatus;
            filterUseModal.AllAccountType = allAccountType;
            filterUseModal.AccountTypes = accountTypes;

            var result = await _userService.FilterUser(filterUseModal);
            if(result?.Data != null)
            {
                var allRecords = result?.Data;
                int recordsTotal = 0;
                recordsTotal = result.Data.Count();

                var data = result.Data.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0 };
            return Ok(jsonData2);
             

            //return Ok(result);
        }

        [HttpGet("api/identity/status")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> StatusUser()
        {
            var result = await _userService.StatusUser();

            return Ok(result);
        }
       
        [HttpGet(Constants.Endpoint.API_Identity_Create_User)]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<CreateUserErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<CreateUserResultDto>))]
        public async Task<IActionResult> CreateUser()
        {
            var result = await _userService.CreateUserAsync();
            if (result.Code != StatusCodes.Status200OK)
            {
                return new JsonResult(new ResponseModel<CreateUserDto>() { Code = result.Code, Message = result.Message, Data = result.Data });
            }
            return Ok(result);
        }

        [HttpPost(Constants.Endpoint.API_Identity_Create_User)]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]        
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<CreateUserErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<CreateUserResultDto>))]
        public async Task<IActionResult> CreateUser(CreateUserDto model)
        {
            try
            {
                var check = await _userService.ValidateCreateUserInputAsync(model);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<CreateUserResultDto>() { Code = check.Code.Value, Message = check.Message });

                var result = await _userService.CreateUserAsync(model);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel { Code = result.Code, Message = result.Message, Data = result.Data });
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<CreateUserResultDto>() { Code = StatusCodes.Status500InternalServerError });
            }
        }
        
        [HttpGet(Constants.Endpoint.API_Identity_Get_User_Detail)]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<UserDetailDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<UserDetailDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<UserDetailDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<UserDetailDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<UserDetailDto>))]
        public async Task<IActionResult> GetUserDetail(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || (!string.IsNullOrEmpty(id) && !Guid.TryParse(id, out var _)))
                {
                    return BadRequest(new ResponseModel<string>(StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ", id));
                }

                var result = await _userService.GetUserDetailAsync(id);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return new JsonResult(new ResponseModel<UserDetailDto>() { Code = result.Code, Message = result.Message, Data = result.Data });
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<UserDetailDto>() { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPut(Constants.Endpoint.API_Identity_Update_User)]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]        
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel<CreateUserResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<CreateUserErrorDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<CreateUserResultDto>))]
        public async Task<IActionResult> UpdateUser(UpdateUserDto model)
        {
            try
            {
                var check = await _userService.ValidateUpdateUserInputAsync(model);
                if (check.IsInvalid)
                    return BadRequest(new ResponseModel<UpdateUserErrorDto>() { Code = check.Code.Value, Message = check.Message, Data = check.Data });

                var result = await _userService.UpdateUserAsync(model);
                if (result.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(new ResponseModel { Code = result.Code, Message = result.Message, Data = result.Data });
                }
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<UpdateUserDto>() { Code = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPut("api/identity/account-type/{userId}/{accountType}/{updateBy}")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> UpdateAccountTypeUser(string userId, AccountType accountType, string updateBy)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), "Id người dùng không hợp lệ");
            }
            var result = await _userService.UpdateAccountTypeUser(userId, accountType, updateBy);

            return Ok(result);
        }

        [HttpPut("api/identity/change/status/{userId}/{status}/{updateBy}")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> UpdateStatusUser(string userId, UserStatus status, string updateBy)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), "Id người dùng không hợp lệ");
            }
            if(status == UserStatus.LockByExpiredPassword || status == UserStatus.LockByUnactive)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Trạng thái không hợp lệ."
                });
            }

            var result = await _userService.UpdateStatusUser(userId, status, updateBy);

            return Ok(result);
        }

        [HttpGet(Constants.Endpoint.API_Identity_Get_User_Info)]
        [@Authorize(Constants.Permissions.MC_BUSINESS, Constants.Permissions.PCB_BUSINESS)]
        public async Task<IActionResult> GetUserInfoMobileAfterLogin(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), "Id người dùng không hợp lệ");
            }

            var result = await _userService.GetUserInfoMobileAfterLogin(userId);
            return Ok(result);
        }

        [HttpPut(Constants.Endpoint.API_Identity_Lock_Users)]
        [BackgroundJob]
        public async Task<IActionResult> LockUsersByExpiredPasswordOrUnactive()
        {
            try
            {
                var result = await _userService.LockUsersByExpiredPasswordOrUnactiveAsync();
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess);
                return new JsonResult(new ResponseModel<LockUserListDto>() { Code = StatusCodes.Status500InternalServerError });
            }

        }

        [HttpGet("api/identity/user/get-user-info-by-id/{userId}")]
        [InternalService]        
        public async Task<IActionResult> GetUserInfoByUserId(string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy id người dùng"
                });
            }
            var result = await _userService.GetUserInfo(userId);

            return Ok(result);
        }

        [HttpPost(Constants.Endpoint.API_Identity_Get_Filter_User_List_Export)]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> GetFilterUserListExport(UserListExportFilterDto filterModel)
        {
            try
            {               
                var result = await _userService.GetFilterUserListExport(filterModel);
                return Ok(result);
            }
            catch (Exception exception)
            {
                var exMess = $"Exception - {exception.Message}";
                var innerExMess = exception.InnerException != null ? $"InnerException - {exception.InnerException.Message}" : string.Empty;
                _logger.LogError($"Request error at {_httpContextAccessor.HttpContext.Request.Path} : {exMess}; {innerExMess}");
                Log.Error(exception, "Request error: {0} ; {1}", exMess, innerExMess); ;
            }
            return BadRequest(new ResponseModel { Code = StatusCodes.Status500InternalServerError });
        }
    }
}
