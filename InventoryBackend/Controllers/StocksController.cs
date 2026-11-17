using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace InventoryBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("fixed")]
public class StocksController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly ILogger<StocksController> _logger;

    public StocksController(IStockService stockService, ILogger<StocksController> logger)
    {
        _stockService = stockService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockDto>>> GetAll(CancellationToken cancellationToken)
    {
        var stocks = await _stockService.GetAllAsync(cancellationToken);
        return Ok(stocks);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var stock = await _stockService.GetByIdAsync(id, cancellationToken);
        return stock is null ? NotFound() : Ok(stock);
    }

    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<StockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StockDto>>> GetByProductId(int productId, CancellationToken cancellationToken)
    {
        var stocks = await _stockService.GetByProductIdAsync(productId, cancellationToken);
        return Ok(stocks);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StockDto>> Create([FromBody] StockCreateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _stockService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StockUpdateDto dto, CancellationToken cancellationToken)
    {
        var updated = await _stockService.UpdateAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _stockService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/audit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AuditStock(int id, CancellationToken cancellationToken)
    {
        var audited = await _stockService.AuditStockAsync(id, cancellationToken);
        return audited ? NoContent() : NotFound();
    }
}
