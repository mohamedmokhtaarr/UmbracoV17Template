using System.Diagnostics.CodeAnalysis;
using MyProject.Model.Gen;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace MyProject.PageServices
{
    internal class UmbracoPageService : IUmbracoPageService
    {
        private readonly IPublishedContentQuery _publishedContent;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UrlSegmentProviderCollection _urlSegments;
        private readonly IContentService _contentService;

        public UmbracoPageService(IPublishedContentQuery publishedContent, IShortStringHelper shortStringHelper,
            UrlSegmentProviderCollection urlSegments, IContentService contentService)
        {
            _publishedContent = publishedContent;
            _shortStringHelper = shortStringHelper;
            _urlSegments = urlSegments;
            _contentService = contentService;
        }

        public string GetHomeUrl(string? culture = default, UrlMode mode = UrlMode.Absolute)
        {
            IPublishedContent? homePageContentModel = _publishedContent.ContentAtRoot().First(x => x.IsDocumentType(nameof(Website)));
            if (homePageContentModel is null)
            {
                throw new ArgumentNullException(nameof(homePageContentModel));
            }

            return Encoded( homePageContentModel.Url(culture, mode) );
        }        

        public string GetWebSiteRootPageUrl(string contentTypeAlias, string? culture = default, UrlMode mode = UrlMode.Default) 
        {
            IPublishedContent? page = _publishedContent.ContentAtRoot().OfType<Website>().FirstOrDefault()?.FirstChildOfType(contentTypeAlias);
            return page != null ? page.Url(culture, mode) : string.Empty;
        }

        public IPublishedContent? GetContentPage(string contentTypeAlias)
        {
            if (string.IsNullOrWhiteSpace(contentTypeAlias))
            {
                throw new ArgumentException($"'{nameof(contentTypeAlias)}' cannot be null or whitespace.", nameof(contentTypeAlias));
            }

            if (!EnsureRootContainer(out IPublishedContent? rootNode))
            {
                throw new ArgumentNullException(nameof(rootNode), "Could not Get Root Node");
            }

            return rootNode.FirstChild(x => x.IsDocumentType(contentTypeAlias));
        }

        public TContentModel? GetContentPageAs<TContentModel>(string contentTypeAlias)
            where TContentModel : class, IPublishedContent
        {
            IPublishedContent? content = GetContentPage(contentTypeAlias);
            if(content is null)
            {
                return default;
            }

            return content as TContentModel;
        }

        public string? GetContentPageUrl(string contentTypeAlias, string? culture = default, UrlMode mode = UrlMode.Absolute)
        {
            IPublishedContent? content = GetContentPage(contentTypeAlias);
            if (content is null)
            {
                return default;
            }

            return Encoded( content.Url(culture, mode) );
        }

        public string Encoded(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(url);
            }

            var uri = new Uri(url);
            return uri.AbsoluteUri;
        }

        public IPublishedContent? GetRootNode(string contentTypeAlias)
        {
            return _publishedContent.ContentAtRoot().FirstOrDefault(x => x.IsDocumentType(contentTypeAlias));
        }

        public TContentModel? GetRootNodeAs<TContentModel>(string contentTypeAlias) where TContentModel : class, IPublishedContent
        {
            return GetRootNode(contentTypeAlias) as TContentModel;
        }

        private bool EnsureRootContainer([NotNullWhen(true)] out IPublishedContent? rootNode)
        {
            rootNode = _publishedContent.ContentAtRoot()
                .FirstOrDefault(x => x.IsDocumentType(Website.ModelTypeAlias));
            return rootNode is not null;
        }

        public string GetContentModelUrl(IContent model , string? culture = default)
        {
            string? contentUrlSegment = model.GetUrlSegment(_shortStringHelper, _urlSegments, culture);
            IContent? parent = _contentService.GetById(model.ParentId);
            if (parent is null || parent.ContentType.Alias == Website.ModelTypeAlias)
            {
                return contentUrlSegment;
            }

            string parentUrlSegment = GetContentModelUrl(parent, culture);
            return  $"{parentUrlSegment}/{contentUrlSegment}";
        }
    }
}
