using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MK.Repository.Specifications
{
    public class AuditLogSpecification : BaseSpecification<AuditLog>
    {
        public AuditLogSpecification(Expression<Func<AuditLog, bool>> criteria) : base(criteria)
        {
            AddInclude(x => x.User);
            AddOrderByDes(x => x.CreatedAt);
        }
    }
}
