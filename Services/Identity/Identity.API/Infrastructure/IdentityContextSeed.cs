using BIVN.FixedStorage.Services.Common.API.Dto.Factory;

namespace BIVN.FixedStorage.Identity.API.Infrastructure
{
    public class IdentityContextSeed
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IdentityContext _context;
        private readonly IConfiguration _configuration;
        private readonly RestClientFactory _restClientFactory;

        public IdentityContextSeed(RoleManager<AppRole> roleManager,
                                    UserManager<AppUser> userManager,
                                    IdentityContext context,
                                    IConfiguration configuration,
                                    RestClientFactory restClientFactory
                                )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _restClientFactory = restClientFactory;
        }

        public async Task SeedAsync(IdentityContext context, IWebHostEnvironment env, ILogger<IdentityContextSeed> logger)
        //public async Task SeedAsync(ILogger<IdentityContextSeed> logger)
        {
            var policy = CreatePolicy(logger, nameof(IdentityContextSeed));
            //var userManager = (UserManager<IdentityUser>) context.GetService<typeof(UserManager<IdentityUser>>();
            //var roleManager = (RoleManager<IdentityRole>)_serviceProvider.GetService(typeof(RoleManager<IdentityRole>));
            await policy.ExecuteAsync(async () =>
            {
                var phongItId = Guid.NewGuid();
                var mcDepartmentId = Guid.NewGuid();
                var pcbDepartmentId = Guid.NewGuid();
                var adminRoleId = Guid.NewGuid();
                var tpitRoleId = Guid.NewGuid();
                var mcRoleId = Guid.NewGuid();
                var pcbRoleId = Guid.NewGuid();
                if (!_context.Departments.Any())
                {
                    //await _context.Departments.AddAsync(new Department() { Id = phongItId, Name = "Phòng IT", CreatedAt = DateTime.Now });
                    //await _context.Departments.AddAsync(new Department() { Id = mcDepartmentId, Name = "Phòng MC", CreatedAt = DateTime.Now });
                    //await _context.Departments.AddAsync(new Department() { Id = pcbDepartmentId, Name = "Phòng PCB", CreatedAt = DateTime.Now });
                }

                if (!_roleManager.Roles.Any())
                {
                    var adminRole = new AppRole() { Id = adminRoleId, Name = "Administrator", Description = "Admin", CreatedAt = DateTime.Now };
                    var tpitRole = new AppRole() { Id = tpitRoleId, Name = "Trưởng phòng IT", Description = "TruongPhongIT", CreatedAt = DateTime.Now };
                    var nvMcRole = new AppRole() { Id = mcRoleId, Name = "Nhân viên MC", Description = "NhanVienMC", CreatedAt = DateTime.Now };
                    var nvPcbRole = new AppRole() { Id = pcbRoleId, Name = "Nhân viên PCB", Description = "NhanVienPCB", CreatedAt = DateTime.Now };
                    await _roleManager.CreateAsync(adminRole);
                    await _roleManager.CreateAsync(tpitRole);
                    await _roleManager.CreateAsync(nvMcRole);
                    await _roleManager.CreateAsync(nvPcbRole);
                    await _context.SaveChangesAsync();

                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.WEBSITE_ACCESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.MOBILE_ACCESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.DEPARTMENT_MANAGEMENT, ClaimValue = phongItId.ToString() });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.USER_MANAGEMENT, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.ROLE_MANAGEMENT, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.MC_BUSINESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.PCB_BUSINESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.DEPARTMENT_DATA_INQUIRY, ClaimValue = phongItId.ToString() });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.FACTORY_DATA_INQUIRY, ClaimValue = phongItId.ToString() });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = adminRoleId, ClaimType = Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT, ClaimValue = phongItId.ToString() });

                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.WEBSITE_ACCESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.MOBILE_ACCESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.DEPARTMENT_MANAGEMENT, ClaimValue = phongItId.ToString() });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.USER_MANAGEMENT, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.ROLE_MANAGEMENT, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.MC_BUSINESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.PCB_BUSINESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.DEPARTMENT_DATA_INQUIRY, ClaimValue = phongItId.ToString() });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.FACTORY_DATA_INQUIRY, ClaimValue = phongItId.ToString() });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = tpitRoleId, ClaimType = Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT, ClaimValue = phongItId.ToString() });

                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = mcRoleId, ClaimType = Constants.Permissions.MOBILE_ACCESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = mcRoleId, ClaimType = Constants.Permissions.MC_BUSINESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = mcRoleId, ClaimType = Constants.Permissions.DEPARTMENT_DATA_INQUIRY, ClaimValue = mcDepartmentId.ToString() });

                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = pcbRoleId, ClaimType = Constants.Permissions.MOBILE_ACCESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = pcbRoleId, ClaimType = Constants.Permissions.PCB_BUSINESS, ClaimValue = "" });
                    await _context.RoleClaims.AddAsync(new IdentityRoleClaim<Guid>() { RoleId = pcbRoleId, ClaimType = Constants.Permissions.DEPARTMENT_DATA_INQUIRY, ClaimValue = pcbDepartmentId.ToString() });

                    await _context.SaveChangesAsync();
                }

                if (!_userManager.Users.Any())
                {
                    await _userManager.CreateAsync(new AppUser()
                    {
                        UserName = "tinhvan",
                        Email = "tso@tinhvan.vn",
                        FullName = "Tinhvan Software",
                        PhoneNumber = "02462852502",
                        CreatedAt = DateTime.Now,
                        Gender = Gender.Male,
                        Status = UserStatus.Active,
                        AccountType = AccountType.TaiKhoanRieng,
                        DepartmentId = phongItId.ToString()
                    }, Constants.Password.TinhVan_Account);
                    var admin = await _userManager.FindByUserNameAsync("tinhvan");
                    var adminRole = await _roleManager.FindByNameAsync("Administrator");
                    if (admin != null && adminRole != null)
                    {
                        await _userManager.AddToRoleAsync(admin, adminRole.Name);
                    }

                    await _userManager.CreateAsync(new AppUser()
                    {
                        UserName = "truongphongit",
                        Email = "truongphongit@bivn.com",
                        FullName = "Trưởng Phòng IT BIVN",
                        PhoneNumber = "0900000000",
                        CreatedAt = DateTime.Now,
                        Gender = Gender.Male,
                        Status = UserStatus.Active,
                        AccountType = AccountType.TaiKhoanRieng,
                        DepartmentId = phongItId.ToString()
                    }, Constants.Password.TruongPhongIT_Account);
                    var tpit = await _userManager.FindByUserNameAsync("truongphongit");
                    var tpit_role = await _roleManager.FindByNameAsync("Trưởng phòng IT");
                    if (tpit != null && tpit_role != null)
                    {
                        await _userManager.AddToRoleAsync(tpit, tpit_role.Name);
                    }

                    await _userManager.CreateAsync(new AppUser()
                    {
                        UserName = "nhanvienmc1",
                        Email = "nhanvienmc1@bivn.com",
                        FullName = "nhanvienmc1",
                        PhoneNumber = "02462852502",
                        CreatedAt = DateTime.Now,
                        Gender = Gender.Male,
                        Status = UserStatus.Active,
                        AccountType = AccountType.TaiKhoanRieng,
                        DepartmentId = mcDepartmentId.ToString()
                    }, Constants.Password.NhanVienMC_Account);
                    var mc1 = await _userManager.FindByUserNameAsync("nhanvienmc1");
                    var mcRole = await _roleManager.FindByNameAsync("Nhân viên MC");
                    if (mc1 != null && mcRole != null)
                    {
                        await _userManager.AddToRoleAsync(mc1, mcRole.Name);
                    }

                    await _userManager.CreateAsync(new AppUser()
                    {
                        UserName = "nhanvienpcb1",
                        Email = "nhanvienpcb1@bivn.com",
                        FullName = "nhanvienpcb1",
                        PhoneNumber = "0900000000",
                        CreatedAt = DateTime.Now,
                        Gender = Gender.Male,
                        Status = UserStatus.Active,
                        AccountType = AccountType.TaiKhoanRieng,
                        DepartmentId = pcbDepartmentId.ToString()
                    }, Constants.Password.NhanVienPCB_Account);
                    var pcb1 = await _userManager.FindByUserNameAsync("nhanvienpcb1");
                    var pcbRole = await _roleManager.FindByNameAsync("Nhân viên PCB");
                    if (pcb1 != null && pcbRole != null)
                    {
                        await _userManager.AddToRoleAsync(pcb1, pcbRole.Name);
                    }
                }

                try
                {
                    await SeedingDefaultDepartment(logger);
                }
                catch (Exception ex)
                {
                    logger.LogError("Có lỗi khi khởi tạo phòng ban admin mặc định, quyền mặc định, người dùng mặc định");
                    logger.LogError(ex.Message);
                }
            });
        }

        private IEnumerable<AppRole> GetPreconfiguredAppRoles()
        {
            return new List<AppRole>() { new() { Id = Guid.NewGuid(), Name = "Administrator" } };
        }

        private AsyncRetryPolicy CreatePolicy(ILogger<IdentityContextSeed> logger, string prefix, int retries = 3)
        {
            return Policy.Handle<SqlException>().
                WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(exception, "[{prefix}] Error seeding database (attempt {retry} of {retries})", prefix, retry, retries);
                    }
                );
        }

        /// <summary>
        /// Seeding phòng ban mặc định Admin, Người dùng mặc định Admin và quyền Admin
        /// </summary>
        /// <returns></returns>
        private async Task SeedingDefaultDepartment(ILogger<IdentityContextSeed> logger)
        {
            //Khởi tạo phòng ban mặc định Phòng Admin 
            var existAdminDepartment = _context.Departments.Any(x => x.Name.Trim() == Constants.DefaultAccount.DepartmentName);
            if (!existAdminDepartment)
            {
                var newDepartment = new Department
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now.AddYears(-1),
                    CreatedBy = Guid.NewGuid().ToString(),
                    Name = Constants.DefaultAccount.DepartmentName,
                };

                await _context.AddAsync(newDepartment);
                await _context.SaveChangesAsync();
            }

            //Khởi tạo tài khoản mặc định Administrator 
            var administrator = _context.AppUsers.Any(x => x.FullName.Trim() == Constants.DefaultAccount.UserName);
            if (!administrator)
            {
                var adminDepartmentEntity = await _context.Departments.FirstOrDefaultAsync(x => x.Name == Constants.DefaultAccount.DepartmentName);
                var newUser = new AppUser
                {
                    Id = Guid.NewGuid(),
                    AccountType = AccountType.TaiKhoanRieng,
                    CreatedAt = DateTime.Now.AddYears(-1),
                    FullName = Constants.DefaultAccount.FullName,
                    UserName = Constants.DefaultAccount.UserName,
                    Code = Constants.DefaultAccount.UserCode,
                    Status = UserStatus.Active,
                    Email = "bivn@gmail.com",
                    DepartmentId = adminDepartmentEntity.Id.ToString()
                };

                await _userManager.CreateAsync(newUser, Constants.DefaultAccount.Password);
            }
            //Khởi tạo quyền admin mặc định
            var anyAdminRole = _context.AppRoles.Any(x => x.Name == Constants.DefaultAccount.RoleName);
            if (!anyAdminRole)
            {
                var defaultAdminRole = new AppRole
                {
                    Id = Guid.NewGuid(),
                    Name = Constants.DefaultAccount.RoleName,
                    CreatedAt = DateTime.Now.AddYears(-1),
                    Description = Constants.DefaultAccount.RoleName,
                };
                await _roleManager.CreateAsync(defaultAdminRole);
            }

            var adminDepartment = await _context.Departments.FirstOrDefaultAsync(x => x.Name == Constants.DefaultAccount.DepartmentName);
            var adminUser = await _context.AppUsers.FirstOrDefaultAsync(x => x.FullName == Constants.DefaultAccount.FullName);
            //Cập nhật trưởng phòng cho phòng ban
            adminDepartment.ManagerId = adminUser.Id.ToString();

            //Set toàn bộ quyền cho quyền admin
            var getAdminUser = await _userManager.FindByNameAsync(Constants.DefaultAccount.FullName);
            var adminRole = await _roleManager.FindByNameAsync(Constants.DefaultAccount.RoleName);

            //Xóa tất cả quyền cũ
            var roleClaims = await _context.RoleClaims.Where(x => x.RoleId.ToString().ToLower() == adminRole.Id.ToString().ToLower()).ToListAsync();
            _context.RemoveRange(roleClaims);

            //Thêm các quyền mới
            List<IdentityRoleClaim<Guid>> claims = new();
            //Thêm quyền website và mobile
            foreach (var permission in Constants.Permissions.PermissionsList)
            {
                claims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>
                {
                    RoleId = adminRole.Id,
                    ClaimType = permission.Item1,
                    ClaimValue = ""
                });
            }
            //Thêm quyền xem các phòng ban mặc định
            var departments = await _context.Departments.ToListAsync();
            foreach (var department in departments)
            {
                claims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>
                {
                    RoleId = adminRole.Id,
                    ClaimType = Constants.Permissions.DEPARTMENT_DATA_INQUIRY,
                    ClaimValue = department.Id.ToString().ToUpper()
                });
            }

            //Thêm quyền tạo phiếu theo phòng ban mặc định
            foreach (var department in departments)
            {
                claims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>
                {
                    RoleId = adminRole.Id,
                    ClaimType = Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT,
                    ClaimValue = department.Id.ToString().ToUpper()
                });
            }


            //Thêm quyền xem các nhà máy mặc định
            var factories = await GetInternalFactories();
            if (factories != null)
            {
                foreach (var factory in factories)
                {
                    claims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>
                    {
                        RoleId = adminRole.Id,
                        ClaimType = Constants.Permissions.FACTORY_DATA_INQUIRY,
                        ClaimValue = factory.Id.ToString()
                    });
                }
            }

            

            var userRoles = _context.UserRoles.Where(x => x.UserId == adminUser.Id);
            _context.UserRoles.RemoveRange(userRoles);
            _context.UserRoles.Add(new IdentityUserRole<Guid>
            {
                RoleId = adminRole.Id,
                UserId = getAdminUser.Id
            });

            claims.RemoveAll(x => x.ClaimType == Constants.Permissions.DEPARTMENT_DATA_INQUIRY && x.ClaimValue == ""
                                || x.ClaimType == Constants.Permissions.FACTORY_DATA_INQUIRY && x.ClaimValue == ""
                                || x.ClaimType == Constants.Permissions.CREATE_DOCUMENT_BY_DEPARTMENT && x.ClaimValue == "");

            _context.RoleClaims.AddRange(claims);
            await _context.SaveChangesAsync();
        }

        private async Task<IEnumerable<FactoryInfoModel>> GetInternalFactories()
        {
            var req = new RestRequest(Constants.Endpoint.api_Internal_Factories);
            req.AddHeader(Constants.HttpContextModel.ClientIdKey, _configuration.GetSection(Constants.AppSettings.ClientId).Value);
            req.AddHeader(Constants.HttpContextModel.ClientSecretKey, _configuration.GetSection(Constants.AppSettings.ClientSecret).Value);
            var result = await _restClientFactory.StorageClient().ExecuteGetAsync(req);
            var convertedResult = System.Text.Json.JsonSerializer.Deserialize<ResponseModel<IEnumerable<FactoryInfoModel>>>(result.Content, JsonDefaults.CamelCasing);
            return convertedResult?.Data;
        }
    }
}
