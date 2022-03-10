using Common.Lib.Dtos;
using Common.Lib.Exceptions;
using Common.Lib.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

            var originUri = new Uri(originalUrl);

            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(originUri.Query);
            queryDictionary.Add(Constants.QueryString.SERVICE_NAME, ApiService.Name);
            queryDictionary.Add(Constants.QueryString.EXPIRED_TIME, signatureExpiredTime.ToString(Constants.SignatureConfiguration.SIGNATURE_EXPIRED_TIME_FORMAT, CultureInfo.InvariantCulture));

            var uriPath = originUri.GetLeftPart(UriPartial.Path);

            var urlWithoutSignature = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(uriPath, queryDictionary.ToDictionary(k => k.Key, k => k.Value.ToString()));

            var signature = StringHelper.GenerateSignature(System.Net.WebUtility.UrlDecode(urlWithoutSignature), ApiService.ApiClientKey.ClientSecret);

            queryDictionary.Add(Constants.QueryString.SIGNATURE, signature);

            var urlIncludeSignalture = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(uriPath, queryDictionary.ToDictionary(k => k.Key, k => k.Value.ToString()));

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

            var supportProtocols = new List<string> { "http", "https" };

            var originUri = new Uri(httpContext.Request.GetDisplayUrl());

            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(originUri.Query);

            queryDictionary.Remove(Constants.QueryString.SIGNATURE);

            var urlWithoutSignature = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(originUri.GetLeftPart(UriPartial.Path), queryDictionary.ToDictionary(k => k.Key, k => k.Value.ToString()));

            var uriWithoutSignature = new Uri(urlWithoutSignature);

            var hasValidSignature = false;

            foreach (var protocol in supportProtocols)
            {
                var urlNeedToVerify = uriWithoutSignature.IsDefaultPort
                            ? string.Format("{0}://{1}{2}", protocol, uriWithoutSignature.Host, uriWithoutSignature.AbsolutePath)
                            : string.Format("{0}://{1}{2}", protocol, uriWithoutSignature.Authority, uriWithoutSignature.AbsolutePath);
                urlNeedToVerify += uriWithoutSignature.Query;

                var signature = StringHelper.GenerateSignature(System.Net.WebUtility.UrlDecode(urlNeedToVerify), ApiService.ApiClientKey.ClientSecret);

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