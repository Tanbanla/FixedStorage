namespace Inventory.API.Service.Dto
{
    public class CheckInventoryDocumentDto
    {
        public HashSet<string> PhysInvDoc { get; set; }
        public HashSet<string> PositionPhysInvSONo { get; set; }
        public List<(string, string)> ComponentUniqueName { get; set; }
        public HashSet<string> PlanLocationForBE { get; set; }

        public CheckInventoryDocumentDto()
        {
            PhysInvDoc = new HashSet<string>();
            PositionPhysInvSONo = new HashSet<string>();
            ComponentUniqueName = new List<(string, string)>();
            PlanLocationForBE = new HashSet<string>();
        }
    }
}
