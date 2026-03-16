using System;

namespace WebApi.Entities
{
    public class RequestLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string EndpointPath { get; set; }
        public string HttpMethod { get; set; }
        public string RequestBody { get; set; }
        public string ClientIp { get; set; }
        public int? UserId { get; set; }
    }
}
