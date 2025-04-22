using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using BIERKELLER.BIERUI;
using System.ComponentModel;
using static Raylib_CsLo.Raylib;
using static System.Net.Mime.MediaTypeNames;
using Raylib_CsLo;
using _4Chess.Game.Input;

namespace _4Chess.Game.Multiplayer
{
    public static class MultiplayerManager
    {
        public static bool IsMultiplayer = false;
        public static bool IsHost = true;
        public static bool IsHostingLive = false;
        public static bool IsHostingLiveERROR = false;
        public static bool IsPlayerContected = false;
        public static bool IsPlayerContectedERROR = false;

        public static int Port = 5000;
        public static string HostIp = "";

        private static Raylib_CsLo.Font _romulusFont;
        public static List<BIERUIComponent> UIComponents { get; set; } = [];

        private static HttpListener? _listener;
        public static WebSocket? HostSocket;
        public static ClientWebSocket? ClientSocket;
        public static bool Connected = false;

        private static readonly System.Collections.Concurrent.ConcurrentQueue<string> ReceivedMessages = new();

        /// <summary>
        /// Startet den WebSocket‑Server (Host) und wartet auf einen Client.
        /// </summary>
        public static async Task StartHostingAsync()
        {
            try
            {
                HostIp = GetLocalIPAddress();
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://" + HostIp + ":" + Port + "/");
                _listener.Start();
                Console.WriteLine("Hosting on: " + HostIp + ":" + Port);
                IsHostingLive = true;
                HttpListenerContext context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    HostSocket = wsContext.WebSocket;
                    Connected = true;
                    Console.WriteLine("Client connected.");
                    IsPlayerContected = true;
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
                IsHostingLiveERROR = true;
            }
        }

        /// <summary>
        /// Verbindet als Client zu einem WebSocket‑Server.
        /// </summary>
        public static async Task JoinGameAsync(string serverIp)
        {
            try
            {
                string trimmedIp = serverIp.Trim();
                if (trimmedIp.Contains(":"))
                {
                    trimmedIp = trimmedIp.Split(':')[0];
                }
                Uri serverUri = new($"ws://{trimmedIp}:{Port}/");
                ClientSocket = new ClientWebSocket();
                await ClientSocket.ConnectAsync(serverUri, CancellationToken.None);
                Connected = true;
                Console.WriteLine("Connected to host.");
                await ReceiveLoop(ClientSocket);

                IsPlayerContected = true;
               //_4ChessMove.TurnChange();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error joining game: " + ex.Message);
                IsPlayerContectedERROR = true;
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
            if (IsHost && HostSocket != null)
            {
                await HostSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else if (ClientSocket != null)
            {
                await ClientSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            _4ChessGame.IsLocalTurn = false;
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
            var msg = ReceivedMessages.TryDequeue(out string? result);
            message = result ?? "";
            return msg;
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
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && ip.ToString().Contains('.') && ip.ToString().Split(".")[0] != "127")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
