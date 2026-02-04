using Umbraco.Cms.Core.Models;

namespace MyProject.SiteSettings
{
    public interface ISiteSettingsService
    {
        MediaWithCrops? DefaultWebsiteImage { get; }
        string? DefaultWebsiteImageUrl { get; }
        string? GoogleAnalyticsTrackingCode { get; }
    }
}