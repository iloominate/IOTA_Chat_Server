using IOTA_Chat_Server.Exceptions;
using IOTA_Chat_Server.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Chat_Server.Converters
{
    public static class MessageToBytesConverter
    {
        private static readonly int M_TYPE_BYTE_SIZE = 1;
        private static readonly int ID_BYTE_SIZE = 2;
        public static byte[] MessageToBytes(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");

            byte[] idBytes = new byte[2];

            if (BitConverter.TryWriteBytes(idBytes, message.Id) == false)
            {
                throw new Exception("Unable to convert message id to bytes");
            }
            if (BitConverter.IsLittleEndian)
            {
                // If the system is little-endian, reverse the byte order
                Array.Reverse(idBytes);
            }
            byte messageTypeByte = MTypeToBytesConverter.MTypeToByte(message.Type);

            switch (message.Type)
            {
                // |  0x02  |    MessageID    |  Username  | 0 |  DisplayName  | 0 |  Secret  | 0 |
                case MessageType.AUTH:
                    {

                        byte[] usernameBytes = Encoding.ASCII.GetBytes(message.Username);
                        byte[] displaynameBytes = Encoding.ASCII.GetBytes(message.DisplayName);
                        byte[] secretBytes = Encoding.ASCII.GetBytes(message.Secret);

                        // null bytes = 3
                        int mLength = M_TYPE_BYTE_SIZE + ID_BYTE_SIZE + 3 + usernameBytes.Length + displaynameBytes.Length + secretBytes.Length;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
                        arrIndex += idBytes.Length;

                        Array.Copy(usernameBytes, 0, messageBytes, arrIndex, usernameBytes.Length);
                        arrIndex += usernameBytes.Length;

                        messageBytes[arrIndex] = 0;
                        arrIndex += 1;

                        Array.Copy(displaynameBytes, 0, messageBytes, arrIndex, displaynameBytes.Length);
                        arrIndex += displaynameBytes.Length;

                        messageBytes[arrIndex] = 0;
                        arrIndex += 1;

                        Array.Copy(secretBytes, 0, messageBytes, arrIndex, secretBytes.Length);
                        arrIndex += secretBytes.Length;

                        messageBytes[arrIndex] = 0;
                        return messageBytes;
                    }
                // |  0x03  |    MessageID    |  ChannelID | 0 |  DisplayName  | 0 |
                case MessageType.JOIN:
                    {
                        byte[] channelIdBytes = new byte[2];

                        if (BitConverter.TryWriteBytes(channelIdBytes, message.ChannelID ?? throw new NullReferenceException())
                            == false)
                        {
                            throw new Exception("Unable to convert message id to bytes");
                        }

                        if (BitConverter.IsLittleEndian)
                        {
                            // If the system is little-endian, reverse the byte order
                            Array.Reverse(channelIdBytes);
                        }

                        byte[] displaynameBytes = Encoding.ASCII.GetBytes(message.DisplayName);

                        // null bytes = 2
                        int mLength = M_TYPE_BYTE_SIZE + ID_BYTE_SIZE + 2 + channelIdBytes.Length + displaynameBytes.Length;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
                        arrIndex += idBytes.Length;

                        Array.Copy(channelIdBytes, 0, messageBytes, arrIndex, channelIdBytes.Length);
                        arrIndex += displaynameBytes.Length;

                        messageBytes[arrIndex] = 0;

                        Array.Copy(displaynameBytes, 0, messageBytes, arrIndex, displaynameBytes.Length);
                        arrIndex += displaynameBytes.Length;

                        messageBytes[arrIndex] = 0;
                        return messageBytes;
                    }
                // |  0x04  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 | 
                case MessageType.MSG:
                    {
                        byte[] displaynameBytes = Encoding.ASCII.GetBytes(message.DisplayName);
                        byte[] contentsBytes = Encoding.ASCII.GetBytes(message.Content);
                        // null bytes = 2
                        int mLength = M_TYPE_BYTE_SIZE + ID_BYTE_SIZE + 2 + displaynameBytes.Length + contentsBytes.Length;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
                        arrIndex += idBytes.Length;

                        Array.Copy(displaynameBytes, 0, messageBytes, arrIndex, displaynameBytes.Length);
                        arrIndex += displaynameBytes.Length;

                        messageBytes[arrIndex] = 0;
                        arrIndex += 1;

                        Array.Copy(contentsBytes, 0, messageBytes, arrIndex, contentsBytes.Length);
                        arrIndex += contentsBytes.Length;

                        messageBytes[arrIndex] = 0;
                        return messageBytes;
                        
                    }
                // |  0xFE  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
                case MessageType.ERR:
                    {
                        byte[] displaynameBytes = Encoding.ASCII.GetBytes(message.DisplayName);
                        byte[] contentsBytes = Encoding.ASCII.GetBytes(message.Content);

                        // null bytes = 2
                        int mLength = M_TYPE_BYTE_SIZE + ID_BYTE_SIZE + 2 + displaynameBytes.Length + contentsBytes.Length;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
                        arrIndex += idBytes.Length;

                        Array.Copy(displaynameBytes, 0, messageBytes, arrIndex, displaynameBytes.Length);
                        arrIndex += displaynameBytes.Length;

                        messageBytes[arrIndex] = 0;
                        arrIndex += 1;

                        Array.Copy(contentsBytes, 0, messageBytes, arrIndex, contentsBytes.Length);
                        arrIndex += contentsBytes.Length;

                        messageBytes[arrIndex] = 0;
                        return messageBytes;
                    }
                // |  0xFF  |    MessageID    |
                case MessageType.BYE:
                    {
                        int mLength = M_TYPE_BYTE_SIZE + ID_BYTE_SIZE;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
                        return messageBytes;
                    }
                // |  0x00  |  Ref_MessageID  |
                case MessageType.CONFIRM:
                    {
                        if (BitConverter.TryWriteBytes(idBytes, message.RefId ?? throw new NullReferenceException("Ref id is null"))
                            == false)
                        {
                            throw new Exception("Unable to convert message id to bytes");
                        }

                        if (BitConverter.IsLittleEndian)
                        {
                            // If the system is little-endian, reverse the byte order
                            Array.Reverse(idBytes);
                        }
                        int mLength = M_TYPE_BYTE_SIZE + ID_BYTE_SIZE;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
                        return messageBytes;
                    }
                // | 0x01 | MessageID | Result | Ref_MessageID | MessageContents | 0 |
                case MessageType.REPLY:
                    { 
                        bool res = message.Result ?? throw new NullReferenceException();
                        byte resultByte = res ? (byte)1 : (byte)0;

                        byte[] refIdBytes = new byte[2];

                        byte[] contentsBytes = Encoding.ASCII.GetBytes(message.Content);
                       
                        if (BitConverter.TryWriteBytes(refIdBytes, message.RefId ?? throw new NullReferenceException("Ref id is null"))
                            == false)   
                        {
                            throw new Exception("Unable to convert message id to bytes");
                        }

                        // null bytes = 1
                        int mLength = M_TYPE_BYTE_SIZE + ID_BYTE_SIZE + 1 + refIdBytes.Length + contentsBytes.Length + 1;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
                        arrIndex += idBytes.Length;
                        
                        messageBytes[arrIndex] = resultByte;
                        arrIndex += 1;

                        Array.Copy(refIdBytes, 0, messageBytes, arrIndex, refIdBytes.Length);
                        arrIndex += refIdBytes.Length;

                        Array.Copy(contentsBytes, 0, messageBytes, arrIndex, contentsBytes.Length);
                        arrIndex += contentsBytes.Length;

                        messageBytes[arrIndex] = 0;

                        return messageBytes;
                    }
                default:
                    {
                        throw new Exception("Can't construct a message for that type");
                    }
            }
        }


        public static Message BytesToMessage(byte[] msgBytes)
        {
            if (msgBytes == null)
            {
                throw new ArgumentNullException("Can't convert null to Message");
            }

            var messageType = MTypeToBytesConverter.ByteToMType(msgBytes[0]);


            var inputMessage = new Message(0, messageType);

            try
            {
                switch (messageType)
                {

                    // |  0x00  |  Ref_MessageID  |
                    case MessageType.CONFIRM:
                        {
                            byte[] refIdBytes = new byte[] { msgBytes[1], msgBytes[2] };
                            if (BitConverter.IsLittleEndian)
                            {
                                // Reverse the byte order
                                Array.Reverse(refIdBytes);
                            }

                            inputMessage.RefId = BitConverter.ToUInt16(refIdBytes, 0);
                            break;
                        }
                    // | 0x01 | MessageID | Result | Ref_MessageID | MessageContents | 0 |
                    case MessageType.REPLY:
                        {
                            byte[] msgId = new byte[] { msgBytes[1], msgBytes[2] };
                            if (BitConverter.IsLittleEndian)
                            {
                                // Reverse the byte order
                                Array.Reverse(msgId);
                            }

                            inputMessage.Id = BitConverter.ToUInt16(msgId, 0);

                            inputMessage.Result = msgBytes[3] == 1 ? true : false;

                            byte[] refIdBytes = new byte[] { msgBytes[4], msgBytes[5] };
                            if (BitConverter.IsLittleEndian)
                            {
                                // Reverse the byte order
                                Array.Reverse(refIdBytes);
                            }
                            inputMessage.RefId = BitConverter.ToUInt16(refIdBytes, 0);

                            int contentLength = msgBytes.Length - 6 - 1;
                            inputMessage.Content = Encoding.ASCII.GetString(msgBytes, 6, contentLength);
                            break;
                        }
                    // |  0x04  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
                    case MessageType.MSG:
                        {
                            byte[] msgId = new byte[] { msgBytes[1], msgBytes[2] };
                            if (BitConverter.IsLittleEndian)
                            {
                                // Reverse the byte order
                                Array.Reverse(msgId);
                            }
                            inputMessage.Id = BitConverter.ToUInt16(msgId, 0);

                            var displayNameBytesList = new List<byte>();
                            int counter = 3;
                            while (msgBytes[counter] != 0)
                            {
                                displayNameBytesList.Add(msgBytes[counter]);
                                counter++;
                            }
                            inputMessage.DisplayName = Encoding.ASCII.GetString(displayNameBytesList.ToArray());

                            counter++;
                            int contentLength = msgBytes.Length - counter - 1;
                            inputMessage.Content = Encoding.ASCII.GetString(msgBytes, counter, contentLength);
                            break;
                        }
                    // |  0xFE  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
                    case MessageType.ERR:
                        {
                            byte[] msgId = new byte[] { msgBytes[1], msgBytes[2] };
                            if (BitConverter.IsLittleEndian)
                            {
                                // Reverse the byte order
                                Array.Reverse(msgId);
                            };
                            inputMessage.Id = BitConverter.ToUInt16(msgId, 0);

                            var displayNameBytesList = new List<byte>();
                            int counter = 3;
                            while (msgBytes[counter] != 0)
                            {
                                displayNameBytesList.Add(msgBytes[counter]);
                                counter++;
                            }
                            counter++;
                            inputMessage.DisplayName = Encoding.ASCII.GetString(displayNameBytesList.ToArray());
                            int contentLength = msgBytes.Length - counter - 1;
                            inputMessage.Content = Encoding.ASCII.GetString(msgBytes, counter, contentLength);
                            break;
                        }
                    case MessageType.BYE:
                        {
                            byte[] messageId = new byte[] { msgBytes[1], msgBytes[2] };
                            if (BitConverter.IsLittleEndian)
                            {
                                // Reverse the byte order
                                Array.Reverse(messageId);
                            }
                            inputMessage.Id = BitConverter.ToUInt16(messageId, 0);
                            break;
                        }
                    // |  0x02  |    MessageID    |  Username  | 0 |  DisplayName  | 0 |  Secret  | 0 |
                    case MessageType.AUTH:
                        {
                            byte[] messageId = new byte[] { msgBytes[1], msgBytes[2] };
                            if (BitConverter.IsLittleEndian)
                            {
                                // Reverse the byte order
                                Array.Reverse(messageId);
                            }
                            inputMessage.Id = BitConverter.ToUInt16(messageId, 0);

                            int counter = 3;
                            var userNameBytesList = new List<byte>();
                            while (msgBytes[counter] != 0)
                            {
                                userNameBytesList.Add(msgBytes[counter]);
                                counter++;
                            }
                            inputMessage.Username = Encoding.ASCII.GetString(userNameBytesList.ToArray());

                            counter++;
                            var displayNameBytes = new List<byte>();
                            while (msgBytes[counter] != 0)
                            {
                                displayNameBytes.Add(msgBytes[counter]);
                                counter++;
                            }
                            
                            inputMessage.DisplayName = Encoding.ASCII.GetString(displayNameBytes.ToArray());
                            
                            counter++;
                            var sercretBytes = new List<byte>();
                            while (msgBytes[counter] != 0)
                            {
                                sercretBytes.Add(msgBytes[counter]);
                                counter++;
                            }
                            inputMessage.Secret = Encoding.ASCII.GetString(sercretBytes.ToArray());
                            break;
                        }
                    default:
                        {
                            throw new Exception(message: "ERR: Unrecognizable packet format");
                        }
                }
            }
            catch (Exception ex)
            {

                throw new MessageConvertException();
            }
            return inputMessage;
        }
    }
}
