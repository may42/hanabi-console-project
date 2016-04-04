using System;
using System.Text.RegularExpressions;

namespace my_console_project
{
    public class GameCommandException : ArgumentException
    {
        public GameCommandException()
            : base()
        {
        }
        public GameCommandException(string message)
            : base(message)
        {
        }
        public GameCommandException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    class Program
    {
        private static Hanabi game;

        private static bool GameIsCurrentlyPlayed()
        {
            return game != null && game.GameIsActive;
        }

        private static Match ProcessCommand(string command, string pattern, string expected)
        {
            if (!GameIsCurrentlyPlayed() && !command.StartsWith("Start"))
            {
                throw new GameCommandException("Cant process command, no game is currently played");
            }
            Match match = new Regex(pattern).Match(command);
            if (!match.Success)
            {
                throw new GameCommandException("Expected \"" + expected + "\", instead got: " + command);
            }
            return match;
        }

        private static void ParseAndRunCommand(string command)
        {
            Match match;
            if (string.IsNullOrEmpty(command) || command == "esc" || command == "exit" || command == "quit" || command == "q")
            {
                Environment.Exit(0);
            }
            else if (command.StartsWith("Start"))
            {
                match = ProcessCommand(command, @"^Start new game with deck(( +\w+)+) *$", "Start new game with deck %ABBREVIATIONS%");
                Hanabi newGame = new Hanabi(match.Groups[1].ToString());
#if DEBUG
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                if (GameIsCurrentlyPlayed()) Console.WriteLine("Previous game was terminated, new game was created");
                Console.ResetColor();
                //game.GameOver += reason => Console.WriteLine("Game over. Reason: {0}", reason);
                newGame.GameOver += reason =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Game over. Reason: {0}", reason);
                    Console.ResetColor();
                };
#endif
                // Writing into separated local variable is needed for correct detection of previous game termination
                game = newGame;
            }
            else if (command.StartsWith("Tell color"))
            {
                match = ProcessCommand(command, @"^Tell color (\w+) for cards(( \d+)+) *$", "Tell color %COLOR_NAME% for cards %CARD_NUMBERS%");
                game.TellColor(match.Groups[1].ToString(), match.Groups[2].ToString());
            }
            else if (command.StartsWith("Tell rank"))
            {
                match = ProcessCommand(command, @"^Tell rank (\d+) for cards(( \d+)+) *$", "Tell rank %RANK_NO% for cards %CARD_NUMBERS%");
                game.TellRank(match.Groups[1].ToString(), match.Groups[2].ToString());
            }
            else if (command.StartsWith("Play card"))
            {
                match = ProcessCommand(command, @"^Play card (\d+) *$", "Play card %CARD_NUMBER%");
                game.PlayCard(match.Groups[1].ToString());
            }
            else if (command.StartsWith("Drop card"))
            {
                match = ProcessCommand(command, @"^Drop card (\d+) *$", "Drop card %CARD_NUMBER%");
                game.DropCard(match.Groups[1].ToString());
            }
            else
            {
                throw new GameCommandException("Unknown command: " + command);
            }
        }

        static void Main()
        {
#if DEBUG
            Console.WriteLine("Hanabi game is loaded, starting main command loop, awaiting for commands");
#endif
            while (true)
            {
                try
                {
                    ParseAndRunCommand(Console.ReadLine());
                }
                catch (GameCommandException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    game = null;
#if DEBUG
                    Console.BackgroundColor = ConsoleColor.Red;
#endif
                    Console.WriteLine("Sorry, game crashed! Unknown error: {0}\n{1}", e.GetType(), e.Message);
#if DEBUG
                    Console.ResetColor();
                    Console.WriteLine("Stack trace:\n{0}", e.StackTrace);
#endif
                }
            }
        }
    }
}
