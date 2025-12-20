namespace Inventory.API.Service.Validate
{
    public class AfterInventoryDaysCheck : IApplicationValidation
    {
        private readonly int _day;
        public AfterInventoryDaysCheck(int day)
        {
            _day = day;
        }

        public async Task<ResponseModel<bool>> Validate(HttpContext httpContext)
        {
            if (_day == 0)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status200OK,
                    Data = true
                };
            }

            var currInventory = httpContext?.UserFromContext()?.InventoryLoggedInfo?.InventoryModel;
            if (currInventory == null)
            {
                return new ResponseModel<bool>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Data = false,
                    Message = commonAPIConstant.ResponseMessages.Inventory.NotFound
                };
            }

            var canEdit = DateTime.Now.Date > currInventory.InventoryDate.AddDays(_day).Date;
            if (canEdit)
                return new ResponseModel<bool> { Code = StatusCodes.Status200OK, Data = true };

            return new ResponseModel<bool> { Code = StatusCodes.Status400BadRequest, Data = false, Message = commonAPIConstant.ResponseMessages.Inventory.ShcheduleDaysMessage(_day) };
        }
    }
}
