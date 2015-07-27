using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client.Constants
{
    public static class Message
    {
        public const string InvalidData = "Invalid data";
        public const string SuccesfulSave = "Data has been saved succeessfully";
        public const string SuccesfulDelete = "Data has been deleted succeessfully";
        public const string Unauthenticated = "You need to login to perform this action";
        public const string ErrorOcurred = "An unexpected error has ocurred. If the problem persists, please contact your administrator.";
        public const string Unauthorized = "You don't have enough permpissions to perform this action.";        
    }
}