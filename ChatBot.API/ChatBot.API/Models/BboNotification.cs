using System;
using System.Collections.Generic;

namespace ChatBot.API.Models;

public partial class BboNotification
{
    public int Id { get; set; }

    public string? Notification { get; set; }

    public DateTime? Createdat { get; set; }

    public bool? Status { get; set; }

    public DateTime? Updateat { get; set; }
}
