using System;

namespace my_console_project
{
    class Program
    {
        static void Main()
        {
            do
            {
                try
                {
#if DEBUG
                    Console.WriteLine("Creating new Game instance. Awaiting for Start-command");
#endif
                    Hanabi game = new Hanabi();
                    // Содержимое метода StartNewGame я бы вообще добавил внутрь конструктора
                    game.StartNewGame(Console.ReadLine());
#if DEBUG
                    Console.WriteLine("Game ended with OnGameOver() method");
#endif
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Sorry, game crashed! Unknown error: {0}\n{1}", e.GetType(), e.Message);
#if DEBUG
                    Console.WriteLine("Stack trace:\n{0}", e.StackTrace);
#endif
                }
            } while (true);
        }
    }
}
