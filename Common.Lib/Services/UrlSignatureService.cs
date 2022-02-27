using Common.Lib.Dtos;
using Common.Lib.Exceptions;
using Common.Lib.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;

namespace Common.Lib.Services
{
    public class UrlSignatureService : IUrlSignatureService
    {
        public ApiServiceDto ApiService { get; set; }

        public void CreateApiServiceInstance(string serviceName, string clientId, string clientSecret)
        {
            ApiService = new ApiServiceDto
            {
                Name = serviceName,
                ApiClientKey = new ApiClientKeyDto
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            };
        }

        public string CreateUrlSignature(string originalUrl)
        {
            var signatureExpiredTime = DateTimeOffset.UtcNow.AddMinutes(Constants.SignatureConfiguration.SIGNATURE_EXPIRED_IN_MINUTES);
            var uri = new Uri(originalUrl);
            var queryString = new NameValueCollection
            {
                HttpUtility.ParseQueryString(uri.Query),
                { Constants.QueryString.CLIENT_ID, ApiService.ApiClientKey.ClientId },
                { Constants.QueryString.EXPIRED_TIME, signatureExpiredTime.ToString(Constants.SignatureConfiguration.SIGNATURE_EXPIRED_TIME_FORMAT, CultureInfo.InvariantCulture) }
            };
            var uriPath = uri.GetLeftPart(UriPartial.Path);
            var urlWithoutSignature = string.Format("{0}{1}", uriPath, StringHelper.ConvertNameValueCollectionToQueryString(queryString));
            var signature = StringHelper.GenerateSignature(System.Net.WebUtility.UrlDecode(urlWithoutSignature), ApiService.ApiClientKey.ClientSecret);
            queryString.Add(Constants.QueryString.SIGNATURE, signature);

            var urlIncludeSignalture = string.Format("{0}{1}", uriPath, StringHelper.ConvertNameValueCollectionToQueryString(queryString));

            return urlIncludeSignalture;
        }

        public bool ValidateUrlSignature(HttpContext httpContext)
        {
            return HasValidSignatureExpiredTime(httpContext) && HasValidSignature(httpContext);
        }

        #region Private Methods

        private bool HasValidSignatureExpiredTime(HttpContext httpContext)
        {
            var expiredTime = httpContext.Request.Query[Constants.QueryString.EXPIRED_TIME];
            if (string.IsNullOrWhiteSpace(expiredTime))
                throw new UnAuthorizedException("INVALID_SIGNATURE_TIMESTAMP_EMPTY");

            var signatureExpiredTime = DateTimeOffset.ParseExact(expiredTime, Constants.SignatureConfiguration.SIGNATURE_EXPIRED_TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

            var absTotalMinutes = Math.Abs((DateTimeOffset.UtcNow - signatureExpiredTime).TotalMinutes);

            var hasValidSignatureExpiredTime = absTotalMinutes <= Constants.SignatureConfiguration.SIGNATURE_EXPIRED_IN_MINUTES;

            if (!hasValidSignatureExpiredTime)
                throw new UnAuthorizedException("INVALID_SIGNATURE_EXPIRED_TIME");

            return hasValidSignatureExpiredTime;
        }

        private bool HasValidSignature(HttpContext httpContext)
        {
            var signatureOrigin = httpContext.Request.Query[Constants.QueryString.SIGNATURE];
            var clientId = httpContext.Request.Query[Constants.QueryString.CLIENT_ID];
            if (string.IsNullOrWhiteSpace(clientId))
                throw new UnAuthorizedException("INVALID_URL_SIGNATURE_ACCESSKEY");

            if (ApiService == null)
                throw new UnAuthorizedException("INVALID_URL_SIGNATURE_SECRETKEY");

            var supportProtocols = new List<string> { "http", "https" };
            var uri = new Uri(httpContext.Request.GetDisplayUrl());
            var queryString = new NameValueCollection
            {
                HttpUtility.ParseQueryString(uri.Query)
            };

            var hasValidSignature = false;

            foreach (var protocol in supportProtocols)
            {
                var url = uri.IsDefaultPort
                             ? string.Format("{0}://{1}{2}", protocol, uri.Host, uri.AbsolutePath)
                             : string.Format("{0}://{1}{2}", protocol, uri.Authority, uri.AbsolutePath);

                var urlWithoutSignature = StringHelper.RemoveQueryStrings(string.Format("{0}{1}", url, StringHelper.ConvertNameValueCollectionToQueryString(queryString)), new string[] { Constants.QueryString.SIGNATURE });

                var signature = StringHelper.GenerateSignature(System.Net.WebUtility.UrlDecode(urlWithoutSignature), ApiService.ApiClientKey.ClientSecret);
                hasValidSignature = signature.Equals(signatureOrigin);

                if (hasValidSignature)
                    return hasValidSignature;
            }

            if (!hasValidSignature)
                throw new UnAuthorizedException("INVALID_URL_SIGNATURE");

            return hasValidSignature;
        }

        #endregion Private Methods
    }
}