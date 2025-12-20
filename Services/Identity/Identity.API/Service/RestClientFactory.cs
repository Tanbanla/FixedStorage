namespace Identity.API.Service
{
    public class RestClientFactory
    {
        private readonly Dictionary<string, RestClient> _restClients = new Dictionary<string, RestClient>();
        private readonly ConfigurationManager _configuration;

        public RestClientFactory(ConfigurationManager configuration)
        {
            _configuration = configuration;
        }
        public RestClient GetClient(string baseUrl)
        {
            if (!_restClients.ContainsKey(baseUrl))
            {
                var client = new RestClient(baseUrl);
                _restClients.Add(baseUrl, client);
            }

            return _restClients[baseUrl];
        }

        public RestClient InventoryClient()
        {
            string baseUrl = _configuration.GetSection("InventoryServiceUrl:Address")?.Value;
            return GetClient(baseUrl);
        }
        public RestClient StorageClient()
        {
            string baseUrl = _configuration.GetSection("StorageServiceUrl:Address")?.Value;
            return GetClient(baseUrl);
        }
    }
}
