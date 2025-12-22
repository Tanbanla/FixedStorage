namespace BIVN.FixedStorage.Services.Storage.API.Controllers
{
    public class PositionController : ControllerBase
    {
        private readonly ILogger<PositionController> _logger;
        public readonly IPositionService _positionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PositionController(ILogger<PositionController> logger,
                                 IPositionService positionService,
                                 IHttpContextAccessor httpContextAccessor
                                )
        {
            _logger = logger;
            _positionService = positionService;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Thông tin linh kiện và danh sách mã khu vực theo linh kiện.
        /// Cho phép bỏ qua layout: nếu không truyền layout => tìm theo mã linh kiện trên tất cả khu vực thuộc phạm vi phân quyền.
        /// </summary>
        /// <param name="componentDto">layout (tùy chọn) + componentCode (bắt buộc)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/storage/{layout}/component/{componentCode}/info")]
        [Route("api/storage/component/{componentCode}/info")] // layout optional variant
        //[Authorize(Constants.Permissions.MC_BUSINESS, Constants.Permissions.PCB_BUSINESS, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<ComponentDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetComponentInfoAndListPosition([FromRoute] ComponentDto componentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu không hợp lệ",
                    Data = ModelState.ErrorMessages()
                });
            }
            var result = await _positionService.GetComponentInfoAndListPosition(componentDto);

            return Ok(result);
        }

        /// <summary>
        /// Danh sách kho
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/storage")]
        [Authorize(Constants.Permissions.MC_BUSINESS, Constants.Permissions.PCB_BUSINESS, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<ComponentDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetListStorage()
        {
            var result = await _positionService.GetListStorage();
            return Ok(result);
        }

        /// <summary>
        /// Danh sách khu vực
        /// </summary>
        /// <returns></returns>
        [HttpGet(Constants.Endpoint.API_Storage_Get_Layout_List)]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(ResponseModel<List<LayoutDto>>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Type = typeof(ResponseModel<List<LayoutDto>>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Unauthorized, Type = typeof(ResponseModel<List<LayoutDto>>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Forbidden, Type = typeof(ResponseModel<List<LayoutDto>>))]
        public async Task<IActionResult> GetLayoutList()
        {
            var result = await _positionService.GetLayoutList();
            return Ok(result);
        }
        /// <summary>
        /// Thêm mới linh kiện:
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/storage/component/add")]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> AddNewComponent(CreateComponentDto createComponentDto, string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages(),
                });
            }
            if (double.Parse(createComponentDto.MinInventoryNumber) > double.Parse(createComponentDto.MaxInventoryNumber))
            {
                return BadRequest(new ResponseModel
                {
                    Code = (int)HttpStatusCodes.MinInventeryNumber_IsNotGreater_MaxInventeryNumber,
                    Message = "Tồn kho nhỏ nhất đang lớn hơn tồn kho lớn nhất, vui lòng kiểm tra lại.",
                });
            }
            if (double.Parse(createComponentDto.InventoryNumber) > double.Parse(createComponentDto.MaxInventoryNumber))
            {
                return BadRequest(new ResponseModel
                {
                    Code = (int)HttpStatusCodes.InventeryNumber_IsNotGreater_MaxInventeryNumber,
                    Message = "Tồn kho thực tế đang lớn hơn tồn kho lớn nhất, vui lòng kiểm tra lại.",
                });
            }
            var result = await _positionService.AddNewComponent(createComponentDto, userId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết linh kiện:
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/storage/component/{id}")]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> GetDetailComponent(string id)
        {
            if (id.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Id của linh kiện không có dữ liệu",
                });
            }
            var result = await _positionService.GetDetailComponent(id);
            return Ok(result);
        }

        /// <summary>
        /// Danh sách linh kiện
        /// </summary>
        /// <returns></returns>
        [HttpPost(Constants.Endpoint.API_Storage_Get_Component_List)]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Type = typeof(ResponseModel<ComponentDto>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Forbidden, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetAllComponentsPaging()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var currentPage = (skip / pageSize) + 1;
            var componentsFilterModel = new ComponentsFilterDto()
            {
                ComponentCode = Request.Form["ComponentCode"].FirstOrDefault(),
                ComponentName = Request.Form["ComponentName"].FirstOrDefault(),
                SupplierName = Request.Form["SupplierName"].FirstOrDefault(),
                ComponentPosition = Request.Form["ComponentPosition"].FirstOrDefault(),
                AllLayouts = Request.Form["AllLayouts"].FirstOrDefault(),
                LayoutIds = Request.Form["LayoutIds[]"].ToList(),
                ComponentInventoryQtyStart = Int32.TryParse(Request.Form["ComponentInventoryQtyStart"].FirstOrDefault(), out int qtyInventoryStart) ? qtyInventoryStart : null,
                ComponentInventoryQtyEnd = Int32.TryParse(Request.Form["ComponentInventoryQtyEnd"].FirstOrDefault(), out int qtyInventoryEnd) ? qtyInventoryEnd : null,
                Paging = new Common.API.Dto.PagedList.PagingInfo(pageSize, currentPage),
                InventoryStatus = Request.Form["InventoryStatus"].FirstOrDefault(),
            };
            var validateFilterModel = await _positionService.ValidateFilterModelGetFilterComponents(componentsFilterModel);
            if (validateFilterModel.IsInvalid)
            {
                return BadRequest(new ResponseModel() { Code = validateFilterModel.Code.Value, Message = validateFilterModel.Message });
            }

            var result = await _positionService.GetAllComponentsPaging(componentsFilterModel);
            if (result?.Data == null)
            {
                var jsonDataEmptyRes = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = 0 };
                return Ok(jsonDataEmptyRes);
            }
            var jsonData = new { draw = draw, recordsFiltered = result.Data.Paging.RowsCount, recordsTotal = result.Data.Paging.RowsCount, data = result.Data.List };
            return Ok(jsonData);
        }

        /// <summary>
        /// Danh sách khu vực (Dropdown-list)
        /// </summary>
        /// <returns></returns>
        [HttpGet(Constants.Endpoint.API_Storage_Get_Layout_DropDownList)]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE, Constants.Permissions.FACTORY_DATA_INQUIRY, 
                   Constants.Permissions.HISTORY_MANAGEMENT)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Type = typeof(ResponseModel<ComponentDto>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Forbidden, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetLayoutDropdownList()
        {
            var result = await _positionService.GetLayoutDropDownList();
            return Ok(result);
        }
        /// <summary>
        /// Chỉnh sửa linh kiện:
        /// </summary>
        /// <param name="createComponentDto"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/storage/component/update")]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> UpdateComponent(UpdateComponentDto updateComponentDto, string componentId, string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = ModelState.ErrorTextMessages(),
                });
            }

            if (double.Parse(updateComponentDto.MinInventoryNumber) > double.Parse(updateComponentDto.MaxInventoryNumber))
            {
                return BadRequest(new ResponseModel
                {
                    Code = (int)HttpStatusCodes.MinInventeryNumber_IsNotGreater_MaxInventeryNumber,
                    Message = "Tồn kho nhỏ nhất đang lớn hơn tồn kho lớn nhất, vui lòng kiểm tra lại.",
                });
            }

            if (double.Parse(updateComponentDto.InventoryNumber) > double.Parse(updateComponentDto.MaxInventoryNumber))
            {
                return BadRequest(new ResponseModel
                {
                    Code = (int)HttpStatusCodes.InventeryNumber_IsNotGreater_MaxInventeryNumber,
                    Message = "Tồn kho thực tế đang lớn hơn tồn kho lớn nhất, vui lòng kiểm tra lại.",
                });
            }
            var result = await _positionService.UpdateComponent(updateComponentDto, componentId, userId);
            return Ok(result);
        }
        /// <summary>
        /// Xóa danh sách linh kiện:
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>

        [HttpDelete]
        [Route("api/storage/component/delete")]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> DeleteComponents(List<string>ids)
        {
            if (!ids.Any())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Yêu cầu nhập ids để xóa linh kiện",
                });
            }
            var result = await _positionService.DeleteComponents(ids);
            return Ok(result);
        }

        /// <summary>
        /// Danh sách linh kiện(Export Excel)
        /// </summary>
        /// <returns></returns>
        [HttpPost(Constants.Endpoint.API_Storage_Get_Component_List_Export)]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Type = typeof(ResponseModel<ComponentDto>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Forbidden, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetAllComponentsToExport([FromBody] ComponentsFilterToExportDto model)
        {
            var result = await _positionService.GetAllComponentsToExport(model);
            return Ok(result);
        }

        /// <summary>
        /// Xóa danh sách khu vực:
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpDelete]
        [Route("api/storage/layout/delete")]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.MASTER_DATA_WRITE)]
        public async Task<IActionResult> DeleteLayout(string layout)
        {
            if (layout.IsNullOrEmpty())
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Yêu cầu nhập vị trí linh kiện",
                });
            }
            var result = await _positionService.DeleteLayout(layout);
            return Ok(result);
        }

        /// <summary>
        /// Import Danh sách linh kiện
        /// </summary>
        /// <returns></returns>
        [HttpPost(Constants.Endpoint.API_Storage_Import_Component_List)]
        [Authorize(Constants.Permissions.MASTER_DATA_WRITE, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Type = typeof(ResponseModel<ComponentDto>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Forbidden, Type = typeof(ResponseModel))]
        public async Task<IActionResult> ImportExcelComponentListAsync([FromForm] IFormFile file)
        {
            var result = await _positionService.ImportExcelComponentListAsync(file);

            return Ok(result);
        }

        /// <summary>
        /// Danh sách trạng thái tồn kho (Dropdown-list)
        /// </summary>
        /// <returns></returns>
        [HttpGet(Constants.Endpoint.API_Storage_Get_InventoryStatus_DropDownList)]
        [Authorize(Constants.Permissions.MASTER_DATA_READ, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Type = typeof(ResponseModel<ComponentDto>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Forbidden, Type = typeof(ResponseModel))]
        public async Task<IActionResult> GetInventoryStatusDropdownList()
        {
            var result = await _positionService.GetInventoryStatusDropDownList();
            return Ok(result);
        }
    }
}
