using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboUser
{
    public int Id { get; set; }

    public int? Telegramid { get; set; }

    public string? Username { get; set; }

    public DateTime? Joindate { get; set; }

    public DateTime? Lastactive { get; set; }

    public bool? Isactive { get; set; }

    public int? Roleid { get; set; }

    public string? Password { get; set; }

    public bool? Allownoti { get; set; }

    public string? Onchainid { get; set; }

    public string? Language { get; set; }

    public virtual ICollection<BboChathistory> BboChathistories { get; set; } = new List<BboChathistory>();

    public virtual ICollection<BboChathistoryBk> BboChathistoryBks { get; set; } = new List<BboChathistoryBk>();

    public virtual ICollection<BboCredit> BboCredits { get; set; } = new List<BboCredit>();

    public virtual ICollection<BboFeedback> BboFeedbacks { get; set; } = new List<BboFeedback>();

    public virtual BboRole? Role { get; set; }
}
