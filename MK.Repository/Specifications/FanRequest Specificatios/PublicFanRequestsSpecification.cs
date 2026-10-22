using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using System.Linq.Expressions;

namespace MK.Repository.Specifications.FanRequests_Specifications
{
    public class PublicFanRequestsSpecification : BaseSpecification<FanRequest>
    {
        public PublicFanRequestsSpecification(PublicFanRequestsSpecPrams prams) : base(BuildCriteria(prams))
        {
            AddInclude(x => x.User);

            switch (prams.Sort)
            {
                case FanRequestSort.MostLikes:
                    AddOrderByDes(x => x.LikesCount);
                    break;
                case FanRequestSort.LeastLikes:
                    AddOrderBy(x => x.LikesCount);
                    break;
                default:
                    AddOrderByDes(x => x.CreatedAt);
                    break;
            }

            AddSecondOrderBy(x => x.Id);
        }

        private static Expression<Func<FanRequest, bool>> BuildCriteria(PublicFanRequestsSpecPrams prams)
        {
            var search = prams.Search?.Trim();
            var hasSearch = !string.IsNullOrWhiteSpace(search);
            var hasStatus = prams.Status.HasValue;
            var status = prams.Status.GetValueOrDefault();

            return x =>
                (x.Status == FanRequestStatus.Approved ||
                 x.Status == FanRequestStatus.ComingSoon ||
                 x.Status == FanRequestStatus.AvailableOnSite) &&
                (!hasSearch || x.Name.Contains(search!)) &&
                (!hasStatus || x.Status == status);
        }
    }
}
