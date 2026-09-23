using MK.Core.Models;

namespace MK.Core.Specifications.SpecificationParameters
{
    public class GetManagementUsersSpecPrams
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public UserStatus? Status { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
