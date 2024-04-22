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
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Net.Http.Json;
using System.Reflection;

namespace IOTA_Chat_Server
{
    internal class Program
    {
        private static ConcurrentDictionary<string, Client> clients = new ConcurrentDictionary<string, Client>();
        public static byte udpRetransmissionLimit = 3;
        public static UInt16 udpRetransmissionTimeout = 250;
        private static string defaultChannelId = "default"; 
        static async Task Main(string[] args)
        {
            string? listeningIP = null;
            ushort? serverPort = null;
            ushort? timeout = null;
            byte retransmissions = 0;

            // Register the Console.CancelKeyPress event handler
            Console.CancelKeyPress += async (sender, e) =>
            {
                // Prevent the application from terminating immediately
                e.Cancel = true;

                // Send bye messages to all clients
                await GracefullExit();

                // Exit the application
                Environment.Exit(0);
            };

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
                Console.WriteLine($"{e}");
                return;
            }
            LogMessageReceived(result.RemoteEndPoint, messageParsed);
            // Client creation
            IPEndPoint clientEP = result.RemoteEndPoint;
            UdpClient? localEP = new UdpClient();
            localEP.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            Client? client = new Client(clientEP, localEP);

            client.MessagesUprocessed.Add(messageParsed);
            var sender = Task.Run(() => SenderAsync(client));
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


                // Check if user was in the channel before
                if (client.CurrentChannelName != null)
                {
                    // Create left message
                    Message msgLeft = new Message(client.messageId++, MessageType.MSG);
                    msgLeft.Content = $"{client.Displayname} has left {client.CurrentChannelName}.";
                    msgLeft.DisplayName = client.Displayname;
                    // Broadcast left message
                    await SendMessageToChannelAsync(client, msgLeft);
                }

                client.CurrentChannelName = channelId;

                // Create join message
                client.CurrentChannelName = channelId;
                Message msgJoin = new Message(client.messageId++, MessageType.MSG);
                msgJoin.Content = $"{client.Displayname} has joined {channelId}.";
                msgJoin.DisplayName = client.Displayname;

                // Send join message to original sender 
                await SendAndWaitForConfirmAsync(client, msgJoin);
                // Broadcast join message
                await SendMessageToChannelAsync(client, msgJoin);
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

                Task getConfirm = Task.Run(() => { existingMessage = client.ClientConfirms.Take(); });  
                Task delay = Task.Delay(udpRetransmissionTimeout); 
                Task firstCompleted = await Task.WhenAny(getConfirm, delay); 
                if (firstCompleted == getConfirm && getConfirm.IsCompletedSuccessfully && existingMessage != null)
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
        public static async Task SendMessageToChannelAsync(Client sender, Message msg)
        {
    
            var recipients = clients.Where(pair => pair.Value.CurrentChannelName == sender.CurrentChannelName);

            foreach (var pair in recipients)
            {
                // Ingore the sender
                if (pair.Value == sender) continue;
                bool messageConfirmed = await SendAndWaitForConfirmAsync(pair.Value, msg);
                if (!messageConfirmed)
                {
                    // close connection with client
                }
            }
        }

        public static async Task GracefullExit()
        {
            foreach (var pair in clients)
            {
                Message byeMsg = new Message(pair.Value.messageId++, MessageType.BYE);
                await SendAndWaitForConfirmAsync(pair.Value, byeMsg);
            }
        }
        public static async Task GracefullFinishClient(UdpClient server, IPEndPoint clientEP)
        {
        }

        public static void LogMessageReceived(IPEndPoint clientEP, Message msg)
        {

            string msgType = MTypeToBytesConverter.MTypeToString(msg.Type);
            Console.WriteLine($"RECV {clientEP.Address.ToString()}:{clientEP.Port} | {msgType} {msg.Content}");
        }
        public static void LogMessageSent(IPEndPoint clientEP, Message msg)
        {
            string msgType = MTypeToBytesConverter.MTypeToString(msg.Type);
            Console.WriteLine($"SENT {clientEP.Address.ToString()}:{clientEP.Port} | {msgType} {msg.Content}");
        }
        public static async Task ListenerAsync(Client client)
        {
            while (client.Exit != true)
            {
                UdpReceiveResult receiveResult = await client.ServerEndPoint.ReceiveAsync();
                Message newMessage = MessageToBytesConverter.BytesToMessage(receiveResult.Buffer);
                LogMessageReceived(client.ClientEndPoint, newMessage);
                if (newMessage.Type == MessageType.CONFIRM)
                {
                    client.ClientConfirms.Add(newMessage);
                }
                else
                {
                    // Check if the message was already processed
                    if (client.ServerConfirmIds.Any(id => id == newMessage.Id) == false)
                    {
                        client.ServerConfirmIds.Add(newMessage.Id);
                        client.MessagesUprocessed.Add(newMessage);
                    }    
                }
            }
        }

        public static async Task SenderAsync(Client client)
        {

            while (client.Exit != true || client.MessagesUprocessed.Count > 0)
            {
                if (client.MessagesUprocessed.Count > 0)
                {
                    Message messageToProcess = client.MessagesUprocessed.Take();

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
                                        clients.TryAdd(client.Username, client);
                                    }

                                    client.State = ClientState.AUTH;
                                    client.Displayname = messageToProcess.DisplayName;
                                    await HandleJoinAsync(client, defaultChannelId, messageToProcess.Id, "You are authenticated");
                                }
                                break;
                            }
                        case MessageType.MSG:
                            {
                                if (client.State == ClientState.AUTH)
                                {
                                    if (messageToProcess.DisplayName != null)
                                        client.Displayname = messageToProcess.DisplayName;
                                    await SendMessageToChannelAsync(client, messageToProcess);
                                }
                                break;
                            }
                        case MessageType.JOIN:
                            {
                                if (client.State == ClientState.AUTH)
                                {
                                    await HandleJoinAsync(client, messageToProcess.ChannelID, messageToProcess.Id, "You joined channel");
                                }
                                break;
                            }
                        case MessageType.BYE:
                            {


                                // Check if user was in the channel before
                                if (client.CurrentChannelName != null)
                                {
                                    // Create left message
                                    Message msgLeft = new Message(client.messageId++, MessageType.MSG);
                                    msgLeft.Content = $"{client.Displayname} has left {client.CurrentChannelName}.";
                                    msgLeft.DisplayName = client.Displayname;
                                    // Broadcast left message
                                    await SendMessageToChannelAsync(client, msgLeft);
                                }
                                // Remove client
                                clients.TryRemove(client.Username, out _);

                                return;
                            }
                        case MessageType.CONFIRM:
                            {
                                client.ClientConfirms.Add(messageToProcess);
                                break;
                            }
                        case MessageType.ERR:
                            {
                                
                                // Send bye
                                Message byeMsg = new Message(client.messageId++, MessageType.BYE);
                                await SendAndWaitForConfirmAsync(client, byeMsg);

                                // Check if user was in the channel before
                                if (client.CurrentChannelName != null)
                                {
                                    // Create left message
                                    Message msgLeft = new Message(client.messageId++, MessageType.MSG);
                                    msgLeft.Content = $"{client.Displayname} has left {client.CurrentChannelName}.";
                                    msgLeft.DisplayName = client.Displayname;
                                    // Broadcast left message
                                    await SendMessageToChannelAsync(client, msgLeft);
                                }

                                // Remove client
                                clients.TryRemove(client.Username, out _);
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