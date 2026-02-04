using MyProject.SEO.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MyProject.SEO.Contracts
{
    public interface ISEOService
    {
        public SeoModel GetSeoModel(IPublishedContent content);

    }
}
