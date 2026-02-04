using MyProject.Model.Gen;
using MyProject.PageServices;
using MyProject.SEO.Contracts;
using MyProject.SEO.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;

namespace MyProject.SEO
{
    public class SEOService : ISEOService
    {
        private readonly IUmbracoPageService _pageService;

        public SEOService(IUmbracoPageService pageService)
        {
            _pageService = pageService;
        }


        public SeoModel GetSeoModel(IPublishedContent content)
        {
            Website webSiteRoot = _pageService.GetRootNodeAs<Website>(Website.ModelTypeAlias)
                ?? throw new InvalidOperationException("Could not Find Website Root");

            return new SeoModel
            {
                Title = GetMetaTitle(content, webSiteRoot),
                Description = GetMetaDescription(content, webSiteRoot),
                Tags = GetMetaKeywords(content, webSiteRoot),
                SocialMediaShareImage = GetSocialShareImage(content, webSiteRoot),
                Url = content.Url(mode: UrlMode.Absolute) ?? webSiteRoot.Url(mode: UrlMode.Absolute) ?? string.Empty
            };
        }

        private string GetMetaTitle(IPublishedContent seoContent, Website root)
        {
            string metaTitlePropertyAlias = nameof(ISeoCompositionName.MetaTitle).ToFirstLower();
            string? metaTitle = seoContent.Value<string>(metaTitlePropertyAlias);
            string? rootMetaTitle = string.IsNullOrWhiteSpace(root.MetaTitle) ? root.Name : root.MetaTitle;

            return string.IsNullOrWhiteSpace(metaTitle)
                ? $"{seoContent.Name} - {rootMetaTitle}"
                : $"{metaTitle} - {rootMetaTitle}";
        }

        private string GetMetaDescription(IPublishedContent seoContent, Website root)
        {
            string metaDescriptionPropertyAlias = nameof(ISeoCompositionName.MetaDescription).ToFirstLower();
            string? metaDescription = seoContent.Value<string>(metaDescriptionPropertyAlias);
            if (!string.IsNullOrWhiteSpace(metaDescription))
            {
                return metaDescription;
            }

            IPublishedProperty? descriptionProperty = seoContent.Properties
                .FirstOrDefault(
                    x => x.Alias.Contains(SEOConstants.DescriptionPropertyAliasTarget, StringComparison.OrdinalIgnoreCase) && x.Alias != metaDescriptionPropertyAlias
                );

            string? computedDescription = descriptionProperty?.PropertyType?.EditorAlias switch
            {
                Constants.PropertyEditors.Aliases.TextArea or
                Constants.PropertyEditors.Aliases.TextBox => seoContent.Value<string>(descriptionProperty.Alias)?.Truncate(255),
                Constants.PropertyEditors.Aliases.RichText => seoContent.Value<IHtmlEncodedString>(descriptionProperty.Alias)?.ToHtmlString()?.StripHtml().StripNewLines().Truncate(255),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(computedDescription))
            {
                computedDescription = string.IsNullOrWhiteSpace(root.MetaDescription) ? seoContent.Name : root.MetaDescription;
            }

            return computedDescription;
        }

        private IEnumerable<string> GetMetaKeywords(IPublishedContent seoContent, Website root)
        {
            string metaKeywordsPropertyAlias = nameof(ISeoCompositionName.MetaKeywords).ToFirstLower();
            IEnumerable<string>? metaKeywords = seoContent.Value<IEnumerable<string>>(metaKeywordsPropertyAlias);
            if (metaKeywords is not null && metaKeywords.Any())
            {
                return metaKeywords;
            }

            IPublishedProperty? keywordsProperty = seoContent
                .Properties
                .FirstOrDefault(
                    x => {
                        bool isKeywordPropertyByAlias = x.Alias.Contains(SEOConstants.KeywordPropertyAliasTarget, StringComparison.OrdinalIgnoreCase);
                        bool isKeywordPropertyByType = x.PropertyType.EditorAlias == Constants.PropertyEditors.Aliases.Tags;
                        return (isKeywordPropertyByAlias || isKeywordPropertyByType) && x.Alias != metaKeywordsPropertyAlias;
                    }
                );

            IEnumerable<string>? computedValue = seoContent.Value<IEnumerable<string>>(keywordsProperty?.Alias ?? string.Empty);
            if (computedValue is null || !computedValue.Any())
            {
                computedValue = root.MetaKeywords ?? [];
            }

            return computedValue;
        }

        private string GetSocialShareImage(IPublishedContent seoContent, Website root)
        {
            string socialShareImagePropertyAlias = nameof(ISeoCompositionName.SocialShareImage).ToFirstLower();
            string? socialShareImageUrl = seoContent.Value<MediaWithCrops>(socialShareImagePropertyAlias)?.Url(mode: UrlMode.Absolute);
            if (!string.IsNullOrWhiteSpace(socialShareImageUrl))
            {
                return socialShareImageUrl;
            }

            IPublishedProperty? imageProperty = seoContent.Properties
                .FirstOrDefault(x => x.Alias.Contains(SEOConstants.ImagePropertyAliasTarget, StringComparison.OrdinalIgnoreCase) && x.Alias != socialShareImagePropertyAlias);

            MediaWithCrops? propertValue = seoContent.Value<MediaWithCrops>(imageProperty?.Alias ?? string.Empty);
            propertValue ??= seoContent.Value<IEnumerable<MediaWithCrops>>(imageProperty?.Alias ?? string.Empty)?
                    .FirstOrDefault();

            return propertValue?.Url(mode: UrlMode.Absolute)
                ?? root.SocialShareImage?.Url(mode: UrlMode.Absolute)
                ?? root?.WebsiteLogo?.Url(mode: UrlMode.Absolute)
                ?? string.Empty; ;
        }
    }
}
