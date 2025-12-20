using BIVN.FixedStorage.Services.Common.API.Dto.Location;

namespace Inventory.API.Controllers
{
    [Route("api/inventory/location")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Location")]
    public class LocationController : ControllerBase
    {
        private readonly ILogger<LocationController> _logger;
        private readonly ILocationService _locationService;

        public LocationController(ILogger<LocationController> logger,
                                  ILocationService locationService
                                )
        {
            _logger = logger;
            _locationService = locationService;
        }

        [HttpGet]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> Locations(string? departmentName)
        {
            try
            {
                var result = await _locationService.GetLocations(departmentName);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, errorMessage: ex.Message);
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDto createLocationDto)
        {
            var userId = HttpContext.CurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.TryAddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
            }
            if (createLocationDto == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }
            if (!createLocationDto.FactoryNames.Any(x => !string.IsNullOrEmpty(x)))
            {
                ModelState.TryAddModelError("factoryName", Constants.ResponseMessages.Inventory.RequiredFactoryName);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            try
            {
                createLocationDto.userId = Guid.Parse(userId);
                var result = await _locationService.AddLocation(createLocationDto);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, errorMessage: ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPut]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDto updateLocationDto)
        {
            var userId = HttpContext.CurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.TryAddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
            }
            if (updateLocationDto == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }
            if (!updateLocationDto.FactoryNames.Any(x => !string.IsNullOrEmpty(x)))
            {
                ModelState.TryAddModelError("factoryName", Constants.ResponseMessages.Inventory.RequiredFactoryName);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            try
            {
                updateLocationDto.UpdateBy = Guid.Parse(userId);
                var result = await _locationService.UpdateLocation(updateLocationDto);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpDelete(commonAPIConstant.Endpoint.InventoryService.Location.deleteLocation)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> DeleteLocation(Guid locationId)
        {
            var userId = HttpContext.CurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
            }
            if(!Guid.TryParse(locationId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(locationId), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            try
            {
                var result = await _locationService.DeleteLocation(locationId, Guid.Parse(userId));
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.Location.locationDetail)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> LocationDetail(Guid locationId)
        {
            if (!Guid.TryParse(locationId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(locationId), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            try
            {
                var result = await _locationService.LocationDetail(locationId);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Location.inventoryAssignedList)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> InventoryAssignedList()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();

                int skip = Int32.Parse(start);
                int pageSize = Int32.Parse(length);
                var result = await _locationService.AssignmentActorList();

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = result?.Data?.Data?.Count() ?? 0,
                    recordsTotal = result?.Data?.Data?.Count() ?? 0,
                    

                    data = result?.Data?.Data?.Skip(skip)?.Take(pageSize)?.ToList() ?? new List<InventoryActorInfoViewModel>()
                };

                return Ok(jsonData);
            }
            catch(Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                var jsonData = new
                {
                    draw = 0,
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = 0
                };

                return Ok(jsonData);
            }
            
        }

        [HttpPut(commonAPIConstant.Endpoint.InventoryService.Location.changeAccountRole)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ChangeAccountRole(Guid userId, int? roleType)
        {
            var currUserId = HttpContext.CurrentUserId();
            if (!Guid.TryParse(currUserId, out _))
            {
                ModelState.AddModelError(nameof(currUserId), Constants.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(userId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            try
            {
                var result = await _locationService.ChangeRole(userId, roleType, Guid.Parse(currUserId));
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPut(commonAPIConstant.Endpoint.InventoryService.Location.changeAccountLocation)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ChangeAccountLocation([FromRoute] Guid userId, [FromBody] List<Guid> locationIds)
        {
            var currUserId = HttpContext.CurrentUserId();
            ModelState.Clear();
            if (!Guid.TryParse(currUserId, out _))
            {
                ModelState.AddModelError(nameof(currUserId), Constants.ResponseMessages.InvalidId);
            }
            if (!Guid.TryParse(userId.ToString(), out _))
            {
                ModelState.AddModelError(nameof(userId), Constants.ResponseMessages.InvalidId);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = Constants.ResponseMessages.InValidValidationMessage
                });
            }

            try
            {
                var result = await _locationService.ChangeLocation(userId, locationIds, Guid.Parse(currUserId));
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.Location.exportActors)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS)]
        public async Task<IActionResult> ExportActors()
        {
            try
            {
                var result = await _locationService.ExportAssignment();
                if (result.Code == StatusCodes.Status200OK)
                {
                    return File(result.Data, Constants.FileResponse.StreamType, "ExportInventoryAssignment.xlsx");
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpGet(commonAPIConstant.Endpoint.InventoryService.Location.deparments)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS, Constants.Permissions.VIEW_ALL_INVENTORY,
            Constants.Permissions.VIEW_CURRENT_INVENTORY, Constants.Permissions.EDIT_INVENTORY,
            Constants.Permissions.VIEW_REPORT)]
        public async Task<IActionResult> Deparments()
        {
            try
            {
                var result = await _locationService.GetDeparments();
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Location.getLocationByDepartments)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS, Constants.Permissions.VIEW_ALL_INVENTORY,
            Constants.Permissions.VIEW_CURRENT_INVENTORY, Constants.Permissions.EDIT_INVENTORY,
            Constants.Permissions.VIEW_REPORT)]
        public async Task<IActionResult> GetLocationByDepartments([FromBody] LocationByDepartmentDto departments)
        {
            try
            {
                var result = await _locationService.GetLocationByDepartments(departments);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }


        [HttpGet(commonAPIConstant.Endpoint.InventoryService.Location.checkLocationAssignedToDoc)]
        public async Task<IActionResult> CheckLocationAssignedToDocument(Guid locationId)
        {
            try
            {
                var result = await _locationService.CanEditLocation(locationId);
                if (result.Code == StatusCodes.Status200OK || result.Code == (int)HttpStatusCodes.UpdateLocationTakeLongTime)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

        [HttpPost(commonAPIConstant.Endpoint.InventoryService.Location.getAuditorByLocations)]
        [Authorize(Constants.Permissions.WEBSITE_ACCESS, Constants.Permissions.VIEW_ALL_INVENTORY,
            Constants.Permissions.VIEW_CURRENT_INVENTORY, Constants.Permissions.EDIT_INVENTORY,
            Constants.Permissions.VIEW_REPORT)]
        public async Task<IActionResult> GetAuditorByLocations([FromBody] AuditorByLocationModel locations)
        {
            try
            {
                var result = await _locationService.GetAuditorByLocations(locations);
                if (result.Code == StatusCodes.Status200OK)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(HttpContext, ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = Constants.ResponseMessages.InternalServer
                });
            }
        }

    }
}
