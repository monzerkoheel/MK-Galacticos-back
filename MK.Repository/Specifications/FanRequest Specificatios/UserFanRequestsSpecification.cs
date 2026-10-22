using MK.Core.Models;

namespace MK.Repository.Specifications.FanRequests_Specifications
{
    public class UserFanRequestsSpecification : BaseSpecification<FanRequest>
    {
        public UserFanRequestsSpecification(string userId) : base(x => x.UserId == userId)
        {
            AddOrderByDes(x => x.CreatedAt);
            AddSecondOrderBy(x => x.Id);
        }
    }
}
