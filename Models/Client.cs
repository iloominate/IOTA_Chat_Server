using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Chat_Server.Models
{
    public class Client
    {
        public string Username { get; set; }
        public string Displayname { get; set; }
        public string? CurrentChannelName { get; set; }
        public ClientState State { get; set; }

    };
    
    public enum ClientState
    {
        NONAUTH, 
        AUTH,
        BYE
    }
}
