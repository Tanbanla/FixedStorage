using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Storage.API.Service;
using BIVN.FixedStorage.Services.Common.API;
using Microsoft.VSDiagnostics;

namespace Storage.API.Benchmarks
{
    [CPUUsageDiagnoser]
    // Simple benchmark harness for HistoryService.GetHistories to establish baseline
    public class HistoryService_GetHistories_Benchmark
    {
        private HistoryService _service;
        private StorageContext _db;
        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var configDict = new Dictionary<string, string>
            {
                {
                    "AppSettings:ClientId",
                    "bench-client"
                },
                {
                    "AppSettings:ClientSecret",
                    "bench-secret"
                },
                {
                    "ConnectionStrings:DefaultConnection",
                    "DataSource=:memory:"
                }
            };
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();
            services.AddSingleton(configuration);
            var options = new DbContextOptionsBuilder<StorageContext>().UseInMemoryDatabase("HistoryServiceBenchDb").Options;
            _db = new StorageContext(options);
            SeedData(_db);
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<HistoryService>>();
            var httpAccessor = new HttpContextAccessor();
            var restClient = new FakeRestClient();
            _service = new HistoryService(logger, _db, httpAccessor, restClient, configuration);
        }

        private void SeedData(StorageContext db)
        {
            // Seed minimal PositionHistories to exercise filters
            var factoryId = Guid.NewGuid();
            var deptId = Guid.NewGuid();
            for (int i = 0; i < 1000; i++)
            {
                db.PositionHistories.Add(new PositionHistory { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddMinutes(-i), PositionHistoryType = i % 3, ComponentCode = "CMP" + i, PositionCode = "POS" + (i % 50), Quantity = i, InventoryNumber = i, Note = i % 2 == 0 ? "note" : null, Layout = (i % 2 == 0) ? "A" : "B", EmployeeCode = "E" + i, FactoryId = factoryId, DepartmentId = deptId, CreatedBy = "user-1" });
            }

            db.SaveChanges();
        }

        [Benchmark]
        public async Task Run_GetHistories_All()
        {
            var filter = new HistoryFilterModel
            {
                UserId = "user-1",
                isAllDepartments = true,
                Departments = new List<string>(),
                Factories = new List<string>
                {
                    Guid.Empty.ToString()
                },
                Skip = 0,
                PageSize = 50,
                IsGetAll = true
            };
            await _service.GetHistories(filter);
        }

        // Fake REST client returns permissions and users/departments cheaply
        private class FakeRestClient : IRestClient
        {
            public Task<IRestResponse> ExecuteGetAsync(RestRequest request)
            {
                var url = request.Url;
                object data;
                if (url.Contains("/users/roles/"))
                {
                    data = new[]
                    {
                        new RoleClaimDto
                        {
                            ClaimType = Constants.Permissions.FACTORY_DATA_INQUIRY,
                            ClaimValue = Guid.Empty.ToString()
                        },
                        new RoleClaimDto
                        {
                            ClaimType = Constants.Permissions.DEPARTMENT_DATA_INQUIRY,
                            ClaimValue = Guid.Empty.ToString()
                        },
                    };
                    var resp = new ResponseModel<IEnumerable<RoleClaimDto>>
                    {
                        Data = (IEnumerable<RoleClaimDto>)data
                    };
                    return Task.FromResult<IRestResponse>(new FakeResponse(JsonDefaults.Serialize(resp)));
                }
                else if (url.EndsWith("/users"))
                {
                    var resp = new ResponseModel<IEnumerable<InternalUserDto>>
                    {
                        Data = new[]
                        {
                            new InternalUserDto
                            {
                                Id = "user-1",
                                Name = "Bench User",
                                Code = "U1"
                            }
                        }
                    };
                    return Task.FromResult<IRestResponse>(new FakeResponse(JsonDefaults.Serialize(resp)));
                }
                else if (url.EndsWith("/departments"))
                {
                    var resp = new ResponseModel<IEnumerable<InternalDepartmentDto>>
                    {
                        Data = new[]
                        {
                            new InternalDepartmentDto
                            {
                                Id = Guid.Empty.ToString(),
                                Name = "Dept",
                                IsDeleted = false
                            }
                        }
                    };
                    return Task.FromResult<IRestResponse>(new FakeResponse(JsonDefaults.Serialize(resp)));
                }

                return Task.FromResult<IRestResponse>(new FakeResponse("{}"));
            }
        }

        private class FakeResponse : IRestResponse
        {
            public FakeResponse(string content)
            {
                Content = content;
            }

            public string Content { get; }
        }
    }
}