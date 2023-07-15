using System.Diagnostics;
using TwitchTextToVoice.TwitchIntegration;

namespace TwitchTextToVoice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Title = "Twitch text to voice";
            Console.CursorVisible = false;

            Console.WriteLine("Se va a abrir una ventana en el navegador para autorizar la aplicación.");
            

            TokenService tokenService = new TokenService();
            if (tokenService.tokenEntity.error != null)
            {
                Console.Clear();
                Console.WriteLine("Ha ocurrido un error al autorizar la aplicación, ejecutala de nuevo para volver a intentarlo.");
                Console.WriteLine();
                Console.WriteLine("Pulsa cualquier tecla para salir.");
                Console.ReadKey(true);
                Environment.Exit(0);
            }
            else
            {
                Menu(tokenService);
            }
        }

        static void Menu(TokenService tokenService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("**************************************************************************************************************");
                Console.WriteLine();
                Console.WriteLine("Bienvenido al conversor de texto a voz con integración del chat de Twitch.");
                Console.WriteLine("Si se ha abierto una ventana en el navegador ya puedes cerrarla.");
                Console.WriteLine();
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("1.Empezar    2.Opciones    3.Salir");
                char input = Console.ReadKey(true).KeyChar;
                if (input == '1')
                {
                    Application(tokenService);
                }
                else if (input == '2')
                {
                    while (Settings()) continue;
                }
                else if (input == '3')
                {
                    Environment.Exit(0);
                }
            }
        }

        static bool Settings()
        {
            Console.Clear();
            Console.WriteLine("**************************************************************************************************************");
            Console.WriteLine();
            Console.WriteLine("                                 AJUSTES");
            Console.WriteLine();
            Console.WriteLine("Selecciona de quien se leeran los mensajes en voz alta.");
            Console.WriteLine("Introduce un número para cambiar el ajuste, ESC para volver.");
            Console.WriteLine();
            Console.WriteLine("1.Todos - " + (Settings1.Default.todos ? "Si" : "No"));
            Console.WriteLine("2.Suscriptores - " + (Settings1.Default.subs ? "Si" : "No"));
            Console.WriteLine("3.VIPs - " + (Settings1.Default.vips ? "Si" : "No"));
            Console.WriteLine("4.Moderadores - " + (Settings1.Default.mods ? "Si" : "No"));
            var input = Console.ReadKey(true);
            if (input.KeyChar == '1')
            {
                Settings1.Default.todos = !Settings1.Default.todos;
            }
            else if (input.KeyChar == '2')
            {
                Settings1.Default.subs = !Settings1.Default.subs;
            }
            else if (input.KeyChar == '3')
            {
                Settings1.Default.vips = !Settings1.Default.vips;
            }
            else if (input.KeyChar == '4')
            {
                Settings1.Default.mods = !Settings1.Default.mods;
            }
            else if (input.Key == ConsoleKey.Escape)
            {
                return false;
            }
            return true;
        }

        static void Application(TokenService tokenService)
        {
            Console.Clear();
            Console.WriteLine("**************************************************************************************************************");
            Console.WriteLine();
            Console.WriteLine("                                 LEYENDO CHAT");
            Console.WriteLine();
            Console.WriteLine("Pulsa ESC para detener la lectura y volver al menú.");
            ChatBot bot = new ChatBot(tokenService);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Thread t = new Thread(() => bot.Communicate(tokenSource.Token));
            t.Start();



            while (Console.ReadKey(true).Key != ConsoleKey.Escape) continue;

            tokenSource.Cancel();
            t.Join();
            tokenSource.Dispose();
            bot.Disconnect();
        }
    }
}