using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemInventory.ViewModels
{
    public class Configuration
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Password { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
    }
}