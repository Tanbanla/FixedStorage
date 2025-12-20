using BIVN.FixedStorage.Services.Common.API.Dto.Report;
using Inventory.API.Infrastructure.Entity;

namespace Inventory.API.Service
{
    public class ReportService : IReportService
    {
        private readonly ILogger<ReportService> _logger;
        private readonly InventoryContext _inventoryContext;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly RestClientFactory _restClientFactory;

        public ReportService(ILogger<ReportService> logger,
                               InventoryContext inventoryContext,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,
                               RestClientFactory restClientFactory
                            )
        {
            _logger = logger;
            _inventoryContext = inventoryContext;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _restClientFactory = restClientFactory;
        }

        public async Task<ResponseModel<ProgressReportResult>> ProgressReport(ProgressReportDto progressReport)
        {
            var getInventorys = _inventoryContext.Inventories.FirstOrDefault(x => x.Id.ToString().ToLower() == progressReport.InventoryId.ToLower());

            //Chưa đến ngày kiểm kê thì không hiển thị dữ liệu báo cáo:
            //if(DateTime.Now.Date < getInventorys.InventoryDate.Date)
            //{
            //    return new ResponseModel<ProgressReportResult>
            //    {
            //        Code = (int)HttpStatusCodes.NotYetInventoryDate,
            //        Message = "Không có dữ liệu báo cáo do chưa đến ngày kiểm kê.",
            //    };
            //}

            var currUser = _httpContext.UserFromContext();

            var getReportDocType = _inventoryContext.ReportingDocTypes.AsNoTracking().Where(x => x.InventoryId.ToString().ToLower() == progressReport.InventoryId.ToLower() && (int)x.CaptureTimeType == progressReport.CaptureTimeType)
                                                                        .Where(x => x.InventoryId.ToString().ToLower() == progressReport.InventoryId.ToLower())
                                                                        //.OrderBy(x => x.DocType)
                                                                        .Select(x => new ProgressReportModel
                                                                        {
                                                                            TotalDoc = x.TotalDoc,
                                                                            TotalTodo = x.TotalTodo,
                                                                            TotalInventory = x.TotalInventory,
                                                                            TotalConfirm = x.TotalConfirm,
                                                                            DocType = (int)x.DocType,
                                                                        });
            //Tim kiem theo loai phieu:
            if (progressReport.DocTypes.Any())
            {
                getReportDocType = getReportDocType.Where(x => progressReport.DocTypes.Contains(x.DocType));
            }

            var getReportDepartment = _inventoryContext.ReportingDepartments.AsNoTracking().Where(x => x.InventoryId.ToString().ToLower() == progressReport.InventoryId.ToLower() && (int)x.CaptureTimeType == progressReport.CaptureTimeType)
                                                                        .Where(x => x.InventoryId.ToString().ToLower() == progressReport.InventoryId.ToLower())
                                                                        //.OrderBy(x => x.DepartmentName)
                                                                        .Select(x => new ProgressReportModel
                                                                        {
                                                                            TotalDoc = x.TotalDoc,
                                                                            TotalTodo = x.TotalTodo,
                                                                            TotalInventory = x.TotalInventory,
                                                                            TotalConfirm = x.TotalConfirm,
                                                                            Department = x.DepartmentName ?? string.Empty
                                                                        });

            var canViewAll = (currUser.AccountType == nameof(AccountType.TaiKhoanRieng) &&
                                        (currUser.IsGrant(commonAPIConstant.Permissions.VIEW_ALL_INVENTORY) ||
                                            currUser.IsGrant(commonAPIConstant.Permissions.VIEW_CURRENT_INVENTORY) ||
                                            currUser.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY) ||
                                            currUser.IsGrant(commonAPIConstant.Permissions.VIEW_REPORT)))
                                        || _httpContext.IsPromoter();

            //Danh sách khu vực được quyền xem
            if (!canViewAll)
            {
                var viewLocationsByRole = _inventoryContext.AccountLocations.Include(x => x.InventoryLocation)
                                                                            .Include(x => x.InventoryAccount)
                                                                            .Where(x => !x.InventoryLocation.IsDeleted
                                                                                        && x.InventoryAccount.UserId == Guid.Parse(currUser.UserId))
                                                                            .Select(x => x.InventoryLocation.DepartmentName);

                getReportDepartment = getReportDepartment.Where(x => viewLocationsByRole.Contains(x.Department) == true);
            }

            //Tim kiem theo danh sach phong ban:
            if (progressReport.Departments.Any())
            {
                getReportDepartment = getReportDepartment.Where(x => !string.IsNullOrEmpty(x.Department) && progressReport.Departments.Contains(x.Department));
            }

            var getReportLocation = _inventoryContext.ReportingLocations.AsNoTracking().Where(x => x.InventoryId.ToString().ToLower() == progressReport.InventoryId.ToLower() && (int)x.CaptureTimeType == progressReport.CaptureTimeType)
                                                                        .Where(x => x.InventoryId.ToString().ToLower() == progressReport.InventoryId.ToLower())
                                                                        //.OrderBy(x => x.LocationName)
                                                                        .Select(x => new ProgressReportModel
                                                                        {
                                                                            TotalDoc = x.TotalDoc,
                                                                            TotalTodo = x.TotalTodo,
                                                                            TotalInventory = x.TotalInventory,
                                                                            TotalConfirm = x.TotalConfirm,
                                                                            Location = x.LocationName ?? string.Empty
                                                                        });

            //Tim kiem theo danh sach khu vuc:
            if (progressReport.Locations.Any())
            {
                getReportLocation = getReportLocation.Where(x => !string.IsNullOrEmpty(x.Location) && progressReport.Locations.Contains(x.Location));
            }

            ProgressReportResult result = new ProgressReportResult();

            result.ProgressReportDocTypes = await getReportDocType.Distinct().OrderBy(x => x.DocType).ToListAsync();
            result.ProgressReportDepartments = await getReportDepartment.Distinct().OrderBy(x => x.Department).ToListAsync();
            result.ProgressReportLocations = await getReportLocation.Distinct().OrderBy(x => x.Location).ToListAsync();

            return new ResponseModel<ProgressReportResult>
            {
                Code = StatusCodes.Status200OK,
                Message = "Báo cáo tiến độ thành công.",
                Data = result
            };
        }
        public async Task<ResponseModel<AuditReportViewModel>> AggregateAuditReport(ProgressReportDto progressReport)
        {
            var inventoryInfo = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == progressReport.InventoryId.ToLower());
            if (inventoryInfo == null)
            {
                return new ResponseModel<AuditReportViewModel>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tìm thấy đợt kiểm kê."
                };
            }

            var currUser = _httpContext.UserFromContext();

            ////Chưa đến ngày kiểm kê thì không hiển thị dữ liệu báo cáo:
            ///Tạm comment lại để debug chức năng
            //if (DateTime.Now.Date < inventoryInfo.InventoryDate.Date)
            //{
            //    return new ResponseModel<AuditReportViewModel>
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = "Không có dữ liệu báo cáo do chưa đến ngày kiểm kê.",
            //    };
            //}

            var auditReports = _inventoryContext.ReportingAudits.AsNoTracking()
                                                    .AsEnumerable()
                                                    .Where(x => x.InventoryId == Guid.Parse(progressReport.InventoryId))
                                                    .OrderBy(x => x.LocationtName)
                                                    .Select(x => new ProgressReportModelAudit
                                                    {
                                                        TotalDoc = x.TotalDoc,
                                                        TotalTodo = x.TotalTodo,
                                                        LocationName = x.LocationtName,
                                                        TotalFail = x.TotalFail,
                                                        TotalPass = x.TotalPass,
                                                    });

            var canViewAll = (currUser.AccountType == nameof(AccountType.TaiKhoanRieng) &&
                                        (currUser.IsGrant(commonAPIConstant.Permissions.VIEW_ALL_INVENTORY) ||
                                            currUser.IsGrant(commonAPIConstant.Permissions.VIEW_CURRENT_INVENTORY) ||
                                            currUser.IsGrant(commonAPIConstant.Permissions.EDIT_INVENTORY) ||
                                            currUser.IsGrant(commonAPIConstant.Permissions.VIEW_REPORT)))
                                        || _httpContext.IsPromoter();

            //Danh sách khu vực được quyền xem
            if (!canViewAll)
            {
                var viewLocationsByRole = _inventoryContext.AccountLocations.Include(x => x.InventoryLocation)
                                                                            .Include(x => x.InventoryAccount)
                                                                            .Where(x => !x.InventoryLocation.IsDeleted
                                                                                        && x.InventoryAccount.UserId == Guid.Parse(currUser.UserId))
                                                                            .Select(x => x.InventoryLocation.Name);

                auditReports = auditReports.Where(x => viewLocationsByRole?.Contains(x.LocationName) == true);
            }

            if (auditReports == null || !auditReports.Any())
            {
                return new ResponseModel<AuditReportViewModel>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu hợp lệ"
                };
            }

            //Tim kiem theo danh sach khu vuc:
            if (progressReport.Locations.Any())
            {
                auditReports = auditReports.Where(x => x.LocationName != null ? progressReport.Locations.Contains(x.LocationName) : false);
            }

            AuditReportViewModel result = new();
            result.ProgressReportLocations = auditReports.ToList();

            return new ResponseModel<AuditReportViewModel>
            {
                Code = StatusCodes.Status200OK,
                Data = result,
            };
        }

        public async Task<ResponseModel<IEnumerable<AuditReportDto>>> AuditReports(AuditReportModel auditReportModel)
        {
            var inventoryInfo = await _inventoryContext.Inventories.FirstOrDefaultAsync(x => x.Id == auditReportModel.InventoryId);
            if (inventoryInfo == null)
            {
                return new ResponseModel<IEnumerable<AuditReportDto>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tìm thấy đợt kiểm kê."
                };
            }


            if (auditReportModel.AuditReportType == AuditReportType.Department)
            {
                //Đếm số lượng phòng ban được gán cho tài khoản giám sát:
                //var departmentAssignees = from ia in _inventoryContext.InventoryAccounts.AsNoTracking()
                //                        join al in _inventoryContext.AccountLocations.AsNoTracking() on ia.Id equals al.AccountId
                //                        join il in _inventoryContext.InventoryLocations.AsNoTracking() on al.LocationId equals il.Id
                //                        where ia.RoleType == InventoryAccountRoleType.Audit
                //                        group ia by new { il.DepartmentName, il.Name } into g
                //                        select new
                //                        {
                //                            DepartmentName = g.Key.DepartmentName,
                //                            LocationName = g.Key.Name,
                //                            NumberOfUsers = g.Count()
                //                        };


                var departmentAuditReports = _inventoryContext.ReportingAudits.AsNoTracking()
                                                    .Where(d => d.InventoryId == auditReportModel.InventoryId && d.DepartmentName != null)
                                                    .GroupBy(d => new { d.DepartmentName, d.Type })
                                                    .Select(g => new AuditReportDto
                                                    {
                                                        Name = g.Key.DepartmentName,
                                                        ReportingAuditType = (ReportingAuditType)g.Key.Type,
                                                        TotalDoc = g.Sum(d => d.TotalDoc),
                                                        TotalTodo = g.Sum(d => d.TotalTodo),
                                                        TotalPass = g.Sum(d => d.TotalPass),
                                                        TotalFail = g.Sum(d => d.TotalFail)
                                                    });

                if (auditReportModel.Departments.Any())
                {
                    departmentAuditReports = departmentAuditReports.Where(x => auditReportModel.Departments.Contains(x.Name));
                }
                if (auditReportModel.ReportingAuditTypes.Any())
                {
                    departmentAuditReports = departmentAuditReports.Where(x => auditReportModel.ReportingAuditTypes.Contains(x.ReportingAuditType));
                }
                var resultDepartmentAuditReports = await departmentAuditReports.ToListAsync();
                //var existedDepartmentAuditReports = _inventoryContext.ReportingAudits.AsNoTracking()
                //                                    .Where(d => d.InventoryId == auditReportModel.InventoryId && d.LocationtName != null)
                //                                    .GroupBy(d => d.LocationtName)
                //                                    .Select(g => new 
                //                                    {
                //                                        LocationName = g.Key
                //                                    });
                // Cập nhật TotalDoc nếu là FreeReportingAudit:
                //foreach (var report in resultDepartmentAuditReports)
                //{
                //    if (report.ReportingAuditType == ReportingAuditType.FreeReportingAudit)
                //    {
                //        var locationNames = departmentAssignees
                //                            .Where(da => da.DepartmentName == report.Name 
                //                                && existedDepartmentAuditReports.Any(e => e.LocationName == da.LocationName))
                //                            .Select(da => da.LocationName)
                //                            .ToList();

                //        report.TotalDoc = departmentAssignees
                //                            .Where(da => locationNames.Contains(da.LocationName))
                //                            .Sum(da => da.NumberOfUsers * 20);
                //    }

                //}

                return new ResponseModel<IEnumerable<AuditReportDto>>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Dữ liệu báo cáo giám sát theo phòng ban thành công.",
                    Data = resultDepartmentAuditReports
                };
            }
            else if (auditReportModel.AuditReportType == AuditReportType.Location)
            {
                //Đếm số lượng phòng ban được gán cho tài khoản giám sát:
                //var locationAssignees = from ia in _inventoryContext.InventoryAccounts.AsNoTracking()
                //                          join al in _inventoryContext.AccountLocations.AsNoTracking() on ia.Id equals al.AccountId
                //                          join il in _inventoryContext.InventoryLocations.AsNoTracking() on al.LocationId equals il.Id
                //                          where ia.RoleType == InventoryAccountRoleType.Audit
                //                          group ia by il.Name into g
                //                          select new
                //                          {
                //                              LocationName = g.Key,
                //                              NumberOfUsers = g.Count()
                //                          };

                var locationAuditReports = _inventoryContext.ReportingAudits.AsNoTracking()
                                                .Where(d => d.InventoryId == auditReportModel.InventoryId && d.LocationtName != null)
                                                .GroupBy(d => new { d.LocationtName, d.DepartmentName, d.Type })
                                                .Select(g => new AuditReportDto
                                                {
                                                    Name = g.Key.LocationtName,
                                                    ParentName = g.Key.DepartmentName,
                                                    ReportingAuditType = (ReportingAuditType)g.Key.Type,
                                                    TotalDoc = g.Sum(d => d.TotalDoc),
                                                    TotalTodo = g.Sum(d => d.TotalTodo),
                                                    TotalPass = g.Sum(d => d.TotalPass),
                                                    TotalFail = g.Sum(d => d.TotalFail)
                                                });

                if (auditReportModel.Locations.Any())
                {
                    locationAuditReports = locationAuditReports.Where(x => auditReportModel.Locations.Contains(x.Name));
                }
                if (auditReportModel.ReportingAuditTypes.Any())
                {
                    locationAuditReports = locationAuditReports.Where(x => auditReportModel.ReportingAuditTypes.Contains(x.ReportingAuditType));
                }

                var resultLocationAuditReports = locationAuditReports.ToList();
                // Cập nhật TotalDoc nếu là FreeReportingAudit
                //foreach (var report in resultLocationAuditReports)
                //{
                //    if (report.ReportingAuditType == ReportingAuditType.FreeReportingAudit)
                //    {
                //        var locationAssignee = locationAssignees.FirstOrDefault(x => x.LocationName == report.Name);
                //        if (locationAssignee != null)
                //        {
                //            report.TotalDoc = locationAssignee.NumberOfUsers * 20;
                //        }
                //    }
                //}

                return new ResponseModel<IEnumerable<AuditReportDto>>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Dữ liệu báo cáo giám sát theo khu vực thành công.",
                    Data = resultLocationAuditReports
                };
            }

            //1 tài khoản giám sát được gán cho 20 phiếu:
            var auditorAuditReports = _inventoryContext.ReportingAudits.AsNoTracking()
                                                .Where(d => d.InventoryId == auditReportModel.InventoryId && d.AuditorName != null)
                                                .GroupBy(d => new { d.AuditorName, d.LocationtName, d.Type })
                                                .Select(g => new AuditReportDto
                                                {
                                                    Name = g.Key.AuditorName,
                                                    ParentName = g.Key.LocationtName,
                                                    ReportingAuditType = (ReportingAuditType)g.Key.Type,
                                                    //TotalDoc = (ReportingAuditType)g.Key.Type == ReportingAuditType.FreeReportingAudit ? 20 : g.Sum(d => d.TotalDoc),
                                                    TotalDoc = g.Sum(d => d.TotalDoc),
                                                    TotalTodo = g.Sum(d => d.TotalTodo),
                                                    TotalPass = g.Sum(d => d.TotalPass),
                                                    TotalFail = g.Sum(d => d.TotalFail)
                                                });
            if (auditReportModel.Auditors.Any())
            {
                auditorAuditReports = auditorAuditReports.Where(x => auditReportModel.Auditors.Contains(x.Name));
            }
            if (auditReportModel.ReportingAuditTypes.Any())
            {
                auditorAuditReports = auditorAuditReports.Where(x => auditReportModel.ReportingAuditTypes.Contains(x.ReportingAuditType));
            }
            return new ResponseModel<IEnumerable<AuditReportDto>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Dữ liệu báo cáo giám sát theo người giám sát thành công.",
                Data = auditorAuditReports
            };
        }
    }
}
