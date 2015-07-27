using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TravelPlanner.Controllers;
using TravelPlanner.DAL;
using TravelPlanner.Enums;
using TravelPlanner.Utilities;

namespace TravelPlanner.Tests.Controllers
{
    public class BaseControllerTest
    {
        #region Internal fields

        internal readonly Mock<IUserHelper> UserHelper = new Mock<IUserHelper>();
        internal UserController UserController;
        internal static readonly string EncodedExamplePassword = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes("Password"));
        internal const string TestUserAdmnistratorName = "testUserAdmnistrator";
        internal const string TestUserManagerName = "testUserManager";
        internal const string TestUserRegularUserName = "testUserRegularUser";
        internal int TestUserAdmnistratorId;
        internal int TestUserManagerId;
        internal int TestUserRegularUserId;
        internal readonly Role AdminRole = new Role { Id = 1, Name = RolesEnum.Administrator.ToString() };
        internal readonly Role ManagerRole = new Role { Id = 2, Name = RolesEnum.Manager.ToString() };
        internal readonly Role UserRole = new Role { Id = 3, Name = RolesEnum.User.ToString() };

        #endregion

        public User AdminTestUser { get; set; }
        public User ManagerTestUser { get; set; }
        public User RegularUserTestUser { get; set; }

        #region Helpers

        internal static User GetInitialAdminUser()
        {
            return new User { Id = 1, Username = "Administrator", RoleId = 1 };
        }

        internal static User GetAdministratorTestUser()
        {
            return new User
            {
                Username = TestUserAdmnistratorName,
                Password = EncodedExamplePassword,
                ConfirmPassword = EncodedExamplePassword,
                RoleId = 1 
            };
        }

        internal static User GetManagerTestUser()
        {
            return new User
            {
                Username = TestUserManagerName,
                Password = EncodedExamplePassword,
                ConfirmPassword = EncodedExamplePassword,
                RoleId = 2
            };
        }

        internal static User GetRegularUserTestUser()
        {
            return new User
            {
                Username = TestUserRegularUserName,
                Password = EncodedExamplePassword,
                ConfirmPassword = EncodedExamplePassword,
                RoleId = 3 
            };
        }

        internal User GetCreatedAdministratorTestUser()
        {
            if (AdminTestUser != null)
            {
                return AdminTestUser;
            }
            using (var context = new TravelPlannerEntities())
            {

                return context.User.First(x => x.Username == TestUserAdmnistratorName);
            }
        }

        internal User GetCreatedManagerTestUser()
        {
            if (ManagerTestUser != null)
            {
                return ManagerTestUser;
            }
            using (var context = new TravelPlannerEntities())
            {

                return context.User.First(x => x.Username == TestUserManagerName);
            }
        }

        internal User GetCreatedRegularUserTestUser()
        {
            if (RegularUserTestUser != null)
            {
                return RegularUserTestUser;
            }
            using (var context = new TravelPlannerEntities())
            {

                return context.User.First(x => x.Username == TestUserRegularUserName);
            }
        }

        internal void SetCurrentIdentity(User user, Role role)
        {
            var identity = new GenericIdentity(user.Username);
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.Id.ToString()));

            Thread.CurrentPrincipal = new GenericPrincipal(identity, new[] { role.Name });

            UserHelper.Setup(x => x.GetLoggedUser(It.IsAny<HttpRequestMessage>())).Returns(user);
            UserHelper.Setup(x => x.GetLoggedUserRole(It.IsAny<HttpRequestMessage>())).Returns(new Role { Id = role.Id, Name = role.Name });

            UserController = new UserController(UserHelper.Object);
        }

        #endregion
    }
}
