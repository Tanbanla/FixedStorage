using System.IO;
using System.Reflection;
using System.Resources;
using BIVN.FixedStorage.Services.Common.API.Dto;
using BIVN.FixedStorage.Services.Common.API.Dto.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Dto.Role;
using BIVN.FixedStorage.Services.Common.API.Enum.ErrorInvestigation;
using BIVN.FixedStorage.Services.Common.API.Helpers;
using BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity;
using Inventory.API.Service.Dto.ErrorInvestigation;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.OpenApi.Extensions;
using Microsoft.VisualBasic.FileIO;
using Polly;
using static BIVN.FixedStorage.Services.Common.API.Constants.ValidationRules;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Inventory.API.Service.ErrorInvestigation
{
    public partial class ErrorInvestigationWebService : IErrorInvestigationWebService
    {
        private readonly ILogger<ErrorInvestigationWebService> _logger;
        private readonly IRestClient _resClient;
        private readonly HttpContext _httpContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RestClientFactory _restClientFactory;
        //private readonly IDatabaseFactoryService _databaseFactory;
        private readonly IConfiguration _configuration;
        private readonly InventoryContext _inventoryContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDataAggregationService _dataAggregationService;
        private readonly IMemoryCache _memoryCache;
        private readonly IInventoryWebService _inventoryService;

        private readonly RestClient _identityRestClient;

        private InventoryDocStatus[] ExcludeDocStatus = new InventoryDocStatus[]
        {
            InventoryDocStatus.NotReceiveYet,
            InventoryDocStatus.NoInventory
        };
        public ErrorInvestigationWebService(InventoryContext inventoryContext,
                                         IHttpContextAccessor httpContextAccessor,
                                         IRestClient restClient,
                                         IConfiguration configuration,
                                         ILogger<ErrorInvestigationWebService> logger,
                                         IServiceScopeFactory serviceProviderFactory,
                                         IWebHostEnvironment webHostEnvironment,
                                         RestClientFactory restClientFactory,
                                         IDataAggregationService dataAggregationService,
                                         IInventoryWebService inventoryService
                                        )
        {
            _inventoryContext = inventoryContext;
            _logger = logger;
            _configuration = configuration;
            _resClient = restClient;
            _serviceScopeFactory = serviceProviderFactory;
            _httpContext = httpContextAccessor.HttpContext;
            _webHostEnvironment = webHostEnvironment;
            _restClientFactory = restClientFactory;
            _identityRestClient = _restClientFactory.IdentityClient();
            _dataAggregationService = dataAggregationService;
            _inventoryService = inventoryService;
        }

        public async Task<ResponseModel<byte[]>> ExportDataAjustment(Guid invetoryId)
        {
            var query = await GetDataAdjustmentsQuery(invetoryId);


            var errorCategorySettings = _inventoryContext.GeneralSettings.AsNoTracking()
                                       .Where(x => x.Type == GeneralSettingType.ErrorCategory && x.InventoryId == invetoryId && !x.IsDelete)
                                       .AsEnumerable()
                                       .Select(x => new
                                       {
                                           ErrorCategory = int.Parse(x.Key1),
                                           ErrorCategoryName = x.Value1
                                       })
                                       .OrderBy(x => x.ErrorCategory)
                                       .ToList();
          
            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                foreach (var sheetName in errorCategorySettings)
                {
                    if (sheetName.ErrorCategory == 0)
                    {
                        var sheetData = query.InventoryMisuse.ToList();
                        if (sheetData.Any())
                        {
                            // Loại bỏ bản ghi bị trùng:
                            sheetData = sheetData
                                .GroupBy(x => new
                                {
                                    x.DocCode,
                                    x.Note,
                                    x.ComponentCode,
                                    x.ErrQuantity,
                                    x.BOMxQuantity,
                                    x.AdjustQuantity
                                })
                                .Select(g => g.First())
                                .ToList();

                            workbook.Worksheets.Add(sheetName.ErrorCategoryName);
                            var worksheet = workbook.Worksheets[sheetName.ErrorCategoryName];
                            sheetData = sheetData.Select((x, index) => new InventoryMisuseErrorDataAdjustmentDto
                            {
                                No = index + 1,
                                ErrorInvestigationId = x.ErrorInvestigationId,
                                DocCode = x.DocCode,
                                Note = x.Note,
                                ComponentCode = x.ComponentCode,
                                ErrQuantity = x.ErrQuantity,
                                AdjustQuantity = x.AdjustQuantity,
                                BOMxQuantity = x.BOMxQuantity
                            }).ToList();
                            SetDataToInventoryMisuseSheet(worksheet, sheetData);
                        }
                    }
                    else
                    {
                        var sheetData = query.PackagingBom.Where(x => x.ErrorCategory == sheetName.ErrorCategory).ToList();
                        if (sheetData.Any())
                        {
                            // Loại bỏ bản ghi bị trùng:
                            sheetData = sheetData
                                .GroupBy(x => new
                                {
                                    x.Plant,
                                    x.WareHouseLocation,
                                    x.ComponentCode,
                                    x.ErrorCategory,
                                    x.AdjustQuantity
                                })
                                .Select(g => g.First())
                                .ToList();

                            workbook.Worksheets.Add(sheetName.ErrorCategoryName);
                            var worksheet = workbook.Worksheets[sheetName.ErrorCategoryName];
                            sheetData = sheetData.Select((x, index) => new PackagingBomErrorDataAjustmentDto
                            {
                                No = index + 1,
                                ErrorInvestigationId = x.ErrorInvestigationId,
                                Plant = x.Plant,
                                WareHouseLocation = x.WareHouseLocation,
                                ComponentCode = x.ComponentCode,
                                ErrorCategory = x.ErrorCategory,
                                AdjustQuantity = x.AdjustQuantity
                            }).ToList();
                            SetDataToSheet(worksheet, sheetData);
                        }
                    }
                }

                //delete all adjsutment data of current inventory

                var errorInvestigationIds = query.InventoryMisuse.Select(x => x.ErrorInvestigationId).ToList();
                await _inventoryContext.ErrorInvestigations.Where(x => x.InventoryId == invetoryId && errorInvestigationIds.Contains(x.Id) && x.Status == ErrorInvestigationStatusType.Investigated).ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDelete, true));

                return new ResponseModel<byte[]> { Data = package.GetAsByteArray(), Code = 200, Message = "Success" };
            }
        }

        private async Task<ExportDataAdjustmentDto> GetDataAdjustmentsQuery(Guid inventoryId)
        {
            var exportDataAdjustment = new ExportDataAdjustmentDto();

            var inventoryMisuseQuery = await (from ei in _inventoryContext.ErrorInvestigations.AsNoTracking()
                                              join eid in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking() on ei.Id equals eid.ErrorInvestigationId into eidGroup
                                              from eid in eidGroup.DefaultIfEmpty()
                                              join eih in _inventoryContext.ErrorInvestigationHistories.AsNoTracking().OrderByDescending(x => x.CreatedAt) on ei.Id equals eih.ErrorInvestigationId into eihGroup
                                              from eih in eihGroup.DefaultIfEmpty()
                                              where ei.InventoryId == inventoryId
                                              //&& eid.ErrorQuantity != null && eid.ErrorQuantity != 0
                                              && eid.DocType != InventoryDocType.C
                                              select new
                                              {
                                                  ErrorInvestigationId = ei.Id,
                                                  DocCode = eid.DocCode,
                                                  ei.ComponentCode,
                                                  eid.Plant,
                                                  eid.WareHouseLocation,
                                                  ErrQuantity = eid.ErrorQuantity,
                                                  Note = eih != null ? eih.ErrorDetails : string.Empty,
                                                  AdjustQty = eih != null ? eih.NewValue - eih.OldValue : 0,
                                                  CreatedAt = eih != null ? eih.CreatedAt : (DateTime?)null,
                                                  eid.DocType,
                                                  AccountQuantity = eid.Quantity,
                                                  ErrorCategory = eih != null ? (int)eih.ErrorCategory : (int?)null,

                                              }
                                              ).OrderBy(x=>x.ErrorCategory)
                                              .ToListAsync();


            var grpInvMisQuery = inventoryMisuseQuery.GroupBy(x => new { x.ComponentCode, x.ErrorCategory, x.Plant, x.WareHouseLocation })
                .Select(x => new
                {
                    ComponentCode = x.Key.ComponentCode,
                    ErrorCategory = x.Key.ErrorCategory,
                    Plant = x.Key.Plant,
                    WareHouseLocation = x.Key.WareHouseLocation,
                    Note = x.FirstOrDefault().Note,
                    ErrorQty = x.Sum(y => y.ErrQuantity),
                    AdjQty = x.FirstOrDefault(y => y.ErrorCategory == x.Key.ErrorCategory && y.ComponentCode == x.Key.ComponentCode)?.AdjustQty,
                    Docs = x.Where(d=>d.ErrorCategory == x.Key.ErrorCategory).Select(x => new DocDataDto
                    {
                        DocCode = x.DocCode,
                        DocType = x.DocType,
                        AccountQuantity = x.AccountQuantity,
                    }).DistinctBy(x => x.DocCode).OrderByDescending(x => x.AccountQuantity).ToList()
                }).Where(x => x.AdjQty != null && x.AdjQty != 0).ToList();


            var invMisList = new List<InventoryMisuseErrorDataAdjustmentDto>();

            var currentAdjustDocCodes = new Dictionary<string, double?>();

            foreach (var group in grpInvMisQuery)
            {
                var totalAdjustQty = Math.Abs(group.AdjQty.Value);

                var absErrorQty = Math.Abs(group.ErrorQty ?? 0);

                if (group.ErrorQty < 0) // add adjusted value to A doc if error quantity < 0
                {
                    var docA = group.Docs.FirstOrDefault(x => x.DocType == InventoryDocType.A);

                    var invMisA = new InventoryMisuseErrorDataAdjustmentDto();
                    invMisA.No = invMisList.Count + 1;
                    invMisA.ComponentCode = group.ComponentCode;
                    invMisA.Note = group.Note;
                    invMisA.DocCode = docA?.DocCode;
                    invMisA.BOMxQuantity = $"{docA?.AccountQuantity + totalAdjustQty}*1";
                    invMisA.AdjustQuantity = totalAdjustQty;
                    invMisA.ErrorCategory = group.ErrorCategory;
                    invMisA.Plant = group.Plant;
                    invMisA.WHLoc = group.WareHouseLocation;

                    invMisList.Add(invMisA);
                    //await _inventoryContext.InventoryDocs.Where(x => x.DocCode == invMisA.DocCode).ExecuteUpdateAsync(x => x.SetProperty(p => p.Quantity, p => p.Quantity + invMisA.AdjustQuantity));
                }
                else //quantity allocation in order of highest to lowest
                {
                    if (!group.Docs.Any(x => x.DocType == InventoryDocType.A))
                    {
                        continue;
                    }
                    //// Separate the elements with DocType of InventoryDocType.A
                    //var docTypeAElements = group.Docs.Where(x => x.DocType == InventoryDocType.A).ToList();
                    //var otherElements = group.Docs.Where(x => x.DocType != InventoryDocType.A).ToList();

                    //// Concatenate the elements with DocType of InventoryDocType.A to the end of the list
                    //var reorderedDocs = otherElements.Concat(docTypeAElements).ToList();


                    foreach (var doc in group.Docs.Where(x => x.DocType != InventoryDocType.C))
                    {
                        if (totalAdjustQty == 0)
                        {
                            continue;
                        }
                        var invMis = new InventoryMisuseErrorDataAdjustmentDto();
                        invMis.No = invMisList.Count + 1;
                        invMis.ComponentCode = group.ComponentCode;
                        invMis.Note = group.Note;
                        invMis.DocCode = doc.DocCode;
                        invMis.Plant = group.Plant;
                        invMis.WHLoc = group.WareHouseLocation;

                        invMis.ErrorCategory = group.ErrorCategory;



                        if (doc.AccountQuantity > totalAdjustQty)
                        {
                            // get other doc with same component code and adjust quantity
                            if (!currentAdjustDocCodes.ContainsKey(doc.DocCode))
                            {
                                currentAdjustDocCodes.Add(doc.DocCode, doc.AccountQuantity);
                                currentAdjustDocCodes[doc.DocCode] -= totalAdjustQty;
                            }
                            else
                            {
                                currentAdjustDocCodes[doc.DocCode] -= totalAdjustQty;
                            }
                            doc.AccountQuantity = currentAdjustDocCodes[doc.DocCode];
                            invMis.BOMxQuantity = $"{doc.AccountQuantity}*1";

                            invMis.AdjustQuantity = -totalAdjustQty;
                            totalAdjustQty = 0;
                        }
                        else
                        {
                            invMis.BOMxQuantity = $"{doc.AccountQuantity}*1";
                            invMis.AdjustQuantity = -doc.AccountQuantity ?? 0;
                            totalAdjustQty -= doc.AccountQuantity ?? 0;
                            if (!currentAdjustDocCodes.ContainsKey(doc.DocCode))
                            {

                                currentAdjustDocCodes.Add(doc.DocCode, -doc.AccountQuantity ?? 0);
                                //currentAdjustDocCodes[doc.DocCode] -= doc.AccountQuantity;
                            }
                            //else
                            //{
                            //    currentAdjustDocCodes[doc.DocCode] -= doc.AccountQuantity;
                            //}
                        }
                        invMisList.Add(invMis);
                        //await _inventoryContext.InventoryDocs.Where(x => x.DocCode == invMis.DocCode).ExecuteUpdateAsync(x => x.SetProperty(p => p.Quantity, p => p.Quantity + invMis.AdjustQuantity));
                    }
                }
            }

            // Gán dữ liệu lỗi Misuse với ErrorCategory == 0(Để xuất template có số lượng thùng * số thùng):
            exportDataAdjustment.InventoryMisuse = invMisList.Where(x => x.ErrorCategory == 0).AsQueryable();

            // Lấy danh sách từ invMisList có ErrorCategory != 0 và mapping PackagingBomErrorDataAjustmentDto:
            var misuseNotAdjustments = invMisList
                .Where(x => x.ErrorCategory != 0)
                .Select(x => new PackagingBomErrorDataAjustmentDto
                {
                    ErrorInvestigationId = x.ErrorInvestigationId, 
                    Plant = x.Plant,              
                    WareHouseLocation = x.WHLoc,  
                    ComponentCode = x.ComponentCode,
                    ErrorCategory = x.ErrorCategory,
                    AdjustQuantity = x.AdjustQuantity
                }).ToList();

            var query = (from ei in _inventoryContext.ErrorInvestigations.AsNoTracking()
                         join eid in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking() on ei.Id equals eid.ErrorInvestigationId into eidGroup
                         from eid in eidGroup.DefaultIfEmpty()
                         join eih in _inventoryContext.ErrorInvestigationHistories.AsNoTracking().OrderByDescending(x => x.CreatedAt) on ei.Id equals eih.ErrorInvestigationId into eihGroup
                         from eih in eihGroup.DefaultIfEmpty()
                         where ei.InventoryId == inventoryId
                               && eid.ErrorQuantity != 0
                         select new
                         {
                             ErrorInvestigationId = ei.Id,
                             Plant = eid.Plant,
                             WareHouseLocation = eid.WareHouseLocation,
                             ComponentCode = ei.ComponentCode,
                             ErrorCategory = eih != null ? (int)eih.ErrorCategory : (int?)null,
                             AdjustQuantity = eih != null ? eih.NewValue - eih.OldValue : 0
                         } into data
                         group data by new
                         {
                             data.ErrorInvestigationId,
                             data.Plant,
                             data.WareHouseLocation,
                             data.ComponentCode,
                             data.ErrorCategory
                         } into grp
                         select new PackagingBomErrorDataAjustmentDto
                         {
                             ErrorInvestigationId = grp.Key.ErrorInvestigationId,
                             Plant = grp.Key.Plant,
                             WareHouseLocation = grp.Key.WareHouseLocation,
                             ComponentCode = grp.Key.ComponentCode,
                             ErrorCategory = grp.Key.ErrorCategory,
                             AdjustQuantity = grp.FirstOrDefault(y => y.ErrorCategory == grp.Key.ErrorCategory && y.ComponentCode == grp.Key.ComponentCode).AdjustQuantity,
                         })
                        ;
            var packingBoms = query.ToList();
            // Kết hợp query ban đầu với misuseAdjustments:
            exportDataAdjustment.PackagingBom = packingBoms.Concat(misuseNotAdjustments).AsQueryable();
            return exportDataAdjustment;
        }

        private class DocDataDto
        {
            public string DocCode { get; set; }
            public InventoryDocType DocType { get; set; }
            public double? AccountQuantity { get; set; }
        }


        private static void SetDataToSheet(ExcelWorksheet worksheet, List<PackagingBomErrorDataAjustmentDto> sheetData)
        {
            worksheet.Cells[1, 1].Value = Constants.DataAdjustmentExcelExport.No;
            worksheet.Cells[1, 2].Value = DataAdjustmentExcelExport.Plant;
            worksheet.Cells[1, 3].Value = DataAdjustmentExcelExport.WarehouseLocation;
            worksheet.Cells[1, 4].Value = DataAdjustmentExcelExport.ComponentCode;
            worksheet.Cells[1, 5].Value = DataAdjustmentExcelExport.AdjustmentQuantity;


            var exportProperties = new MemberInfo[]
            {
                typeof(PackagingBomErrorDataAjustmentDto).GetProperty(nameof(PackagingBomErrorDataAjustmentDto.No)),
                typeof(PackagingBomErrorDataAjustmentDto).GetProperty(nameof(PackagingBomErrorDataAjustmentDto.Plant)),
                typeof(PackagingBomErrorDataAjustmentDto).GetProperty(nameof(PackagingBomErrorDataAjustmentDto.WareHouseLocation)),
                typeof(PackagingBomErrorDataAjustmentDto).GetProperty(nameof(PackagingBomErrorDataAjustmentDto.ComponentCode)),
                typeof(PackagingBomErrorDataAjustmentDto).GetProperty(nameof(PackagingBomErrorDataAjustmentDto.AdjustQuantity)),
            };
            worksheet.Cells["A2"].LoadFromCollection(sheetData, false, OfficeOpenXml.Table.TableStyles.Medium5, System.Reflection.BindingFlags.Public, exportProperties);

            worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
            worksheet.Row(1).Height = 30;
            worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Top.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Bottom.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Left.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 5].Style.Border.Right.Color.SetColor(Color.Black);

            worksheet.Column(1).Width = 6;
            worksheet.Column(2).Width = 8;
            worksheet.Column(3).Width = 8;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 20;
        }

        private static void SetDataToInventoryMisuseSheet(ExcelWorksheet worksheet, List<InventoryMisuseErrorDataAdjustmentDto> sheetData)
        {
            worksheet.Cells[1, 1].Value = Constants.DataAdjustmentExcelExport.No;
            worksheet.Cells[1, 2].Value = Constants.DataAdjustmentExcelExport.ComponentCode;
            worksheet.Cells[1, 3].Value = DataAdjustmentExcelExport.DocCode;
            worksheet.Cells[1, 4].Value = DataAdjustmentExcelExport.Note;
            worksheet.Cells[1, 5].Value = DataAdjustmentExcelExport.BomXPerBom;
            worksheet.Cells[1, 6].Value = DataAdjustmentExcelExport.BomXPerBom;
            worksheet.Cells[1, 7].Value = DataAdjustmentExcelExport.BomXPerBom;
            worksheet.Cells[1, 8].Value = DataAdjustmentExcelExport.BomXPerBom;
            worksheet.Cells[1, 9].Value = DataAdjustmentExcelExport.BomXPerBom;
            worksheet.Cells[1, 10].Value = DataAdjustmentExcelExport.BomXPerBom;


            var exportProperties = new MemberInfo[]
            {
                typeof(InventoryMisuseErrorDataAdjustmentDto).GetProperty(nameof(InventoryMisuseErrorDataAdjustmentDto.No)),
                typeof(InventoryMisuseErrorDataAdjustmentDto).GetProperty(nameof(InventoryMisuseErrorDataAdjustmentDto.ComponentCode)),
                typeof(InventoryMisuseErrorDataAdjustmentDto).GetProperty(nameof(InventoryMisuseErrorDataAdjustmentDto.DocCode)),
                typeof(InventoryMisuseErrorDataAdjustmentDto).GetProperty(nameof(InventoryMisuseErrorDataAdjustmentDto.Note)),
                typeof(InventoryMisuseErrorDataAdjustmentDto).GetProperty(nameof(InventoryMisuseErrorDataAdjustmentDto.BOMxQuantity)),
            };
            worksheet.Cells["A2"].LoadFromCollection(sheetData, false, OfficeOpenXml.Table.TableStyles.Medium5, System.Reflection.BindingFlags.Public, exportProperties);

            worksheet.Cells[1, 1, 1, 10].Style.Font.Bold = true;
            worksheet.Row(1).Height = 30;
            worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Top.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Bottom.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Left.Color.SetColor(Color.Black);
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, worksheet.Dimension.Rows, 10].Style.Border.Right.Color.SetColor(Color.Black);

            worksheet.Column(1).Width = 6;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 20;
            worksheet.Column(4).Width = 60;
            worksheet.Column(5).Width = 20;
            worksheet.Column(6).Width = 20;
            worksheet.Column(7).Width = 20;
            worksheet.Column(8).Width = 20;
            worksheet.Column(9).Width = 20;
            worksheet.Column(10).Width = 20;
        }

        public async Task<ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationWebDto>>> ListErrorInvestigaiton(ListErrorInvestigationWebModel listErrorInvestigation)
        {
            var user = _httpContext.UserFromContext();
            var userId = Guid.Parse(user.UserId);
            var isAdminRole = user.RoleName;
            var inventoryAccounts = _inventoryContext.InventoryAccounts.AsNoTracking()
                                            .Select(ia => new
                                            {
                                                ia.UserId,
                                                ia.UserName
                                            });
            var inventoryNames = _inventoryContext.Inventories.AsNoTracking().Where(x => listErrorInvestigation.InventoryIds.Any() && listErrorInvestigation.InventoryIds.Contains(x.Id))
                                    .Select(i => new
                                    {
                                        i.Id,
                                        i.Name
                                    });

            //Lấy ra danh sách người dùng:
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;


            var investigationHistoryErrorCategory = from ei in _inventoryContext.ErrorInvestigations.AsNoTracking()
                                                    where listErrorInvestigation.InventoryIds.Contains(ei.InventoryId)
                                                    join eih in _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                        on ei.Id equals eih.ErrorInvestigationId
                                                    group eih by eih.ErrorInvestigationId into g
                                                    select new
                                                    {
                                                        ErrorInvestigationId = g.Key,
                                                        ErrorCategory = g.OrderByDescending(eih => eih.CreatedAt)
                                                                        .Select(eih => eih.ErrorCategory)
                                                                        .FirstOrDefault()
                                                    };

            var errorInvestigationListQuery = (from ei in _inventoryContext.ErrorInvestigations.AsNoTracking()
                                               join eid in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking() on ei.Id equals eid.ErrorInvestigationId into eidGroup
                                               from eid in eidGroup.DefaultIfEmpty()
                                               where listErrorInvestigation.InventoryIds.Contains(ei.InventoryId)
                                                     && eid.DocType == InventoryDocType.A
                                                     && eid.ErrorQuantity != 0
                                                     && ((isAdminRole == Constants.DefaultAccount.RoleName || isAdminRole == Constants.DefaultAccount.InvGRoleName ||
                                                        isAdminRole == Constants.DefaultAccount.AdministratorRoleName || isAdminRole == Constants.DefaultAccount.MCRoleName ||
                                                        isAdminRole == Constants.DefaultAccount.InventoryRoleName
                                                     ) || (eid.AssignedAccount.HasValue && eid.AssignedAccount == userId))
                                               select new ListErrorInvestigationWebDto
                                               {
                                                   ErrorInvestigationId = ei.Id,
                                                   InventoryId = ei.InventoryId,
                                                   InventoryName = inventoryNames.Where(x => x.Id == ei.InventoryId).Select(x => x.Name).FirstOrDefault(),
                                                   Plant = eid.Plant,
                                                   WHLoc = eid.WareHouseLocation,
                                                   ComponentCode = ei.ComponentCode,
                                                   ComponentName = ei.ComponentName,
                                                   Position = eid.PositionCode,
                                                   TotalQuantity = Math.Round(eid.TotalQuantity ?? 0, 3),
                                                   AccountQuantity = Math.Round(eid.AccountQuantity ?? 0, 3),
                                                   ErrorQuantity = Math.Round(eid.ErrorQuantity ?? 0, 3),
                                                   ErrorMoney = Math.Round(eid.ErrorMoney ?? 0, 3),
                                                   //UnitPrice = Math.Round(eid.UnitPrice ?? 0, 3),
                                                   //ErrorQuantityAbs = Math.Round(Math.Abs(eid.ErrorQuantity ?? 0), 3),
                                                   //ErrorMoneyAbs = Math.Round(Math.Abs((eid.ErrorQuantity ?? 0) * (eid.UnitPrice ?? 0)), 3),
                                                   AssigneeAccount = eid.AssignedAccount.HasValue ? inventoryAccounts.Where(ia => ia.UserId == eid.AssignedAccount).Select(ia => ia.UserName).FirstOrDefault() : string.Empty,
                                                   Status = ei.Status,
                                                   ErrorCategory = investigationHistoryErrorCategory.FirstOrDefault(x => x.ErrorInvestigationId == ei.Id) != null ? (int)investigationHistoryErrorCategory.FirstOrDefault(x => x.ErrorInvestigationId == ei.Id).ErrorCategory : (int?)null
                                               }).AsEnumerable();

            var condition = ErrorInvestigationBuildFilterCondition(listErrorInvestigation);
            var query = errorInvestigationListQuery.Where(condition);
            var totalRecords = query.Count();

            var itemsPagination = new List<ListErrorInvestigationWebDto>();

            if (listErrorInvestigation.IsExportExcel)
            {
                itemsPagination = SortErrorInvestigations(query, listErrorInvestigation);
            }
            else
            {
                itemsPagination = SortErrorInvestigationsWithPagination(query, listErrorInvestigation);
            }

            var errorInvestigationIds = itemsPagination.Select(x => x.ErrorInvestigationId).ToList();

            var investigationHistories = await _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                            .Where(eih => errorInvestigationIds.Contains(eih.ErrorInvestigationId))
                                            .OrderByDescending(x => x.CreatedAt)
                                            .ToListAsync();

            var inventoryId = user.InventoryLoggedInfo?.InventoryModel?.InventoryId;
            var errorCategories = await _inventoryContext.GeneralSettings.AsNoTracking()
                                        .Where(gs => gs.Type == GeneralSettingType.ErrorCategory && gs.InventoryId == inventoryId)
                                        .Select(x => new
                                        {
                                            ErrorCategoryKey = x.Key1,
                                            ErrorCategoryName = x.Value1
                                        }).ToListAsync();

            var investigationHistoryGrouped = investigationHistories
                                                .GroupBy(eih => eih.ErrorInvestigationId)
                                                .Select(g => new
                                                {
                                                    ErrorInvestigationId = g.Key,
                                                    InvestigationTotal = g.Sum(eih => eih.NewValue - eih.OldValue),
                                                    InvestigationQuantity = g.Select(eih => eih.NewValue - eih.OldValue).FirstOrDefault(),
                                                    ErrorCategory = g.Select(eih => eih.ErrorCategory).FirstOrDefault(),
                                                    ErrorDetails = g.Select(eih => eih.ErrorDetails).FirstOrDefault(),
                                                    InvestigatorUserCode = g.Select(eih => eih.InvestigatorUserCode).FirstOrDefault(),
                                                    InvestigatorId = g.Select(eih => eih.InvestigatorId).FirstOrDefault(),
                                                    InvestigationHistoryCount = g.Count(),
                                                    ErrorCategoryName = errorCategories.Where(x => x.ErrorCategoryKey == (g.Select(eih => eih.ErrorCategory).FirstOrDefault()).ToString()).Select(x => x.ErrorCategoryName).FirstOrDefault()
                                                }).ToList();
            var items = itemsPagination.Select(x =>
                        {
                            var history = investigationHistoryGrouped.FirstOrDefault(ih => ih.ErrorInvestigationId == x.ErrorInvestigationId);
                            return new ListErrorInvestigationWebDto
                            {
                                ErrorInvestigationId = x.ErrorInvestigationId,
                                InventoryId = x.InventoryId,
                                InventoryName = x.InventoryName,
                                Plant = x.Plant,
                                WHLoc = x.WHLoc,
                                ComponentCode = x.ComponentCode,
                                ComponentName = x.ComponentName,
                                Position = x.Position,
                                TotalQuantity = x.TotalQuantity,
                                AccountQuantity = x.AccountQuantity,
                                ErrorQuantity = x.ErrorQuantity,
                                ErrorMoney = x.ErrorMoney,
                                //UnitPrice = x.UnitPrice,
                                //ErrorQuantityAbs = x.ErrorQuantityAbs,
                                //ErrorMoneyAbs = x.ErrorMoneyAbs,
                                AssigneeAccount = x.AssigneeAccount,
                                Status = x.Status,
                                InvestigationQuantity = Math.Round(history?.InvestigationQuantity ?? 0, 3),
                                ErrorCategory = history?.ErrorCategory != null ? (int)history.ErrorCategory : (int?)null,
                                ErrorDetail = history?.ErrorDetails ?? string.Empty,
                                Investigator = string.IsNullOrEmpty(history?.InvestigatorUserCode) ? users.FirstOrDefault(ia => ia.Id == history?.InvestigatorId)?.Code : history?.InvestigatorUserCode,
                                InvestigationTotal = Math.Round(history?.InvestigationTotal ?? 0, 3),
                                InvestigationHistoryCount = history != null ? $"Lần {history.InvestigationHistoryCount}" : string.Empty,
                                ErrorCategoryName = history?.ErrorCategoryName
                            };
                        }).ToList();
            return new ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationWebDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = items,
                TotalRecords = totalRecords,
                Message = Constants.ResponseMessages.ListErrorInvestigationWebSuccessfully
            };
        }

        private List<ListErrorInvestigationWebDto> SortErrorInvestigationsWithPagination(IEnumerable<ListErrorInvestigationWebDto> query, ListErrorInvestigationWebModel listErrorInvestigation)
        {
            List<ListErrorInvestigationWebDto> sortedItems;

            switch (listErrorInvestigation.SortColumn)
            {
                case Constants.ErrorInvestigationColumn.ErrorQuantity:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.ErrorQuantity).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList()
                        : query.OrderByDescending(x => x.ErrorQuantity).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList();
                    break;
                case Constants.ErrorInvestigationColumn.Plant:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.Plant).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList()
                        : query.OrderByDescending(x => x.Plant).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList();
                    break;
                case Constants.ErrorInvestigationColumn.WHLoc:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.WHLoc).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList()
                        : query.OrderByDescending(x => x.WHLoc).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList();
                    break;
                case Constants.ErrorInvestigationColumn.ComponentCode:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.ComponentCode).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList()
                        : query.OrderByDescending(x => x.ComponentCode).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList();
                    break;
                default:
                    sortedItems = query.OrderByDescending(x => x.ErrorQuantity).Skip(listErrorInvestigation.Skip).Take(listErrorInvestigation.Take).ToList();
                    break;
            }
            return sortedItems;
        }
        private List<ListErrorInvestigationWebDto> SortErrorInvestigations(IEnumerable<ListErrorInvestigationWebDto> query, ListErrorInvestigationWebModel listErrorInvestigation)
        {
            List<ListErrorInvestigationWebDto> sortedItems;

            switch (listErrorInvestigation.SortColumn)
            {
                case Constants.ErrorInvestigationColumn.ErrorQuantity:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.ErrorQuantity).ToList()
                        : query.OrderByDescending(x => x.ErrorQuantity).ToList();
                    break;
                case Constants.ErrorInvestigationColumn.Plant:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.Plant).ToList()
                        : query.OrderByDescending(x => x.Plant).ToList();
                    break;
                case Constants.ErrorInvestigationColumn.WHLoc:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.WHLoc).ToList()
                        : query.OrderByDescending(x => x.WHLoc).ToList();
                    break;
                case Constants.ErrorInvestigationColumn.ComponentCode:
                    sortedItems = listErrorInvestigation.SortColumnDirection == Constants.ErrorInvestigationColumn.OrderByAsc
                        ? query.OrderBy(x => x.ComponentCode).ToList()
                        : query.OrderByDescending(x => x.ComponentCode).ToList();
                    break;
                default:
                    sortedItems = query.OrderByDescending(x => x.ErrorQuantity).ToList();
                    break;
            }

            return sortedItems;
        }
        private ExpressionStarter<ListErrorInvestigationWebDto> ErrorInvestigationBuildFilterCondition(ListErrorInvestigationWebModel listErrorInvestigation)
        {
            var condition = PredicateBuilder.New<ListErrorInvestigationWebDto>(true);
            if (!string.IsNullOrEmpty(listErrorInvestigation.Plant))
            {
                condition = condition.And(x => x.Plant.Contains(listErrorInvestigation.Plant));
            }
            if (!string.IsNullOrEmpty(listErrorInvestigation.WHLoc))
            {
                condition = condition.And(x => x.WHLoc.Contains(listErrorInvestigation.WHLoc));
            }
            if (!string.IsNullOrEmpty(listErrorInvestigation.AssigneeAccount))
            {
                condition = condition.And(x => x.AssigneeAccount.Contains(listErrorInvestigation.AssigneeAccount));
            }
            if (!string.IsNullOrEmpty(listErrorInvestigation.ComponentCode))
            {
                condition = condition.And(x => x.ComponentCode.Contains(listErrorInvestigation.ComponentCode));
            }
            if (listErrorInvestigation.ErrorQuantityFrom.HasValue && listErrorInvestigation.ErrorQuantityTo.HasValue)
            {
                condition = condition.And(x => x.ErrorQuantity >= listErrorInvestigation.ErrorQuantityFrom &&
                                               x.ErrorQuantity <= listErrorInvestigation.ErrorQuantityTo);
            }
            if (listErrorInvestigation.ErrorMoneyFrom.HasValue && listErrorInvestigation.ErrorMoneyTo.HasValue)
            {
                condition = condition.And(x => x.ErrorMoney >= listErrorInvestigation.ErrorMoneyFrom &&
                                               x.ErrorMoney <= listErrorInvestigation.ErrorMoneyTo);
            }
            if (listErrorInvestigation.ErrorCategories.Any())
            {
                condition = condition.And(x => listErrorInvestigation.ErrorCategories!.Any(ec => ec == x.ErrorCategory));
            }
            if (listErrorInvestigation.Statuses.Any())
            {
                condition = condition.And(x => listErrorInvestigation.Statuses.Contains(x.Status));
            }
            if (listErrorInvestigation.InventoryIds.Any())
            {
                condition = condition.And(x => listErrorInvestigation.InventoryIds.Contains(x.InventoryId));
            }
            if (!string.IsNullOrEmpty(listErrorInvestigation.ComponentName))
            {
                condition = condition.And(x => x.ComponentName.Contains(listErrorInvestigation.ComponentName));
            }
            return condition;
        }

        public async Task<ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationDocumentsDto>>> ListErrorInvestigationDocuments(Guid inventoryId, string componentCode, int pageNum, int pageSize)
        {
            var inventoryAccounts = _inventoryContext.InventoryAccounts.AsNoTracking()
                                            .Select(ia => new
                                            {
                                                ia.UserId,
                                                ia.UserName
                                            });

            var listErrorInvestigationDocuments = from eid in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking()
                                                  join ei in _inventoryContext.ErrorInvestigations.AsNoTracking() on eid.ErrorInvestigationId equals ei.Id
                                                  join invDoc in _inventoryContext.InventoryDocs.AsNoTracking() on eid.InventoryDocId equals invDoc.Id
                                                  //join docCDetail in _inventoryContext.DocTypeCDetails.AsNoTracking() on invDoc.Id equals docCDetail.InventoryDocId into docCDetailGroup
                                                  //from docCDetail in docCDetailGroup.DefaultIfEmpty()
                                                  where ei.InventoryId == inventoryId && ei.ComponentCode == componentCode
                                                  orderby eid.DocCode
                                                  select new ListErrorInvestigationDocumentsDto
                                                  {
                                                      DocId = eid.InventoryDocId,
                                                      DocCode = eid.DocCode ?? string.Empty,
                                                      ModelCode = eid.ModelCode ?? string.Empty,
                                                      ComponentCode = ei.ComponentCode ?? string.Empty,
                                                      AttachModule = eid.DocType == InventoryDocType.C ? eid.AttachModule : eid.AttachModule ?? string.Empty,
                                                      Position = eid.PositionCode ?? string.Empty,
                                                      BOM = eid.BOM.HasValue ? eid.BOM.Value : 0,
                                                      TotalQuantity = invDoc.Quantity,
                                                      AssigneeAccount = eid.AssignedAccount.HasValue ? inventoryAccounts.Where(ia => ia.UserId == eid.AssignedAccount.Value).Select(x => x.UserName).FirstOrDefault() : string.Empty
                                                  };



            var totalRecords = listErrorInvestigationDocuments.Count();
            var items = await listErrorInvestigationDocuments.Skip(pageNum).Take(pageSize).ToListAsync();

            var errorInvestigation = await _inventoryContext.ErrorInvestigations.FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode);
            if (errorInvestigation == null)
            {
                return new ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationDocumentsDto>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.ErrorInvestigationNotFound
                };
            }

            if (errorInvestigation.Status != ErrorInvestigationStatusType.Investigated)
            {
                errorInvestigation.Status = ErrorInvestigationStatusType.UnderInvestigation;
            }
            errorInvestigation.UpdatedBy = _httpContext.UserFromContext()?.UserId;
            errorInvestigation.UpdatedAt = DateTime.Now;
            errorInvestigation.CurrentInvestigatorId = Guid.Parse(_httpContext.UserFromContext()?.UserId);
            errorInvestigation.InvestigatorId = Guid.Parse(_httpContext.UserFromContext()?.UserId);
            errorInvestigation.InvestigatorUserCode = _httpContext.UserFromContext()?.UserCode;
            _inventoryContext.ErrorInvestigations.Update(errorInvestigation);
            await _inventoryContext.SaveChangesAsync();
            return new ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationDocumentsDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = items,
                TotalRecords = totalRecords,
                Message = Constants.ResponseMessages.ListErrorInvestigationDocumentsSuccessfully
            };
        }

        public async Task<ResponseModel> ListErrorInvestigationDocumentsCheck(string? userCode, Guid inventoryId, string componentCode)
        {
            var currentUserCode = !string.IsNullOrEmpty(userCode) ? userCode : _httpContext.UserFromContext()?.UserCode;
            var errorInvestigation = await _inventoryContext.ErrorInvestigations.FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.ComponentCode == componentCode);
            if (errorInvestigation == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.ErrorInvestigationNotFound
                };
            }

            if (errorInvestigation.Status != ErrorInvestigationStatusType.UnderInvestigation)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK
                };
            }

            //.Kiểm tra người dùng đang đang đăng nhập và người dùng lần gần nhất điều tra(Trạng thái là: Đang điều tra).Nếu 2 tài khoản giống nhau thì kiểm tra mã nhân viên:
            //Mã nhân viên khác lúc đăng nhập và mã nhân viên lần cuối cùng điều tra(Trạng thái: Đang điều tra) khác nhau thì không cho phép vào điều tra:

            //Lấy ra danh sách người dùng:
            var currentUserId = Guid.TryParse(_httpContext?.UserFromContext()?.UserId, out var guid) ? guid : Guid.Empty;
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            //CurrentErrorInvestigationUserCode:
            var investigationUserCode = errorInvestigation?.InvestigatorUserCode;
            var currentErrorInvestigationUserCode = string.IsNullOrEmpty(investigationUserCode) ? users?.Where(x => x.Id == errorInvestigation.CurrentInvestigatorId).Select(x => x.Code).FirstOrDefault() : investigationUserCode;

            if (errorInvestigation.Status == ErrorInvestigationStatusType.UnderInvestigation &&
                (currentUserCode != currentErrorInvestigationUserCode) && errorInvestigation.CurrentInvestigatorId == currentUserId)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ComponentErrorInvestigating,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ComponentErrorInvestigating)
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK
            };
        }

        public async Task<ResponseModel> ListErrorInvestigationInventoryDocsHistory(string componentCode, ErrorInvestigationInventoryDocsHistoryModel inventories)
        {
            var getErrorInvestigation = await _inventoryContext.ErrorInvestigations.AsNoTracking()
                                                        .Where(x => x.ComponentCode == componentCode)
                                                        .FirstOrDefaultAsync();
            //Check ErrorInvestigation not found:
            if (getErrorInvestigation == null)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ErrorInvestigationNotFound,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ErrorInvestigationNotFound)
                };
            }
            var user = _httpContext.UserFromContext();
            var inventoryId = user.InventoryLoggedInfo?.InventoryModel?.InventoryId;


            var errorCategoryNames = await _inventoryContext.GeneralSettings.AsNoTracking()
                                        .Where(x => x.Type == GeneralSettingType.ErrorCategory && x.InventoryId == inventoryId)
                                        .Select(x => new
                                        {
                                            ErrorCategory = x.Key1,
                                            ErrorCategoryName = x.Value1
                                        }).ToListAsync();

            //Lấy ra danh sách người dùng:
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;


            var result = (from history in _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                          join investigation in _inventoryContext.ErrorInvestigations.AsNoTracking()
                              on history.ErrorInvestigationId equals investigation.Id into investigationGroup
                          from investigation in investigationGroup.DefaultIfEmpty()
                          join inventory in _inventoryContext.Inventories.AsNoTracking()
                              on investigation.InventoryId equals inventory.Id into inventoryGroup
                          from inventory in inventoryGroup.DefaultIfEmpty()
                          where investigation.ComponentCode == componentCode
                          orderby history.CreatedAt descending
                          select new ErrorInvestigationInventoryDocsHistoryDto
                          {
                              Index = 0,
                              InvestigatingCount = 0,
                              InventoryName = inventory.Name,
                              OldValue = history.OldValue,
                              NewValue = history.NewValue,
                              ErrorCategory = history.ErrorCategory,
                              ErrorDetails = history.ErrorDetails,
                              Investigator = history.InvestigatorUserCode,
                              InvestigationDatetime = history.CreatedAt.ToString("HH:mm dd/MM/yyyy"),
                              ConfirmInvestigationDatetime = history.ConfirmationTime.ToString("HH:mm dd/MM/yyyy"),
                              InvestigationImage1 = history.ConfirmationImage1,
                              InvestigationImage2 = history.ConfirmationImage2,
                              InvestigatorId = history.InvestigatorId
                          });

            if (inventories.InventoryNames != null && inventories.InventoryNames.Any())
            {
                result = result.Where(x => inventories.InventoryNames.Contains(x.InventoryName));
            }
            else
            {
                result = result.GroupBy(x => x.InventoryName)
                                .Select(g => g.FirstOrDefault()).AsQueryable();
            }

            var resultList = await result.ToListAsync();
            resultList = resultList.Select((item, index) =>
            {
                item.Index = index + 1;
                item.InvestigatingCount = resultList.Count - index;
                item.ErrorCategoryName = errorCategoryNames.FirstOrDefault(x => x.ErrorCategory == item.ErrorCategory.ToString())?.ErrorCategoryName ?? string.Empty;
                item.Investigator = string.IsNullOrEmpty(item?.Investigator) ? users.FirstOrDefault(ia => ia.Id == item?.InvestigatorId)?.Code : item?.Investigator;
                return item;
            }).ToList();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Data = resultList
            };
        }

        public async Task<ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationHistoryWebDto>>> ListErrorInvestigaitonHistory(ListErrorInvestigationHistoryWebModel listErrorInvestigationHistory)
        {
            var inventoryAccounts = _inventoryContext.InventoryAccounts.AsNoTracking()
                                            .Select(ia => new
                                            {
                                                ia.UserId,
                                                ia.UserName
                                            });
            var inventoryNames = _inventoryContext.Inventories.AsNoTracking().Where(x => listErrorInvestigationHistory.InventoryIds.Any() && listErrorInvestigationHistory.InventoryIds.Contains(x.Id))
                                        .Select(i => new
                                        {
                                            i.Id,
                                            i.Name
                                        });

            var user = _httpContext.UserFromContext();
            var inventoryId = user.InventoryLoggedInfo?.InventoryModel?.InventoryId;

            var errorCategories = await _inventoryContext.GeneralSettings.AsNoTracking()
                                        .Where(gs => gs.Type == GeneralSettingType.ErrorCategory && gs.InventoryId == inventoryId)
                                        .Select(x => new
                                        {
                                            ErrorCategoryKey = x.Key1,
                                            ErrorCategoryName = x.Value1
                                        }).ToListAsync();

            var tempErrorInvestigationHistories = from eih in _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                  join ei in _inventoryContext.ErrorInvestigations.AsNoTracking() on eih.ErrorInvestigationId equals ei.Id into eiGroup
                                                  from ei in eiGroup.DefaultIfEmpty()
                                                  where listErrorInvestigationHistory.InventoryIds.Contains(ei.InventoryId)
                                                  //orderby eih.CreatedAt descending
                                                  select new
                                                  {
                                                      ErrorInvestigationHistoryId = eih.Id,
                                                      ErrorInvestigationId = ei.Id,
                                                      InventoryId = ei.InventoryId,
                                                      ComponentCode = ei.ComponentCode,
                                                      ComponentName = ei.ComponentName,
                                                      PositionCode = eih.PositionCode,
                                                      OldValue = eih.OldValue,
                                                      InvestigationQuantity = eih.NewValue - eih.OldValue,
                                                      ErrorCategory = eih.ErrorCategory,
                                                      ErrorDetails = eih.ErrorDetails,
                                                      Status = ei.Status,
                                                      ErrorType = eih.ErrorType,
                                                      Investigator = eih.InvestigatorUserCode,
                                                      InvestigatorId = eih.InvestigatorId,
                                                      ConfirmInvestigator = eih.ConfirmInvestigatorId,
                                                      ApproveInvestigator = eih.ApproveInvestigatorId,
                                                      InvestigationDateTime = eih.ConfirmationTime.ToString("dd/MM/yyyy HH:mm")
                                                  };
            var errorInvestigationHistoryListQuery = (from his in tempErrorInvestigationHistories
                                                      join doc in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking()
                                                          on new { his.ErrorInvestigationId, his.PositionCode } equals new { doc.ErrorInvestigationId, doc.PositionCode } into docGroup
                                                      from doc in docGroup.DefaultIfEmpty()
                                                      where doc.DocType == InventoryDocType.A
                                                      select new ListErrorInvestigationHistoryWebDto
                                                      {
                                                          ErrorInvestigationHistoryId = his.ErrorInvestigationHistoryId,
                                                          ErrorInvestigationId = his.ErrorInvestigationId,
                                                          InventoryId = his.InventoryId,
                                                          ComponentCode = his.ComponentCode,
                                                          ComponentName = his.ComponentName,
                                                          Position = his.PositionCode,
                                                          ErrorQuantity = Math.Round(his.OldValue, 3),
                                                          InvestigationQuantity = his.InvestigationQuantity,
                                                          ErrorCategory = (int?)his.ErrorCategory,
                                                          ErrorDetail = his.ErrorDetails,
                                                          Status = his.Status,
                                                          Plant = doc.Plant,
                                                          WHLoc = doc.WareHouseLocation,
                                                          TotalQuantity = Math.Round(doc.TotalQuantity ?? 0, 3),
                                                          AccountQuantity = Math.Round(doc.AccountQuantity ?? 0, 3),
                                                          ErrorMoney = Math.Round(his.OldValue * doc.UnitPrice ?? 0, 3),
                                                          AssigneeAccount = doc.AssignedAccount.HasValue
                                                                            ? (inventoryAccounts.Where(ia => ia.UserId == doc.AssignedAccount).Select(ia => ia.UserName).FirstOrDefault() ?? string.Empty)
                                                                            : string.Empty,
                                                          Investigator = his.Investigator,
                                                          InventoryName = inventoryNames.Where(i => i.Id == his.InventoryId).Select(i => i.Name).FirstOrDefault() ?? string.Empty,
                                                          ErrorType = his.ErrorType,
                                                          ConfirmInvestigator = his.ConfirmInvestigator.HasValue
                                                                                ? (inventoryAccounts.Where(ia => ia.UserId == his.ConfirmInvestigator.Value).Select(ia => ia.UserName).FirstOrDefault() ?? string.Empty)
                                                                                : string.Empty,
                                                          ApproveInvestigator = his.ApproveInvestigator.HasValue
                                                                                ? (inventoryAccounts.Where(ia => ia.UserId == his.ApproveInvestigator.Value).Select(ia => ia.UserName).FirstOrDefault() ?? string.Empty)
                                                                                : string.Empty,
                                                          InvestigationDateTime = his.InvestigationDateTime,
                                                          InvestigatorId = his.InvestigatorId,
                                                      }).AsEnumerable();

            var condition = ErrorInvestigationHistoryBuildFilterCondition(listErrorInvestigationHistory);
            var query = errorInvestigationHistoryListQuery.Where(condition);
            var totalRecords = query.Count();

            var itemsPagination = new List<ListErrorInvestigationHistoryWebDto>();
            if (listErrorInvestigationHistory.IsExportExcel)
            {
                itemsPagination = query.OrderByDescending(x => x.ErrorQuantity).ToList();
            }
            else
            {
                itemsPagination = query.OrderByDescending(x => x.ErrorQuantity).Skip(listErrorInvestigationHistory.Skip).Take(listErrorInvestigationHistory.Take).ToList();
            }

            //Lấy ra danh sách người dùng:
            var request = new RestRequest($"api/internal/list/user");
            request.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            request.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);

            var response = await _identityRestClient.GetAsync(request);

            var responseModel = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<ListUserModel>>>(response?.Content ?? string.Empty);
            var users = responseModel?.Data;

            var items = itemsPagination.GroupBy(x => x.ComponentCode)
                                        .SelectMany(group =>
                                        {
                                            int count = group.Count();
                                            return group.Select((item, index) =>
                                            {
                                                item.ErrorCategoryName = errorCategories.Where(x => x.ErrorCategoryKey == item.ErrorCategory.ToString())
                                                                        .Select(x => x.ErrorCategoryName)
                                                                        .FirstOrDefault();
                                                item.InvestigationHistoryCount = $"Lần {index + 1}";
                                                item.Investigator = string.IsNullOrEmpty(item?.Investigator) ? users.FirstOrDefault(ia => ia.Id == item?.InvestigatorId)?.Code : item?.Investigator;
                                                return item;
                                            });
                                        }).ToList();

            return new ErrorInvestigationResponseModel<IEnumerable<ListErrorInvestigationHistoryWebDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = items,
                TotalRecords = totalRecords,
                Message = Constants.ResponseMessages.ListErrorInvestigationHistoryWebSuccessfully
            };
        }

        private ExpressionStarter<ListErrorInvestigationHistoryWebDto> ErrorInvestigationHistoryBuildFilterCondition(ListErrorInvestigationHistoryWebModel listErrorInvestigationHistory)
        {
            var condition = PredicateBuilder.New<ListErrorInvestigationHistoryWebDto>(true);

            if (!string.IsNullOrEmpty(listErrorInvestigationHistory.Plant))
            {
                condition = condition.And(x => x.Plant.Contains(listErrorInvestigationHistory.Plant));
            }
            if (!string.IsNullOrEmpty(listErrorInvestigationHistory.WHLoc))
            {
                condition = condition.And(x => x.WHLoc.Contains(listErrorInvestigationHistory.WHLoc));
            }
            if (!string.IsNullOrEmpty(listErrorInvestigationHistory.ComponentCode))
            {
                condition = condition.And(x => x.ComponentCode.Contains(listErrorInvestigationHistory.ComponentCode));
            }
            if (!string.IsNullOrEmpty(listErrorInvestigationHistory.AssigneeAccount))
            {
                condition = condition.And(x => x.AssigneeAccount.Contains(listErrorInvestigationHistory.AssigneeAccount));
            }
            if (listErrorInvestigationHistory.ErrorQuantityFrom.HasValue && listErrorInvestigationHistory.ErrorQuantityTo.HasValue)
            {
                condition = condition.And(x => x.ErrorQuantity >= listErrorInvestigationHistory.ErrorQuantityFrom &&
                                               x.ErrorQuantity <= listErrorInvestigationHistory.ErrorQuantityTo);
            }
            if (listErrorInvestigationHistory.ErrorMoneyFrom.HasValue && listErrorInvestigationHistory.ErrorMoneyTo.HasValue)
            {
                condition = condition.And(x => x.ErrorMoney >= listErrorInvestigationHistory.ErrorMoneyFrom &&
                                               x.ErrorMoney <= listErrorInvestigationHistory.ErrorMoneyTo);
            }
            if (listErrorInvestigationHistory.ErrorCategories.Any())
            {
                condition = condition.And(x => listErrorInvestigationHistory.ErrorCategories!.Any(ec => ec == x.ErrorCategory));
            }
            if (listErrorInvestigationHistory.InventoryIds.Any())
            {
                condition = condition.And(x => listErrorInvestigationHistory.InventoryIds.Contains(x.InventoryId));
            }
            if (listErrorInvestigationHistory.ErrorTypes.Any())
            {
                condition = condition.And(x => listErrorInvestigationHistory.ErrorTypes!.Any(et => et == x.ErrorType));
            }

            return condition;
        }

        public async Task<ResponseModel> UpdateErrorTypesForInvestigationHistory(List<Guid> docIds, AdjustmentType type)
        {
            var currUserId = _httpContext.CurrentUserId();
            if (type == AdjustmentType.Adjust)
            {
                //Check ErrorType != Retain => show error message: "Chỉ điều chỉnh lịch sử điều tra với trạng thái Giữ lại";
                var checkErrorTypes = await _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                                  .AnyAsync(x => docIds.Contains(x.Id) && x.ErrorType != ErrorType.Retain);
                if (checkErrorTypes)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.UpdateErrorTypesIsRemain,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.UpdateErrorTypesIsRemain)
                    };
                }
                var updateErrorTypes = _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                                  .Where(x => docIds.Contains(x.Id) && x.ErrorType == ErrorType.Retain);
                if (!updateErrorTypes.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = Constants.ResponseMessages.NotFound
                    };
                }
                try
                {
                    //Cập nhật ErrorType từ giữ lại sang chờ xác nhận
                    await updateErrorTypes.ExecuteUpdateAsync(t => t.SetProperty(b => b.ErrorType, x => ErrorType.AwaitConfirmation)
                                                                .SetProperty(b => b.UpdatedBy, x => currUserId)
                                                                .SetProperty(b => b.ConfirmInvestigatorId, x => Guid.Parse(currUserId))
                                                                .SetProperty(b => b.UpdatedAt, x => DateTime.Now)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogHttpContext(_httpContext, ex.Message);
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Lỗi hệ thống khi thực hiện điều chỉnh loại sai số."
                    };
                }
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = $"Điều chỉnh loại sai số thành công."
                };
            }
            else if (type == AdjustmentType.AdjustReject)
            {
                //Check ErrorType != AwaitConfirmation => show error message: "Chỉ xác nhận điều chỉnh lịch sử điều tra với trạng thái Chờ xác nhận";
                var errorTypeIsAwaitConfirmationOrCancelCheck = await _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                                      .AnyAsync(x => docIds.Contains(x.Id) && (x.ErrorType == ErrorType.AwaitConfirmation || x.ErrorType == ErrorType.Cancel));
                if (!errorTypeIsAwaitConfirmationOrCancelCheck)
                {
                    return new ResponseModel
                    {
                        Code = (int)HttpStatusCodes.UpdateErrorTypesIsAwaitConfirmOrCancel,
                        Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.UpdateErrorTypesIsAwaitConfirmOrCancel)
                    };
                }

                var errorTypeIsAwaitConfirmationUpdate = _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                                  .Where(x => docIds.Contains(x.Id) && (x.ErrorType == ErrorType.AwaitConfirmation || x.ErrorType == ErrorType.Cancel));
                if (!errorTypeIsAwaitConfirmationUpdate.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = Constants.ResponseMessages.NotFound
                    };
                }
                try
                {
                    //Cập nhật ErrorType từ chờ xác nhận sang từ chối, đồng thời xóa tạm thời bản ghi đó:
                    await errorTypeIsAwaitConfirmationUpdate.ExecuteUpdateAsync(t => t.SetProperty(b => b.ErrorType, x => ErrorType.Cancel)
                                                                .SetProperty(b => b.UpdatedBy, x => currUserId)
                                                                .SetProperty(b => b.ApproveInvestigatorId, x => Guid.Parse(currUserId))
                                                                .SetProperty(b => b.UpdatedAt, x => DateTime.Now)
                                                             );
                }
                catch (Exception ex)
                {
                    _logger.LogHttpContext(_httpContext, ex.Message);

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Lỗi hệ thống khi thực hiện xác nhận điều chỉnh loại sai số."
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = $"Từ chối điều chỉnh loại sai số thành công."
                };
            }
            //"Chỉ xác nhận điều chỉnh lịch sử điều tra với trạng thái Chờ xác nhận và từ chối";
            var checkErrorTypeIsAwaitConfirmationOrCancel = await _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                                  .AnyAsync(x => docIds.Contains(x.Id) && (x.ErrorType == ErrorType.AwaitConfirmation || x.ErrorType == ErrorType.Cancel));
            if (!checkErrorTypeIsAwaitConfirmationOrCancel)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.UpdateErrorTypesIsAwaitConfirmOrCancel,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.UpdateErrorTypesIsAwaitConfirmOrCancel)
                };
            }

            var updateErrorTypeIsAwaitConfirmations = _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                                              .Where(x => docIds.Contains(x.Id) && (x.ErrorType == ErrorType.AwaitConfirmation || x.ErrorType == ErrorType.Cancel));
            if (!updateErrorTypeIsAwaitConfirmations.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.NotFound
                };
            }
            try
            {
                //Cập nhật ErrorType từ chờ xác nhận sang điều chỉnh, đồng thời xóa tạm thời bản ghi đó:
                await updateErrorTypeIsAwaitConfirmations.ExecuteUpdateAsync(t => t.SetProperty(b => b.ErrorType, x => ErrorType.Adjustment)
                                                            .SetProperty(b => b.IsDelete, x => true)
                                                            .SetProperty(b => b.UpdatedBy, x => currUserId)
                                                            .SetProperty(b => b.ApproveInvestigatorId, x => Guid.Parse(currUserId))
                                                            .SetProperty(b => b.UpdatedAt, x => DateTime.Now)
                                                         );
            }
            catch (Exception ex)
            {
                _logger.LogHttpContext(_httpContext, ex.Message);

                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống khi thực hiện xác nhận điều chỉnh loại sai số."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = $"Xác nhận điều chỉnh loại sai số thành công."
            };

        }

        public async Task<ResponseModel> InvestigationPercent(Guid inventoryId)
        {
            //Tổng số lượng điều chỉnh ứng với các linh kiện:
            var adjustmentByComponentTotal = (from e in _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                              join i in _inventoryContext.ErrorInvestigations.AsNoTracking()
                                              on e.ErrorInvestigationId equals i.Id into ei
                                              from i in ei.DefaultIfEmpty()
                                              where e.ErrorType != ErrorType.Adjustment && i.InventoryId == inventoryId
                                              group e by e.ComponentCode into g
                                              select new
                                              {
                                                  ComponentCode = g.Key,
                                                  TotalAdjustment = g.Sum(e => e.OldValue - e.NewValue)
                                              });
            double adjustmentTotal = 0;
            if (adjustmentByComponentTotal.Any())
            {
                adjustmentTotal = adjustmentByComponentTotal.Sum(x => x.TotalAdjustment);
            }
            //Tổng số lượng ban đầu(Loại bỏ đi những bản ghi có ErrorType là điều chỉnh):
            var initialTotal = (from e in _inventoryContext.ErrorInvestigationHistories.AsNoTracking()
                                join i in _inventoryContext.ErrorInvestigations.AsNoTracking()
                                on e.ErrorInvestigationId equals i.Id into ei
                                from i in ei.DefaultIfEmpty()
                                where i.InventoryId == inventoryId
                                group e by e.ComponentCode into g
                                select new
                                {
                                    ComponentCode = g.Key,
                                    FirstOldValue = g.OrderBy(e => e.CreatedAt).First().OldValue,
                                    AdjustmentSum = g.Where(e => e.ErrorType == ErrorType.Adjustment)
                                                     .Sum(e => e.OldValue - e.NewValue)
                                }).Sum(g => g.FirstOldValue - g.AdjustmentSum);

            var result = initialTotal != 0 ? Math.Round(adjustmentTotal / initialTotal, 3) : 0;
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = $"Tính tỷ điều tra số thành công.",
                Data = result
            };
        }

        public async Task<ResponseModel<IEnumerable<ImportErrorInvestigationUpdatePivotDto>>> ErrorPercent(Guid inventoryId)
        {
            var generalSettings = await _inventoryContext.GeneralSettings.AsNoTracking()
                                        .Where(x => x.InventoryId == inventoryId && !x.IsDelete && x.Type == GeneralSettingType.InvestigationPercent)
                                        .Select(x => new ImportErrorInvestigationUpdatePivotDto
                                        {
                                            Plant = x.Value1,
                                            ErrorPercent = Convert.ToDouble(x.Value2)
                                        }).ToListAsync();
            if (generalSettings.Any())
            {
                return new ResponseModel<IEnumerable<ImportErrorInvestigationUpdatePivotDto>>
                {
                    Code = StatusCodes.Status200OK,
                    Message = $"Tính tỷ lệ sai số thành công.",
                    Data = generalSettings
                };
            }
            var query = from ei in _inventoryContext.ErrorInvestigations.AsNoTracking()
                        join eid in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking()
                            on ei.Id equals eid.ErrorInvestigationId into joined
                        from eid in joined.DefaultIfEmpty()
                        where ei.InventoryId == inventoryId
                              && eid.DocType == InventoryDocType.A
                              && eid.ErrorQuantity != 0
                        group eid by eid.Plant into g
                        select new ImportErrorInvestigationUpdatePivotDto
                        {
                            Plant = g.Key,
                            TotalErrorMoney = Math.Round(g.Sum(e => Math.Abs(e.ErrorMoney ?? 0)), 3),
                            TotalAccountQuantity = Math.Round(g.Sum(e => e.AccountQuantity ?? 0), 3),
                            ErrorPercent = Math.Round(g.Sum(e => e.AccountQuantity ?? 0) == 0 ? 0 : g.Sum(e => Math.Abs(e.ErrorMoney ?? 0)) / g.Sum(e => e.AccountQuantity.Value), 3)
                        };

            var result = await query.ToListAsync();

            return new ResponseModel<IEnumerable<ImportErrorInvestigationUpdatePivotDto>>
            {
                Code = StatusCodes.Status200OK,
                Message = $"Tính tỷ lệ sai số thành công.",
                Data = result
            };
        }
        private bool ValidateErrorQuantityImportErrorInvestigation(ImportErrorInvestigationUpdateModel item,
                                                                    IEnumerable<ErrorInvestigationQuantityImportDto> errorInvestigationQuantityImportDtos,
                                                                    IEnumerable<InventoryErrorInvestigationQuantityImportDto> inventoryErrorInvestigationQuantityImportDtos,
                                                                    IEnumerable<ErrorCategoryManagementDto> errorCategoryManagementDtos
                                                                    )
        {
            var inventoryId = inventoryErrorInvestigationQuantityImportDtos.FirstOrDefault(x => x.InventoryName == item.InventoryName)?.InventoryId;
            if (!inventoryId.HasValue)
            {
                item.Error.TryAddModelError(nameof(item.InventoryName), "Đợt kiểm kê không tồn tại");
                return false;
            }
            var getCurrentErrorQuantity = errorInvestigationQuantityImportDtos.FirstOrDefault(x => x.InventoryId == inventoryId
                                                                && x.ComponentCode == item.ComponentCode && x.PositionCode == item.PositionCode)?.ErrorQuantity;

            //Kiểm tra mã linh kiện với vị trí có tồn tại trên hệ thống hay không?
            if (getCurrentErrorQuantity == null)
            {
                item.Error.TryAddModelError(nameof(item.ComponentCode), $"Mã linh kiện {item.ComponentCode} với vị trí {item.PositionCode} không tìm thấy trên hệ thống.");
                return false;
            }

            // Kiểm tra nếu cả hai có cùng dấu (cả đều âm hoặc đều dương)
            if ((getCurrentErrorQuantity < 0 && item.ErrorQuantity < 0) || (getCurrentErrorQuantity > 0 && item.ErrorQuantity > 0))
            {
                item.Error.TryAddModelError(nameof(item.ErrorQuantity), "Không được điều chỉnh số lượng cùng dấu với số lượng sai số.");
                return false;
            }

            // Số lượng điều chỉnh không được lớn hơn số lượng chênh lệch:
            if (Math.Abs(item.ErrorQuantity) > Math.Abs((double)getCurrentErrorQuantity))
            {
                item.Error.TryAddModelError(nameof(item.ErrorQuantity), "Số lượng điều chỉnh không được lớn hơn số lượng chênh lệch.");
                return false;
            }

            //Check xem phân loại lỗi có tồn tại trên hệ thống hay không:
            var checkExistedErrorCategory = errorCategoryManagementDtos.Any(x => x.ErrorCategoryName.ToLower() == item.ErrorCategory.ToLower());
            if (!checkExistedErrorCategory)
            {
                item.Error.TryAddModelError(nameof(item.ErrorCategory), "Phân loại lỗi không tồn tại trên hệ thống.");
                return false;
            }

            return true;
        }
        private bool ValidateRequiredImportErrorInvestigation(ImportErrorInvestigationUpdateModel item)
        {
            //Validate InventoryName required:
            if (string.IsNullOrEmpty(item.InventoryName))
            {
                item.Error.TryAddModelError(nameof(item.InventoryName), "Vui lòng nhập đợt kiểm kê.");
                return false;
            }
            //Validate Plant required:
            if (string.IsNullOrEmpty(item.Plant))
            {
                item.Error.TryAddModelError(nameof(item.Plant), "Vui lòng nhập plant.");
                return false;
            }
            //Validate WHLoc required:
            if (string.IsNullOrEmpty(item.WHLoc))
            {
                item.Error.TryAddModelError(nameof(item.WHLoc), "Vui lòng nhập whloc.");
                return false;
            }
            //Validate ComponentCode required:
            if (string.IsNullOrEmpty(item.ComponentCode))
            {
                item.Error.TryAddModelError(nameof(item.ComponentCode), "Vui lòng nhập mã linh kiện.");
                return false;
            }
            //Validate PositionCode required:
            if (string.IsNullOrEmpty(item.PositionCode))
            {
                item.Error.TryAddModelError(nameof(item.PositionCode), "Vui lòng nhập vị trí.");
                return false;
            }
            //Validate ErrorDetail required:
            if (string.IsNullOrEmpty(item.ErrorDetail))
            {
                item.Error.TryAddModelError(nameof(item.ErrorDetail), "Vui lòng nhập nguyên nhân sai số.");
                return false;
            }
            //Validate ErrorCategory required:
            if (string.IsNullOrEmpty(item.ErrorCategory))
            {
                item.Error.TryAddModelError(nameof(item.ErrorCategory), "Vui lòng nhập phân loại lỗi.");
                return false;
            }

            //Validate ErrorQuantity required:

            if (!double.TryParse(item.ErrorQuantity.ToString(), out var convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.ErrorQuantity), "ErrorQuantity: Số lượng không đúng.");
                return false;
            }

            return true;
        }

        public async Task<ImportResponseModel<byte[]>> ImportErrorInvestigationUpdate(IFormFile file)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string[] allowExtension = new[] { ".xlsx" };

            if (allowExtension.Contains(fileExtension) == false || file.Length < 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật số lượng điều chỉnh."
                };
            }

            //error investigation list:
            var errorInvestigationList = from ei in _inventoryContext.ErrorInvestigations.AsNoTracking()
                                         join eid in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking()
                                             on ei.Id equals eid.ErrorInvestigationId into eidGroup
                                         from eid in eidGroup.DefaultIfEmpty()
                                         where eid.DocType == InventoryDocType.A && eid.ErrorQuantity != 0
                                         select new ErrorInvestigationQuantityImportDto
                                         {
                                             InventoryId = ei.InventoryId,
                                             ComponentCode = ei.ComponentCode,
                                             PositionCode = eid.PositionCode,
                                             Plant = eid.Plant,
                                             WHloc = eid.WareHouseLocation,
                                             ErrorQuantity = eid.ErrorQuantity
                                         };

            //inventories:
            var inventories = _inventoryContext.Inventories.AsNoTracking()
                                .Select(x => new InventoryErrorInvestigationQuantityImportDto
                                {
                                    InventoryId = x.Id,
                                    InventoryName = x.Name
                                });
            var user = _httpContext.UserFromContext();
            var currInventoryId = user.InventoryLoggedInfo?.InventoryModel?.InventoryId;

            //ErrorCategories:
            var errorCategories = _inventoryContext.GeneralSettings.AsNoTracking()
                                    .Where(x => x.Type == GeneralSettingType.ErrorCategory && x.InventoryId == currInventoryId)
                                    .Select(x => new ErrorCategoryManagementDto
                                    {
                                        Id = x.Id,
                                        ErrorCategoryKey = x.Key1,
                                        ErrorCategoryName = x.Value1
                                    });

            List<ImportErrorInvestigationUpdateModel> items = new();
            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    int totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    if (sourceSheet != null)
                    {
                        sourceSheet.Cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sourceSheet.Cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        sourceSheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        //get header column
                        int inventoryNameIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.InventoryName);
                        int plantIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.Plant);
                        int wHLocIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.WHloc);
                        int componentCodeIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.ComponentCode);
                        int positionCodeIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.PositionCode);
                        int errorQuantityIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.ErrorQuantity);
                        int errorCategoryIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.ErrorCategory);
                        int errorDetailIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdateExcel.ErrorDetail);

                        int[] headers = new int[] {
                            inventoryNameIndex,
                            plantIndex,
                            wHLocIndex,
                            componentCodeIndex,
                            positionCodeIndex,
                            errorQuantityIndex,
                            errorCategoryIndex,
                            errorDetailIndex
                        };

                        if (headers.Any(x => x == -1))
                        {
                            return new ImportResponseModel<byte[]>
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật số lượng điều chỉnh.",
                            };
                        }

                        foreach (var row in rows)
                        {
                            //Bỏ qua các dòng null
                            var isEmptyRow = sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.Columns].All(x => string.IsNullOrEmpty(x.Value?.ToString()));
                            if (isEmptyRow)
                            {
                                continue;
                            }
                            var item = new ImportErrorInvestigationUpdateModel();
                            item.InventoryName = sourceSheet.Cells[row, inventoryNameIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.Plant = sourceSheet.Cells[row, plantIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.WHLoc = sourceSheet.Cells[row, wHLocIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.ComponentCode = sourceSheet.Cells[row, componentCodeIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.PositionCode = sourceSheet.Cells[row, positionCodeIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.ErrorQuantity = double.TryParse(sourceSheet.Cells[row, errorQuantityIndex].Value?.ToString()?.Trim(), out double errorQuantity) ? errorQuantity : 0;
                            item.ErrorCategory = sourceSheet.Cells[row, errorCategoryIndex].Value?.ToString()?.Trim();
                            item.ErrorDetail = sourceSheet.Cells[row, errorDetailIndex].Value?.ToString()?.Trim() ?? string.Empty;

                            items.Add(item);
                        }

                        //Validate STT
                        foreach (var item in items)
                        {
                            //validate:
                            //Các trường trong file import bắt buộc nhập dữ liệu:
                            var validateItemsImportResult = ValidateRequiredImportErrorInvestigation(item);

                            //Kiểm tra trường ErrorQuantity nhập vào có cùng dấu (cả 2 cùng âm hoặc cùng dương) => Lỗi:
                            //Số lượng điều chỉnh không được lớn hơn số lượng chênh lệch:
                            var validateErrorQuantityResult = ValidateErrorQuantityImportErrorInvestigation(item, errorInvestigationList, inventories, errorCategories);
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                return new ImportResponseModel<byte[]>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }

            //Ghi vào excel các bản ghi bị lỗi
            var invalidItems = items.Where(x => x.Error.IsValid == false).ToList();

            MemoryStream stream = new MemoryStream();

            if (invalidItems.Any())
            {
                //Ghi lỗi vào file excel
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lỗi khi cập nhật số lượng điều chỉnh");

                    // Đặt tiêu đề cho cột
                    int sttIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.STT) + 1;
                    int inventoryNameIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.InventoryName) + 1;
                    int plantIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.Plant) + 1;
                    int whLocIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.WHloc) + 1;
                    int componentCodeIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.ComponentCode) + 1;
                    int positionCodeIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.PositionCode) + 1;
                    int errorQuantityIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.ErrorQuantity) + 1;
                    int errorCategoryIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.ErrorCategory) + 1;
                    int errorDetailIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.ErrorDetail) + 1;

                    int errorSummaryIndex = ImportErrorInvestigationUpdateExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdateExcel.ErrorSummary) + 1;


                    worksheet.Cells[1, sttIndex].Value = ImportErrorInvestigationUpdateExcel.STT;
                    worksheet.Cells[1, inventoryNameIndex].Value = ImportErrorInvestigationUpdateExcel.InventoryName;
                    worksheet.Cells[1, plantIndex].Value = ImportErrorInvestigationUpdateExcel.Plant;
                    worksheet.Cells[1, whLocIndex].Value = ImportErrorInvestigationUpdateExcel.WHloc;
                    worksheet.Cells[1, componentCodeIndex].Value = ImportErrorInvestigationUpdateExcel.ComponentCode;
                    worksheet.Cells[1, positionCodeIndex].Value = ImportErrorInvestigationUpdateExcel.PositionCode;
                    worksheet.Cells[1, errorQuantityIndex].Value = ImportErrorInvestigationUpdateExcel.ErrorQuantity;
                    worksheet.Cells[1, errorCategoryIndex].Value = ImportErrorInvestigationUpdateExcel.ErrorCategory;
                    worksheet.Cells[1, errorDetailIndex].Value = ImportErrorInvestigationUpdateExcel.ErrorDetail;

                    worksheet.Cells[1, errorSummaryIndex].Value = ImportErrorInvestigationUpdateExcel.ErrorSummary;

                    // Đặt kiểu và màu cho tiêu đề
                    using (var range = worksheet.Cells[1, sttIndex, 1, errorSummaryIndex])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.None;
                    }

                    // Điền dữ liệu vào Excel
                    for (int i = 0; i < invalidItems.Count(); i++)
                    {
                        var item = invalidItems.ElementAtOrDefault(i);

                        int stt = i + 1;
                        //Tổng hợp message lỗi
                        var errMessage = item.Error.Values.SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)
                                                             .Distinct();

                        worksheet.Cells[i + 2, sttIndex].Value = stt;
                        worksheet.Cells[i + 2, inventoryNameIndex].Value = item.InventoryName;
                        worksheet.Cells[i + 2, plantIndex].Value = item.Plant;
                        worksheet.Cells[i + 2, whLocIndex].Value = item.WHLoc;
                        worksheet.Cells[i + 2, componentCodeIndex].Value = item.ComponentCode;
                        worksheet.Cells[i + 2, positionCodeIndex].Value = item.PositionCode;
                        worksheet.Cells[i + 2, errorQuantityIndex].Value = item.ErrorQuantity;
                        worksheet.Cells[i + 2, errorCategoryIndex].Value = item.ErrorCategory;
                        worksheet.Cells[i + 2, errorDetailIndex].Value = item.ErrorDetail;
                        //worksheet.Cells[i + 2, note].Value = "";
                        worksheet.Cells[i + 2, errorSummaryIndex].Value = string.Join("\n", errMessage);

                        using (var errorRange = worksheet.Cells[i + 2, errorSummaryIndex, i + 2, errorSummaryIndex])
                        {
                            errorRange.Style.Font.Color.SetColor(Color.Red);
                            errorRange.Style.Fill.PatternType = ExcelFillStyle.None;
                        }
                    }

                    // Lưu file Excel
                    package.SaveAs(stream);
                }
            }

            //Sau khi đã tổng hợp các bản ghi từ file excel và đánh dấu các lỗi validate vào từng item
            //Với những item hợp lệ sẽ thực hiện những logic sau:
            //1. Cập nhật trạng thái thành đã điều chỉnh
            //2. Cập nhật lại số lượng sai số
            //3. Ghi lại lịch sử điều chỉnh

            var validItems = items.Where(x => x.Error.IsValid).ToList();

            // Lấy danh sách InventoryIds:
            var inventoryDict = inventories.ToDictionary(x => x.InventoryName, x => x.InventoryId);

            // Lấy danh sách ErrorInvestigations cần cập nhật
            var inventoryIds = validItems.Select(item => inventoryDict.ContainsKey(item.InventoryName) ? inventoryDict[item.InventoryName] : (Guid?)null)
                             .Where(id => id.HasValue)
                             .Select(id => id.Value)
                             .ToHashSet();

            var errorInvestigationDict = await _inventoryContext.ErrorInvestigations.AsNoTracking()
                                                .Where(x => inventoryIds.Contains(x.InventoryId) && validItems.Select(i => i.ComponentCode).Contains(x.ComponentCode))
                                                .ToDictionaryAsync(x => new { x.InventoryId, x.ComponentCode });

            // Lấy danh sách ErrorInvestigationInventoryDocs:
            var errorInvestigationIds = errorInvestigationDict.Values.Select(x => x.Id).ToHashSet();
            var errorInvestigationDocsDict = await _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking()
                                                    .Where(x => errorInvestigationIds.Contains(x.ErrorInvestigationId) && x.DocType == InventoryDocType.A)
                                                    .GroupBy(x => x.ErrorInvestigationId)
                                                    .ToDictionaryAsync(g => g.Key, g => g.OrderByDescending(x => x.ErrorQuantity).FirstOrDefault());

            List<BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.ErrorInvestigation> updatedErrorInvestigations = new List<BIVN.FixedStorage.Services.Inventory.API.Infrastructure.Entity.ErrorInvestigation>();
            List<ErrorInvestigationInventoryDoc> updatedErrorInvestigationDocs = new List<ErrorInvestigationInventoryDoc>();
            List<ErrorInvestigationHistory> newErrorInvestigationHistories = new List<ErrorInvestigationHistory>();
            var userId = _httpContext.UserFromContext().InventoryLoggedInfo.AccountId.ToString();
            var investigatorId = _httpContext.UserFromContext().InventoryLoggedInfo.AccountId;

            foreach (var item in validItems)
            {
                if (!inventoryDict.TryGetValue(item.InventoryName, out var inventoryId)) continue;
                if (!errorInvestigationDict.TryGetValue(new { InventoryId = inventoryId, item.ComponentCode }, out var errorInvestigation)) continue;

                // 1. Cập nhật trạng thái thành đã điều chỉnh
                errorInvestigation.Status = ErrorInvestigationStatusType.Investigated;
                updatedErrorInvestigations.Add(errorInvestigation);

                // 2. Cập nhật lại số lượng sai số
                if (!errorInvestigationDocsDict.TryGetValue(errorInvestigation.Id, out var errorInvestigationDoc)) continue;

                var oldErrorQuantity = errorInvestigationDoc.ErrorQuantity ?? 0;
                errorInvestigationDoc.ErrorQuantity = errorInvestigationDoc.ErrorQuantity.HasValue
                                                        ? item.ErrorQuantity + errorInvestigationDoc.ErrorQuantity
                                                        : item.ErrorQuantity;
                errorInvestigationDoc.ErrorMoney = errorInvestigationDoc.ErrorQuantity * errorInvestigationDoc.UnitPrice;
                updatedErrorInvestigationDocs.Add(errorInvestigationDoc);

                // 3. Ghi lại lịch sử điều chỉnh
                newErrorInvestigationHistories.Add(new ErrorInvestigationHistory
                {
                    Id = Guid.NewGuid(),
                    ErrorInvestigationId = errorInvestigation.Id,
                    NewValue = errorInvestigationDoc.ErrorQuantity.Value,
                    OldValue = oldErrorQuantity,
                    ConfirmationTime = DateTime.Now,
                    ErrorCategory = int.Parse(errorCategories.FirstOrDefault(x => x.ErrorCategoryName.ToLower() == item.ErrorCategory.ToLower())?.ErrorCategoryKey ?? "0"),
                    ComponentCode = item.ComponentCode,
                    ErrorDetails = item.ErrorDetail,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    ConfirmationImage1 = string.Empty,
                    ConfirmationImage2 = string.Empty,
                    InvestigatorId = investigatorId,
                    ComponentName = errorInvestigation.ComponentName ?? string.Empty,
                    PositionCode = errorInvestigationDoc.PositionCode ?? string.Empty,
                    ErrorType = ErrorType.Retain
                });
            }

            _inventoryContext.ErrorInvestigations.UpdateRange(updatedErrorInvestigations);

            _inventoryContext.ErrorInvestigationInventoryDocs.UpdateRange(updatedErrorInvestigationDocs);

            await _inventoryContext.ErrorInvestigationHistories.AddRangeAsync(newErrorInvestigationHistories);
            await _inventoryContext.SaveChangesAsync();


            var failCount = items.Where(x => x.Error.IsValid == false).Count();
            var successCount = items.Where(x => x.Error.IsValid == true).Count();

            var fileType = Constants.FileResponse.ExcelType;
            var fileName = string.Format("Capnhatsoluongdieuchinh_Fileloi_{0}", DateTime.Now.ToString(Constants.DatetimeFormat));

            return new ImportResponseModel<byte[]>
            {
                Bytes = stream.ToArray(),
                Code = StatusCodes.Status200OK,
                FailCount = failCount,
                SuccessCount = successCount,
                FileType = fileType,
                FileName = fileName,
            };

        }

        private byte[] InvalidItemsUpdatePivotImportToExcel(List<ImportErrorInvestigationUpdatePivotModel> invalidItems)
        {
            //Ghi lỗi vào file excel
            using (MemoryStream stream = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lỗi khi cập nhật dữ liệu pivot.");

                    // Đặt tiêu đề cho cột
                    int plantIndex = ImportErrorInvestigationUpdatePivotExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdatePivotExcel.Plant) + 1;
                    int wHLocIndex = ImportErrorInvestigationUpdatePivotExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdatePivotExcel.WHloc) + 1;
                    int accountQuantityIndex = ImportErrorInvestigationUpdatePivotExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdatePivotExcel.AccountQuantity) + 1;

                    int errorSummaryIndex = ImportErrorInvestigationUpdatePivotExcel.ExcelHeaders.IndexOf(ImportErrorInvestigationUpdatePivotExcel.ErrorSummary) + 1;

                    worksheet.Cells[1, plantIndex].Value = ImportErrorInvestigationUpdatePivotExcel.Plant;
                    worksheet.Cells[1, wHLocIndex].Value = ImportErrorInvestigationUpdatePivotExcel.WHloc;
                    worksheet.Cells[1, accountQuantityIndex].Value = ImportErrorInvestigationUpdatePivotExcel.AccountQuantity;

                    worksheet.Cells[1, errorSummaryIndex].Value = ImportErrorInvestigationUpdatePivotExcel.ErrorSummary;

                    // Đặt kiểu và màu cho tiêu đề
                    using (var range = worksheet.Cells[1, plantIndex, 1, errorSummaryIndex])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.None;
                    }

                    // Điền dữ liệu vào Excel
                    for (int i = 0; i < invalidItems.Count(); i++)
                    {
                        var item = invalidItems.ElementAtOrDefault(i);

                        //Tổng hợp message lỗi
                        var errMessage = item.Error.Values.SelectMany(x => x.Errors)
                                                             .Select(x => x.ErrorMessage)
                                                             .Distinct();

                        worksheet.Cells[i + 2, plantIndex].Value = item.Plant;
                        worksheet.Cells[i + 2, wHLocIndex].Value = item.WHLoc;
                        worksheet.Cells[i + 2, accountQuantityIndex].Value = item.AccountQuantity;

                        worksheet.Cells[i + 2, errorSummaryIndex].Value = string.Join("\n", errMessage);

                        using (var errorRange = worksheet.Cells[i + 2, errorSummaryIndex, i + 2, errorSummaryIndex])
                        {
                            errorRange.Style.Font.Color.SetColor(Color.Red);
                            errorRange.Style.Fill.PatternType = ExcelFillStyle.None;
                        }
                    }

                    // Lưu file Excel
                    package.SaveAs(stream);
                }
                return stream.ToArray();
            }
        }


        private bool ValidateExistedPlantUpdatePivotImport(ImportErrorInvestigationUpdatePivotModel item, List<ImportErrorInvestigationUpdatePivotDto> importErrorInvestigationUpdatePivotDtos)
        {
            var checkExistedPlant = importErrorInvestigationUpdatePivotDtos.Any(x => x.Plant == item.Plant);
            if (!checkExistedPlant)
            {
                item.Error.TryAddModelError(nameof(item.Plant), $"Plant {item.Plant} không có trên hệ thống.");
                return false;
            }
            return true;
        }

        private bool ValidateRequiredColumnUpdatePivotImport(ImportErrorInvestigationUpdatePivotModel item)
        {
            //Validate STT
            if (string.IsNullOrEmpty(item.Plant))
            {
                item.Error.TryAddModelError(nameof(item.Plant), "Vui lòng nhập Plant.");
                return false;
            }

            if (string.IsNullOrEmpty(item.AccountQuantity))
            {
                item.Error.TryAddModelError(nameof(item.AccountQuantity), "Vui lòng nhập Account Quantity.");
                return false;
            }
            if (!double.TryParse(item.AccountQuantity.ToString(), out var convertedQuantity))
            {
                item.Error.TryAddModelError(nameof(item.AccountQuantity), "Account Quantity: Số lượng không đúng.");
                return false;
            }
            return true;
        }

        public async Task<ImportResponseWithDataModel<byte[], List<ImportErrorInvestigationUpdatePivotDto>>> ImportErrorInvestigationUpdatePivot(IFormFile file, Guid inventoryId)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string[] allowExtension = new[] { ".xlsx" };

            if (allowExtension.Contains(fileExtension) == false || file.Length < 0)
            {
                return new ImportResponseWithDataModel<byte[], List<ImportErrorInvestigationUpdatePivotDto>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật dữ liệu Pivot."
                };
            }


            var user = _httpContext.UserFromContext();
            var userId = user.UserId;
            var updatePivotByPlants = await (from ei in _inventoryContext.ErrorInvestigations.AsNoTracking()
                                             join eid in _inventoryContext.ErrorInvestigationInventoryDocs.AsNoTracking()
                                                 on ei.Id equals eid.ErrorInvestigationId into joined
                                             from eid in joined.DefaultIfEmpty()
                                             where ei.InventoryId == inventoryId
                                                     && eid.DocType == InventoryDocType.A
                                                     && eid.ErrorQuantity != 0
                                             group eid by eid.Plant into g
                                             select new ImportErrorInvestigationUpdatePivotDto
                                             {
                                                 Plant = g.Key,
                                                 TotalErrorMoney = Math.Round(g.Sum(e => Math.Abs(e.ErrorMoney ?? 0)), 3),
                                                 TotalAccountQuantity = Math.Round(g.Sum(e => e.AccountQuantity ?? 0), 3),
                                                 ErrorPercent = Math.Round(g.Sum(e => e.AccountQuantity ?? 0) == 0 ? 0 : g.Sum(e => Math.Abs(e.ErrorMoney ?? 0)) / g.Sum(e => e.AccountQuantity.Value), 3)
                                             }).ToListAsync();

            List<ImportErrorInvestigationUpdatePivotModel> items = new();
            using (var memStream = new MemoryStream())
            {
                file.CopyTo(memStream);
                using (var sourcePackage = new ExcelPackage(memStream))
                {
                    var sourceSheet = sourcePackage.Workbook.Worksheets.FirstOrDefault();
                    int totalRowsCount = sourceSheet.Dimension.End.Row > 1 ? sourceSheet.Dimension.End.Row - 1 : sourceSheet.Dimension.End.Row;
                    var rows = Enumerable.Range(2, totalRowsCount).ToList();
                    if (sourceSheet != null)
                    {
                        sourceSheet.Cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sourceSheet.Cells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        sourceSheet.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        //get header column
                        int plantIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdatePivotExcel.Plant);
                        int wHLocIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdatePivotExcel.WHloc);
                        int accountQuantityIndex = sourceSheet.GetColumnIndex(Constants.ImportErrorInvestigationUpdatePivotExcel.AccountQuantity);


                        int[] headers = new int[] {
                            plantIndex,
                            wHLocIndex,
                            accountQuantityIndex

                        };

                        if (headers.Any(x => x == -1))
                        {
                            return new ImportResponseWithDataModel<byte[], List<ImportErrorInvestigationUpdatePivotDto>>
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "Vui lòng tải biểu mẫu và điền đầy đủ thông tin cần thiết để tiến hành cập nhật dữ liệu Pivot.",
                            };
                        }

                        foreach (var row in rows)
                        {
                            //Bỏ qua các dòng null
                            var isEmptyRow = sourceSheet.Cells[row, 1, row, sourceSheet.Dimension.Columns].All(x => string.IsNullOrEmpty(x.Value?.ToString()));
                            if (isEmptyRow)
                            {
                                continue;
                            }
                            var item = new ImportErrorInvestigationUpdatePivotModel();
                            item.Plant = sourceSheet.Cells[row, plantIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.WHLoc = sourceSheet.Cells[row, wHLocIndex].Value?.ToString()?.Trim() ?? string.Empty;
                            item.AccountQuantity = sourceSheet.Cells[row, accountQuantityIndex].Value?.ToString()?.Trim() ?? string.Empty;

                            items.Add(item);
                        }

                        //Validate STT
                        foreach (var item in items)
                        {
                            //validate:
                            var validateRequiredColumnUpdatePivotImport = ValidateRequiredColumnUpdatePivotImport(item);
                            var validateExistedPlantUpdatePivotImport = ValidateExistedPlantUpdatePivotImport(item, updatePivotByPlants);
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                return new ImportResponseWithDataModel<byte[], List<ImportErrorInvestigationUpdatePivotDto>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy dữ liệu phù hợp."
                };
            }
            byte[] fileByteErrors = null;
            //Ghi vào excel các bản ghi bị lỗi
            var invalidItems = items.Where(x => x.Error.IsValid == false).ToList();

            if (invalidItems.Any())
            {
                fileByteErrors = InvalidItemsUpdatePivotImportToExcel(invalidItems);

            }

            //Sau khi đã tổng hợp các bản ghi từ file excel, tiến hành thêm mới hoặc cập nhật vào GeneralSettings:

            var validItems = items.Where(x => x.Error.IsValid).GroupBy(x => x.Plant).Select(group => new ImportErrorInvestigationUpdatePivotModel
            {
                Plant = group.Key,
                AccountQuantity = group.Sum(item => Convert.ToDouble(item.AccountQuantity)).ToString()
            }).ToList();

            var generalSettings = await _inventoryContext.GeneralSettings.AsNoTracking()
                                        .Where(x => x.InventoryId == inventoryId && !x.IsDelete && x.Type == GeneralSettingType.InvestigationPercent).ToListAsync();
            var updateRangesGeneralSettings = new List<GeneralSetting>();
            var addRangesGeneralSettings = new List<GeneralSetting>();
            foreach (var item in validItems)
            {
                var matchingPivot = updatePivotByPlants.FirstOrDefault(p => p.Plant == item.Plant);

                if (matchingPivot != null)
                {
                    // Cập nhật TotalAccountQuantity từ AccountQuantity của item
                    matchingPivot.TotalAccountQuantity = Math.Round(Convert.ToDouble(item.AccountQuantity), 3);

                    // Cập nhật lại ErrorPercent
                    matchingPivot.ErrorPercent = Math.Round(
                        matchingPivot.TotalAccountQuantity == 0 ? 0 : (matchingPivot.TotalErrorMoney / matchingPivot.TotalAccountQuantity),
                        3
                    );
                }

            }
            foreach (var item in updatePivotByPlants)
            {
                var plantGeneralSetting = generalSettings.FirstOrDefault(x => x.Value1 == item.Plant);
                if (plantGeneralSetting != null)
                {
                    plantGeneralSetting.Value2 = item.ErrorPercent.ToString();
                    plantGeneralSetting.UpdatedAt = DateTime.Now;
                    plantGeneralSetting.UpdatedBy = userId;
                    updateRangesGeneralSettings.Add(plantGeneralSetting);
                }
                else
                {
                    GeneralSetting generalSetting = new GeneralSetting
                    {
                        Id = Guid.NewGuid(),
                        InventoryId = inventoryId,
                        Type = GeneralSettingType.InvestigationPercent,
                        Key1 = "Plant",
                        Value1 = item.Plant,
                        Key2 = "ErrorPercent",
                        Value2 = item.ErrorPercent.ToString(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };
                    addRangesGeneralSettings.Add(generalSetting);
                }
            }

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

                if (updateRangesGeneralSettings.Any())
                {
                    context.GeneralSettings.UpdateRange(updateRangesGeneralSettings);
                }

                if (addRangesGeneralSettings.Any())
                {
                    await context.GeneralSettings.AddRangeAsync(addRangesGeneralSettings);
                }
                await context.SaveChangesAsync();

            }


            var failCount = items.Where(x => x.Error.IsValid == false).Count();
            var successCount = items.Where(x => x.Error.IsValid == true).Count();

            var fileType = Constants.FileResponse.ExcelType;
            var fileName = string.Format("Dulieucapnhatpivot_Fileloi_{0}", DateTime.Now.ToString(Constants.DatetimeFormat));

            return new ImportResponseWithDataModel<byte[], List<ImportErrorInvestigationUpdatePivotDto>>
            {
                Bytes = fileByteErrors,
                Code = StatusCodes.Status200OK,
                FailCount = failCount,
                SuccessCount = successCount,
                FileType = fileType,
                FileName = fileName,
                Data = updatePivotByPlants
            };
        }

        public async Task<ResponseModel<IEnumerable<ErrorCategoryManagementDto>>> ErrorCategoryManagement()
        {
            var user = _httpContext.UserFromContext();
            var inventoryId = user.InventoryLoggedInfo?.InventoryModel?.InventoryId;
            var query = await _inventoryContext.GeneralSettings.AsNoTracking()
                            .Where(x => x.Type == GeneralSettingType.ErrorCategory && !x.IsDelete && x.InventoryId == inventoryId)
                            .Select(x => new ErrorCategoryManagementDto
                            {
                                Id = x.Id,
                                ErrorCategoryKey = x.Key1,
                                ErrorCategoryName = x.Value1
                            })
                            .OrderBy(x => x.ErrorCategoryName)
                            .ToListAsync();

            return new ResponseModel<IEnumerable<ErrorCategoryManagementDto>>
            {
                Code = StatusCodes.Status200OK,
                Message = "Lấy danh sách phân loại lỗi thành công.",
                Data = query
            };
        }

        public async Task<ResponseModel> AddNewErrorCategoryManagement(ErrorCategoryModel errorCategoryModel)
        {
            var user = _httpContext.UserFromContext();
            var inventoryId = user.InventoryLoggedInfo?.InventoryModel?.InventoryId;
            //check existed ErrorCategoryName:
            var existedErrorCategoryName = await _inventoryContext.GeneralSettings.AsNoTracking()
                                                .AnyAsync(x => x.Type == GeneralSettingType.ErrorCategory && !x.IsDelete && x.InventoryId == inventoryId && x.Value1 == errorCategoryModel.Name);
            if (existedErrorCategoryName)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ExistedErrorCategoryName,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ExistedErrorCategoryName)
                };
            }
            var userId = user.UserId;

            var key1List = await _inventoryContext.GeneralSettings.AsNoTracking()
                                .Where(gs => gs.Type == GeneralSettingType.ErrorCategory && !gs.IsDelete && gs.InventoryId == inventoryId)
                                .Select(gs => gs.Key1)
                                .ToListAsync();

            // Lọc và chuyển đổi sang số
            int maxKey1 = key1List.Where(k => int.TryParse(k, out _)) // Lọc chỉ những giá trị có thể chuyển thành số
                                .Select(int.Parse) // Chuyển đổi sang số
                                .DefaultIfEmpty(-1) // Nếu không có bản ghi nào thì mặc định là -1
                                .Max(); // Lấy giá trị lớn nhất

            // Xác định giá trị Key1 mới, nếu chưa có dữ liệu thì bắt đầu từ 0
            int newKey1 = (maxKey1 == -1) ? 0 : maxKey1 + 1;
            GeneralSetting generalSetting = new GeneralSetting
            {
                Id = Guid.NewGuid(),
                Type = GeneralSettingType.ErrorCategory,
                Key1 = newKey1.ToString(),
                Value1 = errorCategoryModel.Name,
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                InventoryId = user?.InventoryLoggedInfo?.InventoryModel?.InventoryId
            };

            await _inventoryContext.GeneralSettings.AddAsync(generalSetting);
            await _inventoryContext.SaveChangesAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Thêm mới phân loại lỗi thành công.",
            };
        }

        public async Task<ResponseModel> UpdateErrorCategoryManagement(Guid errorCategoryId, ErrorCategoryModel errorCategoryModel)
        {
            var user = _httpContext.UserFromContext();
            var inventoryId = user.InventoryLoggedInfo?.InventoryModel?.InventoryId;
            //Get ErrorCategory By Id:
            var errorCategoryById = await _inventoryContext.GeneralSettings.AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.Id == errorCategoryId && !x.IsDelete && x.InventoryId == inventoryId);
            if (errorCategoryById == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tim thấy phân loại lỗi"
                };
            }

            //check existed ErrorCategoryName:
            var existedErrorCategoryName = await _inventoryContext.GeneralSettings.AsNoTracking()
                                                .AnyAsync(x => x.Type == GeneralSettingType.ErrorCategory && !x.IsDelete && x.InventoryId == inventoryId && x.Value1 == errorCategoryModel.Name);
            if (existedErrorCategoryName)
            {
                return new ResponseModel
                {
                    Code = (int)HttpStatusCodes.ExistedErrorCategoryName,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.ExistedErrorCategoryName)
                };
            }

            var userId = user.UserId;

            errorCategoryById.Value1 = errorCategoryModel.Name;
            errorCategoryById.UpdatedAt = DateTime.Now;
            errorCategoryById.UpdatedBy = userId;


            _inventoryContext.GeneralSettings.Update(errorCategoryById);
            await _inventoryContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Cập nhật phân loại lỗi thành công.",
            };
        }

        public async Task<ResponseModel> RemoveErrorCategoryManagement(Guid errorCategoryId)
        {
            //Get ErrorCategory By Id:
            var errorCategoryById = await _inventoryContext.GeneralSettings.AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.Id == errorCategoryId);
            if (errorCategoryById == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tim thấy phân loại lỗi"
                };
            }
            errorCategoryById.IsDelete = true;
            errorCategoryById.DeletedAt = DateTime.Now;
            errorCategoryById.DeletedBy = _httpContext?.UserFromContext()?.UserId;
            _inventoryContext.GeneralSettings.Update(errorCategoryById);
            await _inventoryContext.SaveChangesAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Xóa phân loại lỗi thành công.",
            };
        }

        public async Task<ResponseModel> ErrorCategoryManagementById(Guid errorCategoryId)
        {
            //Get ErrorCategory By Id:
            var errorCategoryById = await _inventoryContext.GeneralSettings.AsNoTracking().Select(x => new ErrorCategoryManagementDto
            {
                Id = x.Id,
                ErrorCategoryKey = x.Key1,
                ErrorCategoryName = x.Value1
            }).FirstOrDefaultAsync(x => x.Id == errorCategoryId);
            if (errorCategoryById == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tim thấy phân loại lỗi"
                };
            }
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Lấy phân loại lỗi theo Id thành công.",
                Data = errorCategoryById
            };
        }
    }
}
