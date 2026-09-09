using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface IOfferRepository
    {
        Task<Offer?> GetByIdAsync(int id);

        Task<(IEnumerable<Offer> Items, int TotalCount)> GetOffersAsync(
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize);

        Task AddAsync(Offer offer);

        Task UpdateAsync(Offer offer);

        Task DeleteAsync(Offer offer);

        Task<bool> ProductExistsAsync(int productId);

        Task<bool> CategoryExistsAsync(int categoryId);
    }
}
