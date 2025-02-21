using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboQuestionanalyst
{
    public int Id { get; set; }

    public string? Questionpattern { get; set; }

    public string? Mainkey { get; set; }

    public int? Askedcount { get; set; }
}
