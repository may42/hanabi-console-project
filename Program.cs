using System;

namespace my_console_project
{
    class Hanabi
    {
        // Точка входа в программу
        static void Main()
        {
            do
            {
                Console.Clear();
                Game theGame = new Game();
                theGame.StartNewGame();
                Console.WriteLine();
                Console.WriteLine("Press any key to start new game.");
                Console.WriteLine("Press Escape  to quit.");
            } while (Console.ReadKey().Key != ConsoleKey.Escape);
        }
    }
}
