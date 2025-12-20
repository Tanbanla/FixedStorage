namespace Inventory.API.Service.Dto
{
    public class InventoryDocCellDto
    {
        public string Plant { get; set; }
        public string WarehouseLocation { get; set; }
        public string CCol { get; set; }
        public string SpecialStock { get; set; }
        public string StockType { get; set; }
        public string SONo { get; set; }
        public string SOList { get; set; }
        public string PhysInv { get; set; }
        public int? FiscalYear { get; set; }
        public string Item { get; set; }
        public string PlannedCountDate { get; set; }
        public string ComponentCode { get; set; }
        public string ComponentName { get; set; }
        public string NCol { get; set; }
        public string OCol { get; set; }
        public string PCol { get; set; }
        public string QCol { get; set; }
        public string RCol { get; set; }
        public string SCol { get; set; }
        public string PositionCode { get; set; }
        public string Note { get; set; }
        public string Assignee { get; set; }
        public double Quantity { get; set; }

        public string ModelCode { get; set; }
        public string StorageBin { get; set; }
        public string AssemblyLoc { get; set; }
        public string ProOrderNo { get; set; }
        public string VendorCode { get; set; }
        /// <summary>
        /// For AuditTarget only
        /// </summary>
        public string LocationName { get; set; }
        /// <summary>
        /// For Type A,B,E
        /// </summary>
        public string RowNumber { get; set; }


        public string MachineModel { get; set; }
        public string MachineType { get; set; }
        public string LineName { get; set; }


    }
}
