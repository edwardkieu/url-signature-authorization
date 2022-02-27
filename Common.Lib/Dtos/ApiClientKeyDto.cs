namespace Common.Lib.Dtos
{
    public class ApiServiceDto
    {
        public string Name { get; set; }

        public ApiClientKeyDto ApiClientKey { get; set; }
    }

    public class ApiClientKeyDto
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}