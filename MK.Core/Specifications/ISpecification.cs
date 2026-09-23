using MK.Core.HelperModels;
using System.Linq.Expressions;


namespace MK.Core.Specifications
{
    public interface ISpecification<T> where T : ModelBase
    {
        public Expression<Func<T, bool>> critria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; }
        public List<string> IncludeStrings { get; set; }
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDes { get; set; }
        public Expression<Func<T, object>> ThenOrder { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool IsPaginationEnabled { get; set; }
        public bool IsSplitQueryEnabled { get; set; }
    }
}
