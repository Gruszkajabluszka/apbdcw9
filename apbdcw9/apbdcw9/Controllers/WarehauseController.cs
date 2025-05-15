using apbdcw9.Modles;
using apbdcw9.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apbdcw9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }
    
    // POST /api/warehouse
    [HttpPost]
    public async Task<IActionResult> AddProductUsingProcedure([FromBody] ProductRequestDTO requestDto)
    {
        if (requestDto.Amount <= 0)
        {
            return BadRequest("Amount must be greater than 0");
        }
        

        try
        {
            await _warehouseService.AddProductUsingProcedure(requestDto);
            return Created("","Uwtorzono");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd: {ex.Message}");
        }
    }
    // POST /api/warehouse/addProduct
    [HttpPost("addProduct")]
    public async Task<IActionResult> AddProduct([FromBody] ProductRequestDTO requestDto)
    {
        
        if (requestDto.Amount <= 0)
        {
            return BadRequest("Amount must be greater than 0");
        }
        try
        {
            var result = await _warehouseService.AddProduct(requestDto);
            return Created("", $"Utworzono z ID: {result}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd: {ex.Message}");
        }
    }

}