using Microsoft.AspNetCore.Mvc;
using MK.API.Errors;
using MK.Core.HelperModels.Exceptions;
using System.Net;
using System.Text.Json;

namespace MK.API.Middleware
{
    public class ExceptionMiddelware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddelware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddelware(
            RequestDelegate next,
            ILogger<ExceptionMiddelware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next.Invoke(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                httpContext.Response.ContentType = "application/json";

                int statusCode;

                ApiExceprtionErrorRespons response;

                if (ex is NotFoundException)
                {
                    statusCode = (int)HttpStatusCode.NotFound;

                    response = new ApiExceprtionErrorRespons(
                        statusCode,
                        ex.Message);
                }
                else if (ex is BadRequestException)
                {
                    statusCode = (int)HttpStatusCode.BadRequest;

                    response = new ApiExceprtionErrorRespons(
                        statusCode,
                        ex.Message);
                }
                else if (ex is ForbiddenException)
                {
                    statusCode = (int)HttpStatusCode.Forbidden;

                    response = new ApiExceprtionErrorRespons(
                        statusCode,
                        ex.Message);
                }
                else if (ex is UnauthorizedException)
                {
                    statusCode = (int)HttpStatusCode.Unauthorized;

                    response = new ApiExceprtionErrorRespons(
                        statusCode,
                        ex.Message);
                }
                else
                {
                    statusCode = (int)HttpStatusCode.InternalServerError;

                    if (_env.IsDevelopment())
                    {
                        response = new ApiExceprtionErrorRespons(
                            statusCode,
                            ex.Message,
                            ex.StackTrace);
                    }
                    else
                    {
                        response = new ApiExceprtionErrorRespons(statusCode);
                    }
                }

                httpContext.Response.StatusCode = statusCode;

                var json = JsonSerializer.Serialize(response);

                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}
