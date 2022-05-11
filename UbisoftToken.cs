using System;

namespace Argali
{
    public class UbisoftToken
    {
        public string Ticket { get; set; }

        public DateTime Expiration { get; set; }

        public bool IsExpired => Expiration <= DateTime.Now;
    }
}