using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Client.Constants;
using Client.Models;
using Client.Utilities;

namespace Client.Controllers
{
    public class UserController : BaseController
    {
        public ActionResult Register()
        {
            return PartialView();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Register(User model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var encodedPass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(model.Password));
                    var encodedConfirmPass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(model.ConfirmPassword));

                    var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("Username", model.Username),
                        new KeyValuePair<string,string>("Password", encodedPass),
                        new KeyValuePair<string,string>("ConfirmPassword", encodedConfirmPass),
                        new KeyValuePair<string,string>("RoleId", "3")
                    };

                    var url = UriHelper.GetFormedUrl(URIs.RegisterUri, parameters);

                    var message = APIHelper.PostRequest(url, string.Empty);

                    var tokenResponse = message.Content.ReadAsAsync<TokenResponse>().Result;

                    if (!String.IsNullOrEmpty(tokenResponse.Error))
                    {
                        return new JsonResult { Data = new { Error = true, Message = tokenResponse.Error } };
                    }

                    SaveLoggedUserDataInSession(model.Username, encodedPass, tokenResponse);

                    return new JsonResult { Data = new { Message = string.Empty, Role = tokenResponse.Role } };
                }
                else
                {
                    return GetModelStateErrors();
                }
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { Error = true, Message = ex.Message } };
            }
        }

        public ActionResult Login()
        {
            return PartialView();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(Login model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var encodedPass = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(model.Password));
                    HttpResponseMessage message;

                    var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("Username", model.Username),
                        new KeyValuePair<string,string>("Password", encodedPass),
                    };

                    var url = UriHelper.GetFormedUrl(URIs.LoginUri, parameters);

                    message = APIHelper.PostRequest(url, string.Empty);
                    var tokenResponse = message.Content.ReadAsAsync<TokenResponse>().Result;

                    if (!String.IsNullOrEmpty(tokenResponse.Error))
                    {
                        return new JsonResult { Data = new { Error = true, Message = tokenResponse.Error } };
                    }

                    SaveLoggedUserDataInSession(model.Username, encodedPass, tokenResponse);

                    return new JsonResult { Data = new { Message = string.Empty, Role = tokenResponse.Role } };
                }
                else
                {
                    return GetModelStateErrors();
                }
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { Error = true, Message = ex.Message } };
            }
        }

        private void SaveLoggedUserDataInSession(string username, string encodedPass, TokenResponse tokenResponse)
        {
            Session[UserSession.Username] = username;

            Session[UserSession.Password] = encodedPass;

            Session[UserSession.Token] = tokenResponse.Token;

            Session[UserSession.Role] = tokenResponse.Role;

            Session[UserSession.ExpirationDateTime] = DateTime.Parse(tokenResponse.ExpirationDate).ToString();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Logoff()
        {
            try
            {
                Session[UserSession.Role] = null;
                return new JsonResult { Data = new { Message = string.Empty } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { Error = true, Message = Message.ErrorOcurred } };
            }
        }

        public ActionResult UsersList()
        {
            try
            {
                var url = URIs.GetAllUsersUri;
                var token = TokenHelper.GetAuthenticationToken();
                if (string.IsNullOrEmpty(token))
                {
                    return View(new List<User>());
                }
                var message = APIHelper.GetResponse(url, token);
                var users = message.Content.ReadAsAsync<List<User>>().Result;

                var roles = GetAllRoles(token);

                foreach (var user in users)
                {
                    user.Role = roles.First(x => x.Id == user.RoleId);
                }

                return PartialView(users);
            }
            catch (Exception ex)
            {
                return PartialView(new List<User>());
            }
        }

        public ActionResult FilterUserList(string username)
        {
            try
            {
                var url = URIs.GetAllUsersUri;
                var token = TokenHelper.GetAuthenticationToken();
                var message = APIHelper.GetResponse(url, token);
                var users = (message.Content.ReadAsAsync<List<User>>().Result)
                    .Where(x => x.Username.ToUpper().Contains(username.ToUpper()));

                var roles = GetAllRoles(token);

                foreach (var user in users)
                {
                    user.Role = roles.First(x => x.Id == user.RoleId);
                }

                return PartialView("UsersList", users);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("UsersList", new List<User>());
            }
        }

        public ActionResult CreateUser()
        {
            var token = TokenHelper.GetAuthenticationToken();
            var roles = GetAllRoles(token)
                .Select(x =>
                        new SelectListItem
                        {
                            Value = x.Id.ToString(),
                            Text = x.Name
                        });

            return PartialView(new CreateUserModel { Roles = new SelectList(roles, "Value", "Text"), User = new User() });
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateUser(User user)
        {
            return CreateOrUpdateUser(user);
        }

        public ActionResult EditUser(int id, bool? saveChangesError = false)
        {
            try
            {
                var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("userId", id.ToString())
                    };

                var url = UriHelper.GetFormedUrl(URIs.GetUserUri, parameters);

                var token = TokenHelper.GetAuthenticationToken();

                var user = APIHelper.GetResponse(url, token).Content.ReadAsAsync<User>().Result;

                if (user == null)
                {
                    return PartialView("_Error");
                }

                var roles = GetAllRoles(token)
                    .Select(x =>
                            new SelectListItem
                            {
                                Value = x.Id.ToString(),
                                Text = x.Name
                            });

                return PartialView(new CreateUserModel { Roles = new SelectList(roles, "Value", "Text"), User = user });

            }
            catch (HttpException ex)
            {
                return new JsonResult { Data = new { Message = ex.Message } };
            }
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditUser(User user)
        {
            return CreateOrUpdateUser(user);
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteUser(int id)
        {
            try
            {
                var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("userId", id.ToString())
                    };

                var url = UriHelper.GetFormedUrl(URIs.DeleteUserUri, parameters);

                var token = TokenHelper.GetAuthenticationToken();

                APIHelper.PostRequest(url, token);

                return new JsonResult { Data = new { Message = Message.SuccesfulDelete } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { Error = true, Message = ex.Message } };
            }
        }

        private JsonResult CreateOrUpdateUser(User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("Id", user.Id.ToString()),
                        new KeyValuePair<string,string>("Username", user.Username),
                        new KeyValuePair<string,string>("RoleId", user.RoleId.ToString()),
                        new KeyValuePair<string,string>("Password", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(user.Password))),
                        new KeyValuePair<string,string>("ConfirmPassword", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(user.ConfirmPassword))),
                    };

                    var url = UriHelper.GetFormedUrl(URIs.CreateOrUpdateUserUri, parameters);

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

        private List<Role> GetAllRoles(string token)
        {
            var url = URIs.GetAllRolesUri;
            var message = APIHelper.GetResponse(url, token);
            return message.Content.ReadAsAsync<List<Role>>().Result;
        }
    }
}