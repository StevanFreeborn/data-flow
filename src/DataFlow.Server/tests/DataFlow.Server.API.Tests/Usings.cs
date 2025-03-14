global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Net.Http.Json;
global using System.Net.Mail;
global using System.Security.Claims;
global using System.Text;
global using System.Text.RegularExpressions;

global using Bogus;

global using DotNet.Testcontainers.Builders;
global using DotNet.Testcontainers.Containers;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using MongoDB.Bson;
global using MongoDB.Driver;

global using Moq;

global using DataFlow.Server.API.Authentication;
global using DataFlow.Server.API.Authentication.Register;
global using DataFlow.Server.API.Cors;
global using DataFlow.Server.API.Data;
global using DataFlow.Server.API.Data.Mongo;
global using DataFlow.Server.API.Email;
global using DataFlow.Server.API.Encryption;
global using DataFlow.Server.API.Identity;
global using DataFlow.Server.API.Tests.Data;
global using DataFlow.Server.API.Tests.Infrastructure;
global using DataFlow.Server.API.Tests.Mailhog;

global using Testcontainers.MongoDb;
