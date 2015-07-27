using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client.Constants
{
    public static class URIs
    {
        private const string ApiUrl = "http://localhost/TravelPlanner";

        public const string RegisterUri = ApiUrl+ "/api/User/RegisterUser?";
        public const string LoginUri = ApiUrl + "/api/User/Login?";

        public const string GetAllTripsUri = ApiUrl + "/api/Trip/GetAllTrips";
        public const string CreateOrUpdateTripUri = ApiUrl + "/api/Trip/CreateOrUpdateTrip?";
        public const string DeleteTripUri = ApiUrl + "/api/Trip/DeleteTrip?";
        public const string GetTripUri = ApiUrl + "/api/Trip/GetTrip?";

        public const string GetAllUsersUri = ApiUrl + "/api/User/GetAllUsers";
        public const string CreateOrUpdateUserUri = ApiUrl + "/api/User/CreateOrUpdateUser?";
        public const string DeleteUserUri = ApiUrl + "/api/User/DeleteUser?";
        public const string GetUserUri = ApiUrl + "/api/User/GetUser?";
        public const string GetAllRolesUri = ApiUrl + "/api/User/GetAllRoles";
    }
}