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
                Console.Clear();
                Console.WriteLine("**************************************************************************************************************");
                Console.WriteLine();
                Console.WriteLine("Bienvenido al conversor de texto a voz con integración del chat de Twitch.");
                Console.WriteLine("Si se ha abierto una ventana en el navegador ya puedes cerrarla.");
                Console.WriteLine();
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("1.Empezar    2.Opciones    3.Salir");
            }

            while (true)
            {
                char input = Console.ReadKey(true).KeyChar;
                if (input == '1')
                {

                }
                else if (input == '2')
                {
                    Settings();
                }
                else if (input == '3')
                {
                    Environment.Exit(0);
                }
            }
        }

        static void Settings()
        {
            Console.Clear();
            Console.WriteLine("**************************************************************************************************************");
            Console.WriteLine();
            Console.WriteLine("                                 AJUSTES");
            Console.WriteLine();
            Console.WriteLine("Introduce un número para cambiar el ajuste:");
            Console.WriteLine();
            Console.WriteLine("1.Nombre de usuario");
            Console.WriteLine("2.Codigo de validación");
        }

        static void Application()
        {

        }
    }
}