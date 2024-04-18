using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Chat_Server.Models
{
    public class Message
    {
        public ushort Id { get; set; }
        public MessageType Type { get; set; }
        [MaxLength(20)]
        public string? Username { get; set; }
        [MaxLength(20)]
        public string? DisplayName { get; set; }
        [MaxLength(128)]
        public string? Secret { get; set; }
        public bool? Result { get; set; }

        [MaxLength(1400)]
        public string? Content { get; set; }
        [MaxLength(20)]
        public ushort? ChannelID { get; set; }
        public ushort? RefId { get; set; }
        public Message(ushort id, MessageType type)
        {
            Id = id;
            Type = type;
        }
    }

    public enum MessageType
    {
        CONFIRM, // 0x00
        REPLY,   // 0x01
        AUTH,    // 0x02
        JOIN,    // 0x03
        MSG,     // 0x04
        ERR,     // 0xFE
        BYE      // 0xFF
    }
}
