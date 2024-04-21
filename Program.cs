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

﻿namespace IOTA_Chat_Server
{
    internal class Program
    {
        static void Main(string[] args)
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
        {
            Console.WriteLine("Hello, World!");
        }
    }
}