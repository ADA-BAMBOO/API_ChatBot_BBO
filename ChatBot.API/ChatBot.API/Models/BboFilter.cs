using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboFilter
{
    public int Id { get; set; }

    public string? Question { get; set; }

    public int? Displayorder { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updateat { get; set; }
}
