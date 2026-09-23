using AutoMapper;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.ShippingMethods;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MK.Service.Services
{
    public class ShippingMethodService : IShippingMethodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShippingMethodService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OperationResultModel> AddShippingMethodAsync(CreateShippingMethodDto dto)
        {
            ValidateTranslations(dto.Translations);
            await CheckDuplicateTranslationsAsync(dto.Translations);

            var shippingMethod = _mapper.Map<ShippingMethod>(dto);

            shippingMethod.CreatedAt = DateTime.UtcNow;
            shippingMethod.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<ShippingMethod>().AddAsync(shippingMethod);

            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.Added_Successfully, nameof(ShippingMethod))
            };
        }

        public async Task<OperationResultModel> UpdateShippingMethodAsync(int shippingMethodId, UpdateShippingMethodDto dto)
        {
            var shippingMethodSpec = new BaseSpecification<ShippingMethod>(x => x.Id == shippingMethodId);
            shippingMethodSpec.AddInclude(x => x.Translations);
            var shippingMethod = await _unitOfWork.Repository<ShippingMethod>().GetFirstOrDefaultAsync(shippingMethodSpec);

            if (shippingMethod is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Shipping Method"));

            if (dto.Translations is not null)
            {
                foreach (var translationDto in dto.Translations)
                {
                    var translation = shippingMethod.Translations.FirstOrDefault(x => x.Language == translationDto.Language);

                    if (translation is null)
                        throw new NotFoundException(string.Format(Messages.NotFound, $"Shipping Method Translation ({translationDto.Language})"));

                    if (!string.IsNullOrWhiteSpace(translationDto.Name))
                        await CheckDuplicateTranslationNameAsync(translationDto.Language, translationDto.Name, shippingMethodId);

                    _mapper.Map(translationDto, translation);
                }
            }

            _mapper.Map(dto, shippingMethod);
            shippingMethod.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ShippingMethod>().Update(shippingMethod);
            var result = await _unitOfWork.CompleteAsync();

            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.UpdatedSuccessfully, "Shipping Method")
            };
        }

        public async Task<OperationResultModel> DeleteShippingMethodAsync(int shippingMethodId)
        {
            var shippingMethodSpec = new BaseSpecification<ShippingMethod>(x => x.Id == shippingMethodId);
            shippingMethodSpec.AddInclude(x => x.OrderShippings);
            var shippingMethod = await _unitOfWork.Repository<ShippingMethod>().GetFirstOrDefaultAsync(shippingMethodSpec);

            if (shippingMethod is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Shipping Method"));

            if (shippingMethod.OrderShippings.Any())
                throw new BadRequestException(Messages.ShippingMethod_CannotDeleteWithOrders);

            _unitOfWork.Repository<ShippingMethod>().Delete(shippingMethod);

            var result = await _unitOfWork.CompleteAsync();
            if (result < 1)
                throw new Exception(Messages.SomethingWentWrong);

            return new OperationResultModel
            {
                IsSuccess = true,
                Message = string.Format(Messages.DeletedSuccessfully, "Shipping Method")
            };
        }

        public async Task<IReadOnlyList<ShippingMethodDto>> GetAllShippingMethodsAsync()
        {
            var shippingMethodSpec = new BaseSpecification<ShippingMethod>();
            shippingMethodSpec.AddInclude(x => x.Translations);
            var shippingMethods = await _unitOfWork.Repository<ShippingMethod>().ListAsync(shippingMethodSpec);
            return _mapper.Map<List<ShippingMethodDto>>(shippingMethods);
        }

        public async Task<IReadOnlyList<ShippingMethodDto>> GetAllActiveShippingMethodsAsync()
        {
            var activeSpec = new BaseSpecification<ShippingMethod>(x => x.Status == Visibility.Active);
            activeSpec.AddInclude(x => x.Translations);
            var shippingMethods = await _unitOfWork.Repository<ShippingMethod>().ListAsync(activeSpec);
            return _mapper.Map<List<ShippingMethodDto>>(shippingMethods);
        }

        public async Task<ShippingMethodDetailsDto> GetShippingMethodByIdAsync(int shippingMethodId)
        {
            var shippingMethodSpec = new BaseSpecification<ShippingMethod>(x => x.Id == shippingMethodId);
            shippingMethodSpec.AddInclude(x => x.Translations);
            var shippingMethod = await _unitOfWork.Repository<ShippingMethod>().GetFirstOrDefaultAsync(shippingMethodSpec);

            if (shippingMethod is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Shipping Method"));

            return _mapper.Map<ShippingMethodDetailsDto>(shippingMethod);
        }

        public async Task<PaginationModel<AuditLogDto>> GetShippingMethodAuditLogsAsync(int shippingMethodId, PaginationRequest dto)
        {
            var auditLogSpec = new AuditLogSpecification(x =>
                x.EntityName == nameof(ShippingMethod) && x.EntityId == shippingMethodId.ToString()
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

        #region Helper Methods
        private void ValidateTranslations(ICollection<CreateShippingMethodTranslationDto> translations)
        {
            if (translations is null || translations.Count < 3)
                throw new BadRequestException(Messages.AllTranslationsRequired);

            var duplicatedLanguages = translations.GroupBy(x => x.Language).Any(x => x.Count() > 1);

            if (duplicatedLanguages)
                throw new BadRequestException(Messages.ShippingMethod_DuplicateTranslations);

            foreach (var translation in translations)
            {
                if (string.IsNullOrWhiteSpace(translation.Name))
                    throw new BadRequestException(string.Format(Messages.ShippingMethod_TranslationNameRequired, translation.Language));
            }
        }
        private async Task CheckDuplicateTranslationsAsync(ICollection<CreateShippingMethodTranslationDto> translations)
        {
            foreach (var translation in translations)
            {
                await CheckDuplicateTranslationNameAsync(translation.Language, translation.Name, null);
            }
        }
        private async Task CheckDuplicateTranslationNameAsync(Language language, string name, int? excludeShippingMethodId)
        {
            var duplicateSpec = new BaseSpecification<ShippingMethodTranslation>(x =>
                x.Language == language &&
                x.Name == name &&
                (!excludeShippingMethodId.HasValue || x.ShippingMethodId != excludeShippingMethodId.Value));

            if (await _unitOfWork.Repository<ShippingMethodTranslation>().AnyAsync(duplicateSpec))
                throw new BadRequestException(string.Format(Messages.AlreadyExists, "Shipping Method Name"));
        }
        #endregion
    }
}
