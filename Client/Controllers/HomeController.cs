using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Client.Constants;
using Client.Models;
using Client.Utilities;

namespace Client.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Role = Session[UserSession.Role];
            return View();
        }

        public ActionResult TripsList()
        {
            try
            {
                var url = URIs.GetAllTripsUri;
                var token = TokenHelper.GetAuthenticationToken();
                if (string.IsNullOrEmpty(token))
                {
                    return View(new List<Trip>());
                }
                ViewBag.LoggedIn = true;
                var message = APIHelper.GetResponse(url, token);
                var model = message.Content.ReadAsAsync<List<Trip>>().Result;
                return PartialView(model);
            }
            catch (Exception ex)
            {
                return PartialView(new List<Trip>());
            }
        }

        public ActionResult TravelPlan()
        {
            try
            {
                var url = URIs.GetAllTripsUri;
                var token = TokenHelper.GetAuthenticationToken();
                var message = APIHelper.GetResponse(url, token);
                var model = (message.Content.ReadAsAsync<List<Trip>>().Result)
                    .Where(x => x.StartDate > DateTime.Today && x.StartDate <= DateTime.Today.AddMonths(1));

                return PartialView("TravelPlan", model);
            }
            catch (Exception ex)
            {
                return PartialView(new List<Trip>());
            }
        }

        public ActionResult FilterList(string tripDestination)
        {
            try
            {
                var url = URIs.GetAllTripsUri;
                var token = TokenHelper.GetAuthenticationToken();
                var message = APIHelper.GetResponse(url, token);
                var model = (message.Content.ReadAsAsync<List<Trip>>().Result)
                    .Where(x => x.Destination.ToUpper().Contains(tripDestination.ToUpper()));

                return PartialView("TripsList", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("TripsList", new List<Trip>());
            }
        }

        public ActionResult Create()
        {
            return PartialView();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Trip Trip)
        {
            return CreateOrUpdateTrip(Trip);
        }

        public ActionResult Edit(int id, bool? saveChangesError = false)
        {
            try
            {
                var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("tripId", id.ToString())
                    };

                var url = UriHelper.GetFormedUrl(URIs.GetTripUri, parameters);

                var token = TokenHelper.GetAuthenticationToken();

                var Trip = APIHelper.GetResponse(url, token).Content.ReadAsAsync<Trip>().Result;

                if (Trip == null)
                {
                    return PartialView("_Error");
                }

                return PartialView(Trip);
            }
            catch (HttpException ex)
            {
                return new JsonResult { Data = new { Message = ex.Message } };
            }
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(Trip Trip)
        {
            return CreateOrUpdateTrip(Trip);
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("tripId", id.ToString())
                    };

                var url = UriHelper.GetFormedUrl(URIs.DeleteTripUri, parameters);

                var token = TokenHelper.GetAuthenticationToken();

                APIHelper.PostRequest(url, token);

                return new JsonResult { Data = new { Message = Message.SuccesfulDelete } };
            }
            catch (HttpException ex)
            {
                return new JsonResult { Data = new { Error = true, Message = ex.Message } };
            }
        }

        private JsonResult CreateOrUpdateTrip(Trip Trip)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("Id", Trip.Id.ToString()),
                        new KeyValuePair<string,string>("Destination", Trip.Destination),
                        new KeyValuePair<string,string>("StartDate", Trip.StartDate.ToString()),
                        new KeyValuePair<string,string>("EndDate", Trip.EndDate.ToString()),
                        new KeyValuePair<string,string>("Comment", Trip.Comment),
                        new KeyValuePair<string,string>("UserId", Trip.UserId.ToString()),
                    };

                    var url = UriHelper.GetFormedUrl(URIs.CreateOrUpdateTripUri, parameters);

                    var token = TokenHelper.GetAuthenticationToken();

                    var intId = APIHelper.PostRequest(url, token).Content.ReadAsAsync<int>().Result;

                    return new JsonResult { Data = new { Message = Message.SuccesfulSave } };
                }
                else
                {
                    return GetModelStateErrors();
                }
            }
            catch (HttpException ex)
            {
                return new JsonResult { Data = new { Error = true, Message = ex.Message } };
            }
        }
    }
}
