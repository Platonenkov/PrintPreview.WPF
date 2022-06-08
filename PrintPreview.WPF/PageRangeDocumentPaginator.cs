using System;
using System.Windows;
using System.Windows.Documents;

namespace PrintPreview.WPF
{
    /// <summary>
    /// Encapsulates a DocumentPaginator and allows
    /// to paginate just some specific pages (a "PageRange")
    /// of the encapsulated DocumentPaginator
    /// (c) Thomas Claudius Huber 2010 
    /// http://www.thomasclaudiushuber.com
    /// </summary>
    public class PageRangeDocumentPaginator : DocumentPaginator
    {
        private readonly int _startIndex;
        private readonly int _endIndex;
        private readonly DocumentPaginator _paginator;
        public PageRangeDocumentPaginator(
          DocumentPaginator paginator,
          string pagerange)
        {
            _paginator = paginator;
            _paginator.ComputePageCount();

            var range = pagerange.Replace(" ", "");
            var ranges = range.Split(new[] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (!(ranges.Length > 0 && int.TryParse(ranges[0], out var from) && from > 0)) { from = 0; }
            if (!(ranges.Length >= 2 && int.TryParse(ranges[1], out var to) && to > from)) { to = 0; }

            if (from > 0 & to > 0)
            {
                _startIndex = Math.Min(from - 1, _paginator.PageCount - 1);
                _endIndex = Math.Min(to - 1, _paginator.PageCount - 1);
            }
            else if (from > 0 & to == 0)
            {
                _startIndex = Math.Min(from - 1, _paginator.PageCount - 1);
                _endIndex = Math.Min(from - 1, _paginator.PageCount - 1);
            }
            else
            {
                _startIndex = 0;
                _endIndex = _paginator.PageCount - 1;
            }
        }
        public override DocumentPage GetPage(int pageNumber)
        {
            return _paginator.GetPage(pageNumber + _startIndex);
        }

        public override bool IsPageCountValid => true;

        public override int PageCount
        {
            get
            {
                if (_startIndex > _paginator.PageCount - 1)
                    return 0;
                if (_startIndex > _endIndex)
                    return 0;

                return _endIndex - _startIndex + 1;
            }
        }

        public override Size PageSize
        {
            get => _paginator.PageSize;
            set => _paginator.PageSize = value;
        }

        public override IDocumentPaginatorSource Source => _paginator.Source;
    }
}
