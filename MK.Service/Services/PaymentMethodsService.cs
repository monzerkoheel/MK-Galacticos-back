using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.LegendCards;
using MK.Core.DTOs.PaymentMethods;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;
using MK.Service.HelperServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace MK.Service.Services
{
    public class PaymentMethodsService : IPaymentMethodsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        public PaymentMethodsService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<OperationResultModel> AddPaymentMethodAsync(CreatePaymentMethodDto dto)
        {
            if (!dto.Translations.Any() || dto.Translations.Count() < 3)
                throw new BadRequestException(Messages.AllTranslationsRequired);

            var duplicatedLanguages = dto.Translations.GroupBy(x => x.Language).Any(x => x.Count() > 1);
            if (duplicatedLanguages)
                throw new BadRequestException(Messages.DuplicateTranslations);

            var translationNames = dto.Translations.Select(x => x.Name).ToList();
            var translationSpec = new BaseSpecification<PaymentMethodTranslation>(x => translationNames.Contains(x.Name));

            if (await _unitOfWork.Repository<PaymentMethodTranslation>().AnyAsync(translationSpec))
                throw new BadRequestException(string.Format(Messages.AlreadyExists, "Payment Method Name"));

            var imageUrl = await _fileService.SaveImageAsync(dto.IconImage, "uploads/images/paymentMethods");
            var paymentMethod = new PaymentMethod
            {
                IconImageUrl = imageUrl,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Translations = dto.Translations.Select(x => new PaymentMethodTranslation
                {
                    Language = x.Language,
                    Name = x.Name
                }).ToList()
            };

            try
            {
                await _unitOfWork.Repository<PaymentMethod>().AddAsync(paymentMethod);
                var result = await _unitOfWork.CompleteAsync();
                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);
            }
            catch (Exception)
            {
                _fileService.DeleteImageIfExists(imageUrl);
                throw;
            }

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.Added_Successfully, nameof(PaymentMethod))
            };
        }

        public async Task<OperationResultModel> UpdatePaymentMethodAsync(int methodId, UpdatePaymentMethodDto dto)
        {
            if (dto.IconImage is null && dto.Status is null && !dto.Translations.Any())
                throw new BadRequestException(Messages.NoDataToUpdate);

            var methodSpec = new BaseSpecification<PaymentMethod>(x => x.Id == methodId);
            methodSpec.AddInclude(x => x.Translations);
            var method = await _unitOfWork.Repository<PaymentMethod>().GetFirstOrDefaultAsync(methodSpec);
            if (method is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Payment Method"));

            if (dto.Status is not null)
                method.Status = dto.Status.Value;

            if (dto.Translations.Any())
            {
                foreach (var translationDto in dto.Translations)
                {
                    var translation = method.Translations.FirstOrDefault(x => x.Language == translationDto.Language);
                    if (translation is null)
                        throw new BadRequestException(string.Format(Messages.NotFound, $"Method {translationDto.Language} translation"));

                    translation.Name = translationDto.Name;
                }
            }

            string? newImageUrl = null;
            string? oldImageUrl = null;
            if (dto.IconImage is not null)
            {
                newImageUrl = await _fileService.SaveImageAsync(dto.IconImage, "uploads/images/paymentMethods");
                oldImageUrl = method.IconImageUrl;
                method.IconImageUrl = newImageUrl;
            }
            try
            {
                method.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<PaymentMethod>().Update(method);
                var result = await _unitOfWork.CompleteAsync(); 
                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);
                _fileService.DeleteImageIfExists(oldImageUrl);

            }
            catch
            {
                _fileService.DeleteImageIfExists(newImageUrl);
                throw;
            }
            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.UpdatedSuccessfully, nameof(PaymentMethod))

            };

        }

        public async Task<OperationResultModel> DeletePaymentMethodAsync(int methodId)
        {
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>().GetByIdAsync(methodId);

            if (paymentMethod is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Payment Method"));

            var imageUrl = paymentMethod.IconImageUrl;

            try
            {
                _unitOfWork.Repository<PaymentMethod>().Delete(paymentMethod);

                var result = await _unitOfWork.CompleteAsync();
                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);

                _fileService.DeleteImageIfExists(imageUrl);
                return new OperationResultModel
                {
                    IsSuccess = true,
                    Message = string.Format(Messages.DeletedSuccessfully, "Payment Method")
                };
            }
            catch (DbUpdateException)
            {
                throw new BadRequestException(string.Format(Messages.CannotDeleteWithRelatedRecords, "Payment Method"));
            }
        }

        public async Task<IReadOnlyList<PaymentMethodDto>> GetAllPaymentMethodsAsync()
        {
            var paymentMethodsSpec = new BaseSpecification<PaymentMethod>(x => true);
            paymentMethodsSpec.AddInclude(x => x.Translations);
            var paymentMethods = await _unitOfWork.Repository<PaymentMethod>().ListAsync(paymentMethodsSpec);
            return _mapper.Map<List<PaymentMethodDto>>(paymentMethods);
        }

        public async Task<IReadOnlyList<PaymentMethodDto>> GetAllAvailablePaymentMethodsAsync()
        {
            var availableMethodsSpec = new BaseSpecification<PaymentMethod>(x => x.Status != PaymentMethodStatus.Inactive);
            availableMethodsSpec.AddInclude(x => x.Translations);
            var paymentMethods = await _unitOfWork.Repository<PaymentMethod>().ListAsync(availableMethodsSpec);
            return _mapper.Map<List<PaymentMethodDto>>(paymentMethods);
        }

        public async Task<PaymentMethodDetailsDto> GetPaymentMethodByIdAsync(int methodId)
        {
            var paymentMethodSpec = new BaseSpecification<PaymentMethod>(x => x.Id == methodId);
            paymentMethodSpec.AddInclude(x => x.Translations);
            var paymentMethod = await _unitOfWork.Repository<PaymentMethod>().GetFirstOrDefaultAsync(paymentMethodSpec);

            if (paymentMethod is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Payment Method"));

            return _mapper.Map<PaymentMethodDetailsDto>(paymentMethod);
        }

        public async Task<PaginationModel<AuditLogDto>> GetPaymentMethodAuditLogsAsync(int methodId, PaginationRequest dto)
        {
            var method = await _unitOfWork.Repository<PaymentMethod>().GetByIdAsync(methodId);
            if (method is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Payment Method"));

            var methodIdStr = methodId.ToString();
            var translationIds = await GetEntityIdsAsync<PaymentMethodTranslation>(x => x.PaymentMethodId == methodId);

            var auditLogSpec = new AuditLogSpecification(x =>
                (
                    (x.EntityName == nameof(PaymentMethod) && x.EntityId == methodIdStr) ||
                    (x.EntityName == nameof(PaymentMethodTranslation) && translationIds.Contains(x.EntityId!))
                )
                && (x.Action != AuditAction.Update || x.OldValues != null || x.NewValues != null));

            var totalCount = await _unitOfWork.Repository<AuditLog>().CountAsync(auditLogSpec);

            auditLogSpec.ApplyPagination((dto.PageSize * (dto.PageIndex - 1)), dto.PageSize);
            var auditLogs = await _unitOfWork.Repository<AuditLog>().ListAsync(auditLogSpec);
            var result = _mapper.Map<List<AuditLogDto>>(auditLogs);
            return new PaginationModel<AuditLogDto>(
                dto.PageIndex,
                dto.PageSize,
                totalCount,
                result);
        }

        #region Private Methods

        private async Task<List<string>> GetEntityIdsAsync<T>(Expression<Func<T, bool>> criteria) where T : ModelBase
        {
            var items = await _unitOfWork.Repository<T>().ListAsync(new BaseSpecification<T>(criteria));
            return items.Select(x => x.Id.ToString()).ToList();
        }

        #endregion

    }
}
