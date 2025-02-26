using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboChathistory
{
    public int Chatid { get; set; }

    public int? Userid { get; set; }

    public string? Message { get; set; }

    public string? Response { get; set; }

    public DateTime? Sentat { get; set; }

    public string? LanguageCode { get; set; }

    public decimal? Responsetime { get; set; }

    public virtual BboUser? User { get; set; }
}
