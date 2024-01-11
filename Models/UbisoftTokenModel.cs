using System;

namespace Argali.Models;

public class UbisoftTokenModel
{
    public string? Ticket { get; set; }

    public DateTime Expiration { get; set; }

    public bool IsExpired => Expiration <= DateTime.Now;
}