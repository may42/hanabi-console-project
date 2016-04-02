using System;

namespace my_console_project
{
    class Hanabi
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
                    Game theGame = new Game();
                    // Содержимое метода StartNewGame я бы вообще добавил внутрь конструктора
                    theGame.StartNewGame(Console.ReadLine());
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
