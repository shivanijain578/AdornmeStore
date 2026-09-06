using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);

        Task<IEnumerable<Category>> GetAllAsync();

        Task AddAsync(Category category);

        Task UpdateAsync(Category category);
    }
}
