using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ChatHistoryController : ControllerBase
{
    private readonly IUnitOfWork unitOfWork;

    public ChatHistoryController(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int pageIndex, int pageSize)
    {
        var _data = await unitOfWork.chatHistoryReponsitory.GetAllAsyncPaged(pageIndex, pageSize);
        return Ok(_data);
    }

    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var _data = await unitOfWork.chatHistoryReponsitory.GetAsync(id);
        return Ok(_data);
    }

    [HttpPost]
    public async Task<IActionResult> Created(BboChathistory bboChatHistory)
    {
        var _data = await unitOfWork.chatHistoryReponsitory.AddEntity(bboChatHistory);
        await unitOfWork.CompleteAsync();
        return Ok(_data);
    }
}
