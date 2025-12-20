using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;

namespace Identity.API.Service
{
    public class InternalUserService : IInternalService
    {
        private readonly ILogger<InternalUserService> _logger;
        private readonly IdentityContext _identityContext;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;

        public InternalUserService(ILogger<InternalUserService> logger,
                                   IdentityContext identityContext,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration
                                    )
        {
            _logger = logger;
            _identityContext = identityContext;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
        }

        public ResponseModel<IEnumerable<InternalUserDto>> GetUsers()
        {
            var users = from u in _identityContext.AppUsers
                        join d in _identityContext.Departments on u.DepartmentId!.ToLower() equals d.Id.ToString().ToLower() into t1Group
                        from t1 in t1Group.DefaultIfEmpty()
                        select new InternalUserDto
                        {
                            Id = u.Id.ToString(),
                            Code = u.Code,
                            Name = u.FullName,
                            DepartmentId = t1 != null ? t1.Id.ToString() : string.Empty,
                            DepartmentName = t1 != null ? t1.Name : string.Empty,
                            IsActive = u.Status == UserStatus.Active,
                            AccountType = u.AccountType
                        };

            if(users?.Any() == false)
            {
                return new ResponseModel<IEnumerable<InternalUserDto>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Không có dữ liệu người dùng"
                };
            }

            return new ResponseModel<IEnumerable<InternalUserDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = users
            };
        }

        public ResponseModel<IEnumerable<RoleClaimDto>> GetUserRoles(string userId)
        {
            var userRoles = from au in _identityContext.AppUsers
                            join auRole in _identityContext.UserRoles on au.Id equals auRole.UserId
                            join rClaims in _identityContext.RoleClaims on auRole.RoleId equals rClaims.RoleId
                            join roles in _identityContext.AppRoles on auRole.RoleId equals roles.Id
                            where au.Id == Guid.Parse(userId)
                            select new RoleClaimDto
                            {
                                RoleId = auRole.RoleId.ToString(),
                                RoleName = roles.Name,
                                ClaimType = rClaims.ClaimType,
                                ClaimValue = rClaims.ClaimValue
                            };

            if (userRoles?.Any() == false)
            {
                return new ResponseModel<IEnumerable<RoleClaimDto>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = userRoles,
                    Message = "Không có dữ liệu quyền người dùng"
                };
            }

            return new ResponseModel<IEnumerable<RoleClaimDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = userRoles,
                Message = "Dữ liệu quyền người dùng"
            };
        }

        public ResponseModel<IEnumerable<InternalDepartmentDto>> GetDepartments()
        {
            //Mặc định filter phòng ban sẽ lọc các phòng ban bị xóa mềm
            //Nhưng phần danh sách lịch sử cần xem full các phòng ban trong lịch sử nên cần ignore filter này đi
            var departments = _identityContext.Departments.IgnoreQueryFilters().Select(x => new InternalDepartmentDto
            {
                Id = x.Id.ToString(),
                CreateDate = x.CreatedAt,
                Name = x.Name,
                ManagerId = x.ManagerId,
                IsDeleted = x.IsDeleted.HasValue ? x.IsDeleted.Value : null
            });

            if(departments?.Any() == false)
            {
                return new ResponseModel<IEnumerable<InternalDepartmentDto>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Data = departments,
                    Message = "Không tìm thấy phòng ban nào"
                };
            }

            return new ResponseModel<IEnumerable<InternalDepartmentDto>>
            {
                Code = StatusCodes.Status200OK,
                Data = departments,
                Message = "Danh sách phòng ban"
            };
        }
        public ResponseModel<IEnumerable<ListUserModel>> ListUser()
        {
            var users = (from u in _identityContext.AppUsers.AsNoTracking()
                         join d in _identityContext.Departments.AsNoTracking() on u.DepartmentId equals d.Id.ToString() into userDept
                         from dept in userDept.DefaultIfEmpty()
                         where u.Status == UserStatus.Active
                         select new ListUserModel
                         {
                             Id = u.Id,
                             FullName = u.FullName,
                             UserName = u.UserName,
                             AccountType = u.AccountType.Value,
                             Code = u.Code,
                             Status = (int)u.Status,
                             DepartmentName = dept != null ? dept.Name : string.Empty
                         }).Distinct().ToList();


            if (users.Count() == 0)
            {
                return new ResponseModel<IEnumerable<ListUserModel>>
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy danh sách người dùng.",
                };
            }

            return new ResponseModel<IEnumerable<ListUserModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = users
            };
        }

        public ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>> GetAllRoleWithUserNames()
        {
            var result = from u in _identityContext.AppUsers
                         join ur in _identityContext.UserRoles on u.Id equals ur.UserId into userRoles
                         from ur in userRoles.DefaultIfEmpty()
                         join r in _identityContext.AppRoles on ur.RoleId equals r.Id into roles
                         from r in roles.DefaultIfEmpty()
                         join rc in _identityContext.RoleClaims on r.Id equals rc.RoleId into roleClaims
                         from rc in roleClaims.DefaultIfEmpty()
                         join d in _identityContext.Departments on rc.ClaimValue equals d.Id.ToString() into departments
                         from d in departments.DefaultIfEmpty()
                         select new GetAllRoleWithUserNameModel
                         {
                             UserId = u.Id,
                             UserName = u.UserName,
                             ClaimType = rc.ClaimType,
                             ClaimValue = rc.ClaimValue,
                             Department = d.Name
                         };

            if (result?.Any() == false)
            {
                return new ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Không tìm thấy quyền nào"
                };
            }

            return new ResponseModel<IEnumerable<GetAllRoleWithUserNameModel>>
            {
                Code = StatusCodes.Status200OK,
                Data = result,
                Message = "Danh sách các quyền người dùng."
            };

        }


    }
}
