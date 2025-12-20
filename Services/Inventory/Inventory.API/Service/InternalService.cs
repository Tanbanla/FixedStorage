using BIVN.FixedStorage.Services.Common.API.Helpers;

namespace Inventory.API.Service
{
    public class InternalService : IInternalService
    {
        private readonly ILogger<InternalService> _logger;
        private readonly HttpContext _httpContext;
        private readonly InventoryContext _inventoryContext;

        public InternalService(ILogger<InternalService> logger,
                               IHttpContextAccessor httpContextAccessor,
                               InventoryContext inventoryContext
                            )
        {
            _logger = logger;
            _httpContext = httpContextAccessor.HttpContext;
            _inventoryContext = inventoryContext;
        }

        public async Task<ResponseModel<InventoryLoggedInfo>> GetInventoryLoggedInfo(Guid userId)
        {
            var account = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserId == userId);

            var currentDate = DateTime.Now.Date;
            var inventoryQueryNotPromotion = await _inventoryContext.Inventories.AsNoTracking()
                                                               .OrderBy(x => x.InventoryStatus)
                                                               .FirstOrDefaultAsync(x => x.InventoryStatus != Infrastructure.Entity.Enums.InventoryStatus.Finish);

            var inventoryQueryPromotion = await _inventoryContext.Inventories.AsNoTracking()
                                                               .OrderBy(x => x.InventoryStatus).ThenByDescending(x => x.CreatedAt)
                                                               .FirstOrDefaultAsync();

            var inventory = (account != null && account.RoleType == InventoryAccountRoleType.Promotion) ? inventoryQueryPromotion : inventoryQueryNotPromotion;
            if (inventory == null)
            {

                return new ResponseModel<InventoryLoggedInfo>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = Constants.ResponseMessages.Inventory.NotFound
                };
            }

            var inventoryLoggedInfo = new InventoryLoggedInfo
            {
                InventoryModel = new InventoryModel
                {
                    InventoryId = inventory.Id,
                    InventoryDate = inventory.InventoryDate,
                    Name = inventory.Name,
                    Status = (int)inventory.InventoryStatus
                }
            };

           
            if(account != null)
            {
                inventoryLoggedInfo.AccountId = account.UserId;
                inventoryLoggedInfo.InventoryRoleType = (int)account.RoleType;
                inventoryLoggedInfo.HasRoleType = account.RoleType.HasValue;
                inventoryLoggedInfo.UserName = account.UserName;
                inventoryLoggedInfo.UserId = userId;
            }


            return new ResponseModel<InventoryLoggedInfo>
            {
                Code = StatusCodes.Status200OK,
                Data = inventoryLoggedInfo,
            };
        }

        /// <summary>
        /// Khi người dùng đổi từ tài khoản chung sang riêng hoặc đổi trạng thái không hoạt động
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel> DeleteInventoryAccount(Guid userId)
        {
            var inventoryAccount = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserId == userId);
            if(inventoryAccount != null)
            {
                var accountLocations = await _inventoryContext.AccountLocations.Where(x => x.Id == inventoryAccount.Id).ToListAsync();

                _inventoryContext.AccountLocations.RemoveRange(accountLocations);
                _inventoryContext.InventoryAccounts.Remove(inventoryAccount);

                await _inventoryContext.SaveChangesAsync();
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = Constants.ResponseMessages.Inventory.DeleteSuccess
            };
        }

        public async Task<ResponseModel> UpdateInventoryAccount(Guid userId, string newUserName)
        {
            var inventoryAccount = await _inventoryContext.InventoryAccounts.FirstOrDefaultAsync(x => x.UserId == userId);
            if (inventoryAccount != null)
            {
                inventoryAccount.UserName = newUserName;
                await _inventoryContext.SaveChangesAsync();
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = Constants.ResponseMessages.Inventory.UpdateInventoryAccountSuccess
            };
        }

        public async Task<ResponseModel<bool>> CheckAuditAccountAssignLocation(Guid userId)
        {
            var anyLocation = await _inventoryContext.InventoryAccounts.Include(x => x.AccountLocations)
                                                                                .ThenInclude(x => x.InventoryLocation)
                                                                                .AsNoTracking()
                                                                                .AnyAsync(x => x.UserId == userId && x.AccountLocations.Any());
            if (!anyLocation)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = anyLocation,
                    Message = EnumHelper<HttpStatusCodes>.GetDisplayValue(HttpStatusCodes.AuditAccountNotAssignLocation)
                };
            }

            return new ResponseModel<bool>
            {
                Code = StatusCodes.Status200OK,
                Data = anyLocation,
            };
        }
    }
}
