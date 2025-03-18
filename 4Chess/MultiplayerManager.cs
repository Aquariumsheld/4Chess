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
        public static bool IsMultiplayer = false;
        public static bool IsHost = false;
        public static int Port = 8080;
        public static string HostIp = "";

        private static HttpListener listener;
        public static WebSocket HostSocket;
        public static ClientWebSocket ClientSocket;
        public static bool Connected = false;

        private static readonly System.Collections.Concurrent.ConcurrentQueue<string> ReceivedMessages = new System.Collections.Concurrent.ConcurrentQueue<string>();

        /// <summary>
        /// Startet den WebSocket‑Server (Host) und wartet auf einen Client.
        /// </summary>
        public static async Task StartHostingAsync()
        {
            try
            {
                HostIp = GetLocalIPAddress();
                listener = new HttpListener();
                listener.Prefixes.Add("http://" + HostIp + ":" + Port + "/");
                listener.Start();
                Console.WriteLine("Hosting on: " + HostIp + ":" + Port);
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    HostSocket = wsContext.WebSocket;
                    Connected = true;
                    Console.WriteLine("Client connected.");
                    await ReceiveLoop(HostSocket);
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
        /// Verbindet als Client zu einem WebSocket‑Server.
        /// </summary>
        public static async Task JoinGameAsync(string serverIp)
        {
            try
            {
                ClientSocket = new ClientWebSocket();
                Uri serverUri = new Uri("ws://" + serverIp + ":" + Port + "/");
                await ClientSocket.ConnectAsync(serverUri, CancellationToken.None);
                Connected = true;
                Console.WriteLine("Connected to host.");
                await ReceiveLoop(ClientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error joining game: " + ex.Message);
            }
        }

        /// <summary>
        /// Sendet eine Nachricht (z. B. Move‑Nachricht) über den aktiven Socket.
        /// </summary>
        public static async Task SendMessageAsync(string message)
        {
            if (!Connected)
                return;
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            if (IsHost)
            {
                await HostSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                await ClientSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// Empfängt Nachrichten in einer Schleife und legt sie in einer Queue ab.
        /// </summary>
        private static async Task ReceiveLoop(WebSocket socket)
        {
            byte[] buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ReceivedMessages.Enqueue(msg);
                    Console.WriteLine("Received: " + msg);
                }
            }
        }

        /// <summary>
        /// Gibt die nächste empfangene Nachricht zurück, falls vorhanden.
        /// </summary>
        public static bool TryDequeueMessage(out string message)
        {
            return ReceivedMessages.TryDequeue(out message);
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
