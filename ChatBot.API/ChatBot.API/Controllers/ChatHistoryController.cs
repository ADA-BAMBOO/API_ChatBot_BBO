using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Newtonsoft.Json;


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

    [HttpGet("export-chat-history")]
    public async Task<IActionResult> ExportChatHistoryToExcel()
    {
        try
        {
            var chatHistory = await unitOfWork.chatHistoryReponsitory.GetAllAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ChatHistory");
                var headers = new string[]
                   {
                    "Id",
                    "Telegram Id",
                    "Message",
                    "Response",
                    "Sent At",
                    "Language Code",
                    "Response Time",
                   };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.Green;
                    worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(1, i + 1).Style.Font.FontSize = 12;
                }
                int curentRow = 2;
                for (int i = 0; i< chatHistory.Count; i++)
                {
                    var chat = chatHistory[i];
                    worksheet.Cell(curentRow, 1).Value = i + 1;
                    worksheet.Cell(curentRow, 2).Value = chat.Userid;
                    worksheet.Cell(curentRow, 3).Value = chat.Message;
                    worksheet.Cell(curentRow, 4).Value = chat.Response;
                    worksheet.Cell(curentRow, 5).Value = chat.Sentat;
                    worksheet.Cell(curentRow, 6).Value = chat.LanguageCode;
                    worksheet.Cell(curentRow, 7).Value = chat.Responsetime;
                    curentRow++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ChatHistory.xlsx");
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
        }
        
        
    }
}
