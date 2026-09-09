using AdornmeStore.Application.DTOs.Inventory;
using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/admin/inventory")]
[Authorize(Roles = "Admin")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ICurrentUserService _currentUser;

    public InventoryController(
        IInventoryService inventoryService,
        ICurrentUserService currentUser)
    {
        _inventoryService = inventoryService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory(
        CancellationToken cancellationToken)
    {
        var inventory =
            await _inventoryService
                .GetInventoryAsync(cancellationToken);

        return Ok(inventory);
    }

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetProductInventory(
        int productId,
        CancellationToken cancellationToken)
    {
        var inventory =
            await _inventoryService
                .GetProductInventoryAsync(
                    productId,
                    cancellationToken);

        if (inventory == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(inventory);
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock(
        [FromBody] InventoryAdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _inventoryService.AdjustStockAsync(
                dto,
                _currentUser.UserId,
                cancellationToken);

            return Ok(new
            {
                message = "Inventory updated successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int? productId,
        CancellationToken cancellationToken)
    {
        var transactions =
            await _inventoryService
                .GetTransactionsAsync(
                    productId,
                    cancellationToken);

        return Ok(transactions);
    }
}