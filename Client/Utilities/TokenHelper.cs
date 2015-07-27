using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Linq;
using System.Web;
using Client.Constants;
using Client.Models;

namespace Client.Utilities
{
    public class TokenHelper : ITokenHelper
    {
        private readonly IAPIHelper ApiHelper = new APIHelper();

        private readonly IUriHelper UriHelper = new UriHelper();

        public string GetAuthenticationToken()
        {
            if (HttpContext.Current.Session[UserSession.Token] != null &&
                !string.IsNullOrEmpty(HttpContext.Current.Session[UserSession.Token].ToString()) &&
                HttpContext.Current.Session[UserSession.ExpirationDateTime] != null)
            {
                DateTime expiration;
                if (DateTime.TryParse(HttpContext.Current.Session[UserSession.ExpirationDateTime].ToString(), out expiration))
                {
                    if (DateTime.Now < expiration)
                    {
                        return HttpContext.Current.Session[UserSession.Token].ToString();
                    }
                }
            }

            var userName = HttpContext.Current.Session[UserSession.Username] == null ? 
                string.Empty :
                HttpContext.Current.Session[UserSession.Username].ToString();
            var password = HttpContext.Current.Session[UserSession.Password] == null ? 
                string.Empty :
                HttpContext.Current.Session[UserSession.Password].ToString();

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new HttpException(0, Message.ErrorOcurred);
            }

            try
            {
                var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("Username", userName),
                        new KeyValuePair<string,string>("Password", password),
                    };

                var url = UriHelper.GetFormedUrl(URIs.LoginUri, parameters);

                var message = ApiHelper.PostRequest(url, string.Empty);
                var messageContent = message.Content.ReadAsAsync<Dictionary<string, string>>().Result;
                var token = messageContent.First(x => x.Key == Parameter.TokenParameter).Value;
                HttpContext.Current.Session[UserSession.Token] = token;
                var expires = messageContent.First(x => x.Key == Parameter.ExpirationDateParameter).Value;
                HttpContext.Current.Session[UserSession.ExpirationDateTime] = DateTime.Parse(expires).ToString();
                return token;
            }
            catch (Exception)
            {
                throw new HttpException(0, Message.ErrorOcurred);
            }
        }
    }
}