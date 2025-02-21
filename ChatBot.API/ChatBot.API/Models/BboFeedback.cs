using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboFeedback
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual BboUser? User { get; set; }
}
