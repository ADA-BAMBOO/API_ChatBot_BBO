using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboCredit
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public int? Point { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updateat { get; set; }

    public virtual BboUser? User { get; set; }
}
