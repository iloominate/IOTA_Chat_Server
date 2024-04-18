using IOTA_Chat_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Chat_Server.Converters
{
    public static class MTypeToBytesConverter
    {
        public static byte MTypeToByte(MessageType type)
        {
            switch (type)
            {
                case MessageType.CONFIRM:
                    return 0x00;
                case MessageType.REPLY:
                    return 0x01;
                case MessageType.AUTH:
                    return 0x02;
                case MessageType.JOIN:
                    return 0x03;
                case MessageType.MSG:
                    return 0x04;
                case MessageType.ERR:
                    return 0xFE;
                case MessageType.BYE:
                    return 0xFF;
                default:
                    throw new ArgumentException("Unexpected message type");
            }
        }

        public static MessageType ByteToMType(byte type)
        {
            switch (type)
            {
                case 0x00:
                    return MessageType.CONFIRM;
                case 0x01:
                    return MessageType.REPLY;
                case 0x02:
                    return MessageType.AUTH;
                case 0x03:
                    return MessageType.JOIN;
                case 0x04:
                    return MessageType.MSG;
                case 0xFE:
                    return MessageType.ERR;
                case 0xFF:
                    return MessageType.BYE;
                default:
                    throw new ArgumentException("Unexpected message type");
            }
        }
    }
}
