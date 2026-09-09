using AdornmeStore.Application.DTOs.Common;
using AdornmeStore.Application.DTOs.Offers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface IOfferService
    {
        Task<PagedResult<OfferResponseDto>> GetOffersAsync(
            OfferQueryDto query);

        Task<OfferResponseDto?> GetByIdAsync(int id);

        Task<OfferResponseDto> CreateAsync(
            CreateOfferDto dto);

        Task<OfferResponseDto?> UpdateAsync(
            int id,
            UpdateOfferDto dto);

        Task<bool> UpdateStatusAsync(
            int id,
            bool isActive);

        Task<bool> DeleteAsync(int id);
    }
}
