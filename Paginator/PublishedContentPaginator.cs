using System.Web;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MyProject.Paginator
{
    public class PublishedContentPaginator
    {
        /// <summary>
        ///     The Default Query Parameter Name for the paginator
        /// </summary>
        public static readonly string ParamName = "page";

        /// <summary>
        ///     The Default Page Size to be used anywhere if the passed PageSize was not passed
        /// </summary>
        public const int DefaultPageSize = 10;

        public int PageSize { get; }
        public int TotalPages { get; private set; }
        public int CurrentPage { get; }
        private QueryString PaginatorQuery { get; }
        private IEnumerable<IPublishedContent> Items { get; set; }
        public bool IsFirstPage => CurrentPage == 1;
        public bool IsLastPage => CurrentPage == TotalPages;
        public bool HasPages => TotalPages > 1;
        public int TotalItemsCount => Items.Count();

        private PublishedContentPaginator(
            IEnumerable<IPublishedContent> items,
            QueryString paginatorQuery,
            int currentPage = 1,
            int pageSize = DefaultPageSize)
        {
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(items.Count() / (double)PageSize);
            Items = items;

            CurrentPage = currentPage;
            if (currentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }
            else if (currentPage < 1)
            {
                CurrentPage = 1;
            }
            PaginatorQuery = paginatorQuery;
        }

        public static PublishedContentPaginator Create(QueryString queryString, IEnumerable<IPublishedContent> items, int pageSize = DefaultPageSize)
        {
            string? pageNumber =
                queryString
                .Value?
                .Split('&')?
                .FirstOrDefault(q => q.Contains(ParamName))?
                .Split('=')
                .ElementAtOrDefault(1);

            int currentPage = string.IsNullOrWhiteSpace(pageNumber) ? 1 : int.Parse(pageNumber);

            return new PublishedContentPaginator(items, queryString, currentPage, pageSize is default(int) ? DefaultPageSize : pageSize);
        }

        /// <summary>
        ///     Paginates on the collection using it's internal state
        /// </summary>
        /// <returns>
        ///     A Collection of Items that should be displayed in the current Page
        /// </returns>
        public IEnumerable<IPublishedContent> Paginate()
        {
            return Items.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        }

        public IEnumerable<IPublishedContent> PaginateWhere(Func<IPublishedContent, bool> predicate)
        {
            return Items.Where(predicate).Skip((CurrentPage - 1) * PageSize).Take(PageSize);

        }

        //public void SetContent(IEnumerable<IPublishedContent> contents)
        //{
        //    TotalPages = (int)Math.Ceiling(contents.Count() / (double)PageSize);
        //    Items = contents;
        //}

        public bool Any() => Items.Any();

        public string GetPageLink(int pageNumber)
        {
            var queryString = HttpUtility.ParseQueryString(PaginatorQuery.ToString());
            if (queryString is null || queryString.Count == default)
            {
                return QueryString.Create(ParamName, pageNumber.ToString()).ToString();
            }

            if (queryString.AllKeys.Contains(ParamName))
            {
                queryString.Set(ParamName, pageNumber.ToString());
            }
            else
            {
                queryString.Add(ParamName, pageNumber.ToString());
            }

            return $"?{queryString}";
        }
    }

}
