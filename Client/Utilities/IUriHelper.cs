using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Utilities
{
    public interface IUriHelper
    {
        string GetFormedUrl(string url, List<KeyValuePair<string, string>> parameters);
    }
}
