using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using Client.Utilities;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public abstract class BaseController : Controller
    {
        private IAPIHelper _apiHelper = new APIHelper();

        private ITokenHelper _tokenHelper = new TokenHelper();

        private IUriHelper _uriHelper = new UriHelper();

        public IAPIHelper APIHelper
        {
            get
            {
                return _apiHelper;
            }
            set
            {
                _apiHelper = value;
            }
        }

        public ITokenHelper TokenHelper
        {
            get
            {
                return _tokenHelper;
            }
            set
            {
                _tokenHelper = value;
            }
        }

        public IUriHelper UriHelper
        {
            get
            {
                return _uriHelper;
            }
            set
            {
                _uriHelper = value;
            }
        }
        internal JsonResult GetModelStateErrors()
        {
            var message = string.Empty;
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    message += error.ErrorMessage + "<br />";
                }
            }
            return new JsonResult { Data = new { Error = true, Message = message } };
        }
    }
}