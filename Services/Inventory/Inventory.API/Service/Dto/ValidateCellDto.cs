namespace Inventory.API.Service.Dto
{
    public class ValidateCellDto
    {
        public int FailCount { get; set; }
        public int SuccessCount { get; set; }
        public bool IsValid { get; set; }
        public string SpecificTitle { get; set; }
        public string SpecificMessage { get; set; }

        public ValidateCellDto()
        {
            FailCount = 0;
            SuccessCount = 0;
            IsValid = true;
        }
    }
}
