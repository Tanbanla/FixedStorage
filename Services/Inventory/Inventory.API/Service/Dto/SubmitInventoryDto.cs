namespace Inventory.API.Service.Dto
{
    public class SubmitInventoryDto
    {
        [RegularExpression(@"^[FMT][0-9]{7}$", ErrorMessage = "Mã nhân viên không đúng định dạng. Vui lòng thử lại.")]
        public string UserCode { get; set; }
        public string Comment { get; set; }
        public IFormFile Image { get; set; }
        public List<DocOutputDto> DocOutputs { get; set; } = new();
        public List<DocTypeCDetailDto> DocTypeCDetails { get; set; } = new();
        public List<Guid> IdsDeleteDocOutPut { get; set; } = new();
        public bool IsAuditWebsite { get; set; } = false;
    }

    public class DocOutputDto
    {
        public Guid? Id { get; set; } = null;
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
    }

    public class DocTypeCDetailDto
    {
        public Guid? Id { get; set; } = null;
        public string ComponentCode { get; set; }
        public double QuantityOfBom { get; set; }
        public double QuantityPerBom { get; set; }
    }

}
