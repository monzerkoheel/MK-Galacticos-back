using AutoMapper;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.DTOs;
using MK.Core.DTOs.LegendCards;
using MK.Core.HelperModels;
using MK.Core.HelperModels.Exceptions;
using MK.Core.Localization.Resources;
using MK.Core.Models;
using MK.Repository.Specifications;
using MK.Service.HelperServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.Services
{
    public class LegendCardService : ILegendCardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        public LegendCardService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService) 
        { 
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
        }
        public async Task<OperationResultModel> AddCardAsync(CreateLegendCardDto dto)
        {
            var legendCardSpec = new BaseSpecification<LegendCard>(x => x.Name == dto.Name);
            if (await _unitOfWork.Repository<LegendCard>().AnyAsync(legendCardSpec))
                throw new BadRequestException(string.Format(Messages.AlreadyExists, "Legend Card Name"));
            var imageUrl = await _fileService.SaveImageAsync(dto.Image, "uploads/images/legendCards");
            var legendCard = _mapper.Map<LegendCard>(dto);

            legendCard.ImageUrl = imageUrl;
            legendCard.CreatedAt = DateTime.UtcNow;
            legendCard.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _unitOfWork.Repository<LegendCard>().AddAsync(legendCard);
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
                Message = string.Format(Messages.Added_Successfully, nameof(LegendCard))
            };
        }

        public async Task<OperationResultModel> UpdateCardAsync(int cardId, UpdateLegendCardDto dto)
        {
            var legendCard = await _unitOfWork.Repository<LegendCard>().GetByIdAsync(cardId);

            if (legendCard is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Legend Card"));

            if (dto.Name is not null)
            {
                var duplicateSpec = new BaseSpecification<LegendCard>(x => x.Name == dto.Name && x.Id != cardId);
                if (await _unitOfWork.Repository<LegendCard>().AnyAsync(duplicateSpec))
                    throw new BadRequestException(string.Format(Messages.AlreadyExists, "Legend Card Name"));
            }

            var oldImageUrl = legendCard.ImageUrl;
            string? newImageUrl = null;

            try
            {
                if (dto.Image is not null)
                {
                    newImageUrl = await _fileService.SaveImageAsync(dto.Image, "uploads/images/legendCards");
                }

                _mapper.Map(dto, legendCard);

                if (newImageUrl is not null)
                    legendCard.ImageUrl = newImageUrl;

                legendCard.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<LegendCard>().Update(legendCard);
                var result = await _unitOfWork.CompleteAsync();

                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);

                if (newImageUrl is not null)
                    _fileService.DeleteImageIfExists(oldImageUrl);

                return new OperationResultModel
                {
                    IsSuccess = true,
                    Message = string.Format(Messages.UpdatedSuccessfully, "Legend Card")
                };
            }
            catch
            {
                if (newImageUrl is not null)
                    _fileService.DeleteImageIfExists(newImageUrl);

                throw;
            }
        }

        public async Task<OperationResultModel> DeleteCardAsync(int cardId)
        {
            var legendCard = await _unitOfWork.Repository<LegendCard>().GetByIdAsync(cardId);

            if (legendCard is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Legend Card"));

            var imageUrl = legendCard.ImageUrl;

            try
            {
                _unitOfWork.Repository<LegendCard>().Delete(legendCard);

                var result = await _unitOfWork.CompleteAsync();

                if (result < 1)
                    throw new Exception(Messages.SomethingWentWrong);

                _fileService.DeleteImageIfExists(imageUrl);
                return new OperationResultModel
                {
                    IsSuccess = true,
                    Message = string.Format(Messages.DeletedSuccessfully, "Legend Card")
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<IReadOnlyList<LegendCardDto>> GetAllCardsAsync()
        {
            var legendCards = await _unitOfWork.Repository<LegendCard>().GetAllAsync();
            return _mapper.Map<List<LegendCardDto>>(legendCards);
        }

        public async Task<IReadOnlyList<LegendCardDto>> GetAllActiveCardsAsync()
        {
            var activeCardsSpec = new BaseSpecification<LegendCard>(x => x.Status == Visibility.Active);
            var legendCards = await _unitOfWork.Repository<LegendCard>().ListAsync(activeCardsSpec);
            return _mapper.Map<List<LegendCardDto>>(legendCards);
        }

        public async Task<LegendCardDto> GetCardByIdAsync(int cardId)
        {
            var legendCard = await _unitOfWork.Repository<LegendCard>().GetByIdAsync(cardId);

            if (legendCard is null)
                throw new NotFoundException(string.Format(Messages.NotFound, "Legend Card"));

            return _mapper.Map<LegendCardDto>(legendCard);
        }

        public async Task<PaginationModel<AuditLogDto>> GetCardAuditLogsAsync(int cardId, PaginationRequest dto)
        {
            var auditLogSpec = new AuditLogSpecification(x =>
                x.EntityName == nameof(LegendCard) && x.EntityId == cardId.ToString()
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
    }
}
