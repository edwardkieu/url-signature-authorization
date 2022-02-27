using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Common.Lib.Services
{
    public interface IUrlSignatureService
    {
        string CreateUrlSignature(string originalUrl);
        bool ValidateUrlSignature(HttpContext httpContext);
        void CreateApiServiceInstance(string serviceName, string clientId, string clientSecret);
    }
}