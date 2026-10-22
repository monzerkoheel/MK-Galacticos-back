using MK.Core.Models;

namespace MK.Core.Specifications.SpecificationParameters
{
    public enum FanRequestSort
    {
        Newest,
        MostLikes,
        LeastLikes
    }

    public class FanRequestsSpecPrams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public string? Search { get; set; }
        public FanRequestStatus? Status { get; set; }
        public FanRequestSort Sort { get; set; } = FanRequestSort.Newest;

        public int PageIndex { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }
    }

    /// <summary>
    /// Public listing: same shape, but the specification only ever returns
    /// Approved / ComingSoon / AvailableOnSite requests (Search matches the request name only).
    /// </summary>
    public class PublicFanRequestsSpecPrams : FanRequestsSpecPrams
    {
    }
}
