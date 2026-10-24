using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs.SupportMessages;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;
using MK.Repository.Specifications;
using MK.Repository.Specifications.SupportMessages_Specifications;
using MK.Service.HelperServices.EmailSevrices;
using System.Net.Mail;

namespace MK.Service.Services
{
    public class SupportMessageService : ISupportMessageService
    {
        private const int MaxMessageLength = 2000;
        private const string ConfirmationEmailSubject = "We Received Your Message - MK Galácticos";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;
        private readonly ILogger<SupportMessageService> _logger;

        public SupportMessageService(IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            ILogger<SupportMessageService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUserService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<OperationResultModel> CreateSupportMessageAsync(CreateSupportMessageDto dto)
        {
            var name = dto.Name?.Trim();
            var email = dto.Email?.Trim();
            var message = dto.Message?.Trim();
           
            if (message.Length > MaxMessageLength)
                throw new BadRequestException(string.Format(Messages.MessageTooLong, MaxMessageLength));

            var supportMessage = new SupportMessage
            {
                Name = name,
                Email = email,
                Message = message,
                UserId = _currentUser.UserId,
                IsHandled = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<SupportMessage>().AddAsync(supportMessage);

            if (await _unitOfWork.CompleteAsync() < 1)
                throw new Exception(Messages.SomethingWentWrong);

            try
            {
                var body = SupportMessageReceivedTemplate.Generate(name, message);
                await _emailService.SendEmailAsync(email, ConfirmationEmailSubject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send support message confirmation email. SupportMessageId={SupportMessageId}",
                    supportMessage.Id);
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.SentSuccessfully
            };
        }

        public async Task<PaginationModel<SupportMessageDto>> GetSupportMessagesAsync(SupportMessagesSpecPrams specPrams)
        {
            var repo = _unitOfWork.Repository<SupportMessage>();

            var spec = new SupportMessagesSpecification(specPrams);
            var totalCount = await repo.CountAsync(spec);
            spec.ApplyPagination((specPrams.PageIndex - 1) * specPrams.PageSize, specPrams.PageSize);

            var items = await repo.ListAsync(spec);
            var data = _mapper.Map<IReadOnlyList<SupportMessageDto>>(items);

            return new PaginationModel<SupportMessageDto>(specPrams.PageIndex, specPrams.PageSize, totalCount, data);
        }

        public async Task<SupportMessageDto> GetSupportMessageByIdAsync(int messageId)
        {
            var supportMessage = await GetWithHandlerAsync(messageId);
            return _mapper.Map<SupportMessageDto>(supportMessage);
        }

        public async Task<OperationResultModel> ChangeHandledStateAsync(int messageId, ChangeSupportMessageHandledStateDto dto)
        {
            var currentUserId = _currentUser.UserId
                ?? throw new BadRequestException(Messages.AccountNotFound);

            var supportMessage = await GetWithHandlerAsync(messageId);

            if (dto.IsHandled)
            {
                // Someone already took it: nobody else can take it.
                if (supportMessage.IsHandled)
                    throw new BadRequestException(
                        supportMessage.HandledByUser is null
                            ? Messages.AlreadyHandledUnknown
                            : string.Format(Messages.AlreadyHandled, supportMessage.HandledByUser.FullName));

                supportMessage.IsHandled = true;
                supportMessage.HandledByUserId = currentUserId;
                supportMessage.HandledAt = DateTime.UtcNow;
            }
            else
            {
                if (!supportMessage.IsHandled)
                    throw new BadRequestException(Messages.NotHandled);

                // Only the person who handled it, a Manager or the Owner can undo it.
                var isManagerOrOwner = _currentUser.Role is "Owner" or "Manager";
                if (!isManagerOrOwner && supportMessage.HandledByUserId != currentUserId)
                    throw new ForbiddenException(Messages.OnlyHandlerOrManagers);

                supportMessage.IsHandled = false;
                supportMessage.HandledByUser = null;
                supportMessage.HandledByUserId = null;
                supportMessage.HandledAt = null;
            }

            // The entity is already tracked (loaded above), so no repo.Update(...) is needed.
            // Calling Update would also mark the loaded HandledByUser as modified.
            try
            {
                if (await _unitOfWork.CompleteAsync() < 1)
                    throw new Exception(Messages.SomethingWentWrong);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Two people pressed the button at the same moment: the second one loses.
                throw new BadRequestException(Messages.ConcurrentChange);
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = dto.IsHandled
                    ? Messages.MarkedAsHandled
                    : Messages.MarkedAsNotHandled
            };
        }
        public async Task<OperationResultModel> ReplyToSupportMessageAsync(int messageId, ReplyToSupportMessageDto dto)
        {
            var reply = dto.Reply?.Trim();

            var supportMessage = await _unitOfWork.Repository<SupportMessage>().GetByIdAsync(messageId);
            if (supportMessage is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(SupportMessage)));

            // Unlike the confirmation email, a failure here must NOT be hidden:
            // the staff member has to know that the user did not receive the reply.
            try
            {
                var body = SupportMessageReplyTemplate.Generate(
                    supportMessage.Name, supportMessage.Message, supportMessage.CreatedAt, reply);

                await _emailService.SendEmailAsync(supportMessage.Email, "Response to Your Message - MK Galácticos", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send support reply email. SupportMessageId={SupportMessageId}",
                    supportMessage.Id);

                throw new Exception(Messages.ReplyEmailFailed);
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = Messages.ReplySentSuccessfully
            };
        }



        private async Task<SupportMessage> GetWithHandlerAsync(int messageId)
        {
            var spec = new BaseSpecification<SupportMessage>(x => x.Id == messageId);
            spec.AddInclude(x => x.HandledByUser!);

            var supportMessage = await _unitOfWork.Repository<SupportMessage>().GetFirstOrDefaultAsync(spec);
            if (supportMessage is null)
                throw new NotFoundException(string.Format(Messages.NotFound, nameof(SupportMessage)));

            return supportMessage;
        }
    }
}
