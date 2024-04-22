using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;

using IOTA_Chat_Server.Models;
using IOTA_Chat_Server.Exceptions;
using IOTA_Chat_Server.Converters;
using System.Reflection.Metadata;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IOTA_Chat_Server
{
    internal class Program
    {
        private static ConcurrentQueue<Client> clients = new ConcurrentQueue<Client>();
        public static byte udpRetransmissionLimit = 3;
        public static UInt16 udpRetransmissionTimeout = 250;
        static async Task Main(string[] args)
        {
            string? listeningIP = null;
            ushort? serverPort = null;
            ushort? timeout = null;
            byte retransmissions = 0;

            Console.WriteLine("Parsing args");
            // Parse command-line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-l":
                        listeningIP = args[++i];
                        break;
                    case "-p":
                        ushort port;
                        if (ushort.TryParse(args[++i], out port))
                        {
                            serverPort = port;
                        }
                        else
                        {
                            Console.Error.WriteLine("ERR: Invalid port number.");
                            return;
                        }
                        break;
                    case "-d":
                        ushort timeoutValue;
                        if (ushort.TryParse(args[++i], out timeoutValue))
                        {
                            timeout = timeoutValue;
                        }
                        else
                        {
                            Console.Error.WriteLine("ERR: Invalid timeout value.");
                            return;
                        }
                        break;
                    case "-r":
                        byte retransmissionsValue;
                        if (byte.TryParse(args[++i], out retransmissionsValue))
                        {
                            retransmissions = retransmissionsValue;
                        }
                        else
                        {   
                            Console.Error.WriteLine("ERR: Invalid retransmissions value.");
                            return;
                        }
                        break;
                    default:
                        Console.Error.WriteLine($"ERR: Unknown option: {args[i]}");
                        return;
                }
            }


            UdpClient udpListener = new UdpClient();
            udpListener.Client.Bind(new IPEndPoint(IPAddress.Parse(listeningIP??"0.0.0.0"), 4567));
           
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 4567);

            
            try
            {
                while (true)
                {

                    UdpReceiveResult result = await udpListener.ReceiveAsync();
                    ProcessMessage(result);
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            { 
            }
        }

        static async Task ProcessMessage(UdpReceiveResult result)
        {

            // Check message validity
            Message? messageParsed = null;
            try
            {
                messageParsed = MessageToBytesConverter.BytesToMessage(result.Buffer);
            } catch (MessageConvertException e) // Unknown message was caught
            {
                // Send Bye or whatever
                return;
            }

            // Client creation
            IPEndPoint clientEP = result.RemoteEndPoint;
            UdpClient? localEP = new UdpClient();
            localEP.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            Client? client = new Client(clientEP, localEP);
            
            client.MessagesUprocessed.Add(messageParsed);
            var sender = Task.Run(() => SenderAsync(client));
            client.Exit = true;
            var listener = Task.Run(() => ListenerAsync(client));
        }

        static async Task HandleJoinAsync(Client client, string channelId, ushort refId, string replyContent)
        {
            // Check if broadcasting is needed
            if (client.CurrentChannelName == channelId)
            {
                // Send Reply
                Message msg = new Message(client.messageId++, MessageType.REPLY);
                msg.Result = true;
                msg.Content = "You are already in this channel";
                msg.RefId = refId;
                bool confirmArrived = await SendAndWaitForConfirmAsync(client, msg);
            } else
            {
                // Send Reply
                Message replyMessage = new Message(client.messageId++, MessageType.REPLY);
                replyMessage.Result = true;
                replyMessage.Content = replyContent;
                replyMessage.RefId = refId;
                bool confirmArrived = await SendAndWaitForConfirmAsync(client, replyMessage);

                // Broadcast join message
                client.CurrentChannelName = channelId;
                Message msg = new Message(client.messageId++, MessageType.MSG);
                msg.Content = $"{client.Displayname} has joined {channelId}.";
                msg.DisplayName = client.Displayname;
                await SendMessageToChannelAsync(client.CurrentChannelName, msg);
            }
        }


        public static async Task<bool> SendAndWaitForConfirmAsync(Client client, Message msg)
        {
            byte[] msgBytes = MessageToBytesConverter.MessageToBytes(msg);
            for (int i = 0; i < udpRetransmissionLimit + 1; i++)
            {
                await client.ServerEndPoint.SendAsync(msgBytes, client.ClientEndPoint);
                LogMessageSent(client.ClientEndPoint, msg);
                Message? existingMessage = null;
                Task getResponse = Task.Run(() =>
                {
                    while (true)
                    {
                        existingMessage = client.ClientConfirms.FirstOrDefault(c => c.RefId == msg.RefId);
                        existingMessage = confirmList.First();
                        if (existingMessage != null)
                            break;
                        Task.Delay(5);
                    }
                });
                Task waitForDelay = Task.Delay(udpRetransmissionTimeout);
                Task firstCompleted = await Task.WhenAny(getResponse, waitForDelay);
                if (getResponse.IsCompletedSuccessfully && existingMessage != null)
                {

                    return true;
                }
            }
            return false; 
        }

        public static async Task SendConfirmAsync(ushort refId, UdpClient server, IPEndPoint clientEP)
        {
            Message confirmM = new Message(0, MessageType.CONFIRM);
            confirmM.RefId = refId;
            byte[] msgBytes = MessageToBytesConverter.MessageToBytes(confirmM);
            await server.SendAsync(msgBytes, clientEP);
            LogMessageSent(clientEP, confirmM);
        }
        public static void LogMessageReceived(IPEndPoint clientEP, Message msg)
        {
            // RECV {FROM_IP}:{FROM_PORT} | {MESSAGE_TYPE}[MESSAGE_CONTENTS]\n
            string msgType = MTypeToBytesConverter.MTypeToString(msg.Type);
            Console.WriteLine($"RECV {clientEP.Address.ToString()}:{clientEP.Port} | {msgType}{msg.Content}");
        }
        public static void LogMessageSent(IPEndPoint clientEP, Message msg)
        {
            // SENT {FROM_IP}:{FROM_PORT} | {MESSAGE_TYPE}[MESSAGE_CONTENTS]\n
            string msgType = MTypeToBytesConverter.MTypeToString(msg.Type);
            Console.WriteLine($"SENT {clientEP.Address.ToString()}:{clientEP.Port} | {msgType}{msg.Content}");
        }
        public static async Task ListenerAsync(Client client)
        {
            while (client.Exit != true)
            {
                UdpReceiveResult receiveResult = await client.ServerEndPoint.ReceiveAsync();
                Message newMessage = MessageToBytesConverter.BytesToMessage(receiveResult.Buffer);
                client.MessagesUprocessed.Add(newMessage);

            }
        }
        public static async Task SenderAsync(Client client)
        {

            while (client.Exit != true || client.MessagesUprocessed.Count > 0)
            {
                if (client.MessagesUprocessed.Count > 0)
                {
                    Message messageToProcess = client.MessagesUprocessed.Take();
                    LogMessageReceived(client.ClientEndPoint, messageToProcess);

                    await SendConfirmAsync(messageToProcess.Id, client.ServerEndPoint, client.ClientEndPoint);
                    switch (messageToProcess.Type)
                    {
                        case MessageType.AUTH:
                            {
                                if (client.State == ClientState.NONAUTH)
                                {
                                    if (client.Username == null)
                                    {
                                        client.Username = messageToProcess.Username;
                                    }
                                    client.Displayname = messageToProcess.DisplayName;

                                    Message msg = new Message(client.messageId++, MessageType.REPLY);
                                    msg.Result = true;
                                    msg.Content = "Success! You are authenticated";
                                    msg.RefId = messageToProcess.Id;
                                    byte[] msgBytes = MessageToBytesConverter.MessageToBytes(msg);
                                    bool confirmArrived = await SendAndWaitForConfirmAsync(client, msg);
                                    if (confirmArrived == false)
                                    {
                                        // Just close the connection
                                    }
                                    else
                                    {
                                        // Send Message to all users in channel
                                    }

                                }
                                else
        {
                                    Console.WriteLine("CLIENT IS AUTHENTICATED");
                                    // send false 
                                }
                                break;
                            }
                        case MessageType.MSG:
                            {
                                if (client.State == ClientState.AUTH)
                                {
                                    await SendMessageToChannelAsync(client, messageToProcess);
                                }
                                else
                                {
                                    // ignore 
                                }
                                break;
                            }
                        case MessageType.JOIN:
                            {
                                if (client.State == ClientState.AUTH)
                                {

                                }
                                else
                                {
                                    // ignore
                                }
                                break;
                            }
                        case MessageType.BYE:
                            {
                                // Send confirm, send buy, close connection
                                return;
                                break;
                            }
                        case MessageType.CONFIRM:
                            {
                                client.ClientConfirms.Add(messageToProcess);
                                break;
                            }
                        case MessageType.ERR:
                            {
                                // Gracefully exit by sending BYE
                                return;
                            }
                    }
                } else
                {
                    await Task.Delay(10);
                }
            }
        }
    }

}