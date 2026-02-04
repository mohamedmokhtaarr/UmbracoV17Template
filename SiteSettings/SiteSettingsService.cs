using MyProject.Model.Gen;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MyProject.SiteSettings
{
    internal class SiteSettingsService : ISiteSettingsService
    {
        private IPublishedContent? Settings { get; }
        private ILogger<SiteSettingsService> Logger { get; }

        public SiteSettingsService(IPublishedContentQuery publishedContentQuery, ILogger<SiteSettingsService> logger)
        {
            Logger = logger;
            Settings = publishedContentQuery
                .ContentAtRoot()
                .FirstOrDefault(x => x.IsDocumentType(SiteSettingsDocument.ModelTypeAlias));

            if (Settings is null)
            {
                Logger.LogWarning("Failed to initialize Site Settings, no Object was found at root");
            }
        }

        #region Public Properties
        public MediaWithCrops? DefaultWebsiteImage => GetSiteSettingValue(Settings?.Value<MediaWithCrops>(nameof(DefaultWebsiteImage)), nameof(DefaultWebsiteImage), null);
        public string? DefaultWebsiteImageUrl => GetSiteSettingValue(Settings?.Value<MediaWithCrops>(nameof(DefaultWebsiteImage))?.Url(), nameof(DefaultWebsiteImage) + "Url", null);
        public string? GoogleAnalyticsTrackingCode => GetSiteSettingValue(Settings?.Value<string>(nameof(GoogleAnalyticsTrackingCode)), nameof(GoogleAnalyticsTrackingCode), null);

        #endregion

        #region Private Methods

        private T GetSiteSettingValue<T>(T? settingValue, string keyName, T defaultValue) where T : class?
        {
            if (settingValue == default(T?))
            {
                Logger.LogWarning("Site setting with keyName: {keyName} doesn't exist", keyName);
            }

            return settingValue ?? defaultValue;
        }
        #endregion
    }
}
