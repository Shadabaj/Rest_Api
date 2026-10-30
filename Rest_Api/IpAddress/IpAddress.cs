namespace Rest_Api.IpAddress
{
    public class IpAddress : IipAddress
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<IpAddress> _logger;

        public IpAddress(IHttpContextAccessor httpContextAccessor, ILogger<IpAddress> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetIpAddress()
        {
            var context= _httpContextAccessor.HttpContext;
            if(context== null)
            {
                return "No-Ip-Address-Found";
            }
            string ipAddress = context.Request.Headers["X-Forrwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] address = ipAddress.Split(".");

                if(address.Length>0)
                return address[0].Trim();
               
            }

            return context.Connection.LocalPort.ToString() ?? context.Connection.RemoteIpAddress.ToString();
        }
    }
}
