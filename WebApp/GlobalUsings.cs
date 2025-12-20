global using System.ComponentModel.DataAnnotations;
global using System.Data;
global using System.Reflection;
global using System.Security.Claims;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using BIVN.FixedStorage.Services.Common.API;
global using BIVN.FixedStorage.Services.Common.API.Dto;
global using BIVN.FixedStorage.Services.Common.API.Dto.Factory;
global using BIVN.FixedStorage.Services.Common.API.Dto.Role;
global using BIVN.FixedStorage.Services.Common.API.Helpers;
global using BIVN.FixedStorage.Services.Common.API.Response;
global using BIVN.FixedStorage.Services.Common.API.User;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.Extensions.Options;
global using NetCore.AutoRegisterDi;
global using Newtonsoft.Json;
global using OfficeOpenXml;
global using RestSharp;
global using Serilog;
global using static WebApp.Application.Utilities;
global using commonAPIConstant = BIVN.FixedStorage.Services.Common.API.Constants;
global using JsonSerializer = System.Text.Json.JsonSerializer;
global using AllowAnonymousAttribute = WebApp.Application.Security.AllowAnonymousAttribute;
global using AuthorizeAttribute = WebApp.Application.Security.AuthorizeAttribute;

global using WebApp.Application.Middlewares;
global using WebApp.Application.Security;

global using WebApp.Application.Services;

global using WebApp.Application.Configurations;

global using WebApp.Infrastructure;

global using WebApp.Application;
global using WebApp.Application.Services.DTO;
global using BIVN.FixedStorage.Services.Common.API.Dto.Layout;
