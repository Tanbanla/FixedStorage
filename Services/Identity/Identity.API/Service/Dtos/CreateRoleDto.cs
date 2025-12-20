namespace BIVN.FixedStorage.Identity.API.Service.Dtos
{
    public class CreateRoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ClaimDto> Claims { get; set; }
    }
}
