using System.Collections.Generic;
using System.Net.WebSockets;
using BIVN.FixedStorage.Inventory.Inventory.API.HostedServices.Dto;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.Enums;
using Dapper;
using Inventory.API.Infrastructure.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using static BIVN.FixedStorage.Services.Common.API.Constants.Endpoint.ErrorInvestigationService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Inventory.API.HostedServices
{
    public class DataAggregationService : IDataAggregationService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IDbContextFactory<InventoryContext> _dbContextFactory;
        public DataAggregationService(IServiceScopeFactory serviceScopeFactory, IBackgroundTaskQueue backgroundTaskQueue, IDbContextFactory<InventoryContext> dbContextFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _backgroundTaskQueue = backgroundTaskQueue;
            _dbContextFactory = dbContextFactory;
        }
        /// <summary>
        /// Recursive
        /// </summary>
        /// <param name="dataFromCell"></param>
        /// <param name="code">model code - parent</param>
        /// <returns></returns>
        private List<InventoryDocTypeCComponentDto> GetAttachments(List<InventoryDocTypeCCellDto> dataFromCell, string code, Regex modelRegex, List<InventoryDocTypeCComponentDto> result, double quantityOfBOM)
        {
            result.AddRange(dataFromCell.Where(x => x.ModelCode == code)
                .Select(x => new InventoryDocTypeCComponentDto
                {
                    ModelCode = x.ModelCode,
                    AttachModelCode = modelRegex.IsMatch(x.MaterialCode) ? x.MaterialCode : string.Empty,
                    ComponentCode = !modelRegex.IsMatch(x.MaterialCode) ? x.MaterialCode : string.Empty,
                    Plant = x.Plant,
                    WarehouseLocation = x.WareHouseLocation,
                    QuantityOfBOM = double.Parse(x.BOMUseQty) * quantityOfBOM,
                    Attachment = GetAttachments(dataFromCell, x.MaterialCode, modelRegex, result, double.Parse(x.BOMUseQty))

                }).ToList());
            return result;
        }

        private List<InventoryDocTypeCComponentDto> GetAttachGroups(List<InventoryDocTypeCCellDto> dataFromCell, string mainModelCode, string attchModelCode, Regex modelRegex, List<InventoryDocTypeCComponentDto> result, double quantityOfBOM)
        {
            var attachGroups = dataFromCell.Where(x => x.ModelCode == attchModelCode).ToList();

            result.AddRange(attachGroups.Select(x => new InventoryDocTypeCComponentDto
            {
                ModelCode = mainModelCode,
                AttachModelCode = attchModelCode,
                ComponentCode = !modelRegex.IsMatch(x.MaterialCode) ? x.MaterialCode : string.Empty,
                Plant = x.Plant,
                WarehouseLocation = x.WareHouseLocation,
                QuantityOfBOM = double.Parse(x.BOMUseQty) /** quantityOfBOM*/,
                Attachment = modelRegex.IsMatch(x.MaterialCode) ? GetAttachGroups(dataFromCell, x.ModelCode, x.MaterialCode, modelRegex, result, double.Parse(x.BOMUseQty)) : new()
            }));


            return result;
        }

        private async Task AddData(List<InventoryDocTypeCSheetDto> dataFromSheets, string importer, Guid inventoryId)
        {
            var modelCodeRegex = new Regex(RegexPattern.ModelCodeRegex);
            var shareGrpRegex = new Regex(RegexPattern.ShareGrpRegex);
            var assenblyAndfinishGrpRegex = new Regex(RegexPattern.AssenblyAndfinishGrpRegex);
            var mainLineGrpRegex = new Regex(RegexPattern.MainLineGrpRegex);
            var shareGrpByModelRegex = new Regex(RegexPattern.ShareGrpByModelRegex);
            var materialCodeRegex = new Regex(RegexPattern.MaterialCodeRegex);

            //recursive for InventoryDocTypeCComponentDto build

            using (var scope = _serviceScopeFactory.CreateScope())
            {

                using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                {

                    var cellsData = dataFromSheets.SelectMany(x => x.Rows.Select(x => x)).ToList();

                    //var listComponentDto = new List<InventoryDocTypeCComponentDto>();
                    //foreach (var cell in dataFromSheets)
                    //{
                    //    foreach (var row in cell.Rows)
                    //    {
                    //        var inventoryDocTypeCComponentDto = new InventoryDocTypeCComponentDto
                    //        {
                    //            ModelCode = row.ModelCode,
                    //            AttachModelCode = modelCodeRegex.IsMatch(row.MaterialCode) ? row.MaterialCode : string.Empty,
                    //            ComponentCode = !modelCodeRegex.IsMatch(row.MaterialCode) ? row.MaterialCode : string.Empty,
                    //            Plant = row.Plant,
                    //            WarehouseLocation = row.WareHouseLocation,
                    //            QuantityOfBOM = int.Parse(row.BOMUseQty),
                    //        };
                    //        if (!modelCodeRegex.IsMatch(inventoryDocTypeCComponentDto.AttachModelCode))
                    //        {
                    //            listComponentDto.Add(inventoryDocTypeCComponentDto);
                    //        }

                    //    }

                    //}


                    var shareGrp = cellsData.Select(x => new InventoryDocTypeCComponentDto
                    {
                        ModelCode = x.ModelCode,
                        AttachModelCode = modelCodeRegex.IsMatch(x.MaterialCode) ? x.MaterialCode : string.Empty,
                        ComponentCode = !modelCodeRegex.IsMatch(x.MaterialCode) ? x.MaterialCode : string.Empty,
                        Plant = x.Plant,
                        WarehouseLocation = x.WareHouseLocation,
                        QuantityOfBOM = double.Parse(x.BOMUseQty),
                        Attachment = modelCodeRegex.IsMatch(x.MaterialCode) ? GetAttachGroups(cellsData, x.ModelCode, x.MaterialCode, modelCodeRegex, new List<InventoryDocTypeCComponentDto>(), double.Parse(x.BOMUseQty)) : new()
                    }).ToList();

                    var mainLineGrpData = shareGrp.GroupBy(x => x.ModelCode).ToList();


                    var listModelCode = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => mainLineGrpData.Select(x => x.Key).Contains(x.ModelCode))
                        .Select(x => new { x.ModelCode, x.Id })
                        .Distinct()
                        .ToListAsync();


                    var docTypeCComponents = await inventoryContext.DocTypeCComponents.AsNoTracking().Where(x => x.InventoryId == inventoryId).ToListAsync();
                    var docCComponentEntities = new List<DocTypeCComponent>();
                    foreach (var item in mainLineGrpData)
                    {
                        //get attachment group from db base on cell data
                        //var listAttachModelCode = item.Where(x => !string.IsNullOrEmpty(x.AttachModelCode) && shareGrpRegex.IsMatch(x.AttachModelCode)).Select(x => x.AttachModelCode);
                        //var listDocCUnitDetails = inventoryContext.DocTypeCUnitDetails.Include(x => x.DocTypeCUnit).Where(x => x.InventoryId == inventoryId && listAttachModelCode.Contains(x.ModelCode)).Select(x => x).ToList();

                        //set entities from cell data
                        foreach (var component in item)
                        {
                            var invDocModelCode = listModelCode.FirstOrDefault(x => x.ModelCode == item.Key);
                            if (!string.IsNullOrEmpty(component.ComponentCode))
                            {

                                var docTypeCComponent = new DocTypeCComponent
                                {
                                    Id = Guid.NewGuid(),
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = importer,
                                    MainModelCode = item.Key,
                                    Plant = component.Plant,
                                    WarehouseLocation = component.WarehouseLocation,
                                    UnitModelCode = component.AttachModelCode,
                                    ComponentCode = component.ComponentCode,
                                    QuantityOfBOM = component.QuantityOfBOM,
                                    InventoryId = inventoryId,
                                    InventoryDocId = invDocModelCode != null ? invDocModelCode.Id : null,

                                };
                                if (!docTypeCComponents.Any(x => x.MainModelCode == docTypeCComponent.MainModelCode && x.UnitModelCode == docTypeCComponent.UnitModelCode && x.ComponentCode == docTypeCComponent.ComponentCode && x.WarehouseLocation == docTypeCComponent.WarehouseLocation && x.Plant == docTypeCComponent.Plant))
                                {
                                    docCComponentEntities.Add(docTypeCComponent);
                                }
                            }
                            else
                            {
                                var listAttachment = component.Attachment.Where(x => !string.IsNullOrEmpty(x.ComponentCode)
                                //&& x.AttachModelCode == component.AttachModelCode
                                );
                                foreach (var attachment in listAttachment)
                                {
                                    var docTypeCComponent = new DocTypeCComponent
                                    {
                                        Id = Guid.NewGuid(),
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = importer,
                                        MainModelCode = item.Key,
                                        Plant = attachment.Plant,
                                        WarehouseLocation = attachment.WarehouseLocation,
                                        UnitModelCode = attachment.AttachModelCode,
                                        ComponentCode = attachment.ComponentCode,
                                        QuantityOfBOM = attachment.QuantityOfBOM,
                                        InventoryId = inventoryId,
                                        InventoryDocId = invDocModelCode != null ? invDocModelCode.Id : null,

                                    };
                                    if (!docTypeCComponents.Any(x => x.MainModelCode == docTypeCComponent.MainModelCode && x.UnitModelCode == docTypeCComponent.UnitModelCode && x.ComponentCode == docTypeCComponent.ComponentCode && x.WarehouseLocation == docTypeCComponent.WarehouseLocation && x.Plant == docTypeCComponent.Plant))
                                    {
                                        docCComponentEntities.Add(docTypeCComponent);
                                    }

                                }
                            }

                        }

                        //foreach (var component in listDocCUnitDetails)
                        //{
                        //    var docTypeCComponent = new DocTypeCComponent
                        //    {
                        //        Id = Guid.NewGuid(),
                        //        CreatedAt = DateTime.Now,
                        //        CreatedBy = importer,
                        //        MainModelCode = item.Key,
                        //        Plant = component.DocTypeCUnit.Plant,
                        //        WarehouseLocation = component.DocTypeCUnit.WarehouseLocation,
                        //        UnitModelCode = component.ModelCode,
                        //        ComponentCode = component.ComponentCode,
                        //        QuantityOfBOM = component.QuantityOfBOM,
                        //        InventoryId = inventoryId,
                        //        InventoryDocId = listModelCode.FirstOrDefault(x => x.ModelCode == item.Key).Id,

                        //    };
                        //    docCComponentEntities.Add(docTypeCComponent);
                        //}

                    }
                    await inventoryContext.DocTypeCComponents.AddRangeAsync(docCComponentEntities);
                    await inventoryContext.SaveChangesAsync();

                }
            }


            //return Task.CompletedTask;
        }
        public Task AddDataToDocTypeCComponent(List<InventoryDocTypeCSheetDto> dataFromSheets, string importer, Guid inventoryId)
        {
            return _backgroundTaskQueue.QueueBackgroundWorkItemAsync(x => AddData(dataFromSheets, importer, inventoryId));

        }
        private async Task UpdateData(InventoryDocSubmitDto inventoryDocSubmitDto)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                {
                    if (!inventoryDocSubmitDto.ForceAggregate.HasValue || (inventoryDocSubmitDto.ForceAggregate.HasValue && !inventoryDocSubmitDto.ForceAggregate.Value))
                    {
                        //check for 100% complete
                        var allDoc = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId).CountAsync();
                        var countCompleteDoc = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.Status == InventoryDocStatus.Confirmed).CountAsync();
                        var allAuditTarg = await inventoryContext.AuditTargets.AsNoTracking().Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId).CountAsync();
                        var countCompleteAT = await inventoryContext.AuditTargets.AsNoTracking().Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.Status == AuditTargetStatus.Pass).CountAsync();
                        var lastDocTotalQuantity = await inventoryContext.InventoryDocs.AsNoTracking().Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId).AnyAsync(x => x.AggregateCount > 0);


                        if (allDoc != countCompleteDoc && allAuditTarg != countCompleteAT)
                        {
                            return;
                        }
                        else if (allDoc == countCompleteDoc && lastDocTotalQuantity && allAuditTarg != countCompleteAT)
                        {
                            return;
                        }
                    }

                    //if (inventoryDocSubmitDto.DocType == InventoryDocType.C)
                    //{
                    //    var docTypeCDetails = inventoryContext.DocTypeCDetails.AsNoTracking().Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId
                    //    && !string.IsNullOrEmpty(x.ComponentCode)
                    //    //&& x.InventoryDocId.HasValue && inventoryDocSubmitDto.InventoryDocIds.Distinct().Contains(x.InventoryDocId.Value)
                    //    //|| inventoryDocSubmitDto.ModelCodes.Contains(x.ModelCode)
                    //    )
                    //    .Select(x => new AggregateDocTypeCDetailDto
                    //    {
                    //        InventoryId = x.InventoryId,
                    //        InventoryDocId = x.InventoryDocId,
                    //        QuantityOfBom = x.QuantityOfBom,
                    //        QuantityPerBom = x.QuantityPerBom,
                    //        ComponentCode = x.ComponentCode,
                    //        ModelCode = x.ModelCode,
                    //        DirectParent = x.DirectParent
                    //    });

                    //    var quatityPerBomOfDetails = await inventoryContext.DocTypeCDetails.AsNoTracking().Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && !string.IsNullOrEmpty(x.ModelCode)).Select(x => new QuantityPerBomOfDetailDto
                    //    {
                    //        ModelCode = x.ModelCode,
                    //        DirectParent = x.DirectParent,
                    //        QuantityPerBOM = x.QuantityPerBom
                    //    }).ToListAsync();

                    //    var listDocTypeCComponent = new List<DocTypeCComponent>();

                    //    var standAlones = await inventoryContext.Database.GetDbConnection().QueryAsync<string>($"SELECT StandAlone.DirectParent FROM (SELECT SUM(CASE WHEN ModelCode='' THEN 0 ELSE 1 END) SumModelCode ,DirectParent FROM DocTypeCDetails WHERE InventoryId ='{inventoryDocSubmitDto.InventoryId}' AND DirectParent NOT IN (SELECT ModelCode from DocTypeCDetails) GROUP BY DirectParent) StandAlone WHERE StandAlone.SumModelCode=0;");
                    //    //var listTask = new List<Task>();

                    //    var listDetails = new List<IEnumerable<AggregateDocTypeCDetailDto>>();
                    //    var detailCount = docTypeCDetails.Count();
                    //    for (int i = 1; i <= 100; i++)
                    //    {
                    //        if (detailCount <= 15_000)
                    //            listDetails.Add(docTypeCDetails.Skip((i - 1) * 150).Take(150));
                    //        else if (detailCount <= 25_000)
                    //            listDetails.Add(docTypeCDetails.Skip((i - 1) * 250).Take(250));
                    //        else if (detailCount <= 50_000)
                    //            listDetails.Add(docTypeCDetails.Skip((i - 1) * 500).Take(500));
                    //        else if (detailCount <= 100_000)
                    //            listDetails.Add(docTypeCDetails.Skip((i - 1) * 1000).Take(1000));
                    //        else
                    //            listDetails.Add(docTypeCDetails.Skip((i - 1) * detailCount / 100).Take(detailCount / 100));
                    //    }

                    //    foreach (var details in listDetails)
                    //    {
                    //        Parallel.ForEach(details, async item =>
                    //                                {

                    //                                    using (var scope = _serviceScopeFactory.CreateScope())
                    //                                    {
                    //                                        using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                    //                                        {
                    //                                            var docTypeCComponents = await inventoryContext.DocTypeCComponents.AsNoTracking().Where(x => x.InventoryId == item.InventoryId && x.ComponentCode == item.ComponentCode && (x.MainModelCode == item.DirectParent || x.UnitModelCode == item.DirectParent
                    //                                                         )).ToListAsync();

                    //                                            var componentEnumerable = docTypeCComponents.Select(x =>
                    //                                            {
                    //                                                var qtyDetails = quatityPerBomOfDetails.Find(q => q.DirectParent == x.MainModelCode);
                    //                                                var qtyDetailChild = quatityPerBomOfDetails.Find(q => q.DirectParent == x.MainModelCode && x.UnitModelCode == q.ModelCode);
                    //                                                var quantityPerBOM = 0d;
                    //                                                if ((qtyDetails == null && qtyDetailChild == null) || standAlones.Any(s => s == x.MainModelCode))
                    //                                                {
                    //                                                    quantityPerBOM = item.QuantityPerBom;
                    //                                                }
                    //                                                else
                    //                                                {
                    //                                                    quantityPerBOM = !string.IsNullOrEmpty(x.UnitModelCode) ? qtyDetails == null ? qtyDetailChild.QuantityPerBOM : qtyDetails.QuantityPerBOM : item.QuantityPerBom;
                    //                                                }

                    //                                                x.QuantityPerBOM = quantityPerBOM;
                    //                                                x.TotalQuantity = (qtyDetails == null && qtyDetailChild == null) || standAlones.Any(s => s == x.MainModelCode) ? quantityPerBOM : x.QuantityOfBOM * quantityPerBOM;

                    //                                                return x;
                    //                                            });

                    //                                            listDocTypeCComponent.AddRange(componentEnumerable);
                    //                                        }
                    //                                    }

                    //                                });
                    //    }


                    //    //foreach (var item in docTypeCDetails)
                    //    //{
                    //    //    //listTask.Add(AddListUpdateDocTypeCComponent(quatityPerBomOfDetails, listDocTypeCComponent, standAlones, item));
                    //    //    var docTypeCComponents = await inventoryContext.DocTypeCComponents.AsNoTracking().Where(x => x.InventoryId == item.InventoryId && x.ComponentCode == item.ComponentCode && (x.MainModelCode == item.DirectParent || x.UnitModelCode == item.DirectParent
                    //    //                   )).ToListAsync();

                    //    //    var componentEnumerable = docTypeCComponents.Select(x =>
                    //    //    {
                    //    //        var qtyDetails = quatityPerBomOfDetails.Find(q => q.ModelCode == x.MainModelCode || x.MainModelCode == q.DirectParent);

                    //    //        var quantityPerBOM = !string.IsNullOrEmpty(x.UnitModelCode) ? Convert.ToInt32(qtyDetails.QuantityPerBOM) : Convert.ToInt32(item.QuantityPerBom);
                    //    //        x.QuantityPerBOM = quantityPerBOM;
                    //    //        x.TotalQuantity = standAlones.Any(s => s == x.MainModelCode) ? quantityPerBOM : x.QuantityOfBOM * quantityPerBOM;

                    //    //        return x;
                    //    //    });

                    //    //    listDocTypeCComponent.AddRange(componentEnumerable);
                    //    //}



                    //    var query = listDocTypeCComponent.Where(x => x != null).DistinctBy(x => x.Id).ToList();
                    //    var list = new List<IEnumerable<DocTypeCComponent>>();
                    //    if (query.Any())
                    //    {
                    //        var count = query.Count();
                    //        for (int i = 1; i <= 100; i++)
                    //        {
                    //            if (count <= 15_000)
                    //                list.Add(query.Skip((i - 1) * 150).Take(150));
                    //            else if (count <= 25_000)
                    //                list.Add(query.Skip((i - 1) * 250).Take(250));
                    //            else if (count <= 50_000)
                    //                list.Add(query.Skip((i - 1) * 500).Take(500));
                    //            else if (count <= 100_000)
                    //                list.Add(query.Skip((i - 1) * 1000).Take(1000));
                    //            else
                    //                list.Add(query.Skip((i - 1) * count / 100).Take(count / 100));
                    //        }

                    //        foreach (var components in list.Where(x => x.Any()))
                    //        {
                    //            //await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(x => UpdateComponent(components));
                    //            inventoryContext.DocTypeCComponents.UpdateRange(components);
                    //            await inventoryContext.SaveChangesAsync();
                    //        }
                    //    }

                    //}

                    var docTypeAs = inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.DocType == InventoryDocType.A && x.DepartmentName != "SHIP");
                    if (docTypeAs.Any())
                    {
                        var listDoc = new List<IEnumerable<InventoryDoc>>();
                        var count = docTypeAs.Count();
                        for (int i = 1; i <= 100; i++)
                        {
                            if (count < 15_000)
                                listDoc.Add(docTypeAs.Skip((i - 1) * 150).Take(150));
                            else if (count < 25_000)
                                listDoc.Add(docTypeAs.Skip((i - 1) * 250).Take(250));
                            else if (count < 50_000)
                                listDoc.Add(docTypeAs.Skip((i - 1) * 500).Take(500));
                            else if (count <= 100_000)
                                listDoc.Add(docTypeAs.Skip((i - 1) * 500).Take(500));
                            else
                                listDoc.Add(docTypeAs.Skip((i - 1) * count / 100).Take(count / 100));

                        }



                        foreach (var documents in listDoc.Where(x => x.Any()))
                        {
                            if (documents?.Count() > 0)
                            {
                                await UpdateDocType(documents, inventoryDocSubmitDto.InventoryId);
                            }

                        }
                    }
                    //for doc ship
                    var docTypeESameComponetCode = inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.DocType == InventoryDocType.E && x.DepartmentName == "SHIP")
                        .LeftJoin(inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.DocType == InventoryDocType.A && x.DepartmentName == "SHIP"), e => new { e.ComponentCode, e.Plant, e.WareHouseLocation }, b => new { b.ComponentCode, b.Plant, b.WareHouseLocation }, (e, a) => new { e, a })
                        .Where(x => x.e.SalesOrderNo != x.a.SalesOrderNo || x.e.SalesOrderNo == x.a.SalesOrderNo).Select(x => x.a)
                        ;

                    if (docTypeESameComponetCode.Any())
                    {
                        var listDoc = new List<IEnumerable<InventoryDoc>>();
                        var count = docTypeESameComponetCode.Count();
                        for (int i = 1; i <= 100; i++)
                        {
                            if (count < 15_000)
                                listDoc.Add(docTypeESameComponetCode.Skip((i - 1) * 150).Take(150));
                            else if (count < 25_000)
                                listDoc.Add(docTypeESameComponetCode.Skip((i - 1) * 250).Take(250));
                            else if (count < 50_000)
                                listDoc.Add(docTypeESameComponetCode.Skip((i - 1) * 500).Take(500));
                            else if (count <= 100_000)
                                listDoc.Add(docTypeESameComponetCode.Skip((i - 1) * 500).Take(500));
                            else
                                listDoc.Add(docTypeESameComponetCode.Skip((i - 1) * count / 100).Take(count / 100));

                        }

                        foreach (var documents in listDoc.Where(x => x.Any()))
                        {
                            if (documents?.Count() > 0)
                            {
                                await UpdateDocTypeShip(documents, inventoryDocSubmitDto.InventoryId);
                            }

                        }
                    }


                    var docTypeEDiffComponetCode = inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.DocType == InventoryDocType.E && x.DepartmentName == "SHIP")
                       .LeftJoin(inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.DocType == InventoryDocType.A && x.DepartmentName == "SHIP"), e => new { e.SalesOrderNo, e.Plant, e.WareHouseLocation }, b => new { b.SalesOrderNo, b.Plant, b.WareHouseLocation }, (e, a) => new { e, a })
                       .Where(x => x.e.ComponentCode != x.a.ComponentCode).Select(x => x.a).Except(docTypeESameComponetCode)
                       ;


                    if (docTypeEDiffComponetCode.Any())
                    {
                        var listDoc = new List<IEnumerable<InventoryDoc>>();
                        var count = docTypeEDiffComponetCode.Count();
                        for (int i = 1; i <= 100; i++)
                        {
                            if (count < 15_000)
                                listDoc.Add(docTypeEDiffComponetCode.Skip((i - 1) * 150).Take(150));
                            else if (count < 25_000)
                                listDoc.Add(docTypeEDiffComponetCode.Skip((i - 1) * 250).Take(250));
                            else if (count < 50_000)
                                listDoc.Add(docTypeEDiffComponetCode.Skip((i - 1) * 500).Take(500));
                            else if (count <= 100_000)
                                listDoc.Add(docTypeEDiffComponetCode.Skip((i - 1) * 500).Take(500));
                            else
                                listDoc.Add(docTypeEDiffComponetCode.Skip((i - 1) * count / 100).Take(count / 100));

                        }

                        foreach (var documents in listDoc.Where(x => x.Any()))
                        {
                            if (documents?.Count() > 0)
                            {
                                await UpdateDocTypeShip(documents, inventoryDocSubmitDto.InventoryId, true);
                            }

                        }
                    }


                    var docTypeAsForShip = inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.DocType == InventoryDocType.A && x.DepartmentName == "SHIP")
                        .LeftJoin(inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryDocSubmitDto.InventoryId && x.DocType == InventoryDocType.E && x.DepartmentName == "SHIP"), a => new { a.ComponentCode, a.Plant, a.WareHouseLocation }, e => new { e.ComponentCode, e.Plant, e.WareHouseLocation }, (a, e) => new { a, e })
                        .Where(x => x.e == null && x.a.SalesOrderNo != x.e.SalesOrderNo || x.a.SalesOrderNo == x.e.SalesOrderNo).Select(x => x.a);

                    if (docTypeAsForShip.Any())
                    {
                        var listDoc = new List<IEnumerable<InventoryDoc>>();
                        var count = docTypeAsForShip.Count();
                        for (int i = 1; i <= 100; i++)
                        {
                            if (count < 15_000)
                                listDoc.Add(docTypeAsForShip.Skip((i - 1) * 150).Take(150));
                            else if (count < 25_000)
                                listDoc.Add(docTypeAsForShip.Skip((i - 1) * 250).Take(250));
                            else if (count < 50_000)
                                listDoc.Add(docTypeAsForShip.Skip((i - 1) * 500).Take(500));
                            else if (count <= 100_000)
                                listDoc.Add(docTypeAsForShip.Skip((i - 1) * 500).Take(500));
                            else
                                listDoc.Add(docTypeAsForShip.Skip((i - 1) * count / 100).Take(count / 100));

                        }

                        foreach (var documents in listDoc.Where(x => x.Any()))
                        {
                            if (documents?.Count() > 0)
                            {
                                await UpdateDocTypeShip(documents, inventoryDocSubmitDto.InventoryId);
                            }

                        }
                    }


                    //inventoryContext.InventoryDocs.UpdateRange(docTypeAs);
                    //await inventoryContext.SaveChangesAsync();
                }
            }
            //return Task.CompletedTask;
        }

        private async Task AddListUpdateDocTypeCComponent(List<QuantityPerBomOfDetailDto> quatityPerBomOfDetails, List<DocTypeCComponent> listDocTypeCComponent, IEnumerable<string> standAlones, AggregateDocTypeCDetailDto item)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                {
                    var docTypeCComponents = await inventoryContext.DocTypeCComponents.AsNoTracking().Where(x => x.InventoryId == item.InventoryId && x.ComponentCode == item.ComponentCode && (x.MainModelCode == item.DirectParent || x.UnitModelCode == item.DirectParent
                                            )).ToListAsync();

                    var componentEnumerable = docTypeCComponents.Select(x =>
                    {
                        var qtyDetails = quatityPerBomOfDetails.Find(q => q.ModelCode == x.MainModelCode || x.MainModelCode == q.DirectParent);

                        var quantityPerBOM = !string.IsNullOrEmpty(x.UnitModelCode) ? qtyDetails.QuantityPerBOM : item.QuantityPerBom;
                        x.QuantityPerBOM = quantityPerBOM;
                        x.TotalQuantity = standAlones.Any(s => s == x.MainModelCode) ? quantityPerBOM : x.QuantityOfBOM * quantityPerBOM;

                        return x;
                    });

                    listDocTypeCComponent.AddRange(componentEnumerable);
                }
            }

        }

        private Task UpdateComponent(IEnumerable<DocTypeCComponent> docTypeCComponent)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                {
                    inventoryContext.DocTypeCComponents.UpdateRange(docTypeCComponent);
                    inventoryContext.SaveChanges();
                }
            }
            return Task.CompletedTask;
        }
        private Task UpdateDocType(IEnumerable<InventoryDoc> inventoryDoc, Guid inventoryId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                {
                    var inventoryDocsToUpdate = inventoryDoc != null ? inventoryDoc.ToList() : inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).ToList();

                    foreach (var document in inventoryDocsToUpdate)
                    {
                        document.TotalQuantity = document.Quantity;


                        var docTypeBAndEQuantities = inventoryContext.InventoryDocs
                            .Where(x => x.InventoryId == inventoryId && (x.DocType == InventoryDocType.B || x.DocType == InventoryDocType.E) &&
                                        x.ComponentCode == document.ComponentCode && x.Plant == document.Plant && x.WareHouseLocation == document.WareHouseLocation
                                        && (string.IsNullOrEmpty(x.SalesOrderNo) || x.SalesOrderNo == document.SalesOrderNo)
                                        )
                            .Select(x => x.Quantity)
                            .ToList();

                        var docTypeCComponents = inventoryContext.DocTypeCComponents
                            .Where(x => x.InventoryId == inventoryId && (x.ComponentCode == document.ComponentCode && x.Plant == document.Plant && x.WarehouseLocation == document.WareHouseLocation))
                            .Select(x => x.TotalQuantity)
                            .ToList();

                        docTypeBAndEQuantities.AddRange(docTypeCComponents);
                        document.TotalQuantity += docTypeBAndEQuantities.Sum();

                        if (document.AccountQuantity > 0)
                        {
                            document.ErrorQuantity = document.TotalQuantity - document.AccountQuantity;
                            document.ErrorMoney = document.ErrorQuantity * document.UnitPrice;
                        }


                        document.AggregateCount = 1;
                    }

                    inventoryContext.InventoryDocs.UpdateRange(inventoryDocsToUpdate);
                    inventoryContext.SaveChanges();
                }
            }
            return Task.CompletedTask;
        }

        private Task UpdateDocTypeShip(IEnumerable<InventoryDoc> inventoryDoc, Guid inventoryId, bool isDiffComponentCode = false)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var inventoryContext = scope.ServiceProvider.GetRequiredService<InventoryContext>())
                {
                    var inventoryDocsToUpdate = inventoryDoc != null ? inventoryDoc.ToList() : inventoryContext.InventoryDocs.Where(x => x.InventoryId == inventoryId).ToList();



                    foreach (var document in inventoryDocsToUpdate)
                    {
                        if (document is null)
                        {
                            continue;
                        }
                        if (!isDiffComponentCode)
                        {
                            document.TotalQuantity = document.Quantity;
                            var docTypeEQuantities = inventoryContext.InventoryDocs
                              .Where(x => x.InventoryId == inventoryId && x.DocType == InventoryDocType.E &&
                                          x.ComponentCode == document.ComponentCode && x.Plant == document.Plant && x.WareHouseLocation == document.WareHouseLocation
                                          && (!string.IsNullOrEmpty(x.SalesOrderNo) && (x.SalesOrderNo == document.SalesOrderNo || x.SalesOrderNo != document.SalesOrderNo))
                                          )
                              .Select(x => x.Quantity)
                              .ToList();
                            document.TotalQuantity += docTypeEQuantities.Sum();

                            if (document.AccountQuantity > 0)
                            {
                                document.ErrorQuantity = document.TotalQuantity - document.AccountQuantity;
                                document.ErrorMoney = document.ErrorQuantity * document.UnitPrice;
                            }
                        }
                        else
                        {
                            document.TotalQuantity = document.Quantity;
                        }


                        document.AggregateCount = 1;
                    }

                    inventoryContext.InventoryDocs.UpdateRange(inventoryDocsToUpdate.Where(x => x != null).ToList());
                    inventoryContext.SaveChanges();
                }
            }
            return Task.CompletedTask;
        }

        private static List<double> GetDocTypeCQuantities(IEnumerable<DocTypeCDetail> docTypeCDetails)
        {
            var listTotalQuantity = new List<double>();
            foreach (var item in docTypeCDetails)
            {
                if (!string.IsNullOrEmpty(item.ComponentCode))
                {
                    listTotalQuantity.Add(item.QuantityOfBom * item.QuantityPerBom);
                }
                else
                {
                    //get attach
                    var child = docTypeCDetails.FirstOrDefault(x => x.ComponentCode == item.ComponentCode && x.DirectParent == item.ModelCode);
                    if (child != null)
                    {
                        listTotalQuantity.Add(child.QuantityOfBom * item.QuantityPerBom);
                    }
                }
            }
            return listTotalQuantity;
        }

        public Task UpdateDataFromInventoryDoc(InventoryDocSubmitDto inventoryDocSubmitDto)
        {

            return _backgroundTaskQueue.QueueBackgroundWorkItemAsync(x => UpdateData(inventoryDocSubmitDto));

        }
        private class ComponentCodeUnion
        {
#nullable enable
            public required string ComponentCode { get; set; }
            public string? ComponentName { get; set; }
        }
        private async Task AddDataErrorInvestigation(Guid inventoryId)
        {

            using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                //Kiểm tra ErrorInvestigations trong đợt hiện tại có dữ liệu hay chưa?
                var checkExistedItem = await dbContext.ErrorInvestigations.AsNoTracking().AnyAsync(x => x.InventoryId == inventoryId);
                if (checkExistedItem)
                {
                    var checkAllDeleted = await dbContext.ErrorInvestigations.AsNoTracking().Where(x => x.InventoryId == inventoryId).AllAsync(x => x.IsDelete);
                    if (!checkAllDeleted)
                    {
                        return;
                    }
                }

                //get list component from doc type A, B, E, C

                var componentCodeABEQuery = await dbContext.InventoryDocs.AsNoTracking()
                                                .Where(x => x.InventoryId == inventoryId && x.Status != InventoryDocStatus.NotReceiveYet && x.Status != InventoryDocStatus.NoInventory
                                                && (x.DocType == InventoryDocType.A ? x.ErrorQuantity != 0 : (x.DocType == InventoryDocType.B || x.DocType == InventoryDocType.E)))
                                                .Select(x => new ComponentCodeUnion { ComponentCode = x.ComponentCode, ComponentName = x.ComponentName }).Distinct().ToListAsync();
                //var componentCodeCQuery = await dbContext.DocTypeCDetails.Include(x => x.InventoryDoc).AsNoTracking()
                //                                .Where(x => x.InventoryId == inventoryId && x.InventoryDoc.Status != InventoryDocStatus.NotReceiveYet && x.InventoryDoc.Status != InventoryDocStatus.NoInventory)
                //                                .Select(x => new ComponentCodeUnion { ComponentCode = x.ComponentCode, ComponentName = x.InventoryDoc.ComponentName }).Distinct().ToListAsync() ?? new List<ComponentCodeUnion>();


                var componenCodeList = componentCodeABEQuery
                    .Where(x => !string.IsNullOrEmpty(x.ComponentCode))
                    .DistinctBy(x => x.ComponentCode)
                    .Select(x => new ErrorInvestigation
                    {
                        Id = Guid.NewGuid(),
                        ComponentCode = x.ComponentCode,
                        ComponentName = x.ComponentName,
                        AdjustmentNo = 0,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "System",
                        InventoryId = inventoryId,
                        IsDelete = false,
                        Note = string.Empty,
                        Status = ErrorInvestigationStatusType.NotYetInvestigated,
                    }).ToList();

                if (!componenCodeList.Any()) return;

                dbContext.ErrorInvestigations.AddRange(componenCodeList);
                await dbContext.SaveChangesAsync();

                var errorInvestigationInvDocABEQuery = from code in componenCodeList
                                                       join invDoc in dbContext.InventoryDocs.AsNoTracking()
                                                       on code.ComponentCode equals invDoc.ComponentCode
                                                       where invDoc.InventoryId == inventoryId
                                                       && invDoc.Status != InventoryDocStatus.NotReceiveYet
                                                       && invDoc.Status != InventoryDocStatus.NoInventory
                                                       && (invDoc.DocType == InventoryDocType.A ? invDoc.ErrorQuantity != 0 : (invDoc.DocType == InventoryDocType.B || invDoc.DocType == InventoryDocType.E))
                                                       select new ErrorInvestigationInventoryDoc
                                                       {
                                                           Id = Guid.NewGuid(),
                                                           ErrorInvestigationId = code.Id,
                                                           InventoryDocId = invDoc.Id,
                                                           DocType = invDoc.DocType,
                                                           DocCode = invDoc.DocCode,
                                                           CreatedAt = DateTime.Now,
                                                           CreatedBy = "System",
                                                           Plant = invDoc.Plant,
                                                           WareHouseLocation = invDoc.WareHouseLocation,
                                                           PositionCode = invDoc.PositionCode,
                                                           Quantity = invDoc.Quantity,
                                                           TotalQuantity = invDoc.TotalQuantity,
                                                           AccountQuantity = invDoc.AccountQuantity,
                                                           ErrorQuantity = invDoc.ErrorQuantity,
                                                           ErrorMoney = invDoc.ErrorMoney,
                                                           UnitPrice = invDoc.UnitPrice,
                                                           AssignedAccount = invDoc.AssignedAccountId,
                                                           InventoryBy = invDoc.InventoryBy,
                                                           QuantityDifference = invDoc.DocType == InventoryDocType.A ? invDoc.ErrorQuantity : invDoc.AccountQuantity - invDoc.TotalQuantity,
                                                           ModelCode = invDoc.ModelCode,
                                                       };

                //dbContext.ErrorInvestigationInventoryDocs.AddRange(errorInvestigationInvDocABEQuery);



                //var componentCodeDocTypeC = componenCodeQuery.Where(x => componentCodeCQuery.Any(c => c.ComponentCode == x.ComponentCode)).ToList();

                var docTypeCDetails = dbContext.DocTypeCDetails.AsNoTracking()
                    .Where(x => x.InventoryId == inventoryId && !string.IsNullOrEmpty(x.ComponentCode))
                    .Select(x => new
                    {
                        InventoryId = x.InventoryId,
                        InventoryDocId = x.InventoryDocId,
                        QuantityOfBom = x.QuantityOfBom,
                        QuantityPerBom = x.QuantityPerBom,
                        ComponentCode = x.ComponentCode,
                        ModelCode = x.ModelCode,
                        DirectParent = x.DirectParent
                    }).ToList();

                var errorInvestigationInvDocCDetailQuery = from code in componenCodeList
                                                           join invDocDetail in docTypeCDetails on code.ComponentCode equals invDocDetail.ComponentCode
                                                           join invDoc in dbContext.InventoryDocs.AsNoTracking() on invDocDetail.InventoryDocId equals invDoc.Id
                                                           where invDoc.InventoryId == inventoryId
                                                           && invDoc.Status != InventoryDocStatus.NotReceiveYet
                                                           && invDoc.Status != InventoryDocStatus.NoInventory
                                                           && invDoc.DocType == InventoryDocType.C
                                                           select new ErrorInvestigationInventoryDoc
                                                           {
                                                               Id = Guid.NewGuid(),
                                                               ErrorInvestigationId = code.Id,
                                                               InventoryDocId = invDoc.Id,
                                                               DocType = invDoc.DocType,
                                                               DocCode = invDoc.DocCode,
                                                               CreatedAt = DateTime.Now,
                                                               CreatedBy = "System",
                                                               Plant = invDoc.Plant,
                                                               WareHouseLocation = invDoc.WareHouseLocation,
                                                               BOM = invDocDetail.QuantityOfBom,
                                                               TotalQuantity = invDoc.Quantity,
                                                               AccountQuantity = null,
                                                               ErrorQuantity = null,
                                                               ErrorMoney = invDoc.ErrorMoney,
                                                               UnitPrice = invDoc.UnitPrice,
                                                               AssignedAccount = invDoc.AssignedAccountId,
                                                               InventoryBy = invDoc.InventoryBy,
                                                               QuantityDifference = (invDocDetail.QuantityOfBom * invDocDetail.QuantityPerBom) - invDoc.Quantity,
                                                               ModelCode = invDocDetail.DirectParent,
                                                               AttachModule = invDocDetail.ModelCode,

                                                           };



                //dbContext.ErrorInvestigationInventoryDocs.AddRange(errorInvestigationInvDocCDetailQuery);

                var batchSize = 1000; // Define the size of each batch
                //var errorInvestigationInventoryDocs = errorInvestigationInvDocCDetailQuery.ToList(); // Materialize the query

                //var errInvBatches = Enumerable.Range(0, (componenCodeQuery.Count() + batchSize - 1) / batchSize)
                //    .Select(i => componenCodeQuery.Skip(i * batchSize).Take(batchSize).ToList())
                //    .ToList();

                var cBatches = Enumerable.Range(0, (errorInvestigationInvDocCDetailQuery.Count() + batchSize - 1) / batchSize)
                    .Select(i => errorInvestigationInvDocCDetailQuery.Skip(i * batchSize).Take(batchSize).ToList())
                    .ToList();

                var abeBatches = Enumerable.Range(0, (errorInvestigationInvDocABEQuery.Count() + batchSize - 1) / batchSize)
                   .Select(i => errorInvestigationInvDocABEQuery.Skip(i * batchSize).Take(batchSize).ToList())
                   .ToList();

                var tasks = new List<Task>();


                foreach (var batch in abeBatches)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                            dbContext.ErrorInvestigationInventoryDocs.AddRange(batch);
                            await dbContext.SaveChangesAsync();
                        }
                    }));
                }

                foreach (var batch in cBatches)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryContext>();
                            dbContext.ErrorInvestigationInventoryDocs.AddRange(batch);
                            await dbContext.SaveChangesAsync();
                        }
                    }));
                }
                await Task.WhenAll(tasks);



            }
        }
        public async Task AddDataToErrorInvestigation(Guid inventoryId)
        {
            await AddDataErrorInvestigation(inventoryId);

        }
    }
}
