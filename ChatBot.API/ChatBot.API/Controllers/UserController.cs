using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUnitOfWork unitOfWork;

    public UserController(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var _data = await unitOfWork.userReponsitory.GetAllAsync();
        return Ok(_data);
    }

    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var _data = await unitOfWork.userReponsitory.GetAsync(id);
        return Ok(_data);
    }

    [HttpGet("getByTelegramId/{telegramId}")]
    public async Task<IActionResult> getByTelegramId(int telegramId)
    {
        var _data = await unitOfWork.userReponsitory.GetFirstOrDefaultAsync(telegramId);
        return Ok(_data);
    }

    [HttpPost]
    public async Task<IActionResult> Created(BboUser bboUser)
    {
        var _data = await unitOfWork.userReponsitory.AddEntity(bboUser);
        await unitOfWork.CompleteAsync();
        return Ok(_data);
    }
}
