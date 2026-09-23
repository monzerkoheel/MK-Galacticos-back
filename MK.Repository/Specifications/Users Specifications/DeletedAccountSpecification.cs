using MK.Core.Models;

namespace MK.Repository.Specifications
{
    public class DeletedAccountSpecification : BaseSpecification<DeletedAccount>
    {
        public DeletedAccountSpecification() : base(x => x.DeleteDate <= DateTime.UtcNow)
        {
        }
    }
}
