using TwitchTextToVoice.TwitchIntegration;

namespace TwitchTextToVoice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (Settings1.Default.usersBanned == null)
            {
                Settings1.Default.usersBanned = new System.Collections.Specialized.StringCollection();
                Settings1.Default.Save();
            }
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
                Console.WriteLine("1.Empezar    2.Opciones    3.Salir   4.Volumen: " + Settings1.Default.volume.ToString() + "%");
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
                else if (input == '4')
                {
                    Console.Clear();
                    Console.WriteLine("**************************************************************************************************************");
                    Console.WriteLine();
                    Console.WriteLine("Introduce un número del 0 al 100 y pulsa enter para aplicar.");
                    string textInput = Console.ReadLine();
                    int volume;
                    if (int.TryParse(textInput, out volume))
                    {
                        if (volume < 0) volume = 0;
                        else if (volume > 100) volume = 100;
                        Settings1.Default.volume = volume;
                        Settings1.Default.Save();
                    }
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
            Console.WriteLine("5.BETA Canal que leer - " + Settings1.Default.channelToJoin);
            Console.WriteLine("6.Leer solo comandos (ej. !tts) - " + (Settings1.Default.commandRequired ? "Si" : "No"));
            Console.WriteLine("7.Texto del comando - !" + Settings1.Default.commandText);
            Console.WriteLine("8.Ver usuarios baneados");
            Console.WriteLine("9.Leer nombre de usuario - " + (Settings1.Default.leerNombreDeUsuario ? "Si" : "No"));

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
            else if (input.KeyChar == '5')
            {
                Console.Clear();
                Console.WriteLine("Introduce el nombre del canal al que conectarse y pulsa enter:");
                Settings1.Default.channelToJoin = Console.ReadLine().ToLower();
            }
            else if (input.KeyChar == '6')
            {
                Settings1.Default.commandRequired = !Settings1.Default.commandRequired;
            }
            else if (input.KeyChar == '7')
            {
                Console.Clear();
                Console.WriteLine("Introduce el nombre del comando (sin el simbolo de exclamación) y pulsa enter:");
                Settings1.Default.commandText = Console.ReadLine().ToLower();
            }
            else if (input.KeyChar == '8')
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Lista de usuarios baneados:");
                    Console.WriteLine();
                    string[] strArray = new string[Settings1.Default.usersBanned.Count];
                    Settings1.Default.usersBanned.CopyTo(strArray, 0);
                    string text = string.Join(", ", strArray);
                    Console.WriteLine($"{text}");
                    Console.WriteLine();
                    Console.WriteLine("1.Añadir nuevo usuario    2.Borrar usuario    3.Volver");

                    char input2 = Console.ReadKey(true).KeyChar;
                    if (input2 == '1')
                    {
                        Console.Clear();
                        Console.WriteLine("Escribe el nombre del usuario y pulsa enter para banearlo.");
                        string newUserBanned = Console.ReadLine().ToLower();
                        Settings1.Default.usersBanned.Add(newUserBanned);
                    }
                    else if (input2 == '2')
                    {
                        Console.Clear();
                        Console.WriteLine("Lista de usuarios baneados:");
                        Console.WriteLine();
                        strArray = new string[Settings1.Default.usersBanned.Count];
                        Settings1.Default.usersBanned.CopyTo(strArray, 0);
                        Console.WriteLine(string.Join(", ", strArray));
                        Console.WriteLine();
                        Console.WriteLine("Escribe el nombre de un usuario y pulsa enter para borrarlo de la lista:");
                        string userToDelete = Console.ReadLine();
                        try
                        {
                            Settings1.Default.usersBanned.Remove(userToDelete.ToLower());
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("No se ha podido borrar al usuario, está escrito incorrectamente o este no existe.");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Pulsa cualquier tecla para continuar.");
                            Console.ReadKey(true);
                        }
                    }
                    else if (input2 == '3')
                    {
                        break;
                    }
                }
            }
            else if (input.KeyChar == '9')
            {
                Settings1.Default.leerNombreDeUsuario = !Settings1.Default.leerNombreDeUsuario;
            }
            else if (input.Key == ConsoleKey.Escape)
            {
                Settings1.Default.Save();
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