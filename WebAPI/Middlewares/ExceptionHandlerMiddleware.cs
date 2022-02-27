using Common.Lib.Dtos;
using Common.Lib.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebAPI.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception error)
            {
                var response = httpContext.Response;
                var responseData = new DataResponse<string>();

                response.ContentType = "application/json";

                switch (error)
                {
                    case UnAuthorizedException:
                        responseData.Data = "UnAuthorized";
                        responseData.Status = 401;
                        break;

                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        responseData.Data = error.Message;
                        responseData.Status = 500;
                        break;
                }
                var result = JsonSerializer.Serialize(responseData);
                await response.WriteAsync(result);
            }
        }
    }
}