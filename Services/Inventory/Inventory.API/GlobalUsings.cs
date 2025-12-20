global using System.Data.SqlClient;
global using System.Text;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.AspNetCore.Mvc.ModelBinding;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Polly;
global using Polly.Retry;
global using System.ComponentModel.DataAnnotations;
global using RestSharp;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Swashbuckle.AspNetCore.Annotations;
global using System.Text.RegularExpressions;
global using System.Security.Claims;
global using CsvHelper;
global using OfficeOpenXml;
global using Microsoft.AspNetCore.Mvc.Controllers;


global using BIVN.FixedStorage.Services.Common.API.Response;
global using BIVN.FixedStorage.Services.Common.API;
global using BIVN.FixedStorage.Services.Common.API.Dto;


global using commonAPIConstant = BIVN.FixedStorage.Services.Common.API.Constants;
global using BIVN.FixedStorage.Services.Common.API.User;
global using Microsoft.IdentityModel.Tokens;

global using static BIVN.FixedStorage.Services.Common.API.Constants;
global using BIVN.FixedStorage.Services.Inventory.API.Attributes;
global using Inventory.API.Service;
global using BIVN.FixedStorage.Services.Infrastructure;

global using Inventory.API.Services;
global using Microsoft.OpenApi.Models;
global using Inventory.API;

global using Inventory.API.Infrastructure.Entity;
global using Inventory.API.Infrastructure.Entity.Enums;

global using BIVN.FixedStorage.Services.Inventory.API.Service;
global using BIVN.FixedStorage.Services.Inventory.API;
global using BIVN.FixedStorage.Services.Common.API.Dto.Inventory;
global using Inventory.API.Service.Dto;

global using BIVN.FixedStorage.Services.Inventory.API.Middlewares;
global using Inventory.API.HostedServices;
global using OfficeOpenXml.Style;
global using static BIVN.FixedStorage.Services.Common.API.Constants.ImportExcelColumns;
global using BIVN.FixedStorage.Inventory.Inventory.API.HostedServices.Dto;
global using BIVN.FixedStorage.Services.Common.API.Enum;

global using Inventory.API.Service.Dto.InventoryWeb;
global using Newtonsoft.Json;
global using System.Globalization;

global using System.Drawing;

global using System.Data;
global using LinqKit;

global using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

global using Microsoft.Extensions.Caching.Memory;
