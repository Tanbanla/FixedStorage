namespace Inventory.API.Service.Dto
{
    public class TreeGroupDto
    {
        public string MachineModel { get; set; }
        public string Line { get; set; }
        public string ModelCode { get; set; }
        public string UnitModelCode { get; set; }
        /// <summary>
        /// Stage name in line
        /// </summary>
        public string StageName { get; set; }
        /// <summary>
        /// Stage name in model code
        /// </summary>
        public string ModelStage { get; set; }
        public string ModelStageNumber { get; set; }
        public List<TreeGroupDto> Attachments { get; set; } = new List<TreeGroupDto>();
        public IEnumerable<string> AttachmentModelCodes { get; set; }
    }
}
