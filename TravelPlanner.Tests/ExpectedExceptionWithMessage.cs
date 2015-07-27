using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TravelPlanner.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ExpectedExceptionWithMessage : ExpectedExceptionBaseAttribute
    {
        public Type ExpectedException { get; set; }
        public string ExpectedMessage { get; set; }

        public ExpectedExceptionWithMessage(Type expectedException)
            : this(expectedException, "", "")
        {
        }

        public ExpectedExceptionWithMessage(Type expectedException,
          string message)
            : this(expectedException, message, "")
        {
        }

        public ExpectedExceptionWithMessage(Type expectedException,
          string message, string noExceptionMessage)
            : base(noExceptionMessage)
        {
            if (expectedException == null){
                throw new ArgumentNullException("exceptionType");}
            if (!typeof(Exception).IsAssignableFrom(expectedException))
                throw new ArgumentException("Expected exception type must be "
                    + "System.Exception or derived from System.Exception.",
                  "expectedException");
            ExpectedException = expectedException;
            ExpectedMessage = message;
        }

        protected override void Verify(Exception exception)
        {
            if (exception.GetType() != ExpectedException)
            {
                base.RethrowIfAssertException(exception);
                string msg = string.Format("Test method {0}.{1} "
                    + "threw exception {2} but {3} was expected.",
                  base.TestContext.FullyQualifiedTestClassName, base.TestContext.TestName,
                  exception.GetType().FullName, ExpectedException.FullName);
                throw new Exception(msg);
            }
            if (exception.Message != ExpectedMessage)
            {
                string msg = string.Format("Exception {0} with the message {1} but the message {2} was expected.",
                  exception.GetType().FullName, exception.Message, ExpectedMessage);
                throw new Exception(msg);
            }
        }
    }
}
