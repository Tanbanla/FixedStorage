namespace BIVN.FixedStorage.Identity.API.Controllers
{
    [ApiController]
    [Route(Constants.Endpoint.api_Identity_Department)]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        // GET: api/<DepartmentsController>
        public DepartmentsController(IDepartmentService departmentService
                                    )
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT, Constants.Permissions.ROLE_MANAGEMENT,
                    Constants.Permissions.HISTORY_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<DepartmentDto>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<IEnumerable<DepartmentDto>>))]
        public async Task<IActionResult> Departments()
        {
            var result = await _departmentService.GetAllDepartmentAsync();
            if (result.Code == StatusCodes.Status200OK)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("{id}")]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<DepartmentDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<DepartmentDto>))]
        public async Task<IActionResult> GetDepartment(string Id)
        {
            if (Guid.TryParse(Id, out Guid deparmentId))
            {
                var result = await _departmentService.GetDepartmentInfo(deparmentId);
                if (result.Code == 200)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest(new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Id không  hợp lệ"
            });
        }

        [HttpGet("users")]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT, Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> Users()
        {
            var result = await _departmentService.UserListAsync();
            return Ok(result);
        }

        [HttpGet("exist-department/{name}")]
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT, Constants.Permissions.USER_MANAGEMENT)]
        public async Task<IActionResult> CheckExistDepartmentName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("name", "Tên không hợp lệ");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu không hợp lệ",
                });
            }

            var result = await _departmentService.CheckExistDepartmentNameAsync(name);
            return Ok(result);
        }

        /// <summary>
        /// API tạo phòng ban
        /// </summary>
        /// <param name="createDepartmentDto"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<Services.Common.API.Dto.Department.CreateDepartmentDto>))]
        public async Task<IActionResult> CreateDepartment([FromBody] Services.Common.API.Dto.Department.CreateDepartmentDto createDepartmentDto)
        {
            if(createDepartmentDto == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng kiểm tra lại dữ liệu"
                });
            }

            if (string.IsNullOrEmpty(createDepartmentDto.UserId))
            {
                ModelState.AddModelError(nameof(createDepartmentDto.UserId), "Không để trống dữ liệu");
            }
            if (string.IsNullOrEmpty(createDepartmentDto.Name))
            {
                ModelState.AddModelError(nameof(createDepartmentDto.Name), "Không để trống dữ liệu");
            }
            if (createDepartmentDto.Name.Trim().Length > 50)
            {
                ModelState.AddModelError(nameof(createDepartmentDto.Name), "Hệ thống chỉ chấp nhận tối đa 50 kí tự");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng kiểm tra lại dữ liệu",
                    Data = ModelState.ErrorMessages()
                });
            }

            var existDepartment = await _departmentService.CheckExistDepartmentNameAsync(createDepartmentDto.Name);
            if(existDepartment?.Data == true)
            {
                ModelState.AddModelError(nameof(createDepartmentDto.Name), "Phòng ban này đã tồn tại, vui lòng nhập lại.");
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Phòng ban này đã tồn tại, vui lòng nhập lại.",
                    Data = ModelState.ErrorMessages()
                });
            }

            var createDeparmentResult = await _departmentService.CreateAsync(createDepartmentDto.Name, createDepartmentDto.UserId, createDepartmentDto.ManagerId);
            if(createDeparmentResult.Data == false)
            {
                return BadRequest(createDeparmentResult);
            }

            return Ok(createDeparmentResult);
        }

        [HttpPut("edit")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<Services.Common.API.Dto.Department.CreateDepartmentDto>))]
        public async Task<IActionResult> EditDepartment(Services.Common.API.Dto.Department.CreateDepartmentDto createDepartmentDto)
        {
            if (string.IsNullOrEmpty(createDepartmentDto.DepartmentId))
            {
                ModelState.AddModelError(nameof(createDepartmentDto.DepartmentId), "Vui lòng cung cấp Id phòng ban");
            }
            if (string.IsNullOrEmpty(createDepartmentDto.UserId))
            {
                ModelState.AddModelError(nameof(createDepartmentDto.UserId), "Vui lòng cung cấp Id người dùng");
            }
            if (string.IsNullOrEmpty(createDepartmentDto.Name))
            {
                ModelState.AddModelError(nameof(createDepartmentDto.Name), "Vui lòng cung cấp tên phòng ban");
            }
            if(createDepartmentDto.Name.Trim().Length > 50)
            {
                ModelState.AddModelError(nameof(createDepartmentDto.Name), "Tên phòng ban tối đa 50 kí tự");
            }
            if (!Guid.TryParse(createDepartmentDto.DepartmentId, out _))
            {
                ModelState.AddModelError("Id", "Id phòng ban không hợp lệ");
            }
            if (!Guid.TryParse(createDepartmentDto.UserId, out _))
            {
                ModelState.AddModelError("Id", "Id phòng ban không hợp lệ");
            }

            //if has Manager Id
            if (!string.IsNullOrEmpty(createDepartmentDto.ManagerId))
            {
                if (!Guid.TryParse(createDepartmentDto.ManagerId, out _))
                {
                    ModelState.AddModelError("Id", "Id trưởng phòng không hợp lệ");
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng kiểm tra lại dữ liệu",
                    Data = ModelState.ErrorMessages()
                });
            }

            var existDepartment = await _departmentService.CheckExistEditNameAsync(createDepartmentDto.DepartmentId, createDepartmentDto.Name.Trim());
            if (existDepartment?.Data == true)
            {
                ModelState.AddModelError("Name", "Phòng ban này đã tồn tại, vui lòng nhập lại.");
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Phòng ban này đã tồn tại, vui lòng nhập lại.",
                    Data = ModelState.ErrorMessages()
                });
            }

            var createDeparmentResult = await _departmentService.EditAsync(createDepartmentDto.DepartmentId, createDepartmentDto.Name, createDepartmentDto.UserId, createDepartmentDto.ManagerId);
            if (createDeparmentResult.Data == false)
            {
                return BadRequest(createDeparmentResult);
            }

            return Ok(createDeparmentResult);
        }

        [HttpDelete("delete/{departmentId}/{userId}")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Delete(string departmentId, string userId)
        {
            if (string.IsNullOrEmpty(departmentId))
            {
                ModelState.AddModelError("Id", "Id phòng ban không được để trống");
            }
            if (!Guid.TryParse(departmentId, out _))
            {
                ModelState.AddModelError("Id", "Id phòng ban không hợp lệ");
            }
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("UserId", "Id người dùng không được để trống");
            }
            if (!Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError("UserId", "Id người dùng không hợp lệ");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng kiểm tra lại dữ liệu",
                    Data = ModelState.ErrorMessages()
                });
            }

            var departmentEmptyResult = await _departmentService.CheckEmptyDepartmentAsync(departmentId);
            if(departmentEmptyResult.Data == false)
            {
                return BadRequest(departmentEmptyResult);
            }

            var deleteResult = await _departmentService.DeleteAsync(departmentId, userId);
            if(deleteResult.Data == false)
            {
                return BadRequest(deleteResult);
            }

            return Ok(deleteResult);
        }

        [HttpPut("assign/user/{userId}/{departmentId}/{updatedBy}")]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.DEPARTMENT_MANAGEMENT, Constants.Permissions.USER_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> AssignDepartmentToUser(string userId, string departmentId,string updatedBy)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), "Id người dùng không hợp lệ");
            }
            if (string.IsNullOrEmpty(departmentId) || !Guid.TryParse(departmentId, out _))
            {
                ModelState.AddModelError(nameof(departmentId), "Id phòng ban không hợp lệ");
            }

            var assignResult = await _departmentService.AssignUserToDepartmentAsync(departmentId, userId, updatedBy);
            if (assignResult?.Data == false)
            {
                return BadRequest(assignResult);
            }

            return Ok(assignResult);
        }
    }
}
