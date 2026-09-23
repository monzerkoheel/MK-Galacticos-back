using Microsoft.AspNetCore.Http;
using MK.Core.Contracts.ServicesContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices
{
    public class RequestContext : IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? IpAddress =>
            _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
