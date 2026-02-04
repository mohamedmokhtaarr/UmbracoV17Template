using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MyProject.PageServices
{
    public interface IUmbracoPageService
    {
        string Encoded(string url);
        IPublishedContent? GetContentPage(string contentTypeAlias);
        TContentModel? GetContentPageAs<TContentModel>(string contentTypeAlias) where TContentModel : class, IPublishedContent;
        string? GetContentPageUrl(string contentTypeAlias, string? culture = null, UrlMode mode = UrlMode.Absolute);
        IPublishedContent? GetRootNode(string contentTypeAlias);
        TContentModel? GetRootNodeAs<TContentModel>(string contentTypeAlias) where TContentModel : class, IPublishedContent;
        /// <summary>
        /// Gets the encoded url for the root content node of type "Website"
        /// </summary>
        /// <param name="culture">the culture of the desired url</param>
        /// <param name="mode">Supports: Default, Absolute, Relative</param>
        /// <returns></returns>
        string GetHomeUrl(string? culture = null, UrlMode mode = UrlMode.Absolute);

        string GetWebSiteRootPageUrl(string contentTypeAlias, string? culture = default,
	        UrlMode mode = UrlMode.Default);
        string GetContentModelUrl(IContent content , string? culture = default);
    }
}