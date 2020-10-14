using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using Core.Server.Tests.ResourceCreators.Interfaces;
using Core.Server.Tests.Unity;
using Core.Server.Tests.Utils;
using Core.Server.Client.Interfaces;
using Core.Server.Shared.Resources;
using Core.Server.Client.Results;
using Core.Server.Shared.Errors;

namespace Core.Server.Tests.ResourceTests
{
    public abstract class TestsBase
    {
        protected Random Random;
        protected IResourcesIdHolder ResourcesHolder;
        protected ITestsUnityContainer TestsUnityContainer;
        protected ITokenHandler TokenHandler;
        protected IConfigHandler ConfigHandler;
        public TestsBase()
        {
            Random = new Random();
            TestsUnityContainer = new TestsUnityContainer();
            ResourcesHolder = TestsUnityContainer.Resolve<IResourcesIdHolder>();
            TokenHandler = TestsUnityContainer.Resolve<ITokenHandler>();
            ConfigHandler = TestsUnityContainer.Resolve<IConfigHandler>();
        }

        protected TIClient GetClient<TIClient>() where TIClient : IClientBase
        {
            var client = TestsUnityContainer.Resolve<TIClient>();
            if (client.ServerUrl == null)
                client.ServerUrl = ConfigHandler.Config.ServerUrl;
            if (client.Token == null)
            {
                TokenHandler.OnTokenChange += (s, t) => client.Token = t;
                client.Token = TokenHandler.Token;
            }
            return client;
        }

        protected void Validate(object expected, object actual)
        {
            Assert.IsNotNull(actual);
            var properties = expected.GetType().GetProperties();
            foreach (var property in properties)
            {
                var expectedValue = property.GetValue(expected);
                var actualProperty = actual.GetType().GetProperties().FirstOrDefault(p => p.Name == property.Name);
                var propertyType = actualProperty?.PropertyType;
                if (actualProperty != null && !propertyType.IsGenericType && propertyType != typeof(DateTime))
                {
                    var actualValue = actualProperty.GetValue(actual);
                    if (propertyType.IsPrimitive || propertyType == typeof(string))
                        Assert.AreEqual(expectedValue, actualValue, "With Property " + actualProperty.Name);
                    else if (actualValue.GetType() == typeof(string[]))
                        Assert.IsTrue(((string[])expectedValue).SequenceEqual((string[])actualValue));
                    else
                        Validate(expectedValue, actualValue);
                }
            }
        }

        protected TFResource GetExistingOrNew<TFResource>()
            where TFResource : Resource
        {
            return ResourcesHolder.GetLastOrCreate<TFResource>().Value;
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

        protected string RandomId
        {
            get
            {
                var buffer = new byte[12];
                Random.NextBytes(buffer);
                return string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            }
        }

        protected TCreateResource GetRandomCreateResource<TCreateResource, TUpdateResource, TResource>() 
            where TCreateResource : CreateResource, new()
            where TUpdateResource : UpdateResource
            where TResource : Resource
        {
            var creator = TestsUnityContainer.Resolve<IResourceCreator<TCreateResource, TUpdateResource,TResource>>();
            return creator.GetRandomCreateResource();
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
    }
}
