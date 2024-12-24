namespace ShopifySharp;

public record GraphRequestCostThrottleStatusExtension
{
    public double RestoreRate { get; set; }

    public double CurrentlyAvailable { get; set; }

    public double MaximumAvailable { get; set; }
}
