﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.PublicAPI.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BackEnd.Exceptions
{

    public class ApiLogicExceptionsHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLogicExceptionsHandlerMiddleware> logger;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public ApiLogicExceptionsHandlerMiddleware(
            RequestDelegate next,
            IOptions<JsonSerializerSettings> jsonSerializerSettings,
            ILogger<ApiLogicExceptionsHandlerMiddleware> logger)
        {
            _next = next;
            this.logger = logger;
            this.jsonSerializerSettings = jsonSerializerSettings.Value;
            this.jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

                switch (context.Response.StatusCode)
                {
                    case StatusCodes.Status401Unauthorized:
                        throw ResponseStatusCode.Unauthorized.ToApiException();
                    case StatusCodes.Status403Forbidden:
                        throw ResponseStatusCode.Forbidden.ToApiException();
                    default:
                        logger.LogTrace(context.Response.StatusCode.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(Content(ex));
            }
        }
        private string Content(Exception ex)
            => JsonConvert.SerializeObject(GetData(ex), Newtonsoft.Json.Formatting.Indented, jsonSerializerSettings);
        private object GetData(Exception ex)
        {
            switch (ex)
            {
                case DbUpdateException dbUpdate:
                    logger.LogInformation(dbUpdate, "error while update DB");
                    return new ResponseBase(ResponseStatusCode.IncorrectRequestData);
                case ApiLogicException api:
                    logger.LogInformation(ex, $"exception in controller {api.ResponseModel.StatusCode}");
                    return api.ResponseModel;
                case NotImplementedException nie:
                    logger.LogWarning(ex, "Not implement");
                    return new ResponseBase(ResponseStatusCode.NotImplemented);
                default:
                    logger.LogWarning(ex, "Unknown exception");
                    return new ResponseBase(ResponseStatusCode.Unknown);
            }
        }

    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiLogicExceptionsHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLogicExceptionsHandlerMiddleware>();
        }
    }
}
