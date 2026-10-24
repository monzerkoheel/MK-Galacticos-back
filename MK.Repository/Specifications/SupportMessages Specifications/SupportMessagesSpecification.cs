using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using System.Linq.Expressions;

namespace MK.Repository.Specifications.SupportMessages_Specifications
{
    /// <summary>
    /// Listing: optional IsHandled filter, newest first.
    /// Pagination is NOT applied here (call ApplyPagination after counting).
    /// </summary>
    public class SupportMessagesSpecification : BaseSpecification<SupportMessage>
    {
        public SupportMessagesSpecification(SupportMessagesSpecPrams prams) : base(BuildCriteria(prams))
        {
            AddInclude(x => x.HandledByUser!);
            AddOrderByDes(x => x.CreatedAt);
            AddSecondOrderBy(x => x.Id);
        }

        private static Expression<Func<SupportMessage, bool>> BuildCriteria(SupportMessagesSpecPrams prams)
        {
            var hasFilter = prams.IsHandled.HasValue;
            var isHandled = prams.IsHandled.GetValueOrDefault();

            return x => !hasFilter || x.IsHandled == isHandled;
        }
    }
}
