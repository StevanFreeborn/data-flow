global using System.Net.Http.Json;
global using System.Net.Mail;
global using System.Text;

global using Bogus;

global using DotNet.Testcontainers.Builders;
global using DotNet.Testcontainers.Containers;

global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Moq;

global using OnxFlow.Server.API.Email;
global using OnxFlow.Server.API.Encryption;
global using OnxFlow.Server.API.Identity;
global using OnxFlow.Server.API.Tests.Data;
global using OnxFlow.Server.API.Tests.Mailhog;