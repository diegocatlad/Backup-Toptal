using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client.Utilities
{
    public class UriHelper: IUriHelper
    {
        public string GetFormedUrl(string url, List<KeyValuePair<string, string>> parameters)
        {
            foreach (var parameter in parameters)
            {
                url = string.Format("{0}{1}={2}&", url, HttpUtility.UrlEncode(parameter.Key), HttpUtility.UrlEncode(parameter.Value));
            }
            return url;
        }
    }
}