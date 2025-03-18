using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace _4Chess.Game.Multiplayer
{
    public static class MultiplayerManager
    {
        public static bool IsMultiplayer { get; set; } = false;
        public static bool IsHost { get; set; } = false;
        public static string HostIp { get; set; } = "";
        public static int Port { get; set; } = 8080;

        private static HttpListener listener;
        public static WebSocket HostSocket { get; private set; }
        public static ClientWebSocket ClientSocket { get; private set; }

        /// <summary>
        /// Startet einen WebSocket-Server auf der lokalen IP und wartet auf eine Verbindung.
        /// </summary>
        public static async Task StartHostingAsync()
        {
            try
            {
               HostIp = GetLocalIPAddress();
                Console.WriteLine("Hosting on: " + HostIp);
                listener = new HttpListener();
                listener.Prefixes.Add("http://" + HostIp + ":" + Port + "/");
                listener.Start();
                Console.WriteLine("WebSocket server started. Waiting for client...");

                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    HostSocket = wsContext.WebSocket;
                    Console.WriteLine("Client connected.");
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while hosting: " + ex.Message);
            }
        }

        /// <summary>
        /// Verbindet als Client zu einem WebSocket-Server, dessen IP der Spieler eingegeben hat.
        /// </summary>
        public static async Task JoinGameAsync(string serverIp)
        {
            try
            {
                ClientSocket = new ClientWebSocket();
                Uri serverUri = new Uri("ws://" + serverIp + ":" + Port + "/");
                await ClientSocket.ConnectAsync(serverUri, CancellationToken.None);
                Console.WriteLine("Connected to host.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error joining game: " + ex.Message);
            }
        }

        /// <summary>
        /// Ermittelt die lokale IPv4-Adresse.
        /// </summary>
        public static string GetLocalIPAddress()
        {
            string localIP = "127.0.0.1";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
