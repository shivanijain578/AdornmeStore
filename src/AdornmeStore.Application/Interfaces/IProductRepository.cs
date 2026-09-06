using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);

        Task<Product?> GetByIdWithCategoryAsync(int id);

        Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            bool sortDescending,
            int pageNumber,
            int pageSize);

        Task AddAsync(Product product);

        Task UpdateAsync(Product product);

        Task DeleteAsync(Product product);
    }
}
