using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TravelPlanner.Constants;
using TravelPlanner.Controllers;
using TravelPlanner.DAL;
using TravelPlanner.Enums;
using TravelPlanner.Models;
using TravelPlanner.Utilities;

namespace TravelPlanner.Tests.Controllers
{
    [TestClass]
    public class UserControllerTest : BaseControllerTest
    {
        private const string RegisterUserTestName = "RegTestUser";

        public UserControllerTest()
        {
            var dataDirectory = ConfigurationManager.AppSettings["DataDirectory"];
            var absoluteDataDirectory = Path.GetFullPath(dataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", absoluteDataDirectory);

            SetCurrentIdentity(GetInitialAdminUser(), AdminRole);

            UserController = new UserController(UserHelper.Object);
        }

        [ClassInitialize]
        /// Create one user for each role
        public static void ClassSetup(TestContext context)
        {
            Mock<IUserHelper> UserHelper = new Mock<IUserHelper>(); ;
            UserHelper.Setup(x => x.GetLoggedUser(It.IsAny<HttpRequestMessage>())).Returns(GetInitialAdminUser());
            UserHelper.Setup(x => x.GetLoggedUserRole(It.IsAny<HttpRequestMessage>())).Returns(new Role { Id = 1, Name = "Administrator" });

            var userController = new UserController(UserHelper.Object);

            using (var entityContext = new TravelPlannerEntities())
            {
                if (!entityContext.User.Any(x => x.Username == TestUserAdmnistratorName))
                {
                    userController.CreateOrUpdateUser(GetAdministratorTestUser());
                }
                if (!entityContext.User.Any(x => x.Username == TestUserManagerName))
                {
                    userController.CreateOrUpdateUser(GetManagerTestUser());
                }
                if (!entityContext.User.Any(x => x.Username == TestUserRegularUserName))
                {
                    userController.CreateOrUpdateUser(GetRegularUserTestUser());
                }
            }
        }

        [ClassCleanup()]
        /// Remove alll tests and users created for testing purposes
        public static void ClassCleanup()
        {
            Mock<IUserHelper> UserHelper = new Mock<IUserHelper>(); ;
            UserHelper.Setup(x => x.GetLoggedUserRole(It.IsAny<HttpRequestMessage>())).Returns(new Role { Id = 1, Name = "Administrator" });

            var userController = new UserController(UserHelper.Object);

            using (var context = new TravelPlannerEntities())
            {
                var users = context.User.Where(x => x.Username == TestUserAdmnistratorName
                    || x.Username == TestUserManagerName
                    || x.Username == TestUserRegularUserName
                    || x.Username.Contains(RegisterUserTestName));

                /// Remove trips
                foreach (var user in users)
                {
                    var trips = context.Trip.Where(x => x.UserId == user.Id);
                    context.Trip.RemoveRange(trips);
                }

                /// Remove users
                context.User.RemoveRange(users);
                context.SaveChanges();
            }
        }

        #region RegisterUser
        [TestMethod]
        public void RegisterUser_Ok()
        {
            var result = UserController.RegisterUser(GetUserForRegistration());

            Assert.IsNull(result.Error);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.PasswordsDontMatch)]
        public void RegisterUserWrongConfirmPass_Fails()
        {
            var user = GetUserForRegistration();
            user.ConfirmPassword += "1";
            var result = UserController.RegisterUser(user);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.UserEmpty)]
        public void RegisterUserEmptyUsername_Fails()
        {
            var user = GetUserForRegistration();
            user.Username = string.Empty;
            var result = UserController.RegisterUser(user);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.PasswordEmpty + Messages.PasswordsDontMatch)]
        public void RegisterUserEmptyPassword_Fails()
        {
            var user = GetUserForRegistration();
            user.Password = string.Empty;
            var result = UserController.RegisterUser(user);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.ConfirmPasswordEmpty + Messages.PasswordsDontMatch)]
        public void RegisterUserEmptyConfirmPassword_Fails()
        {
            var user = GetUserForRegistration();
            user.ConfirmPassword = string.Empty;
            var result = UserController.RegisterUser(user);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.RoleEmpty)]
        public void RegisterUserEmptyRole_Fails()
        {
            var user = GetUserForRegistration();
            user.RoleId = 0;
            var result = UserController.RegisterUser(user);
        }
        #endregion

        #region Login

        [TestMethod]
        public void LoginUser_Ok()
        {
            var user = GetUserForRegistration();
            user.Username += "LoginUser_Ok";
            UserController.RegisterUser(user);
            var result = UserController.Login(user.Username, user.Password);
            Assert.IsNull(result.Error);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.UserNotFound)]
        public void LoginUser_Fails()
        {
            var user = GetUserForRegistration();
            user.Username += "LoginUser_Fails";
            UserController.RegisterUser(user);
            var result = UserController.Login(user.Username + "1", user.Password);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.BadCredentials)]
        public void LoginUser_InvalidCredentialsFails()
        {
            var user = GetUserForRegistration();
            user.Username += "LoginUser_ICF";
            UserController.RegisterUser(user);
            var result = UserController.Login(user.Username, user.Password + "1");
        }

        #endregion

        #region GetAllUsers

        [TestMethod]
        public void GetAllUsersByAdmin_Ok()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            var result = UserController.GetAllUsers();
            Assert.IsTrue(result.Any(x => x.Id == GetCreatedRegularUserTestUser().Id));
        }

        [TestMethod]
        public void GetAllUsersByManager_Ok()
        {
            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);

            var result = UserController.GetAllUsers();
            Assert.IsTrue(result.Any(x => x.Id == GetCreatedRegularUserTestUser().Id));
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void GetAllUsersByRegularUser_Fails()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);

            var result = UserController.GetAllUsers();
            Assert.IsTrue(result.Count() == 0);
        }

        #endregion

        #region CreateOrUpdateUser_Create

        [TestMethod]
        public void CreateOrUpdateUser_CreateByAdmin_Ok()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var user = GetUserForRegistration();
            user.Username += "CoU_CbA_Ok";
            var result = UserController.CreateOrUpdateUser(user);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void CreateOrUpdateUser_CreateByManager_Ok()
        {
            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);
            var user = GetUserForRegistration();
            user.Username += "CoU_CbM_Ok";
            var result = UserController.CreateOrUpdateUser(user);
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void CreateOrUpdateUser_CreateByRegularUser_Fails()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var user = GetUserForRegistration();
            user.Username += "CoU_CbRU_F";
            var result = UserController.CreateOrUpdateUser(user);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.ConfirmPasswordEmpty + Messages.PasswordsDontMatch)]
        public void CreateOrUpdateUser_CreateEmptyConfirmPassword_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var user = GetUserForRegistration();
            user.Username += "CoU_CECP_F";
            user.ConfirmPassword = string.Empty;
            var result = UserController.CreateOrUpdateUser(user);
        }

        #endregion

        #region GetUser

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void GetUserByRegularUser_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();

            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);

            var result = UserController.GetUser(adminUser.Id);
        }

        [TestMethod]
        public void GetUserByAdmin_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();

            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            var result = UserController.GetUser(regularUser.Id);

            Assert.IsInstanceOfType(result, typeof(User));
        }

        [TestMethod]
        public void GetUserByManager_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();

            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);

            var result = UserController.GetUser(regularUser.Id);

            Assert.IsInstanceOfType(result, typeof(User));
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.UserNotFound)]
        public void GetUserNonExistentUser_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var result = UserController.GetUser(Int32.MaxValue);
        }

        #endregion

        #region CreateOrUpdateUser_Update

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void CreateOrUpdateUser_UpdateByRegularUser_Fails()
        {
            var managerUser = GetCreatedManagerTestUser();

            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);

            managerUser.Username += "CoUUUBRUF";

            UserController.CreateOrUpdateUser(managerUser);
        }

        [TestMethod]
        public void CreateOrUpdateUser_UpdateByAdmin_Ok()
        {
            var managerUser = GetCreatedManagerTestUser();

            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            var originalUsername = managerUser.Username;
            var username = managerUser.Username + "CoUUUBAO";

            managerUser.Username = username;
            managerUser.ConfirmPassword = managerUser.Password;

            UserController.CreateOrUpdateUser(managerUser);

            var resultUser = UserController.GetUser(managerUser.Id);
            Assert.IsTrue(resultUser.Username == username);

            managerUser.Username = originalUsername;
            UserController.CreateOrUpdateUser(managerUser);
        }

        [TestMethod]
        public void CreateOrUpdateUser_UpdateByManager_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();

            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);

            var originalUsername = regularUser.Username;
            var username = regularUser.Username + "CoUUUBMO";

            regularUser.Username = username;
            regularUser.ConfirmPassword = regularUser.Password;

            UserController.CreateOrUpdateUser(regularUser);

            var resultUser = UserController.GetUser(regularUser.Id);
            Assert.IsTrue(resultUser.Username == username);

            regularUser.Username = originalUsername;
            UserController.CreateOrUpdateUser(regularUser);
        }

        #endregion

        #region DeleteUser

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void DeleteUserByRegularUser_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();

            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);

            var result = UserController.DeleteUser(adminUser.Id);
        }

        [TestMethod]
        public void DeleteUserFromRegularUserByAdmin_Ok()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var user = GetUserForRegistration();
            user.Username += "DUFRUBA";
            var userId = UserController.CreateOrUpdateUser(user);

            var result = UserController.DeleteUser(userId);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DeleteUserFromRegularUserByManager_Ok()
        {
            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);
            var user = GetUserForRegistration();
            user.Username += "DUFRUBM";
            var userId = UserController.CreateOrUpdateUser(user);

            var result = UserController.DeleteUser(userId);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.UserNotFound)]
        public void DeleteUserNonExistentUser_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var result = UserController.DeleteUser(Int32.MaxValue);
        }

        #endregion


        [TestMethod]
        public void GetAllRolesByAdmin_Ok()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            var result = UserController.GetAllRoles();
            Assert.IsTrue(result.Count() == 3);
        }

        private User GetUserForRegistration()
        {
            return new User
            {
                Username = RegisterUserTestName,
                Password = EncodedExamplePassword,
                ConfirmPassword = EncodedExamplePassword,
                RoleId = 3
            };
        }
    }
}
