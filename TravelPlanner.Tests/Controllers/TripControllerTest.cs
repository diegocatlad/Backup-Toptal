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
    public class TripControllerTest : BaseControllerTest
    {
        private TripController TripController;
        private const string TestDestinationName = "Test_Destination";

        public TripControllerTest()
        {
            var dataDirectory = ConfigurationManager.AppSettings["DataDirectory"];
            var absoluteDataDirectory = Path.GetFullPath(dataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", absoluteDataDirectory);

            SetCurrentIdentity(GetInitialAdminUser(), AdminRole);

            TripController = new TripController(UserHelper.Object);
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
                    || x.Username == TestUserRegularUserName);

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

        #region GetAllTrips

        [TestMethod]
        public void GetAllTripsByAdmin_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            var result = TripController.GetAllTrips().Where(x => x.Destination == TestDestinationName);
            Assert.IsTrue(result.Any(x => x.UserId == regularUser.Id));
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void GetAllTripsByManager_Fails()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);

            var result = TripController.GetAllTrips().Where(x => x.Destination == TestDestinationName);
        }

        [TestMethod]
        public void GetAllTripsByRegularUser_Ok()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            TripController.CreateOrUpdateTrip(GetTripToBeCreated(adminUser.Id));

            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var result = TripController.GetAllTrips().Where(x => x.Destination == TestDestinationName);
            Assert.IsFalse(result.Any(x => x.UserId != regularUser.Id));
        }

        #endregion

        #region CreateOrUpdateTrip_Create

        [TestMethod]
        public void CreateOrUpdateTrip_CreateByAdmin_Ok()
        {
            var user = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(user, AdminRole);
            var result = TripController.CreateOrUpdateTrip(GetTripToBeCreated(user.Id));
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void CreateOrUpdateTrip_CreateByManager_Fails()
        {
            var user = GetCreatedManagerTestUser();
            SetCurrentIdentity(user, ManagerRole);
            var result = TripController.CreateOrUpdateTrip(GetTripToBeCreated(user.Id));
        }

        [TestMethod]
        public void CreateOrUpdateTrip_CreateByRegularUser_Ok()
        {
            var user = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(user, UserRole);
            var result = TripController.CreateOrUpdateTrip(GetTripToBeCreated(user.Id));
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.StartDateHigherThanEndDate)]
        public void CreateOrUpdateTrip_CreateByRegularUserIncorrectDateFields_Fails()
        {
            var user = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(user, UserRole);
            var trip = GetTripToBeCreated(user.Id);
            trip.StartDate = DateTime.Now;
            trip.EndDate = DateTime.Now.AddDays(-1);
            var result = TripController.CreateOrUpdateTrip(trip);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.DestinationEmpty)]
        public void CreateOrUpdateTrip_CreateByRegularUserEmptyDestination_Fails()
        {
            var user = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(user, UserRole);
            var trip = GetTripToBeCreated(user.Id);
            trip.Destination = string.Empty;
            var result = TripController.CreateOrUpdateTrip(trip);
        }

        #endregion

        #region GetTrip

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void GetTripFromAdminByRegularUser_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(adminUser.Id));

            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);

            var result = TripController.GetTrip(tripId);
        }

        [TestMethod]
        public void GetTripFromRegularUserByRegularUser_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var result = TripController.GetTrip(tripId);

            Assert.IsInstanceOfType(result, typeof(Trip));
        }

        [TestMethod]
        public void GetTripFromRegularUserByAdmin_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            var result = TripController.GetTrip(tripId);

            Assert.IsInstanceOfType(result, typeof(Trip));
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void GetTripFromRegularUserByManager_Fails()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);

            var result = TripController.GetTrip(tripId);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.TripNotFound)]
        public void GetTripNonExistentTrip_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var result = TripController.GetTrip(Int32.MaxValue);
        }

        #endregion

        #region CreateOrUpdateTrip_Update

        [TestMethod]
        public void CreateOrUpdateTrip_UpdateFromRegularUserByRegularUser_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var trip = GetTripToBeCreated(regularUser.Id);
            var tripId = TripController.CreateOrUpdateTrip(trip);

            trip.Id = tripId;
            var addedDestinationName = "Updated";

            trip.Destination += addedDestinationName;

            TripController.CreateOrUpdateTrip(trip);

            var resultTrip = TripController.GetTrip(tripId);
            Assert.IsTrue(resultTrip.Destination == TestDestinationName + addedDestinationName);
        }

        [TestMethod]
        public void CreateOrUpdateTrip_UpdateFromRegularUserByAdmin_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var trip = GetTripToBeCreated(regularUser.Id);
            var tripId = TripController.CreateOrUpdateTrip(trip);

            trip.Id = tripId;
            var addedDestinationName = "Updated";

            trip.Destination += addedDestinationName;

            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            TripController.CreateOrUpdateTrip(trip);

            var resultTrip = TripController.GetTrip(tripId);
            Assert.IsTrue(resultTrip.Destination == TestDestinationName + addedDestinationName);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void CreateOrUpdateTrip_UpdateFromAdminByRegularUser_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var trip = GetTripToBeCreated(adminUser.Id);
            var tripId = TripController.CreateOrUpdateTrip(trip);

            trip.Id = tripId;
            var addedDestinationName = "Updated";

            trip.Destination += addedDestinationName;

            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);

            TripController.CreateOrUpdateTrip(trip);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void CreateOrUpdateTrip_UpdateFromRegularUserByManager_Fails()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var trip = GetTripToBeCreated(regularUser.Id);
            var tripId = TripController.CreateOrUpdateTrip(trip);

            trip.Id = tripId;
            var addedDestinationName = "Updated";

            trip.Destination += addedDestinationName;

            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);

            TripController.CreateOrUpdateTrip(trip);
        }

        #endregion

        #region DeleteTrip

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void DeleteTripFromAdminByRegularUser_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(adminUser.Id));

            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);

            var result = TripController.DeleteTrip(tripId);
        }

        [TestMethod]
        public void DeleteTripFromRegularUserByRegularUser_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var result = TripController.DeleteTrip(tripId);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DeleteTripFromRegularUserByAdmin_Ok()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);

            var result = TripController.DeleteTrip(tripId);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.Unauthorized)]
        public void DeleteTripFromRegularUserByManager_Fails()
        {
            var regularUser = GetCreatedRegularUserTestUser();
            SetCurrentIdentity(regularUser, UserRole);
            var tripId = TripController.CreateOrUpdateTrip(GetTripToBeCreated(regularUser.Id));

            var managerUser = GetCreatedManagerTestUser();
            SetCurrentIdentity(managerUser, ManagerRole);

            var result = TripController.DeleteTrip(tripId);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(WebException), ExpectedMessage = Messages.TripNotFound)]
        public void DeleteTripNonExistentTrip_Fails()
        {
            var adminUser = GetCreatedAdministratorTestUser();
            SetCurrentIdentity(adminUser, AdminRole);
            var result = TripController.DeleteTrip(Int32.MaxValue);
        }

        #endregion

        #region Helpers

        private Trip GetTripToBeCreated(int userId)
        {
            return new Trip
            {
                Destination = TestDestinationName,
                StartDate = DateTime.Today.AddDays(4),
                EndDate = DateTime.Today.AddDays(10),
                UserId = userId
            };
        }

        #endregion

    }
}
