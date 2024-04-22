using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Chat_Server.Models
{
    public class ArgumentOptions
    {
        public string ListeningIP { get; set; } = "0.0.0.0";
        public ushort ServerPort { get; set; } = 4567;
        public ushort Timeout { get; set; } = 250;
        public byte Retransmissions { get; set; } = 3;
    }
}
