﻿using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Net.Http.OData;
using Net.Http.WebApi.OData;
using Xunit;

namespace MicroLite.Extensions.WebApi.OData.Tests.Integration
{
    public class MicroLiteODataApiController_GetPropertyTests
    {
        public class InvalidEntityKey_ValidProperty : IntegrationTest
        {
            private readonly HttpResponseMessage _httpResponseMessage;

            public InvalidEntityKey_ValidProperty()
            {
                MockSession
                    .Setup(x => x.SingleAsync<dynamic>(It.Is<SqlQuery>(s => s.CommandText == "SELECT Name FROM Customers WHERE (Id = ?)")))
                    .Returns(Task.FromResult(default(object)));

                _httpResponseMessage = HttpClient.GetAsync("http://server/odata/Customers(122)/Name").Result;
            }

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ODataVersion()
                => Assert.Equal("4.0", _httpResponseMessage.Headers.GetValues(ODataResponseHeaderNames.ODataVersion).Single());

            [Fact]
            [Trait("Category", "Integration")]
            public void DoesNotContain_Content()
                => Assert.Null(_httpResponseMessage.Content);

            [Fact]
            [Trait("Category", "Integration")]
            public void StatusCode_NotFound()
                => Assert.Equal(HttpStatusCode.NotFound, _httpResponseMessage.StatusCode);
        }

        public class ValidEntityKey_InvalidValidProperty : IntegrationTest
        {
            private readonly HttpResponseMessage _httpResponseMessage;

            public ValidEntityKey_InvalidValidProperty()
            {
                _httpResponseMessage = HttpClient.GetAsync("http://server/odata/Customers(122)/Foo").Result;
            }

            [Fact]
            [Trait("Category", "Integration")]
            public async Task Contains_Content_PropertyValue()
            {
                Assert.NotNull(_httpResponseMessage.Content);

                string result = await _httpResponseMessage.Content.ReadAsStringAsync();

                Assert.Equal("{\"error\":{\"code\":\"400\",\"message\":\"The type 'MicroLite.Extensions.WebApi.OData.Tests.TestEntities.Customer' does not contain a property named 'Foo'.\",\"target\":\"MicroLite.Extensions.WebApi.OData.Tests.TestEntities.Customer\"}}", result);
            }

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ContentType_ApplicationJson()
                => Assert.Equal("application/json", _httpResponseMessage.Content.Headers.ContentType.MediaType);

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ContentType_Parameter_ODataMetadata()
                => Assert.Equal("minimal", _httpResponseMessage.Content.Headers.ContentType.Parameters.Single(x => x.Name == ODataMetadataLevelExtensions.HeaderName).Value);

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ODataVersion()
                => Assert.Equal("4.0", _httpResponseMessage.Headers.GetValues(ODataResponseHeaderNames.ODataVersion).Single());

            [Fact]
            [Trait("Category", "Integration")]
            public void StatusCode_BadRequest()
                => Assert.Equal(HttpStatusCode.BadRequest, _httpResponseMessage.StatusCode);
        }

        public class ValidEntityKey_ValidProperty_Metadata_Minimal : IntegrationTest
        {
            private readonly HttpResponseMessage _httpResponseMessage;

            public ValidEntityKey_ValidProperty_Metadata_Minimal()
            {
                dynamic entity = new ExpandoObject();
                entity.Name = "John Smith";

                MockSession
                    .Setup(x => x.SingleAsync<dynamic>(It.Is<SqlQuery>(s => s.CommandText == "SELECT Name FROM Customers WHERE (Id = ?)")))
                    .Returns(Task.FromResult((object)entity));

                _httpResponseMessage = HttpClient.GetAsync("http://server/odata/Customers(122)/Name").Result;
            }

            [Fact]
            [Trait("Category", "Integration")]
            public async Task Contains_Content_PropertyValue()
            {
                Assert.NotNull(_httpResponseMessage.Content);

                string result = await _httpResponseMessage.Content.ReadAsStringAsync();

                Assert.Equal("{\"@odata.context\":\"http://server/odata/$metadata#Customers(122)/Name\",\"value\":\"John Smith\"}", result);
            }

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ContentType_ApplicationJson()
                => Assert.Equal("application/json", _httpResponseMessage.Content.Headers.ContentType.MediaType);

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ContentType_Parameter_ODataMetadata()
                => Assert.Equal("minimal", _httpResponseMessage.Content.Headers.ContentType.Parameters.Single(x => x.Name == "odata.metadata").Value);

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ODataVersion()
                => Assert.Equal("4.0", _httpResponseMessage.Headers.GetValues(ODataResponseHeaderNames.ODataVersion).Single());

            [Fact]
            [Trait("Category", "Integration")]
            public void StatusCode_OK()
                => Assert.Equal(HttpStatusCode.OK, _httpResponseMessage.StatusCode);
        }

        public class ValidEntityKey_ValidProperty_Metadata_None : IntegrationTest
        {
            private readonly HttpResponseMessage _httpResponseMessage;

            public ValidEntityKey_ValidProperty_Metadata_None()
            {
                dynamic entity = new ExpandoObject();
                entity.Name = "John Smith";

                MockSession.Setup(x => x.SingleAsync<dynamic>(It.Is<SqlQuery>(s => s.CommandText == "SELECT Name FROM Customers WHERE (Id = ?)"))).Returns(Task.FromResult((object)entity));

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://server/odata/Customers(122)/Name");
                httpRequestMessage.Headers.Add("Accept", "application/json;odata.metadata=none");

                _httpResponseMessage = HttpClient.SendAsync(httpRequestMessage).Result;
            }

            [Fact]
            [Trait("Category", "Integration")]
            public async Task Contains_Content_PropertyValue()
            {
                Assert.NotNull(_httpResponseMessage.Content);

                string result = await _httpResponseMessage.Content.ReadAsStringAsync();

                Assert.Equal("{\"value\":\"John Smith\"}", result);
            }

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ContentType_ApplicationJson()
                => Assert.Equal("application/json", _httpResponseMessage.Content.Headers.ContentType.MediaType);

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ContentType_Parameter_ODataMetadata()
                => Assert.Equal("none", _httpResponseMessage.Content.Headers.ContentType.Parameters.Single(x => x.Name == "odata.metadata").Value);

            [Fact]
            [Trait("Category", "Integration")]
            public void Contains_Header_ODataVersion()
                => Assert.Equal("4.0", _httpResponseMessage.Headers.GetValues(ODataResponseHeaderNames.ODataVersion).Single());

            [Fact]
            [Trait("Category", "Integration")]
            public void StatusCode_OK()
                => Assert.Equal(HttpStatusCode.OK, _httpResponseMessage.StatusCode);
        }
    }
}
