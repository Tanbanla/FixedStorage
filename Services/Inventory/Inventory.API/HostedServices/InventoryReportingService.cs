using Inventory.API.HostedServices.Dto;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.HostedServices
{
    public class InventoryReportingService : BackgroundService
    {
        private readonly ILogger<InventoryReportingService> _logger;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly CancellationToken _cancellationToken;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public InventoryReportingService(
            IBackgroundTaskQueue backgroundTaskQueue,
            IServiceScopeFactory servicesScopeFactory,
            ILogger<InventoryReportingService> logger
                                                )
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _serviceScopeFactory = servicesScopeFactory;
            _logger = logger;



        }

        //public void Start()
        //{
        //    Task.Run(async () => await ReportingAsync());
        //}

        //private async ValueTask ReportingAsync()
        //{
        //    await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async q => await BuildWorkItem(new CancellationToken()));
        //}
        //private async ValueTask BuildWorkItem(CancellationToken stoppingToken)
        //{

        //    try
        //    {
        //        await DoWork();
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        _logger.LogInformation("Timed Hosted Service is stopping.");
        //    }
        //}

        private async Task DoWork()
        {

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                    {
                        var inventory = inventoryContext.Inventories.Where(x => x.InventoryStatus == InventoryStatus.Doing).OrderByDescending(x => x.InventoryDate).FirstOrDefault();
                        if (inventory != null)
                        {
                            if (DateTime.Now > inventory.InventoryDate.AddDays(30))
                            {
                                inventory.IsReportRunning = false;
                                inventoryContext.Inventories.Update(inventory);
                                inventoryContext.SaveChanges();

                                var stopToken = new CancellationTokenSource(TimeSpan.Zero);
                            }
                            else if (inventory.InventoryDate.AddDays(-10) <= DateTime.Now && DateTime.Now <= inventory.InventoryDate.AddDays(30))
                            {
                                if (!inventory.IsReportRunning)
                                {
                                    inventory.IsReportRunning = true;
                                    inventoryContext.Inventories.Update(inventory);
                                    inventoryContext.SaveChanges();
                                }

                                //get list dtos
                                var reportingDepartmentDtos = await GetReportingDepartmentDtos(inventoryContext, inventory.Id);
                                var reportingLocationDtos = await GetReportingLocationDtos(inventoryContext, inventory.Id);
                                var reportingDocTypeDtos = await GetReportingDocTypeDtos(inventoryContext, inventory.Id);
                                var reportingAuditDtos = await GetReportingAuditDtos(inventoryContext, inventory.Id);

                                //insert to db
                                await InsertToReportTable(inventory.Id, inventoryContext, reportingDepartmentDtos, reportingLocationDtos, reportingDocTypeDtos, null, CaptureTimeType.At10);
                                await InsertToReportTable(inventory.Id, inventoryContext, reportingDepartmentDtos, reportingLocationDtos, reportingDocTypeDtos, null, CaptureTimeType.At11);
                                await InsertToReportTable(inventory.Id, inventoryContext, reportingDepartmentDtos, reportingLocationDtos, reportingDocTypeDtos, null, CaptureTimeType.At12);
                                await InsertToReportTable(inventory.Id, inventoryContext, reportingDepartmentDtos, reportingLocationDtos, reportingDocTypeDtos, reportingAuditDtos, CaptureTimeType.Now);

                            }

                        }
                        await inventoryContext.DisposeAsync();
                    }
                }

            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception:{ex.Message}{ex.InnerException?.Message}");
            }

            return;

        }

        private async Task<List<ReportingDepartmentDto>> GetReportingDepartmentDtos(InventoryContext inventoryContext, Guid inventoryId)
        {
            var locationAccounts = await (from invLoc in inventoryContext.InventoryLocations.AsNoTracking()
                                          join accLoc in inventoryContext.AccountLocations.AsNoTracking() on invLoc.Id equals accLoc.LocationId
                                          join invAcc in inventoryContext.InventoryAccounts.AsNoTracking() on accLoc.AccountId equals invAcc.Id
                                          select invLoc.DepartmentName
                                          ).ToListAsync();
            var reportingDepartmentDtos = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryId && !string.IsNullOrEmpty(x.DepartmentName)
                                                                                                           && locationAccounts.Contains(x.DepartmentName)
                                                                                                           && (InventoryDocStatus.NotInventoryYet == x.Status
                                                                                                           || InventoryDocStatus.WaitingConfirm == x.Status
                                                                                                           || InventoryDocStatus.MustEdit == x.Status
                                                                                                           || InventoryDocStatus.Confirmed == x.Status
                                                                                                           || InventoryDocStatus.AuditPassed == x.Status
                                                                                                           || InventoryDocStatus.AuditFailed == x.Status))
                           .GroupBy(x => new { x.InventoryId, x.DepartmentName })
                           .Select(x => new ReportingDepartmentDto
                           {
                               InventoryId = x.Key.InventoryId.Value,
                               DepartmentName = x.Key.DepartmentName,
                               TotalDoc = x.Count(g => g.DepartmentName == x.Key.DepartmentName),
                               TotalTodo = x.Count(g => g.DepartmentName == x.Key.DepartmentName && g.Status == InventoryDocStatus.NotInventoryYet),
                               TotalInventory = x.Count(g => g.DepartmentName == x.Key.DepartmentName && g.Status == InventoryDocStatus.WaitingConfirm),
                               TotalConfirm = x.Count(g => g.DepartmentName == x.Key.DepartmentName && (g.Status == InventoryDocStatus.MustEdit
                                                                                                                   || g.Status == InventoryDocStatus.Confirmed
                                                                                                                   || g.Status == InventoryDocStatus.AuditPassed
                                                                                                                   || g.Status == InventoryDocStatus.AuditFailed)),

                           }).ToListAsync();




            return reportingDepartmentDtos;
        }
        private async Task<List<ReportingLocationDto>> GetReportingLocationDtos(InventoryContext inventoryContext, Guid inventoryId)
        {
            //get list ReportingLocation
            var reportingLocationDtos = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryId && !string.IsNullOrEmpty(x.LocationName)
                                                                                            && (InventoryDocStatus.NotInventoryYet == x.Status
                                                                                            || InventoryDocStatus.WaitingConfirm == x.Status
                                                                                            || InventoryDocStatus.MustEdit == x.Status
                                                                                            || InventoryDocStatus.Confirmed == x.Status
                                                                                            || InventoryDocStatus.AuditPassed == x.Status
                                                                                            || InventoryDocStatus.AuditFailed == x.Status))
            .GroupBy(x => new { x.InventoryId, x.LocationName })
            .Select(x => new ReportingLocationDto
            {
                InventoryId = x.Key.InventoryId.Value,
                LocationName = x.Key.LocationName,
                TotalDoc = x.Count(g => g.LocationName == x.Key.LocationName),
                TotalTodo = x.Count(g => g.LocationName == x.Key.LocationName && g.Status == InventoryDocStatus.NotInventoryYet),
                TotalInventory = x.Count(g => g.LocationName == x.Key.LocationName && g.Status == InventoryDocStatus.WaitingConfirm),
                TotalConfirm = x.Count(g => g.LocationName == x.Key.LocationName && (g.Status == InventoryDocStatus.MustEdit
                                                                                                    || g.Status == InventoryDocStatus.Confirmed
                                                                                                    || g.Status == InventoryDocStatus.AuditPassed
                                                                                                    || g.Status == InventoryDocStatus.AuditFailed)),

            }).ToListAsync();
            return reportingLocationDtos;
        }

        private async Task<List<ReportingDocTypeDto>> GetReportingDocTypeDtos(InventoryContext inventoryContext, Guid inventoryId)
        {

            //get list ReportingDocType
            var reportingDocTypeDtos = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryId &&
                                                                                              (InventoryDocStatus.NotInventoryYet == x.Status
                                                                                            || InventoryDocStatus.WaitingConfirm == x.Status
                                                                                            || InventoryDocStatus.MustEdit == x.Status
                                                                                            || InventoryDocStatus.Confirmed == x.Status
                                                                                            || InventoryDocStatus.AuditPassed == x.Status
                                                                                            || InventoryDocStatus.AuditFailed == x.Status))
            .GroupBy(x => new { x.InventoryId, x.DocType })
            .Select(x => new ReportingDocTypeDto
            {
                InventoryId = x.Key.InventoryId.Value,
                DocType = x.Key.DocType,
                TotalDoc = x.Count(g => g.DocType == x.Key.DocType),
                TotalTodo = x.Count(g => g.DocType == x.Key.DocType && g.Status == InventoryDocStatus.NotInventoryYet),
                TotalInventory = x.Count(g => g.DocType == x.Key.DocType && g.Status == InventoryDocStatus.WaitingConfirm),
                TotalConfirm = x.Count(g => g.DocType == x.Key.DocType && (g.Status == InventoryDocStatus.MustEdit
                                                                                                    || g.Status == InventoryDocStatus.Confirmed
                                                                                                    || g.Status == InventoryDocStatus.AuditPassed
                                                                                                    || g.Status == InventoryDocStatus.AuditFailed)),

            }).ToListAsync();

            return reportingDocTypeDtos;
        }
        private async Task<List<ReportingAuditDto>> GetReportingAuditDtos(InventoryContext inventoryContext, Guid inventoryId)
        {

            var reportingAuditDtos = new List<ReportingAuditDto>();

            //get list ReportingAudit - Department

            var reportAuditDepartmentDtos = await (from invDoc in inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId && x.Status != InventoryDocStatus.NotReceiveYet && x.Status != InventoryDocStatus.NoInventory).AsNoTracking()
                                                   join auditTgt in inventoryContext.AuditTargets.Where(x => x.InventoryId == inventoryId).AsNoTracking()
                                                       on new { invDoc.ComponentCode, invDoc.PositionCode, InventoryId = invDoc.InventoryId.Value } equals new { auditTgt.ComponentCode, auditTgt.PositionCode, InventoryId = auditTgt.InventoryId }
                                                       into auditGroup
                                                   from auditTgt in auditGroup.DefaultIfEmpty()
                                                   select new
                                                   {
                                                       invDoc.InventoryId,
                                                       invDoc.Status,
                                                       invDoc.DocType,
                                                       ComponentCode = auditTgt.ComponentCode ?? invDoc.ComponentCode,
                                                       Type = (auditTgt.AssignedAccountId != null && auditTgt.AssignedAccountId != Guid.Empty) ? ReportingAuditType.FixedReportingAudit : ReportingAuditType.FreeReportingAudit,
                                                       Plant = auditTgt.Plant ?? invDoc.Plant,
                                                       WareHouseLocation = auditTgt.WareHouseLocation ?? invDoc.WareHouseLocation,
                                                       LocationName = auditTgt.LocationName ?? invDoc.LocationName,
                                                       DepartmentName = auditTgt.DepartmentName ?? invDoc.DepartmentName
                                                   })
                                                    .GroupBy(x => new { x.InventoryId, x.Type, x.DepartmentName })
                                                    .Select(grouped => new ReportingAuditDto
                                                    {
                                                        InventoryId = grouped.Key.InventoryId.Value,
                                                        Type = grouped.Key.Type,
                                                        DepartmentName = grouped.Key.DepartmentName,
                                                        TotalDoc = grouped.Key.Type == ReportingAuditType.FixedReportingAudit ? grouped.Count() : grouped.Count(x => x.Status == InventoryDocStatus.AuditPassed
                                                                                    || x.Status == InventoryDocStatus.AuditFailed),
                                                        TotalTodo = grouped.Count(x => x.Status == InventoryDocStatus.NotInventoryYet
                                                                    || x.Status == InventoryDocStatus.WaitingConfirm
                                                                    || x.Status == InventoryDocStatus.MustEdit
                                                                    || x.Status == InventoryDocStatus.Confirmed),
                                                        TotalPass = grouped.Count(x => x.Status == InventoryDocStatus.AuditPassed),
                                                        TotalFail = grouped.Count(x => x.Status == InventoryDocStatus.AuditFailed)
                                                    }).ToListAsync();


            var reportingAuditLocationDtos = await (from invDoc in inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId && x.Status != InventoryDocStatus.NotReceiveYet && x.Status != InventoryDocStatus.NoInventory).AsNoTracking()
                                                    join auditTgt in inventoryContext.AuditTargets.Where(x => x.InventoryId == inventoryId).AsNoTracking()
                                                        on new { invDoc.ComponentCode, invDoc.PositionCode, InventoryId = invDoc.InventoryId.Value } equals new { auditTgt.ComponentCode, auditTgt.PositionCode, InventoryId = auditTgt.InventoryId }
                                                        into auditGroup
                                                    from auditTgt in auditGroup.DefaultIfEmpty()
                                                    select new
                                                    {
                                                        invDoc.InventoryId,
                                                        invDoc.Status,
                                                        invDoc.DocType,
                                                        ComponentCode = auditTgt.ComponentCode ?? invDoc.ComponentCode,
                                                        Type = (auditTgt.AssignedAccountId != null && auditTgt.AssignedAccountId != Guid.Empty) ? ReportingAuditType.FixedReportingAudit : ReportingAuditType.FreeReportingAudit,
                                                        Plant = auditTgt.Plant ?? invDoc.Plant,
                                                        WareHouseLocation = auditTgt.WareHouseLocation ?? invDoc.WareHouseLocation,
                                                        LocationName = auditTgt.LocationName ?? invDoc.LocationName,
                                                        DepartmentName = auditTgt.DepartmentName ?? invDoc.DepartmentName
                                                    })
                                                    .GroupBy(x => new { x.InventoryId, x.Type, x.LocationName })
                                                    .Select(grouped => new ReportingAuditDto
                                                    {
                                                        InventoryId = grouped.Key.InventoryId.Value,
                                                        Type = grouped.Key.Type,
                                                        LocationtName = grouped.Key.LocationName,
                                                        TotalDoc = grouped.Key.Type == ReportingAuditType.FixedReportingAudit ? grouped.Count() : grouped.Count(x => x.Status == InventoryDocStatus.AuditPassed
                                                                                    || x.Status == InventoryDocStatus.AuditFailed),
                                                        TotalTodo = grouped.Count(x => x.Status == InventoryDocStatus.NotInventoryYet
                                                                    || x.Status == InventoryDocStatus.WaitingConfirm
                                                                    || x.Status == InventoryDocStatus.MustEdit
                                                                    || x.Status == InventoryDocStatus.Confirmed),
                                                        TotalPass = grouped.Count(x => x.Status == InventoryDocStatus.AuditPassed),
                                                        TotalFail = grouped.Count(x => x.Status == InventoryDocStatus.AuditFailed)
                                                    }).ToListAsync();



            var reportingAuditAuditorDtos = await (from invDoc in inventoryContext.InventoryDocs
                                                        .Where(x => x.InventoryId == inventoryId && x.Status != InventoryDocStatus.NotReceiveYet && x.Status != InventoryDocStatus.NoInventory)
                                                        .AsNoTracking()
                                                   join auditTgt in inventoryContext.AuditTargets
                                                       .Where(x => x.InventoryId == inventoryId)
                                                       .AsNoTracking()
                                                       on new { invDoc.ComponentCode, invDoc.PositionCode, InventoryId = invDoc.InventoryId.Value }
                                                       equals new { auditTgt.ComponentCode, auditTgt.PositionCode, InventoryId = auditTgt.InventoryId }
                                                       into auditGroup
                                                   from auditTgt in auditGroup.DefaultIfEmpty()

                                                   join account in inventoryContext.InventoryAccounts
                                                       on auditTgt.AssignedAccountId equals account.UserId
                                                       into accountGroup
                                                   from account in accountGroup.DefaultIfEmpty()

                                                   select new
                                                   {
                                                       invDoc.InventoryId,
                                                       invDoc.Status,
                                                       invDoc.DocType,
                                                       ComponentCode = auditTgt.ComponentCode ?? invDoc.ComponentCode,
                                                       Type = (auditTgt.AssignedAccountId != null && auditTgt.AssignedAccountId != Guid.Empty) ? ReportingAuditType.FixedReportingAudit : ReportingAuditType.FreeReportingAudit,
                                                       Plant = auditTgt.Plant ?? invDoc.Plant,
                                                       WareHouseLocation = auditTgt.WareHouseLocation ?? invDoc.WareHouseLocation,
                                                       LocationName = auditTgt.LocationName ?? invDoc.LocationName,
                                                       DepartmentName = auditTgt.DepartmentName ?? invDoc.DepartmentName,
                                                       AuditorName = (auditTgt.AssignedAccountId != null && auditTgt.AssignedAccountId != Guid.Empty) ? account.UserName : invDoc.AuditBy
                                                   })
                                                    .GroupBy(x => new { x.InventoryId, x.Type, x.AuditorName })
                                                    .Select(grouped => new ReportingAuditDto
                                                    {
                                                        InventoryId = grouped.Key.InventoryId.Value,
                                                        Type = grouped.Key.Type,
                                                        AuditorName = grouped.Key.AuditorName,
                                                        TotalDoc = grouped.Key.Type == ReportingAuditType.FixedReportingAudit ? grouped.Count() : grouped.Count(x => x.Status == InventoryDocStatus.AuditPassed
                                                                                    || x.Status == InventoryDocStatus.AuditFailed),
                                                        TotalTodo = grouped.Count(x => x.Status == InventoryDocStatus.NotInventoryYet
                                                                    || x.Status == InventoryDocStatus.WaitingConfirm
                                                                    || x.Status == InventoryDocStatus.MustEdit
                                                                    || x.Status == InventoryDocStatus.Confirmed),
                                                        TotalPass = grouped.Count(x => x.Status == InventoryDocStatus.AuditPassed),
                                                        TotalFail = grouped.Count(x => x.Status == InventoryDocStatus.AuditFailed)
                                                    }).ToListAsync();

            var result = reportingAuditDtos.Concat(reportAuditDepartmentDtos).Concat(reportingAuditLocationDtos).Concat(reportingAuditAuditorDtos).ToList();

            return result;
        }
        private async Task InsertRerportDepartment(Guid inventoryId, InventoryContext inventoryContext, IEnumerable<ReportingDepartmentDto> reportingDepartmentDtos, CaptureTimeType captureTimeType)
        {
            var now = DateTime.UtcNow.AddHours(7);
            var today = DateTime.UtcNow.AddHours(7).Date;
            var existedReportingAtGivenTime = inventoryContext.ReportingDepartments.AsNoTracking().Any(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
            var inventoryFinished = await inventoryContext.Inventories.AsNoTracking().AnyAsync(x => x.Id == inventoryId && x.InventoryStatus == InventoryStatus.Finish);

            var reportDepartments = reportingDepartmentDtos.Select(x => new ReportingDepartment
            {
                Id = Guid.NewGuid(),
                CaptureTimeType = captureTimeType,
                CreatedAt = now,
                CreatedBy = Guid.Empty.ToString(),
                DepartmentName = x.DepartmentName,
                InventoryId = x.InventoryId,
                TotalDoc = x.TotalDoc,
                TotalTodo = x.TotalTodo,
                TotalConfirm = x.TotalConfirm,
                TotalInventory = x.TotalInventory,

            });
            if (captureTimeType == CaptureTimeType.Now)
            {
                if (existedReportingAtGivenTime && !inventoryFinished)
                {
                    var existedNowReport = inventoryContext.ReportingDepartments.Where(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
                    existedNowReport.ExecuteDelete();
                    //await inventoryContext.SaveChangesAsync();
                    //inventoryContext.ReportingDepartments.AddRange(reportDepartments);
                    //await inventoryContext.SaveChangesAsync();
                }
                //else
                if(!inventoryFinished)
                {
                    inventoryContext.ReportingDepartments.AddRange(reportDepartments);
                    inventoryContext.SaveChanges();
                }

            }
            else if (now > today.AddHours((int)captureTimeType) && now < today.AddHours(1 + (int)captureTimeType) && !existedReportingAtGivenTime)
            {
                //if (existedReportingAtGivenTime)
                //{
                //    var existedNowReport = inventoryContext.ReportingDepartments.Where(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
                //    existedNowReport.ExecuteDelete();
                //    //await inventoryContext.SaveChangesAsync();
                //    //inventoryContext.ReportingDepartments.AddRange(reportDepartments);
                //    //await inventoryContext.SaveChangesAsync();
                //}
                ////else
                {
                    inventoryContext.ReportingDepartments.AddRange(reportDepartments);
                    inventoryContext.SaveChanges();
                }
            }

        }
        private async Task InsertRerportLocation(Guid inventoryId, InventoryContext inventoryContext, IEnumerable<ReportingLocationDto> reportingLocationDtos, CaptureTimeType captureTimeType)
        {
            var now = DateTime.UtcNow.AddHours(7);
            var today = DateTime.UtcNow.AddHours(7).Date;
            var existedReportingAtGivenTime = inventoryContext.ReportingLocations.Any(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
            var inventoryFinished = await inventoryContext.Inventories.AsNoTracking().AnyAsync(x => x.Id == inventoryId && x.InventoryStatus == InventoryStatus.Finish);
            var reportLocations = reportingLocationDtos.Select(x => new ReportingLocation
            {
                Id = Guid.NewGuid(),
                CaptureTimeType = captureTimeType,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty.ToString(),
                LocationName = x.LocationName,
                InventoryId = x.InventoryId,
                TotalDoc = x.TotalDoc,
                TotalTodo = x.TotalTodo,
                TotalConfirm = x.TotalConfirm,
                TotalInventory = x.TotalInventory,

            });
            if (captureTimeType == CaptureTimeType.Now)
            {
                if (existedReportingAtGivenTime && !inventoryFinished)
                {
                    var existedNowReport = inventoryContext.ReportingLocations.Where(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
                    existedNowReport.ExecuteDelete();
                    //await inventoryContext.SaveChangesAsync();
                    //inventoryContext.ReportingLocations.AddRange(reportLocations);
                    //await inventoryContext.SaveChangesAsync();
                }
                //else
                if (!inventoryFinished)
                {
                    inventoryContext.ReportingLocations.AddRange(reportLocations);
                    inventoryContext.SaveChanges();
                }

            }
            else if (now > today.AddHours((int)captureTimeType) && now < today.AddHours(1 + (int)captureTimeType)/* && !existedReportingAtGivenTime*/)
            {
                if (existedReportingAtGivenTime && !inventoryFinished)
                {
                    var existedNowReport = inventoryContext.ReportingLocations.Where(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
                    existedNowReport.ExecuteDelete();
                    //await inventoryContext.SaveChangesAsync();
                    //inventoryContext.ReportingLocations.AddRange(reportLocations);
                    //await inventoryContext.SaveChangesAsync();
                }
                //else
                if (!inventoryFinished)
                {
                    inventoryContext.ReportingLocations.AddRange(reportLocations);
                    inventoryContext.SaveChanges();
                }
            }
        }
        private async Task InsertRerportDocType(Guid inventoryId, InventoryContext inventoryContext, IEnumerable<ReportingDocTypeDto> reportingDocTypeDtos, CaptureTimeType captureTimeType)
        {
            var now = DateTime.UtcNow.AddHours(7);
            var today = DateTime.UtcNow.AddHours(7).Date;

            
            {
                var existedReportingAtGivenTime = inventoryContext.ReportingDocTypes.Any(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
                var inventoryFinished = await inventoryContext.Inventories.AsNoTracking().AnyAsync(x => x.Id == inventoryId && x.InventoryStatus == InventoryStatus.Finish);
                var reportDocTypes = reportingDocTypeDtos.Select(x => new ReportingDocType
                {
                    Id = Guid.NewGuid(),
                    CaptureTimeType = captureTimeType,
                    CreatedAt = DateTime.Now,
                    CreatedBy = Guid.Empty.ToString(),
                    DocType = x.DocType,
                    InventoryId = x.InventoryId,
                    TotalDoc = x.TotalDoc,
                    TotalTodo = x.TotalTodo,
                    TotalConfirm = x.TotalConfirm,
                    TotalInventory = x.TotalInventory,

                });
                if (captureTimeType == CaptureTimeType.Now)
                {
                    if (existedReportingAtGivenTime && !inventoryFinished)
                    {
                        var existedNowReport = inventoryContext.ReportingDocTypes.Where(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
                        existedNowReport.ExecuteDelete();
                        //await inventoryContext.SaveChangesAsync();
                        //inventoryContext.ReportingDocTypes.AddRange(reportDocTypes);
                        //await inventoryContext.SaveChangesAsync();
                    }
                    //else
                    if (!inventoryFinished)
                    {
                        inventoryContext.ReportingDocTypes.AddRange(reportDocTypes);
                        inventoryContext.SaveChanges();
                    }

                }
                else if (now > today.AddHours((int)captureTimeType) && now < today.AddHours(1 + (int)captureTimeType)/* && !existedReportingAtGivenTime*/)
                {
                    if (existedReportingAtGivenTime && !inventoryFinished)
                    {
                        var existedNowReport = inventoryContext.ReportingDocTypes.Where(x => x.InventoryId == inventoryId && x.CaptureTimeType == captureTimeType);
                        existedNowReport.ExecuteDelete();
                        //await inventoryContext.SaveChangesAsync();
                        //inventoryContext.ReportingDocTypes.AddRange(reportDocTypes);
                        //await inventoryContext.SaveChangesAsync();
                    }
                    //else
                    if (!inventoryFinished)
                    {
                        inventoryContext.ReportingDocTypes.AddRange(reportDocTypes);
                        inventoryContext.SaveChanges();
                    }
                }
            }
        }
        private async Task InsertRerportAudit(Guid inventoryId, InventoryContext inventoryContext, IEnumerable<ReportingAuditDto> reportingAuditDtos, CaptureTimeType captureTimeType)
        {

            if (reportingAuditDtos == null || reportingAuditDtos.Count() <= 0)
            {
                return;
            }
            var now = DateTime.UtcNow.AddHours(7);
            var today = DateTime.UtcNow.AddHours(7).Date;
            var existedReportingAtGivenTime = inventoryContext.ReportingAudits.Any(x => x.InventoryId == inventoryId);
            var inventoryFinished = await inventoryContext.Inventories.AsNoTracking().AnyAsync(x => x.Id == inventoryId && x.InventoryStatus == InventoryStatus.Finish);
            var reportAuditTargets = reportingAuditDtos.Select(x => new ReportingAudit
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty.ToString(),
                InventoryId = x.InventoryId,
                TotalDoc = x.TotalDoc,
                TotalTodo = x.TotalTodo,
                LocationtName = x.LocationtName,
                TotalPass = x.TotalPass,
                TotalFail = x.TotalFail,
                DepartmentName = x.DepartmentName,
                AuditorName = x.AuditorName,
                Type = x.Type
            });
            if (captureTimeType == CaptureTimeType.Now)
            {
                if (existedReportingAtGivenTime && !inventoryFinished)
                {
                    var existedNowReport = inventoryContext.ReportingAudits.Where(x => x.InventoryId == inventoryId);
                    existedNowReport.ExecuteDelete();
                    //await inventoryContext.SaveChangesAsync();
                    //inventoryContext.ReportingAudits.AddRange(reportAuditTargets);
                    //await inventoryContext.SaveChangesAsync();
                }
                //else
                if (!inventoryFinished)
                {
                    inventoryContext.ReportingAudits.AddRange(reportAuditTargets);
                    inventoryContext.SaveChanges();
                }

            }
            else if (now > today.AddHours((int)captureTimeType) && now < today.AddHours(1 + (int)captureTimeType)/* && !existedReportingAtGivenTime*/)
            {
                if (existedReportingAtGivenTime && !inventoryFinished)
                {
                    var existedNowReport = inventoryContext.ReportingAudits.Where(x => x.InventoryId == inventoryId);
                    existedNowReport.ExecuteDelete();
                    //await inventoryContext.SaveChangesAsync();
                    //inventoryContext.ReportingAudits.AddRange(reportAuditTargets);
                    //await inventoryContext.SaveChangesAsync();
                }
                //else
                if (!inventoryFinished)
                {
                    inventoryContext.ReportingAudits.AddRange(reportAuditTargets);
                    inventoryContext.SaveChanges();
                }
            }
        }

        private async Task InsertToReportTable(Guid inventoryId, InventoryContext inventoryContext, IEnumerable<ReportingDepartmentDto> reportingDepartmentDtos, IEnumerable<ReportingLocationDto> reportingLocationDtos, IEnumerable<ReportingDocTypeDto> reportingDocTypeDtos, IEnumerable<ReportingAuditDto> reportingAuditDtos, CaptureTimeType captureTimeType)
        {
            await InsertRerportDepartment(inventoryId, inventoryContext, reportingDepartmentDtos, captureTimeType);
            await InsertRerportLocation(inventoryId, inventoryContext, reportingLocationDtos, captureTimeType);
            await InsertRerportDocType(inventoryId, inventoryContext, reportingDocTypeDtos, captureTimeType);
            await InsertRerportAudit(inventoryId, inventoryContext, reportingAuditDtos, captureTimeType);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isValid = true;
            using (var scope = _serviceScopeFactory.CreateScope())

            using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
            {
                var inventory = inventoryContext.Inventories.Where(x => x.InventoryStatus == InventoryStatus.Doing).OrderByDescending(x => x.InventoryDate).FirstOrDefault();
                if (inventory != null)
                {
                    if (DateTime.Now > inventory.InventoryDate.AddDays(30))
                        isValid = false;

                }
                else
                {
                    isValid = false;
                }
            }

            PeriodicTimer periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            while (await periodicTimer.WaitForNextTickAsync(stoppingToken))
            {
                if (isValid)
                {
                    await DoWork();
                }
            }

        }
    }
}

