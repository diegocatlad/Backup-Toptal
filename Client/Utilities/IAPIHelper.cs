using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utilities
{
    public interface IAPIHelper
    {
        HttpResponseMessage GetResponse(string url, string token);

        HttpResponseMessage PostRequest(string url, string token);
    }
}
