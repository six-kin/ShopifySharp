using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace ShopifySharp.Tests.Services.Graph;

[Trait("Category", "Graph"), TestSubject(typeof(GraphService))]
public class GraphServiceErrorHandlingTests
{
    private readonly IRequestExecutionPolicy _policy = A.Fake<IRequestExecutionPolicy>(x => x.Strict());

    private readonly GraphService _sut;

    public GraphServiceErrorHandlingTests()
    {
        const string shopAccessToken = "some-shop-access-token";
        const string myShopifyUrl = "https://example.com";

        _sut = new GraphService(myShopifyUrl, shopAccessToken);
        _sut.SetExecutionPolicy(_policy);
    }

    private static RequestResult<string> MakeRequestResult(string responseJson)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var result = new RequestResult<string>(
            "some-request-info",
            response.Headers,
            responseJson,
            responseJson,
            "some-raw-link-header-value",
            HttpStatusCode.OK
        );
        return result;
    }

    [Theory]
    [CombinatorialData]
    public async Task WhenUserErrorsAreReturnedFromASingleOperation_ShouldThrowAccordingToGraphRequestUserErrorHandling(GraphRequestUserErrorHandling userErrorHandling)
    {
        // Setup
        const string userErrorsJson =
            """
            {
              "data": {
                "someOperation": {
                  "userErrors": [
                    {
                      "code": "some-code",
                      "message": "some-message",
                      "field": ["some-field"]
                    }
                  ]
                }
              }
            }
            """;
        var response = MakeRequestResult(userErrorsJson);

        A.CallTo(_policy)
            .WithReturnType<Task<RequestResult<string>>>()
            .Returns(response);

        // Act
        var act = () => _sut.PostAsync(new GraphRequest
        {
            Query = "some-graph-request-query",
            UserErrorHandling = userErrorHandling
        });

        // Assert
        if (userErrorHandling == GraphRequestUserErrorHandling.Throw)
        {
            await act.Should()
                .ThrowAsync<ShopifyException>("{0} == {1}", nameof(userErrorHandling), GraphRequestUserErrorHandling.Throw);
        }
        else
        {
            await act.Should()
                .NotThrowAsync("{0} != {1} (actual value == {2})", nameof(userErrorHandling), GraphRequestUserErrorHandling.Throw, userErrorHandling);
        }
    }

    [Theory]
    [CombinatorialData]
    public async Task WhenUserErrorsAreReturnedFromMultipleOperations_ShouldThrowAccordingToGraphRequestUserErrorHandling(GraphRequestUserErrorHandling userErrorHandling)
    {
        // Setup
        const string userErrorsJson =
            """
            {
              "data": {
                "someOperation1": {
                  "userErrors": [
                    {
                      "code": "some-code-1",
                      "message": "some-message-1",
                      "field": ["some-field-1"]
                    }
                  ]
                },
                "someOperation2": {
                  "userErrors": [
                    {
                      "code": "some-code-2",
                      "message": "some-message-2",
                      "field": ["some-field-2"]
                    }
                  ]
                }
              }
            }
            """;
        var response = MakeRequestResult(userErrorsJson);

        A.CallTo(_policy)
            .WithReturnType<Task<RequestResult<string>>>()
            .Returns(response);

        // Act
        var act = () => _sut.PostAsync(new GraphRequest
        {
            Query = "some-graph-request-query",
            UserErrorHandling = userErrorHandling
        });

        // Assert
        if (userErrorHandling == GraphRequestUserErrorHandling.Throw)
        {
            await act.Should()
                .ThrowAsync<ShopifyException>("{0} == {1}", nameof(userErrorHandling), GraphRequestUserErrorHandling.Throw);
        }
        else
        {
            await act.Should()
                .NotThrowAsync("{0} != {1} (actual value == {2})", nameof(userErrorHandling), GraphRequestUserErrorHandling.Throw, userErrorHandling);
        }
    }

    [Theory]
    [CombinatorialData]
    public async Task WhenNoUserErrorsAreReturned_ShouldNotThrow(GraphRequestUserErrorHandling userErrorHandling)
    {
        // Setup
        const string expectedPropertyName = "foo";
        const int expectedPropertyValue = 7;
        var responseJson =
            $$"""
              {
                "data": {
                  "{{expectedPropertyName}}": {{expectedPropertyValue}}
                }
              }
              """;
        var response = MakeRequestResult(responseJson);

        A.CallTo(_policy)
            .WithReturnType<Task<RequestResult<string>>>()
            .Returns(response);

        // Act
        var act = async () => await _sut.PostAsync(new GraphRequest
        {
            Query = "some-graph-request-query",
            UserErrorHandling = userErrorHandling
        });

        // Assert
        await act.Should()
            .NotThrowAsync();

        var result = await act();
        result.RootElement.GetProperty("foo")
            .GetInt32()
            .Should()
            .Be(expectedPropertyValue);
    }
}
