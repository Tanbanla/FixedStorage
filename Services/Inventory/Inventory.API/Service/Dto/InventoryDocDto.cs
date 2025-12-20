namespace Inventory.API.Service.Dto
{
    public class InventoryDocDto
    {
        public string InventoryName { get; set; }
        public string DocCode { get; set; }
        public string PlantLocationComponentStorageBin { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string SaleOrderNo { get; set; }
        public string Plant { get; set; }
        public string Description { get; set; }
        public string PhysInv { get; set; }
        public string PositionCode { get; set; }
        public InventoryDocType DocType { get; set; }
        public string AssemblyLocation { get; set; }
        public string StorageBin { get; set; }
    }

    public class DocAComponentNameDto
    {
        public string Plant { get; set; }
        public string WHLoc { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }

    }
}
