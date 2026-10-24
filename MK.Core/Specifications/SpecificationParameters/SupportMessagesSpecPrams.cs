namespace MK.Core.Specifications.SpecificationParameters
{
    public class SupportMessagesSpecPrams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        /// <summary>
        /// null = all messages, true = handled only, false = not handled only.
        /// </summary>
        public bool? IsHandled { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }
    }
}
