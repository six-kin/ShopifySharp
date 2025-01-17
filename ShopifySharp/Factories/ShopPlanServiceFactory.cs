#nullable enable
// Notice:
// This class is auto-generated from a template. Please do not edit it or change it directly.

using ShopifySharp.Credentials;
using ShopifySharp.Utilities;
using ShopifySharp.Services;

namespace ShopifySharp.Factories;

public interface IShopPlanServiceFactory
{
    /// Creates a new instance of the <see cref="IShopPlanService" /> with the given credentials.
    /// <param name="shopDomain">The shop's *.myshopify.com URL.</param>
    /// <param name="accessToken">An API access token for the shop.</param>
    IShopPlanService Create(string shopDomain, string accessToken);

    /// Creates a new instance of the <see cref="IShopPlanService" /> with the given credentials.
    /// <param name="credentials">Credentials for authenticating with the Shopify API.</param>
    IShopPlanService Create(ShopifyApiCredentials credentials);
}

public class ShopPlanServiceFactory(IRequestExecutionPolicy? requestExecutionPolicy = null, IShopifyDomainUtility? shopifyDomainUtility = null) : IShopPlanServiceFactory
{
    /// <inheritDoc />
    public virtual IShopPlanService Create(string shopDomain, string accessToken)
    {
        IShopPlanService service = shopifyDomainUtility is null ? new ShopPlanService(shopDomain, accessToken) : new ShopPlanService(shopDomain, accessToken, shopifyDomainUtility);

        if (requestExecutionPolicy is not null)
        {
            service.SetExecutionPolicy(requestExecutionPolicy);
        }

        return service;
    }

    /// <inheritDoc />
    public virtual IShopPlanService Create(ShopifyApiCredentials credentials) =>
        Create(credentials.ShopDomain, credentials.AccessToken);
}
