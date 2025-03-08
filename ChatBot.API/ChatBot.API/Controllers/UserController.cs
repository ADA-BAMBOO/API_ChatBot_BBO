using ChatBot.API.Interface;
using ChatBot.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Newtonsoft.Json;

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

    [HttpGet("export-user")]
    public async Task<IActionResult> ExportUserToExcel()
    {
        try
        {
            var listUser = await unitOfWork.userReponsitory.GetAllAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ListUser");
                var headers = new string[]
                   {
                    "Id",
                    "Telegram Id",
                    "Username",
                    "Join date",
                    "Status",
                    "Role",
                    "Onchain Id",
                    "Language Code",
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
                for (int i = 0; i < listUser.Count; i++)
                {
                    var chat = listUser[i];
                    worksheet.Cell(curentRow, 1).Value = i + 1;
                    worksheet.Cell(curentRow, 2).Value = chat.Telegramid;
                    worksheet.Cell(curentRow, 3).Value = chat.Username;
                    worksheet.Cell(curentRow, 4).Value = chat.Joindate;
                    worksheet.Cell(curentRow, 5).Value = chat.Isactive ?? false ? "Active" : "Inactive";
                    worksheet.Cell(curentRow, 6).Value = chat.Role?.Rolename;
                    worksheet.Cell(curentRow, 7).Value = chat.Onchainid;
                    worksheet.Cell(curentRow, 8).Value = chat.Language switch
                    {
                        "en" => "English",
                        "vi" => "Vietnamese",
                        _ => chat.Language
                    };
                    curentRow++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListUser.xlsx");
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
        }


    }
}
