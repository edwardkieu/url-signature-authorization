namespace Common.Lib
{
    public static class Constants
    {
        public static class SignatureConfiguration
        {
            public const int SIGNATURE_EXPIRED_IN_MINUTES = 5;
            public const string SIGNATURE_EXPIRED_TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";
        }

        public static class QueryString
        {
            public const string CLIENT_ID = "clientId";
            public const string EXPIRED_TIME = "expiredTime";
            public const string SIGNATURE = "signature";
        }

        public static class UrlApis
        {
            public static class Product
            {
                public const string GET_ALL = "product/get-all";
            }
        }
    }
}