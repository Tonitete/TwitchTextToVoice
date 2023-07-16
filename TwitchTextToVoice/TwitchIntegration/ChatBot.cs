using System.Globalization;
using System.Net.WebSockets;
using System.Speech.Synthesis;
using System.Text;

namespace TwitchTextToVoice.TwitchIntegration
{
    public class ChatBot
    {
        private string WebsocketURL = "wss://irc-ws.chat.twitch.tv:443";
        private string socketID = string.Empty;

        private Thread voiceReader;
        private List<string> textToRead;
        private PromptBuilder builder = new PromptBuilder();
        private SpeechSynthesizer synt = new SpeechSynthesizer();

        public CancellationTokenSource cancellReading;

        ClientWebSocket _webSocket;

        private TokenService _tokenService;


        public ChatBot(TokenService tokenService)
        {
            _tokenService = tokenService;
            textToRead = new List<string>();
            cancellReading = new CancellationTokenSource();
            voiceReader = new Thread(() => ReadMessagesToVoice(cancellReading.Token));
            voiceReader.Start();
            _webSocket = new ClientWebSocket();

            Console.WriteLine("Conectando con Twitch...");
            Connect().Wait();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Conectado al canal de " + Settings1.Default.channelToJoin);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
        }

        private async Task Connect()
        {
            await _webSocket.ConnectAsync(new Uri(WebsocketURL), cancellReading.Token);

            var buffer = Encoding.UTF8.GetBytes("CAP REQ :twitch.tv/tags twitch.tv/commands");
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            buffer = Encoding.UTF8.GetBytes("PASS oauth:" + _tokenService.tokenEntity.access_token);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            buffer = Encoding.UTF8.GetBytes("NICK " + _tokenService.userName);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            buffer = new byte[1024];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            buffer = Encoding.UTF8.GetBytes("JOIN #" + Settings1.Default.channelToJoin);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            buffer = new byte[1024];
            result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            buffer = new byte[1024];
            result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        public void Communicate(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var buffer = new byte[1024];
                    var result = _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).Result;
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    if (message.StartsWith("PING"))
                    {
                        buffer = Encoding.UTF8.GetBytes("PONG :tmi.twitch.tv");
                        _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                    }
                    else
                    {
                        var parsedMessage = ParseMessage(message);
                        int indx = parsedMessage.Source.IndexOf("!");
                        var usernameSender = parsedMessage.Source.Substring(0, indx).ToLower();

                        if (parsedMessage.Command.Contains("PRIVMSG") && !Settings1.Default.usersBanned.Contains(usernameSender))
                        {

                            if (Settings1.Default.commandRequired && !parsedMessage.Parameters.StartsWith("!" + Settings1.Default.commandText + " "))
                            {
                                continue;
                            }
                            else if (Settings1.Default.commandRequired)
                            {
                                parsedMessage.Parameters = parsedMessage.Parameters.Substring(2 + Settings1.Default.commandText.Length);
                            }

                            if (Settings1.Default.todos)
                            {
                                string user = parsedMessage.Source.Substring(0, parsedMessage.Source.IndexOf('!'));
                                Console.WriteLine(user + " - " + parsedMessage.Parameters);
                                textToRead.Add(parsedMessage.Parameters);
                            }
                            else if (Settings1.Default.mods && parsedMessage.Tags.Contains("moderator=1"))
                            {
                                string user = parsedMessage.Source.Substring(0, parsedMessage.Source.IndexOf('!'));
                                Console.WriteLine(user + " - " + parsedMessage.Parameters);
                                textToRead.Add(parsedMessage.Parameters);
                            }
                            else if (Settings1.Default.subs && parsedMessage.Tags.Contains("subscriber=1"))
                            {
                                string user = parsedMessage.Source.Substring(0, parsedMessage.Source.IndexOf('!'));
                                Console.WriteLine(user + " - " + parsedMessage.Parameters);
                                textToRead.Add(parsedMessage.Parameters);
                            }
                            else if (Settings1.Default.vips && parsedMessage.Tags.Contains("vip=1"))
                            {
                                string user = parsedMessage.Source.Substring(0, parsedMessage.Source.IndexOf('!'));
                                Console.WriteLine(user + " - " + parsedMessage.Parameters);
                                textToRead.Add(parsedMessage.Parameters);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void Disconnect()
        {
            cancellReading.Cancel();
            _webSocket.Dispose();
        }

        private void ReadMessagesToVoice(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (textToRead.Count > 0)
                {
                    builder.ClearContent();
                    builder.StartVoice(new CultureInfo("es-ES"));
                    builder.AppendText(textToRead[0]);
                    builder.EndVoice();
                    synt.Speak(builder);
                    builder.ClearContent();
                    textToRead.RemoveAt(0);
                }
            }
        }

        private TwitchMessage ParseMessage(string message)
        {
            int idx = 0;

            string rawTagsComponent = "";
            string rawSourceComponent = "";
            string rawCommandComponent = "";
            string rawParametersComponent = "";

            if (message[idx] == '@')
            {
                int endIdx1 = message.IndexOf(' ');
                rawTagsComponent = message.Substring(1, endIdx1);
                idx = endIdx1 + 1;
            }

            if (message[idx] == ':')
            {
                idx++;
                int endIdx1 = message.IndexOf(' ', idx);
                rawSourceComponent = message.Substring(idx, endIdx1 - idx);
                idx = endIdx1 + 1;
            }

            int endIdx = message.IndexOf(':', idx);
            if (endIdx < 0)
            {
                endIdx = message.Length;
            }

            rawCommandComponent = message.Substring(idx, endIdx - idx).Trim();

            if (endIdx != message.Length)
            {
                idx = endIdx + 1;
                rawParametersComponent = message.Substring(idx);
            }

            var result = new TwitchMessage
            {
                Tags = rawTagsComponent,
                Source = rawSourceComponent,
                Command = rawCommandComponent,
                Parameters = rawParametersComponent
            };

            return result;
        }
    }
}
