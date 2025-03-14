global using System.IdentityModel.Tokens.Jwt;
global using System.Linq.Expressions;
global using System.Net;
global using System.Net.Mail;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.RegularExpressions;

global using DataFlow.Server.API.Authentication;
global using DataFlow.Server.API.Authentication.Login;
global using DataFlow.Server.API.Authentication.Logout;
global using DataFlow.Server.API.Authentication.Register;
global using DataFlow.Server.API.Authentication.Verify;
global using DataFlow.Server.API.Cors;
global using DataFlow.Server.API.Data;
global using DataFlow.Server.API.Data.Mongo;
global using DataFlow.Server.API.Email;
global using DataFlow.Server.API.Encryption;
global using DataFlow.Server.API.Identity;

global using FluentResults;

global using FluentValidation;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using MongoDB.Bson.Serialization;
global using MongoDB.Bson.Serialization.IdGenerators;
global using MongoDB.Driver;
