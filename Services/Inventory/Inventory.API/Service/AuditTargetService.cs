using BIVN.FixedStorage.Services.Common.API.Helpers;

namespace Inventory.API.Service
{
    public class AuditTargetWebService : IAuditTargetWebService
    {
        private readonly ILogger<AuditTargetWebService> _logger;
        private readonly InventoryContext _inventoryContext;
        private readonly HttpContext _httpContext;

        public AuditTargetWebService(ILogger<AuditTargetWebService> logger,
                                    InventoryContext inventoryContext,
                                    IHttpContextAccessor httpContextAccessor
                                )
        {
            _logger = logger;
            _inventoryContext = inventoryContext;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ResponseModel<BIVN.FixedStorage.Services.Common.API.AuditTargetViewModel>> GetAuditTargetDetail(Guid inventoryId, Guid auditTargetId)
        {
            var auditTarget = await _inventoryContext.AuditTargets.AsNoTracking()
                                                                        .FirstOrDefaultAsync(x => x.Id == auditTargetId &&
                                                                                                  x.InventoryId == inventoryId);
            if (auditTarget == null)
            {
                return new ResponseModel<BIVN.FixedStorage.Services.Common.API.AuditTargetViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.NotFound
                };
            }

            var inventoryAccounts = await _inventoryContext.InventoryAccounts.AsNoTracking()
                                                                            .ToDictionaryAsync(x => x.UserId, x => x.UserName);

            BIVN.FixedStorage.Services.Common.API.AuditTargetViewModel auditTargetViewModel = new();
            auditTargetViewModel.Id = auditTarget.Id;
            auditTargetViewModel.InventoryId = auditTarget.InventoryId;
            auditTargetViewModel.Plant = auditTarget.Plant ?? string.Empty;
            auditTargetViewModel.LocationName = auditTarget.LocationName ?? string.Empty;
            auditTargetViewModel.ComponentCode = auditTarget.ComponentCode ?? string.Empty;
            auditTargetViewModel.SaleOrderNo = auditTarget.SaleOrderNo ?? string.Empty;
            auditTargetViewModel.PositionCode = auditTarget.PositionCode ?? string.Empty;
            auditTargetViewModel.Status = (int)auditTarget.Status;
            auditTargetViewModel.WHLOC = auditTarget.WareHouseLocation;
            auditTargetViewModel.AssigneeName = inventoryAccounts.ContainsKey(auditTarget.AssignedAccountId) ? inventoryAccounts[auditTarget.AssignedAccountId] : string.Empty;

            return new ResponseModel<BIVN.FixedStorage.Services.Common.API.AuditTargetViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = auditTargetViewModel,
            };
        }


        private bool ValidatePlant(UpdateAuditTargetDto updateAuditTargetDto, out string err)
        {
            err = string.Empty;

            List<string> PlantTemplate = Constants.ValidationRules.Plant.Keys;
            //Validate plant
            if (string.IsNullOrEmpty(updateAuditTargetDto.Plant))
            {
                err = "Vui lòng nhập plant.";
                return false;
            }

            if (updateAuditTargetDto.Plant.Length != 4)
            {
                err = "Plant không đúng";
                return false;
            }

            var validPlantInTemplate = PlantTemplate.Contains(updateAuditTargetDto.Plant);
            if (!validPlantInTemplate)
            {
                err = "Plant không đúng.";
                return false;
            }

            return true;
        }
        private bool ValidateComponentCode(UpdateAuditTargetDto updateAuditTargetDto, Guid inventoryId, out string err)
        {
            err = string.Empty;

            //Validate mã linh kiện
            if (string.IsNullOrEmpty(updateAuditTargetDto.ComponentCode))
            {
                err = "Vui lòng nhập mã linh kiện.";
                return false;
            }

            if (updateAuditTargetDto.ComponentCode.Length < 9 || updateAuditTargetDto.ComponentCode.Length > 12)
            {
                err = $"Độ dài ký tự không hợp lệ";
                return false;
            }

            if (!StringHelper.HasOnlyNormalEnglishCharacters(updateAuditTargetDto.ComponentCode))
            {
                err = "Mã linh kiện chỉ chứa ký tự tự chữ, số.";
                return false;
            }

            //Mã linh kiện có tồn tại trên phiếu A không
            var existInDocAWithComponent = _inventoryContext.InventoryDocs.AsNoTracking().Any(x => x.InventoryId == inventoryId
                                                                            && x.DocType == InventoryDocType.A
                                                                            && x.ComponentCode == updateAuditTargetDto.ComponentCode);

            if (!existInDocAWithComponent)
            {
                err = $"Chưa tồn tại mã linh kiện này trên phiếu A.";
                return false;
            }

            var existDocA = _inventoryContext.InventoryDocs.AsNoTracking().Any(x => x.InventoryId == inventoryId
                                                                            && x.DocType == InventoryDocType.A
                                                                            && x.Plant == updateAuditTargetDto.Plant
                                                                            && x.WareHouseLocation == updateAuditTargetDto.WHLOC
                                                                            && x.ComponentCode == updateAuditTargetDto.ComponentCode);
            if (!existDocA)
            {
                err = $"Mã linh kiện {updateAuditTargetDto.ComponentCode} có plant {updateAuditTargetDto.Plant} và WH Loc. {updateAuditTargetDto.WHLOC} không tồn tại trên phiếu A.";
                return false;
            }

            //Nếu người dùng nhập Mã linh kiện có 9 ký tự nhưng có thông tin trường S/ O No.
            if (updateAuditTargetDto.ComponentCode.Length == 9 && !string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo))
            {
                err = $"Mã linh kiện {updateAuditTargetDto.ComponentCode} không thể có số S/O No.";
                return false;
            }

            //Nếu người dùng nhập Mã linh kiện có 10 ký tự hoặc 11 ký tự nhưng trống thông tin trường S/ O No
            if (updateAuditTargetDto.ComponentCode.Length == 10 && string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo))
            {
                err = $"Mã linh kiện {updateAuditTargetDto.ComponentCode} thiếu số S/O No.";
                return false;
            }

            return true;
        }
        private bool ValidateSaleOrderNo(UpdateAuditTargetDto updateAuditTargetDto, out string err)
        {
            err = string.Empty;

            //Validate S/O No.
            if (!string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo))
            {
                if (updateAuditTargetDto.SaleOrderNo.Length > 25)
                {
                    err = "Tối đa 25 ký tự.";
                    return false;
                }
            }

            return true;
        }
        private bool ValidatePositionCode(UpdateAuditTargetDto updateAuditTargetDto, out string err)
        {
            err = string.Empty;

            //Validate PositionCode
            if (string.IsNullOrEmpty(updateAuditTargetDto.PositionCode))
            {
                err = "Vui lòng nhập vị trí.";
                return false;
            }
            if (updateAuditTargetDto.PositionCode.Length > 20)
            {
                err = "Tối đa 20 ký tự.";
                return false;
            }

            return true;
        }
        private bool ValidateWHLOC(UpdateAuditTargetDto updateAuditTargetDto, out string err)
        {
            err = string.Empty;

            List<string> WHLocTemplate = new List<string> { "S001", "S002", "S402", "S090" };

            //Validate WHLoc.
            if (string.IsNullOrEmpty(updateAuditTargetDto.WHLOC))
            {
                err = "Vui lòng nhập WH Loc.";
                return false;
            }

            if (updateAuditTargetDto.WHLOC.Length != 4)
            {
                err = "WH Loc không đúng.";
                return false;
            }

            var validWHLOC = WHLocTemplate.Contains(updateAuditTargetDto.WHLOC);
            if (!validWHLOC)
            {
                err = "WH Loc không đúng.";
                return false;
            }

            return true;
        }
        private bool ValidateLocation(UpdateAuditTargetDto updateAuditTargetDto, out string err)
        {
            err = string.Empty;

            //Validate khu vực
            if (string.IsNullOrEmpty(updateAuditTargetDto.LocationName))
            {
                err = "Vui lòng nhập khu vực.";
                return false;
            }
            if (updateAuditTargetDto.LocationName.Length > 50)
            {
                err = "Tối đa 50 ký tự.";
                return false;
            }
            //Check tên khu vực có tồn tại trong quản lý khu vực không
            var locationMatch = _inventoryContext.InventoryLocations.FirstOrDefault(x => x.Name == updateAuditTargetDto.LocationName);
            if (locationMatch == null)
            {
                err = "Khu vực không tồn tại.";
                return false;
            }
            else if (locationMatch != null)
            {
                //Check đã được gán cho người giám sát chưa
                var locationsAssignedAuditor = _inventoryContext.AccountLocations.Include(x => x.InventoryAccount)
                                                                                 .Include(x => x.InventoryLocation)
                                                                                 .AsNoTracking()
                                                                                 .Any(x => !x.InventoryLocation.IsDeleted &&
                                                                                              x.InventoryAccount.RoleType.Value == InventoryAccountRoleType.Audit &&
                                                                                              x.InventoryLocation.Name == updateAuditTargetDto.LocationName);

                if (!locationsAssignedAuditor)
                {
                    err = "Khu vực chưa được gán cho người giám sát.";
                    return false;
                }
            }

            return true;
        }
        private bool ValidateAssigneeAccount(UpdateAuditTargetDto updateAuditTargetDto, out string err)
        {
            err = string.Empty;
            if (string.IsNullOrEmpty(updateAuditTargetDto.AssigneeAccount))
            {
                err = "Vui lòng nhập tài khoản phân phát.";
                return false;
            }

            //Kiểm tra tài khoản phân phát có tồn tại không
            var existAccount = _inventoryContext.InventoryAccounts.AsNoTracking().Any(x => x.UserName == updateAuditTargetDto.AssigneeAccount);
            if (!existAccount)
            {
                err = "Tài khoản phân phát không tồn tại.";
                return false;
            }

            //Kiểm tra tài khoản này có được phân khu vực phân phát đã nhập không
            var assignedLocations = _inventoryContext.AccountLocations.Include(x => x.InventoryAccount)
                                                .Include(x => x.InventoryLocation)
                                                .AsNoTracking()
                                                .Where(x => !x.InventoryLocation.IsDeleted &&
                                                            x.InventoryAccount.UserName == updateAuditTargetDto.AssigneeAccount)
                                                 .Select(x => x.InventoryLocation.Name).ToList();

            var validLocation = assignedLocations?.Contains(updateAuditTargetDto.LocationName) == true;
            if (!validLocation)
            {
                err = "Khu vực và tài khoản phân phát không được gán với nhau trong chức năng phân quyền thao tác.";
                return false;
            }

            return true;
        }

        public async Task<ResponseModel<Dictionary<string, string>>> UpdateAuditTarget(Guid inventoryId, Guid auditTargetId, UpdateAuditTargetDto updateAuditTargetDto)
        {
            Dictionary<string, string> errs = new();

            updateAuditTargetDto?.Plant?.Trim();
            updateAuditTargetDto?.WHLOC?.Trim();
            updateAuditTargetDto?.PositionCode?.Trim();
            updateAuditTargetDto?.LocationName?.Trim();
            updateAuditTargetDto?.SaleOrderNo?.Trim();
            updateAuditTargetDto?.ComponentCode?.Trim();
            updateAuditTargetDto?.AssigneeAccount?.Trim();

            if (!ValidatePlant(updateAuditTargetDto, out string err))
            {
                errs.TryAdd(nameof(UpdateAuditTargetDto.Plant), err);
            }
            if (!ValidateWHLOC(updateAuditTargetDto, out string errWHLoc))
            {
                errs.TryAdd(nameof(UpdateAuditTargetDto.WHLOC), errWHLoc);
            }

            var validateLocationResult = ValidateLocation(updateAuditTargetDto, out string errLocationName);
            if (!validateLocationResult)
            {
                errs.TryAdd(nameof(UpdateAuditTargetDto.LocationName), errLocationName);
            }

            if (!ValidateAssigneeAccount(updateAuditTargetDto, out string errAssignee))
            {
                errs.TryAdd(nameof(UpdateAuditTargetDto.AssigneeAccount), errAssignee);
            }

            var validateComponentResult = ValidateComponentCode(updateAuditTargetDto, inventoryId, out string errComponentCode);
            if (!validateComponentResult)
            {
                errs.TryAdd(nameof(UpdateAuditTargetDto.ComponentCode), errComponentCode);
            }

            if (!ValidateSaleOrderNo(updateAuditTargetDto, out string errSOno))
            {
                errs.TryAdd(nameof(UpdateAuditTargetDto.SaleOrderNo), errSOno);
            }

            if (validateComponentResult && (updateAuditTargetDto.ComponentCode.Length == 10 || updateAuditTargetDto.ComponentCode.Length == 11))
            {
                if (string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo))
                {
                    errs.TryAdd(nameof(UpdateAuditTargetDto.SaleOrderNo), $"Mã linh kiện {updateAuditTargetDto.ComponentCode} thiếu số S/O No.");
                }
            }
            if (validateComponentResult && (updateAuditTargetDto.ComponentCode.Length == 9))
            {
                if (!string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo))
                {
                    errs.TryAdd(nameof(UpdateAuditTargetDto.SaleOrderNo), $"Mã linh kiện {updateAuditTargetDto.ComponentCode} không thể có số S/O No.");
                }
            }

            if (!ValidatePositionCode(updateAuditTargetDto, out string errPositionCode))
            {
                errs.TryAdd(nameof(UpdateAuditTargetDto.PositionCode), errPositionCode);
            }

            if (errs.Any())
            {
                return new ResponseModel<Dictionary<string, string>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = errs,
                    Message = "Dữ liệu không hợp lệ."
                };
            }

            //Tìm phiếu A để check validate
            var existDocA = await _inventoryContext.InventoryDocs.AnyAsync(x => x.InventoryId == inventoryId &&
                                                                                      x.Plant == updateAuditTargetDto.Plant &&
                                                                                      x.ComponentCode == updateAuditTargetDto.ComponentCode &&
                                                                                      x.WareHouseLocation == updateAuditTargetDto.WHLOC);
            if (existDocA == false)
            {
                return new ResponseModel<Dictionary<string, string>>
                {
                    Code = (int)HttpStatusCodes.UpdateAuditInfoNotExistInDocA,
                    Message = $"Thông tin mã linh kiện {updateAuditTargetDto.ComponentCode} " +
                              $"có plant {updateAuditTargetDto.Plant} và " +
                              $"WH Loc {updateAuditTargetDto.WHLOC} không tồn tại trong phiếu A."
                };
            }

            //Validate lần 2
            var auditTarget = await _inventoryContext.AuditTargets.FirstOrDefaultAsync(x => x.Id == auditTargetId && x.InventoryId == inventoryId);
            if (auditTarget == null)
            {
                return new ResponseModel<Dictionary<string, string>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy linh kiện giám sát."
                };
            }

            //Từ tài khoản phân phát người dùng chỉnh sửa hợp lệ => cập nhật tài khoản giám sát mới
            var newAuditor = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserName == updateAuditTargetDto.AssigneeAccount);

            auditTarget.LocationName = updateAuditTargetDto.LocationName;
            auditTarget.ComponentCode = updateAuditTargetDto.ComponentCode;
            auditTarget.SaleOrderNo = updateAuditTargetDto.SaleOrderNo;
            auditTarget.PositionCode = updateAuditTargetDto.PositionCode;
            auditTarget.Plant = updateAuditTargetDto.Plant;
            auditTarget.WareHouseLocation = updateAuditTargetDto.WHLOC;
            auditTarget.AssignedAccountId = newAuditor.UserId;

            var docMatchComponent = await _inventoryContext.InventoryDocs.AsNoTracking().FirstOrDefaultAsync(x => x.InventoryId == inventoryId
                                                                                                         && x.ComponentCode == updateAuditTargetDto.ComponentCode);
            if (docMatchComponent != null) auditTarget.ComponentName = docMatchComponent.ComponentName;
            //Audit 
            string? currUserId = _httpContext?.CurrentUserId();
            auditTarget.UpdatedAt = DateTime.Now;
            auditTarget.UpdatedBy = currUserId ?? null;

            //Xác định phiếu giám sát để cập nhật thông tin linh kiện
            var docs = _inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).AsQueryable();

            if (updateAuditTargetDto.Plant == Constants.ValidationRules.Plant.L401 && !string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo))
            {
                docs = docs.Where(x => x.Plant == updateAuditTargetDto.Plant && x.WareHouseLocation == updateAuditTargetDto.WHLOC && x.SalesOrderNo == updateAuditTargetDto.SaleOrderNo);

            }
            else if (updateAuditTargetDto.Plant == Constants.ValidationRules.Plant.L402)
            {
                docs = docs.Where(x => x.Plant == updateAuditTargetDto.Plant && x.WareHouseLocation == updateAuditTargetDto.WHLOC && x.ComponentCode == updateAuditTargetDto.ComponentCode);
            }
            else if (updateAuditTargetDto.Plant == Constants.ValidationRules.Plant.L404 || (updateAuditTargetDto.Plant == Constants.ValidationRules.Plant.L401 && string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo)))
            {
                docs = docs.Where(x => x.Plant == updateAuditTargetDto.Plant && x.WareHouseLocation == updateAuditTargetDto.WHLOC && x.ComponentCode == updateAuditTargetDto.ComponentCode);
            }
            if (docs.Any())
            {
                var updateDocs = await docs.ToListAsync();
                foreach (var doc in updateDocs)
                {
                    doc.Plant = updateAuditTargetDto.Plant;
                    doc.WareHouseLocation = updateAuditTargetDto.WHLOC;
                    doc.LocationName = updateAuditTargetDto.LocationName;
                    doc.ComponentCode = updateAuditTargetDto.ComponentCode;
                    doc.PositionCode = updateAuditTargetDto.PositionCode;

                    if (!string.IsNullOrEmpty(updateAuditTargetDto.SaleOrderNo)) doc.SalesOrderNo = updateAuditTargetDto.SaleOrderNo;
                }

                _inventoryContext.UpdateRange(updateDocs);
            }

            //Save changes
            await _inventoryContext.SaveChangesAsync();

            return new ResponseModel<Dictionary<string, string>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Chỉnh sửa linh kiện giám sát thành công."
            };
        }

        public async Task<InventoryResponseModel<IEnumerable<ListAuditTargetViewModel>>> ListAuditTarget(ListAuditTargetDto listAuditTargetDto, Guid inventoryId)
        {
            var componentNames = _inventoryContext.InventoryDocs.AsNoTracking()
                    .Where(doc => doc.InventoryId == inventoryId && doc.DocType == 0)
                    .GroupBy(doc => new { doc.ComponentCode, doc.ComponentName })
                    .Select(g => new { g.Key.ComponentCode, g.Key.ComponentName });

            var result = from a in _inventoryContext.AuditTargets.AsNoTracking()
                         join in_ac in _inventoryContext.InventoryAccounts.AsNoTracking() on a.AssignedAccountId equals in_ac.UserId into joinedData2
                         from ai2 in joinedData2.DefaultIfEmpty()
                         where a.InventoryId == inventoryId
                         // orderby a.LocationName, a.PositionCode
                         select new ListAuditTargetViewModel
                         {
                             InventoryId = a.InventoryId,
                             Plant = a.Plant,
                             WHLoc = a.WareHouseLocation,
                             Location = a.LocationName,
                             ComponentCode = a.ComponentCode,
                             SaleOrderNo = a.SaleOrderNo,
                             Position = a.PositionCode,
                             ComponentName = componentNames.Where(c => c.ComponentCode == a.ComponentCode)
                                               .Select(c => c.ComponentName)
                                               .FirstOrDefault() ?? string.Empty,
                             AuditTargetId = a.Id,
                             Status = (int)(a.Status),
                             AssigneeAccount = ai2.UserName,
                             DepartmentName = a.DepartmentName
                         };

            if (result.Count() == 0)
            {
                return new InventoryResponseModel<IEnumerable<ListAuditTargetViewModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy danh sách phiếu giám sát.",
                };
            }

            var predictBuilder = PredicateBuilder.New<ListAuditTargetViewModel>(true);

            //Tim kiem theo ma linh kien:
            if (!string.IsNullOrEmpty(listAuditTargetDto.ComponentCode))
            {
                predictBuilder = predictBuilder.And(x => x.ComponentCode.ToLower().Contains(listAuditTargetDto.ComponentCode.ToLower()));
                //result = result.Where(x => x.ComponentCode.ToLower().Contains(listAuditTargetDto.ComponentCode.ToLower()));
            }

            //Tim kiem theo SaleOrderNo:
            if (!string.IsNullOrEmpty(listAuditTargetDto.SaleOrderNo))
            {
                predictBuilder = predictBuilder.And(x => x.SaleOrderNo.ToLower().Contains(listAuditTargetDto.SaleOrderNo.ToLower()));
                //result = result.Where(x => x.SaleOrderNo.ToLower().Contains(listAuditTargetDto.SaleOrderNo.ToLower()));
            }

            //Tim kiem theo Position:
            if (!string.IsNullOrEmpty(listAuditTargetDto.Position))
            {
                predictBuilder = predictBuilder.And(x => x.Position.ToLower().Contains(listAuditTargetDto.Position.ToLower()));
                //result = result.Where(x => x.Position.ToLower().Contains(listAuditTargetDto.Position.ToLower()));
            }

            //Tim kiem theo Tai khoan phan phat:
            if (!string.IsNullOrEmpty(listAuditTargetDto.AssigneeAccount))
            {
                predictBuilder = predictBuilder.And(x => x.AssigneeAccount.ToLower().Contains(listAuditTargetDto.AssigneeAccount.ToLower()));
                //result = result.Where(x => x.AssigneeAccount.ToLower().Contains(listAuditTargetDto.AssigneeAccount.ToLower()));
            }

            //Tim kiem theo Statuses:
            if (listAuditTargetDto.Statuses?.Any() == true)
            {
                predictBuilder = predictBuilder.And(x => listAuditTargetDto.Statuses.Select(x => int.Parse(x)).Contains(x.Status));
                //result = result.Where(x => listAuditTargetDto.Statuses.Select(x => int.Parse(x)).Contains(x.Status));
            }

            //Tim kiem theo Departments:
            if (listAuditTargetDto.Departments?.Any() == true)
            {
                predictBuilder = predictBuilder.And(x => listAuditTargetDto.Departments.Select(x => x.ToLower()).Contains(x.DepartmentName.ToLower()));
                //result = result.Where(x => listAuditTargetDto.Departments.Select(x => x.ToLower()).Contains(x.DepartmentName.ToLower()));
            }

            //Tim kiem theo Locations:
            if (listAuditTargetDto.Locations?.Any() == true)
            {
                predictBuilder = predictBuilder.And(x => listAuditTargetDto.Locations.Select(x => x.ToLower()).Contains(x.Location.ToLower()));
                //result = result.Where(x => listAuditTargetDto.Locations.Select(x => x.ToLower()).Contains(x.Location.ToLower()));
            }
            var items = result.Where(predictBuilder).ToList();
            var totalRecords = items.Count();
            var itemsFromQuery = new List<ListAuditTargetViewModel>();
            if (listAuditTargetDto.IsExport)
            {
                itemsFromQuery = items;
            }
            else
            {
                itemsFromQuery = items.Skip(listAuditTargetDto.Skip).Take(listAuditTargetDto.Take).ToList();
            }

            return new InventoryResponseModel<IEnumerable<ListAuditTargetViewModel>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Danh sách giám sát thành công.",
                Data = itemsFromQuery,
                TotalRecords = totalRecords,
            };
        }
    }
}
