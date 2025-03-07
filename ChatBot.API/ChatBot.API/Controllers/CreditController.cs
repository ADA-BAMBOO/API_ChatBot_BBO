using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CreditController : ControllerBase
{
    private readonly IUnitOfWork unitOfWork;

    public CreditController(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var _data = await unitOfWork.creditReponsitory.GetAllAsync();
        return Ok(_data);
    }

}

