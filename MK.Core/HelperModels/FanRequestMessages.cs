namespace MK.Service.Services
{
    /// <summary>
    /// Temporary raw-string messages for the Fan Request module.
    /// Move them to MK.Core.Localization.Resources.Messages later and replace
    /// "FanRequestMessages.X" with "Messages.X".
    /// </summary>
    internal static class FanRequestMessages
    {
        // Create
        public const string NameRequired = "Fan Request name is required.";
        public const string DescriptionRequired = "Fan Request description is required.";
        public const string ImageRequired = "Fan Request image is required.";
        public const string ImageTooLarge = "Image size must not exceed 5 MB.";
        public const string NameTooLong = "Fan Request name must not exceed {0} characters.";
        public const string DescriptionTooLong = "Fan Request description must not exceed {0} characters.";
        public const string AccountNotAllowedToCreate = "Your account is not allowed to create Fan Requests.";
        public const string CreatedAndPending = "Your Fan Request was submitted successfully and is waiting for admin review.";
        public const string RejectedByAi = "Your Fan Request was rejected by our automatic review. Reason: {0}";
        public const string RejectedByAiAndBanned = "Your Fan Request was rejected by our automatic review. Reason: {0} Your account has been banned because of repeated violations.";
        public const string AiRejectionFallbackReason = "The content violates the platform rules.";
        public const string AutoBanReason = "Automatically banned after {0} Fan Requests were rejected by AI moderation.";

        // Likes
        public const string NotAvailableForLikes = "This Fan Request is not available for likes.";
        public const string AlreadyLiked = "You have already liked this Fan Request.";
        public const string NotLiked = "You have not liked this Fan Request.";
        public const string LikedSuccessfully = "Fan Request liked successfully.";
        public const string UnlikedSuccessfully = "Like removed successfully.";

        // Status
        public const string InvalidStatusTransition = "Changing the status from {0} to {1} is not allowed.";
        public const string RejectionReasonRequired = "A rejection reason is required when rejecting a Fan Request.";
        public const string RejectionReasonTooLong = "Rejection reason must not exceed {0} characters.";
        public const string StatusChangedSuccessfully = "Fan Request status changed to {0} successfully.";

        // Notifications
        public const string AvailableOnSiteNotification = "The Fan Request you supported is now available on the website.";
    }
}
