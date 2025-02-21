using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboRole
{
    public int Id { get; set; }

    public string? Rolename { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual ICollection<BboUser> BboUsers { get; set; } = new List<BboUser>();
}
