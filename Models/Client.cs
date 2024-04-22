using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace IOTA_Chat_Server.Models
{
    public class Client
    {
        public string Username { get; set; }
        public string Displayname { get; set; }
        public string? CurrentChannelName { get; set; }
        public IPEndPoint ClientEndPoint { get; set; }
        public BlockingCollection<Message> ClientConfirms { get; set; }
        public BlockingCollection<ushort> ServerConfirmIds { get; set; }
        public BlockingCollection<Message> MessagesUprocessed { get; set; }
        public BlockingCollection<byte> BytesSent { get; set; }
        public UdpClient ServerEndPoint { get; set; }
        public UInt16 messageId { get; set; }
        public ClientState State { get; set; }

        public bool Exit { get; set; }
    

        public Client(IPEndPoint clientEP, UdpClient serverEP)
        { 
            CurrentChannelName = null;
            ClientEndPoint = clientEP;
            ServerEndPoint = serverEP;
            ServerConfirmIds = new BlockingCollection<ushort>();
            ClientConfirms = new BlockingCollection<Message>();
            MessagesUprocessed = new BlockingCollection<Message>();
            BytesSent = new BlockingCollection<byte>();
            State = ClientState.NONAUTH;
            messageId = 0;
            Exit = false;
        }
    }
    
    public enum ClientState
    {
    
        NONAUTH, 
        AUTH,
        BYE
    }
};
