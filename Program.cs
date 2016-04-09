using System;
using System.Text.RegularExpressions;

namespace my_console_project
{
    /// <summary>Special exception class, for handling errors, caused by incorrect commands</summary>
    class GameCommandException : ArgumentException
    {
        public GameCommandException()
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
        static Hanabi game;

        /// <summary>Checks if there is an active game at the moment</summary>
        static bool GameIsCurrentlyPlayed()
        {
            return game != null && !game.GameIsFinished;
        }

        /// <summary>Display message in console without changing color</summary>
        static void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>Display message in console using specific text color</summary>
        static void DisplayMessage(string message, ConsoleColor messageColor)
        {
            Console.ForegroundColor = messageColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>Parses game command and checks the validity of its format</summary>
        /// <param name = "command">Parsed command</param>
        /// <param name = "pattern">Regex format pattern, captures command parameters</param>
        /// <param name = "expectedFormat">Expected format that will be placed as a message
        /// in thrown GameCommandException, if command does not satisfy the pattern</param>
        /// <returns>Match-object with command parameters</returns>
        static Match ParseCommand(string command, string pattern, string expectedFormat)
        {
            if (!GameIsCurrentlyPlayed() && !command.StartsWith("Start"))
            {
                throw new GameCommandException("Cant process command, no game is currently played");
            }
            Match match = new Regex(pattern).Match(command);
            if (!match.Success)
            {
                throw new GameCommandException($"Expected \"{expectedFormat}\", instead got: \"{command}\"");
            }
            return match;
        }

        /// <summary>Checks spelling of input commands, determines them and performs corresponding actions</summary>
        /// <remarks>Most input-output formatting is handled here</remarks>
        /// <param name = "command">Input command</param>
        static void ProcessAndRunCommand(string command)
        {
            Match match;
            if (string.IsNullOrEmpty(command) || command == "exit" || command == "quit" || command == "q")
            {
                Environment.Exit(0);
            }
            else if (command.StartsWith("Start"))
            {
                match = ParseCommand(command, @"^Start new game with deck(( +\w+)+) *$",
                                                "Start new game with deck %ABBREVIATIONS%");
                // This local variable is needed for correct detection of previous game termination.
                Hanabi newGame = new Hanabi(match.Groups[1].ToString());
                newGame.GameOver += s => DisplayMessage(newGame.Stats);
#if DEBUG
                if (GameIsCurrentlyPlayed()) {
                    DisplayMessage("Previous game was terminated, new game was created", ConsoleColor.DarkYellow);
                }
                DisplayMessage(newGame.DetailedStats, ConsoleColor.DarkCyan);
                newGame.MovePerformed += () => DisplayMessage(newGame.DetailedStats, ConsoleColor.DarkCyan);
                newGame.GameOver += reason => DisplayMessage("Game over. Reason: " + reason, ConsoleColor.DarkYellow);
                newGame.RiskyMove += info => DisplayMessage("Risky move " + info, ConsoleColor.Red);
#endif
                game = newGame;
            }
            else if (command.StartsWith("Tell color"))
            {
                match = ParseCommand(command, @"^Tell color (\w+) for cards(( \d+)+) *$",
                                                "Tell color %COLOR_NAME% for cards %CARD_NUMBERS%");
                game.TellColor(match.Groups[1].ToString(), match.Groups[2].ToString());
            }
            else if (command.StartsWith("Tell rank"))
            {
                match = ParseCommand(command, @"^Tell rank (\d+) for cards(( \d+)+) *$",
                                                "Tell rank %RANK_NUMBER% for cards %CARD_NUMBERS%");
                game.TellRank(match.Groups[1].ToString(), match.Groups[2].ToString());
            }
            else if (command.StartsWith("Play card"))
            {
                match = ParseCommand(command, @"^Play card (\d+) *$", "Play card %CARD_NUMBER%");
                game.PlayCard(match.Groups[1].ToString());
            }
            else if (command.StartsWith("Drop card"))
            {
                match = ParseCommand(command, @"^Drop card (\d+) *$", "Drop card %CARD_NUMBER%");
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
            DisplayMessage("Hanabi game is loaded, starting main command loop, awaiting for commands",
                ConsoleColor.DarkGreen);
#endif
            while (true)
            {
                try
                {
                    ProcessAndRunCommand(Console.ReadLine());
                }
                catch (GameCommandException e)
                {
                    DisplayMessage(e.Message, ConsoleColor.DarkRed);
                }
                catch (Exception e)
                {
                    DisplayMessage($"Game terminated because of exception: {e.GetType()}\n{e.Message}",
                        ConsoleColor.Red);
#if DEBUG
                    DisplayMessage($"Stack trace:\n{e.StackTrace}", ConsoleColor.DarkGray);
                    if (e.InnerException != null)
                    {
                        DisplayMessage($"Inner exception: {e.InnerException.GetType()}\n{e.InnerException.Message}",
                            ConsoleColor.Red);
                        DisplayMessage($"Inner exception stack trace:\n{e.InnerException.StackTrace}",
                            ConsoleColor.DarkGray);
                    }
#endif
                    game = null;
                }
            }
        }
    }
}
