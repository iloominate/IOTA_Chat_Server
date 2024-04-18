using IOTA_Chat_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Chat_Server.Converters
{
    public static class MessageToBytesConverter
    {

        public static byte[] MessageToByte(Message message)
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

                        // mType + mId + null bytes = 1 + 2 + 3
                        int mLength = 1 + 2 + 3 + usernameBytes.Length + displaynameBytes.Length + secretBytes.Length;
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

                        // mType + mId + null bytes = 1 + 2 + 2
                        int mLength = 1 + 2 + 2 + channelIdBytes.Length + displaynameBytes.Length;
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

                        // mType + mId + null bytes = 1 + 2 + 2
                        int mLength = 1 + 2 + 2 + displaynameBytes.Length + contentsBytes.Length;
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

                        // mType + mId + null bytes = 1 + 2 + 2
                        int mLength = 1 + 2 + 2 + displaynameBytes.Length + contentsBytes.Length;
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
                        int mLength = 1 + 2;
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
                        int mLength = 1 + 2;
                        byte[] messageBytes = new byte[mLength];

                        // index for copying data to messageBytes
                        int arrIndex = 0;


                        messageBytes[arrIndex] = messageTypeByte;
                        arrIndex += 1;


                        Array.Copy(idBytes, 0, messageBytes, arrIndex, idBytes.Length);
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

                            var displayNameBytes = new byte[] { };
                            int counter = 3;
                            while (msgBytes[counter] != 0)
                            {
                                displayNameBytes.Append(msgBytes[counter]);
                                counter++;
                            }
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

                            var displayNameBytes = new byte[] { };
                            int counter = 3;
                            while (msgBytes[counter] != 0)
                            {
                                displayNameBytes.Append(msgBytes[counter]);
                                counter++;
                            }
                            counter++;
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
                    default:
                        {
                            throw new Exception(message: "ERR: Unrecognizable packet format");
                        }
                }
            }
            catch (Exception ex)
            {
                UdpData.UdpHandleLocalErr(ex.Message);
            }
            return inputMessage;
        }
    }
}
