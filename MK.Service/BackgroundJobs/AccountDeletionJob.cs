using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Models;
using MK.Repository.Specifications;

namespace MK.Service.BackgroundJobs
{
    public class AccountDeletionJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountDeletionJob> _logger;

        public AccountDeletionJob(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,ILogger<AccountDeletionJob> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var deletedUsers = await _unitOfWork.Repository<DeletedAccount>().ListAsync(new DeletedAccountSpecification());

            foreach (var deletedUser in deletedUsers)
            {
                var user = await _userManager.FindByIdAsync(deletedUser.UserId);

                if (user is null)
                {
                    _logger.LogWarning("User with ID {UserId} was not found during account deletion.", deletedUser.UserId);

                    _unitOfWork.Repository<DeletedAccount>().Delete(deletedUser);
                    continue;
                }

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _unitOfWork.Repository<DeletedAccount>().Delete(deletedUser);

                    _logger.LogInformation("User with ID {UserId} was permanently deleted.", deletedUser.UserId);
                }
                else
                {
                    _logger.LogError(
                        "Failed to permanently delete user with ID {UserId}. Errors: {Errors}",
                        deletedUser.UserId,
                        string.Join(", ", result.Errors.Select(x => x.Description)));
                }
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}

