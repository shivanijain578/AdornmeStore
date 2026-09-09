using AdornmeStore.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface IProductPricingService
    {
        Task<ProductPricingDto> GetProductPricingAsync(
            int productId,
            CancellationToken cancellationToken = default);
    }
}
