using Microsoft.AspNetCore.Authorization;

namespace BIVN.FixedStorage.Identity.API.Controllers
{

    [ApiController]
    [Route(Constants.Endpoint.api_Identity_Role)]
    public class RolesController : ControllerBase
    {
        private readonly ILogger<RolesController> _logger;
        private readonly IRoleService _roleService;

        public RolesController(ILogger<RolesController> logger,
                               IRoleService roleService
                               )
        {
            _logger = logger;
            _roleService = roleService;
        }

        [HttpGet]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.ROLE_MANAGEMENT, Constants.Permissions.USER_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<RoleInfoModel>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<IEnumerable<RoleInfoModel>>))]
        public async Task<IActionResult> AllRoles()
        {
            var result = await _roleService.GetRolesAsync();
            if (result?.Data == null)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("exist-name/{roleName}")]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.ROLE_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        public async Task<IActionResult> ExistRoleName(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                ModelState.AddModelError("Name", "Tên không hợp lệ");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu không hợp lệ",
                });
            }

            var existNameResult = await _roleService.ExistRoleNameAsync(roleName);
            return Ok(existNameResult);
        }


        [HttpPost("create")]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [@Authorize(Constants.Permissions.ROLE_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<CreateRoleModel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        public async Task<IActionResult> Create([FromBody] CreateRoleModel createRoleModel)
        {
            if (createRoleModel == null || createRoleModel.Permissions?.Any() == false)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng cung cấp dữ liệu"
                });
            }
            if (string.IsNullOrEmpty(createRoleModel.Name))
            {
                ModelState.AddModelError(nameof(createRoleModel.Name), "Vui lòng không để trống tên");
            }
            if (string.IsNullOrEmpty(createRoleModel.UserId))
            {
                ModelState.AddModelError(nameof(createRoleModel.UserId), "Vui lòng không để trống Id người tạo");
            }
            if (!string.IsNullOrEmpty(createRoleModel.UserId))
            {
                if (!Guid.TryParse(createRoleModel.UserId, out _))
                {
                    ModelState.AddModelError(nameof(createRoleModel.UserId), "Id người tạo không hợp lệ");
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

            var checkExistNameResult = await _roleService.ExistRoleNameAsync(createRoleModel.Name);
            if (checkExistNameResult?.Data == true)
            {
                return BadRequest(checkExistNameResult);
            }

            var createResult = await _roleService.CreateAsync(createRoleModel);
            if (createResult?.Data == false)
            {
                return BadRequest(createResult);
            }

            return Ok(createResult);
        }

        [HttpPut("edit")]
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [@Authorize(Constants.Permissions.ROLE_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<EditRoleModel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel<>))]
        public async Task<IActionResult> Update([FromBody] EditRoleModel editRoleModel)
        {
            if (editRoleModel == null || editRoleModel.Permissions?.Any() == false)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng cung cấp dữ liệu"
                });
            }
            if (string.IsNullOrEmpty(editRoleModel.Name))
            {
                ModelState.AddModelError(nameof(editRoleModel.Name), "Vui lòng không để trống tên");
            }
            if (string.IsNullOrEmpty(editRoleModel.UserId) || !Guid.TryParse(editRoleModel.UserId, out _))
            {
                ModelState.AddModelError(nameof(editRoleModel.UserId), "Id người tạo không hợp lệ");
            }
            if (string.IsNullOrEmpty(editRoleModel.RoleId) || !Guid.TryParse(editRoleModel.RoleId, out _))
            {
                ModelState.AddModelError(nameof(editRoleModel.RoleId), "Id quyền không hợp lệ");
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

            var editResult = await _roleService.EditAsync(editRoleModel);
            if (editResult?.Data == false)
            {
                return BadRequest(editResult);
            }

            return Ok(editResult);
        }

        [HttpDelete("delete/{roleId}/{userId}")]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.USER_MANAGEMENT)]
        [@Authorize(Constants.Permissions.ROLE_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<>))]
        public async Task<IActionResult> Delete(string roleId, string userId)
        {
            if (string.IsNullOrEmpty(roleId) || !Guid.TryParse(roleId, out _))
            {
                ModelState.AddModelError(nameof(roleId), "Id quyền không hợp lệ");
            }
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), "Id người dùng không hợp lệ");
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

            var deleteResult = await _roleService.DeleteAsync(roleId, userId);
            if (deleteResult?.Data == false)
            {
                return BadRequest(deleteResult);
            }

            return Ok(deleteResult);
        }

        [HttpPut("assign/{userId}/{roleId}/{updatedBy}")]
        //Use AllowAnonymous for debugging
        [@Authorize(Constants.Permissions.ROLE_MANAGEMENT, Constants.Permissions.USER_MANAGEMENT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> AssignUserRole(string userId, string roleId, string updatedBy)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                ModelState.AddModelError(nameof(userId), "Id người dùng không hợp lệ");
            }
            if (string.IsNullOrEmpty(roleId) || !Guid.TryParse(roleId, out _))
            {
                ModelState.AddModelError(nameof(roleId), "Id quyền không hợp lệ");
            }

            var assignResult = await _roleService.AssignUserRoleAsync(userId, roleId, updatedBy);
            if (assignResult?.Data == false)
            {
                return BadRequest(assignResult);
            }

            return Ok(assignResult);
        }
    }
}
