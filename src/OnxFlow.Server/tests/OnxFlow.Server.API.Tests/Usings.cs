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

global using OnxFlow.Server.API.Authentication;
global using OnxFlow.Server.API.Authentication.Register;
global using OnxFlow.Server.API.Cors;
global using OnxFlow.Server.API.Data;
global using OnxFlow.Server.API.Data.Mongo;
global using OnxFlow.Server.API.Email;
global using OnxFlow.Server.API.Encryption;
global using OnxFlow.Server.API.Identity;
global using OnxFlow.Server.API.Tests.Data;
global using OnxFlow.Server.API.Tests.Infrastructure;
global using OnxFlow.Server.API.Tests.Mailhog;

global using Testcontainers.MongoDb;
