using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers;


[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IDbService _dbService;
    
    public WarehouseController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost]
    public IActionResult Post([FromBody] Warehouse warehouse)
    {
        if (warehouse == null || warehouse.Amount <= 0 )
        {
            return BadRequest();
        }

       var res = _dbService.addProduct(warehouse).Result;

       if (res < 0)
       {
           return BadRequest(res);
       }

       return Ok(res);
    }
    
}