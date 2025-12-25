namespace BIVN.FixedStorage.Services.Storage.API.Controllers
{
    [ApiController]
    [Route("api/storage")]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;
        public readonly IStorageService _storageService;

        public StorageController(ILogger<StorageController> logger,
                                 IStorageService StorageService
                                )
        {
            _logger = logger;
            _storageService = StorageService;
        }

        #region Input Storage
        /// <summary>
        /// Nhập kho trên mobile
        /// </summary>
        [HttpPost("input")]
        [Authorize(Constants.Permissions.PCB_BUSINESS, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> InputStorage([FromBody] InOutStorageParameterForMobile model)
        {
            List<ResponseModel> responseModels = new();

            if(model == null || model.Params == null || !model.Params.Any())
            {
                var responseModel = new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số lượng nhập kho",
                };

                return BadRequest(responseModel);
            }
            if(model != null && model.Params != null && model.Params.All(x => x.Quantity <= 0) == true)
            {
                var responseModel = new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số lượng nhập kho",
                };

                return BadRequest(responseModel);
            }

            foreach (var inOutStorageDto in model.Params)
            {
                if (inOutStorageDto?.Quantity <= 0)
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.Quantity), "Số lượng không hợp lệ.");
                }else if (inOutStorageDto.Quantity.ToString().Length > 8)
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.Quantity), "Số lượng tối đa 8 ký tự.");
                }

                if (string.IsNullOrEmpty(inOutStorageDto.PositionCode))
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.PositionCode), "Dữ liệu không hợp lệ.");
                }

                if (string.IsNullOrEmpty(inOutStorageDto.SupplierCode))
                {
                    ModelState.TryAddModelError(nameof(inOutStorageDto.SupplierCode), "Mã nhà cung cấp không hợp lệ.");
                }

                if (inOutStorageDto?.TypeOfBusiness == TypeOfBusiness.NULL || !Enum.IsDefined(typeof(TypeOfBusiness), inOutStorageDto.TypeOfBusiness))
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.TypeOfBusiness), "Dữ liệu không hợp lệ.");
                }

                if (inOutStorageDto.TypeOfBusiness == TypeOfBusiness.MC)
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.TypeOfBusiness), "MC không được phép thực hiện hành động này.");
                }

                if (!Guid.TryParse(inOutStorageDto.UserId, out Guid userId))
                {
                    ModelState.TryAddModelError(nameof(inOutStorageDto.UserId), "Id người dùng không hợp lệ.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Dữ liệu không hợp lệ",
                        Data = ModelState.ErrorMessages()
                    });

                    ModelState.Clear();
                    break;
                }

                var inOutResult = await _storageService.InOutStorageActivityAsync((int)PositionHistoryType.Input, (int)inOutStorageDto.TypeOfBusiness, 
                    inOutStorageDto.PositionCode, inOutStorageDto.SupplierCode, userId, inOutStorageDto.Quantity, inOutStorageDto.Reason, inOutStorageDto.EmployeeCode);
                if(inOutResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(inOutResult);
                    break;
                }
            }

            return Ok(new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Nhập kho thành công"
            });
        }
        #endregion

        #region Output Storage
        /// <summary>
        /// Xuất kho trên mobile
        /// </summary>
        [HttpPost("output")]
        [Authorize(Constants.Permissions.PCB_BUSINESS, Constants.Permissions.MC_BUSINESS, Constants.Permissions.FACTORY_DATA_INQUIRY)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> OutputStorage([FromBody] InOutStorageParameterForMobile model)
        {
            List<dynamic> responseModels = new();

            if(model == null || model.Params == null || !model.Params.Any())
            {
                var responseModel = new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số lượng xuất kho",
                };

                return BadRequest(responseModel);
            }
            if (model != null && model.Params != null && model.Params.All(x => x.Quantity <= 0) == true)
            {
                var responseModel = new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng nhập số lượng xuất kho",
                };

                return BadRequest(responseModel);
            }

            foreach (var inOutStorageDto in model.Params)
            {
                if (inOutStorageDto?.Quantity <= 0)
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.Quantity), "Số lượng không hợp lệ.");
                }
                else if (inOutStorageDto.Quantity.ToString().Length > 8)
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.Quantity), "Số lượng tối đa 8 ký tự.");
                }

                if (string.IsNullOrEmpty(inOutStorageDto.PositionCode))
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.PositionCode), "Không được để trống");
                }

                if (string.IsNullOrEmpty(inOutStorageDto.SupplierCode))
                {
                    ModelState.TryAddModelError(nameof(inOutStorageDto.SupplierCode), "Mã nhà cung cấp không hợp lệ");
                }

                if (inOutStorageDto?.TypeOfBusiness == TypeOfBusiness.NULL || !Enum.IsDefined(typeof(TypeOfBusiness), inOutStorageDto.TypeOfBusiness))
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.TypeOfBusiness), "Dữ liệu không hợp lệ");
                }

                if (!Guid.TryParse(inOutStorageDto.UserId, out Guid userId))
                {
                    ModelState.AddModelError(nameof(inOutStorageDto.UserId), "Id người dùng không hợp lệ");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Dữ liệu không hợp lệ",
                        Data = ModelState.ErrorMessages()
                    });

                    ModelState.Clear();
                    break;
                }

                var inOutResult = await _storageService.InOutStorageActivityAsync((int)PositionHistoryType.Output, (int)inOutStorageDto.TypeOfBusiness, inOutStorageDto.PositionCode,
                    inOutStorageDto.SupplierCode, userId, inOutStorageDto.Quantity, inOutStorageDto.Reason, inOutStorageDto.EmployeeCode);
                if (inOutResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(inOutResult);
                    break;
                }
            }

            return Ok(new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Xuất kho thành công"
            });
        }
        #endregion

        #region  Import Input storage

        [HttpPost("import/{userId}")]
        [Authorize(Constants.Permissions.INPUT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> InputStorage(string userId, IFormFile file)
        {
            var allowExtension = new string[] { ".csv" };

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Id người dùng không hợp lệ"
                });
            }

            if (file == null || file.Length <= 0)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "File sai định dạng, vui lòng chọn lại file."
                });
            }

            string fileExtension = Path.GetExtension(file.FileName);
            if (allowExtension.Contains(fileExtension) == false)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "File sai định dạng, vui lòng chọn lại file."
                });
            }

            //Kiểm tra nếu lần nhập trước chưa xác nhận thì chưa được nhập mới
            var allowImportResult = await _storageService.AllowImport();
            if (allowImportResult.Data == false)
            {
                return BadRequest(allowImportResult);
            }

            var importResult = await _storageService.Import(userId, file);
            if (importResult.Code == StatusCodes.Status200OK)
            {
                var fileByte = importResult.Data.ExcelFile;
                var importedList = importResult.Data.AllImportRecords;

                var validRow = importedList?.Where(x => x.Valid)?.Count() ?? 0;
                var invalidRow = importedList?.Where(x => !x.Valid)?.Count() ?? 0;

                var currDate = DateTime.Now.ToString(Constants.DatetimeFormat);
                var fileType = Constants.FileResponse.ExcelType;
                var fileName = string.Format("Import_NhapKho_{0}.xlsx", currDate);

                return Ok(new ImportResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Bytes = fileByte,
                    FileName = fileName,
                    FileType = fileType,
                    FailCount = invalidRow,
                    SuccessCount = validRow,
                    Message = importResult.Message
                });
            }

            return BadRequest(importResult);
        }
        #endregion

        [HttpPost("input-storage")]
        [Authorize(Constants.Permissions.INPUT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> InputStorageList()
        {
            const string dateFormat = Constants.DayMonthYearFormat;

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            var userId = Request.Form["userId"].FirstOrDefault();
            var userName = Request.Form["userName"].FirstOrDefault();
            var fromDate = Request.Form["fromDate"].FirstOrDefault();
            var toDate = Request.Form["toDate"].FirstOrDefault();

            var factories = Request.Form["factories[]"].ToList();
            var statuses = Request.Form["statuses[]"].ToList();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            InputStorageListQueryModel queryModel = new();
            queryModel.UserId = userId;
            queryModel.UserName = userName;

            if (!string.IsNullOrEmpty(fromDate))
            {
                if (DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateFrom))
                {
                    queryModel.DateFrom = parsedDateFrom;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    queryModel.DateFrom = null;
                }
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                if (DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTo))
                {
                    queryModel.DateTo = parsedDateTo;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    queryModel.DateTo = null;
                }
            }
            queryModel.Factories = factories;
            queryModel.Statuses = statuses;
            queryModel.Skip = skip;
            queryModel.PageSize = pageSize;

            var storageListResult = await _storageService.GetInputStorageList(queryModel);
            if (storageListResult?.Data?.Data != null)
            {
                var recordsTotal = storageListResult.Data.TotalRecords;
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = storageListResult.Data.Data };
                return Ok(jsonData);
            }

            var jsonData2 = new { draw = draw, recordsFiltered = 0, recordsTotal = 0, data = Enumerable.Empty<InputStorageListModel>() };
            return Ok(jsonData2);
        }

        [HttpPost("input-storage/details")]
        [Authorize(Constants.Permissions.INPUT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> InputStorageDetails()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var inputId = Request.Form["inputId"].FirstOrDefault();
            var userId = Request.Form["userId"].FirstOrDefault();
            var isAllFactories = Request.Form["factories"].FirstOrDefault();
            var factories = Request.Form["factories[]"].ToList();

            FilterInputDetailModel filterModel = new();
            filterModel.UserId = userId;
            filterModel.InputId = inputId;
            filterModel.IsAllFactories = isAllFactories;
            filterModel.Factories = factories;

            var inputDetailsResult = await _storageService.GetInputDetails(filterModel);

            int remainingTypeConverted = (int)InputDetailType.REMAIN;
            var totalQuantity = inputDetailsResult?.Data?.Where(x => x.InputId.Value == Guid.Parse(inputId))?.Sum(x => Convert.ToDouble(x.Quantity)) ?? default;
            var remainingNumber = inputDetailsResult?.Data?.Where(x => x.Type == remainingTypeConverted)?.Sum(x => Convert.ToDouble(x.Quantity)) ?? default;

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = inputDetailsResult?.Data?.Count() ?? 0,
                recordsTotal = inputDetailsResult?.Data?.Count() ?? 0,
                data = inputDetailsResult?.Data?.Skip(skip)?.Take(pageSize) ?? null,
                totalNumber = totalQuantity,
                remainingNumber = remainingNumber
            };

            return Ok(jsonData);
        }

        [HttpPut("input-storage/edit/{inputDetailId}")]
        [Authorize(Constants.Permissions.INPUT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> InputStorageDetails(string inputDetailId, UpdateInputDetailDto updateInputDetailModel)
        {
            if (string.IsNullOrEmpty(inputDetailId) && !Guid.TryParse(inputDetailId, out _))
            {
                ModelState.AddModelError(nameof(inputDetailId), "Dữ liệu không hợp lệ");
            }
            if (updateInputDetailModel == null)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = "Vui lòng cung cấp dữ liệu hợp lệ"
                });
            }
            if (string.IsNullOrEmpty(updateInputDetailModel.Note))
            {
                ModelState.AddModelError(nameof(updateInputDetailModel.Note), "Dữ liệu không hợp lệ");
            }
            if (updateInputDetailModel?.Note?.Length < 0)
            {
                ModelState.AddModelError(nameof(updateInputDetailModel.Note), "Vui lòng nhập ít nhất 1 kí tự");
            }
            if (updateInputDetailModel?.Note?.Length > 50)
            {
                ModelState.AddModelError(nameof(updateInputDetailModel.Note), "Tối đa 50 kí tự");
            }
            if (updateInputDetailModel.Quantity <= 0)
            {
                ModelState.AddModelError(nameof(updateInputDetailModel.Note), "Nhập số lượng từ 1 trở lên");
            }
            if (updateInputDetailModel.Quantity.ToString().Length > 8)
            {
                ModelState.AddModelError(nameof(updateInputDetailModel.Note), "Số lượng nhập tối đa 8 kí tự");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var updateInputDetailResult = await _storageService.UpdateInputDetail(Guid.Parse(inputDetailId), updateInputDetailModel);

            if (updateInputDetailResult.Code == StatusCodes.Status200OK)
            {
                return Ok(updateInputDetailResult);
            }

            return BadRequest(updateInputDetailResult);
        }

        /// <summary>
        /// Phần chi tiết lần nhập kho => Cập nhật trạng thái trả về Bwin hoặc lưu tạm
        /// </summary>
        [HttpPut("input-storage/update-remaininghandle/{inputDetailId}/{status}")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> ChangeInputDetailStatus(string inputDetailId, RemainingHanle status)
        {
            if (string.IsNullOrEmpty(inputDetailId) || !Guid.TryParse(inputDetailId, out _))
            {
                ModelState.TryAddModelError(nameof(inputDetailId), "Mã lần nhập kho không hợp lệ");
            }
            if ((int)status == 0 || !Enum.IsDefined(typeof(RemainingHanle), status))
            {
                ModelState.TryAddModelError(nameof(status), "Trạng thái không hợp lệ");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var changeInputDetailStatusResult = await _storageService.ChangeInputDetailStatus(Guid.Parse(inputDetailId), status);
            if (changeInputDetailStatusResult?.Data == false)
            {
                return BadRequest(changeInputDetailStatusResult);
            }

            return Ok(changeInputDetailStatusResult);
        }

        [HttpPost("input-storage/confirm/{inputId}/{userId}")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> ConfirmImport(string userId, string inputId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                {
                    ModelState.TryAddModelError(nameof(userId), "Id không hợp lệ");
                }

                if (string.IsNullOrEmpty(inputId) || !Guid.TryParse(inputId, out _))
                {
                    ModelState.TryAddModelError(nameof(inputId), "Id không hợp lệ");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Data = ModelState.ErrorMessages(),
                        Message = "Dữ liệu không hợp lệ."
                    });
                }

                var confirmImportResult = await _storageService.ConfirmImport(Guid.Parse(userId), Guid.Parse(inputId));
                if(confirmImportResult.Code != StatusCodes.Status200OK)
                {
                    return BadRequest(confirmImportResult);
                }

                //Trả về file
                var successCount = confirmImportResult.Data.AllImportRecords?.Where(x => x.Valid)?.Count() ?? default;
                var failCount = confirmImportResult.Data.AllImportRecords?.Where(x => !x.Valid)?.Count() ?? default;

                var currDate = DateTime.Now.ToString(Constants.DatetimeFormat);
                var fileType = Constants.FileResponse.ExcelType;
                var fileName = string.Format("XacNhanNhapKho_{0}.xlsx", currDate);

                return Ok(new ImportResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Bytes = confirmImportResult.Data.ExcelFile,
                    FileName = fileName,
                    FileType = fileType,
                    FailCount = failCount,
                    SuccessCount = successCount,
                    Message = confirmImportResult.Message
                });

            }
            catch (Exception ex)
            {
                _logger.LogError("Có lỗi khi thực hiện xác nhận nhập kho.");
                _logger.LogError(ex.Message);

                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống khi thực hiện xác nhận nhập kho."
                });
            }
        }


        [HttpDelete("input-storage/{inputId}")]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<bool>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> DeleteBwinImport(string inputId)
        {
            if (string.IsNullOrEmpty(inputId) || !Guid.TryParse(inputId, out _))
            {
                ModelState.TryAddModelError(nameof(inputId), "Id không hợp lệ");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = ModelState.ErrorMessages(),
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            //Xóa lần nhập này
            var result = await _storageService.DeleteBwinImport(Guid.Parse(inputId));
            if (result.Data == false)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("input-storage/export/{userId}")]
        [Authorize(Constants.Permissions.INPUT)]
        [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ResponseModel<>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ResponseModel))]
        public async Task<IActionResult> Export(string userId)
        {
            const string dateFormat = Constants.DayMonthYearFormat;
            var userName = Request.Form["userName"].FirstOrDefault();
            var fromDate = Request.Form["fromDate"].FirstOrDefault();
            var toDate = Request.Form["toDate"].FirstOrDefault();
            var factories = Request.Form["factories"].ToList();
            var statuses = Request.Form["statuses"].ToList();

            InputStorageListQueryModel queryModel = new();
            queryModel.UserName = userName;
            queryModel.Factories = factories;
            queryModel.Statuses = statuses;

            if (!string.IsNullOrEmpty(fromDate))
            {
                if (DateTime.TryParseExact(fromDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateFrom))
                {
                    queryModel.DateFrom = parsedDateFrom;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    queryModel.DateFrom = null;
                }
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                if (DateTime.TryParseExact(toDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTo))
                {
                    queryModel.DateTo = parsedDateTo;
                }
                else
                {
                    _logger.LogError("Có lỗi khi định dạng thời gian dd/MM/yyyy");
                    queryModel.DateTo = null;
                }
            }

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
            {
                return BadRequest(new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Id người dùng không hợp lệ"
                });
            }

            var exportResult = await _storageService.Export(userId, queryModel);
            if(exportResult.Code != StatusCodes.Status200OK)
            {
                return BadRequest(exportResult);
            }

            var fileBytes = exportResult.Data as byte[];

            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return File(fileBytes, commonAPIConstant.FileResponse.ExcelType, "EXPORT_STORAGE_RESULT.xlsx");
        }
    }
}
