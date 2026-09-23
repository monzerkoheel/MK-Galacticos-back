using MK.Core.HelperModels;

namespace MK.Core.Models
{
    public class DeletedAccount : ModelBase
    {
        public string UserId { get; set; } = null!;

        public DateTime DeletedAt { get; set; }
        public DateTime DeleteDate { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
