using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using Core.Server.Client.Results;
using System.Collections.Generic;
using Core.Server.Shared.Resources;
using Unity;
using Core.Server.Tests.ResourceCreators.Interfaces;
using Core.Server.Tests.Utils;
using Core.Server.Test.ResourcesCreators.Interfaces;

namespace Core.Server.Tests.ResourceTests
{
    public abstract class TestsBase
    {
        [Dependency]
        public IResourcesClean ResourcesClean;

        [Dependency]
        public IResourcesIdsHolder ResourcesIdsHolder;

        [Dependency]
        public ICurrentUser CurrentUser;

        public virtual void TestInit()
        {

        }

        public virtual void Cleanup()
        {
            ResourcesClean.Clean();
            CurrentUser.Logout();
        }

        protected void ValidateList<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            for (int i = 0; i < expected.Count(); i++)
                Validate(expected.ElementAt(i), actual.ElementAt(i));
        }

        protected void ValidateNotEqual<T>(T expected, T actual)
        {
            try
            {
                Validate(expected, actual);
            }
            catch (AssertFailedException e)
            {
                return;
            }
            Assert.Fail();
        }
        protected void Validate<T>(T expected, T actual)
        {
            Assert.IsNotNull(actual);
            var properties = expected.GetType().GetProperties();
            foreach (var property in properties)
                ValidateProperty(expected, actual, property);
        }

        protected void AssertUnauthorized(ActionResult response)
        {
            Assert.IsTrue(response is UnauthorizedResult);
        }
        protected void AssertUnauthorized<T>(ActionResult<T> response)
        {
            Assert.IsInstanceOfType(response.Result, typeof(UnauthorizedResult));
        }

        protected void AssertNotFound(ActionResult response)
        {
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));
        }

        protected void AssertNotFound<T>(ActionResult<T> response)
        {
            Assert.IsInstanceOfType(response.Result, typeof(NotFoundResult));
        }

        protected void AssertNotErrors<T>(ActionResult<T> response)
        {
            Assert.IsNull(response.Result);
        }

        protected void AssertNoError<T>(ActionResult<T> response)
        {
            Assert.IsTrue(response.Value != null);
        }

        protected void AssertOk<T>(ActionResult<T> response)
        {
            Assert.IsTrue(response.IsSuccess);
        }

        protected void AssertOk(ActionResult response)
        {
            Assert.IsTrue(response is OkResult);
        }

        protected void AssertBadRequestReason<T, TReson>(ActionResult<T> response, TReson badRequestReason)
            where TReson : struct, Enum
        {
            AssertBadRequestReason(response.Result, badRequestReason);
        }

        protected void AssertBadRequestReason<TReson>(ActionResult response, TReson badRequestReason)
            where TReson : struct, Enum
        {
            var result = response as BadRequestResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(Convert.ToInt32(badRequestReason), result.Reason);
        }

        protected void AssertAreEqual<TResource>(TResource expected, ActionResult<IEnumerable<TResource>> actual)
            where TResource: Resource
        {
            Assert.IsTrue(actual.IsSuccess);
            Assert.AreEqual(1, actual.Value.Count());
            Validate(expected, actual.Value.First());
        }

        protected string GetRandomId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 24);
        }

        private void ValidateProperty(object expected, object actual, System.Reflection.PropertyInfo property)
        {
            var expectedValue = property.GetValue(expected);
            var actualProperty = actual.GetType().GetProperties().FirstOrDefault(p => p.Name == property.Name);
            var propertyType = actualProperty?.PropertyType;
            if (actualProperty != null && !propertyType.IsGenericType && propertyType != typeof(DateTime))
            {
                var actualValue = actualProperty.GetValue(actual);
                ValidateValue(expectedValue, actualProperty, propertyType, actualValue);
            }
        }

        private void ValidateValue(object expectedValue, System.Reflection.PropertyInfo actualProperty, Type propertyType, object actualValue)
        {
            if (actualValue == null) return;
            if (propertyType.IsPrimitive || propertyType == typeof(string))
                Assert.AreEqual(expectedValue, actualValue, "With Property " + actualProperty.Name);
            else if (actualValue.GetType() == typeof(string[]))
                Assert.IsTrue(((string[])expectedValue).SequenceEqual((string[])actualValue));
            else
                Validate(expectedValue, actualValue);
        }
    }
}
