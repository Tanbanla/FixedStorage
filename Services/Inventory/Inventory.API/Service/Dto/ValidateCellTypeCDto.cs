namespace Inventory.API.Service.Dto
{
    public class ValidateCellTypeCDto
    {
        public bool IsValid { get; set; }
        public bool IsWarning { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public byte[] Data { get; set; }
        public bool HasSpecificMessage { get; set; }
        public bool IsEditDocTypeCRole { get; set; } = false;

        public ValidateCellTypeCDto()
        {
            IsValid = true;
            IsWarning = false;
            Title = string.Empty;
            Content = string.Empty;
        }

    }
}
