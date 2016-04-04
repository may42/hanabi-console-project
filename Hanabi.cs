using System;
using System.Collections.Generic;

namespace my_console_project
{
    class Hanabi
    {
        public delegate void GameMethodsContainer(string info);
        public event GameMethodsContainer GameOver = delegate { }; // Заглушка чтобы не проверять на null

    #region Fields

        private readonly Player player1, player2;
        private Player currentActivePlayer, currentPassivePlayer;
        // todo: init those fields with constructor:
        public readonly int NumberOfCardsInPlayersHand = 5, RankLimit = 5, NumberOfColors = 5;
        public readonly int MaxCardsAmountOnTable, MinCardsAmountInDeck;

    #endregion
    #region Props

        public bool GameIsActive { get; private set; }
        public int CurrentTurn { get; private set; }
        public int SuccessfullyPlayedCards { get; private set; }
        public int MovesWithRisk { get; private set; }
        public List<Card> Deck { get; }

        public readonly Dictionary<Card.Colors, int> ColorSequences = new Dictionary<Card.Colors, int>();

    #endregion
    #region Constructors

        /// <summary>
        /// Starts new Hanabi game with diven deck
        /// </summary>
        /// <param name = "abbreviations">String, that contains all abbreviations of starting deck</param>
        public Hanabi(string abbreviations)
        {
            CurrentTurn = SuccessfullyPlayedCards = MovesWithRisk = 0;
            // На случай изменения максимального количества карт на руках кастомным конструктором:
            MaxCardsAmountOnTable = RankLimit * NumberOfColors;
            MinCardsAmountInDeck = NumberOfCardsInPlayersHand * 2 + 1;
            Deck = new List<Card>();
            FillDeck(abbreviations);
            GameIsActive = true;
            foreach (Card.Colors color in Enum.GetValues(typeof(Card.Colors)))
            {
                ColorSequences.Add(color, 0);
            }
            currentActivePlayer = player1 = new Player(this);
            currentPassivePlayer = player2 = new Player(this);
            // Подписка метода OnGameOver() на событие GameOver
            GameOver += GameOverActions;
#if DEBUG
            ShowStatistics();
#endif
        }

    #endregion
    #region Private Methods

        /// <summary>
        /// Move exchange from fisrt player to second player, while game is played
        /// </summary>
        private void ChangeMove()
        {
            CurrentTurn++;
            currentActivePlayer = CurrentTurn % 2 == 0 ? player1 : player2;
            currentPassivePlayer = CurrentTurn % 2 == 1 ? player1 : player2;
#if DEBUG
            Console.ForegroundColor = ConsoleColor.DarkGray;
            ShowStatistics();
            Console.ResetColor();
#endif
        }

        /// <summary>
        /// Check if it's possible to play card of given color and rank
        /// </summary>
        /// <param name = "color">Color, which need to be checked</param>
        /// <param name = "rank">Rank, which need to be checked</param>
        /// <returns><value>true</value> if card of given color and rank can be legally played, <value>false</value> otherwise</returns>
        private bool CheckMoveAvaliability(Card.Colors color, int rank)
        {
            if (!ColorSequences.ContainsKey(color))
            {
                throw new ArgumentException("Failed to find this color in current sequences dictrionary: " + color);
            }
            if (rank < 1 || rank > RankLimit)
            {
                throw new ArgumentOutOfRangeException("Rank out of range: " + rank);
            }
            return ColorSequences[color] + 1 == rank;
        }

        /// <summary>
        /// Last game actions, berfore closing: increment current turn, display game results
        /// </summary>
        /// <param name = "info">Unused parameter, containg GameOver reason</param>
        private void GameOverActions(string info)
        {
            CurrentTurn++;
            GameIsActive = false;
            ShowStatistics();
        }

        /// <summary>
        /// Writes game results to console
        /// </summary>
        private void ShowStatistics()
        {
            // todo: replace this with method public string GiveStatistics() + event SuccessfulMove
            Console.WriteLine("Turn: {0}, cards: {1}, with risk: {2}", CurrentTurn, SuccessfullyPlayedCards, MovesWithRisk);
        }

        /// <summary>
        /// Filling deck with cards, written in input text
        /// </summary>
        /// <param name = "abbreviationsString">Text with cards abbreviations kind of <value>"R1"</value>, <value>"Y3"</value>, <value>"W4"</value>...</param>
        private void FillDeck(string abbreviationsString)
        {
            if (string.IsNullOrEmpty(abbreviationsString))
            {
                throw new GameCommandException("Card abbreviations expected");
            }
            string[] abbreviations = abbreviationsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (abbreviations.Length < MinCardsAmountInDeck)
            {
                throw new GameCommandException("Expected deck to be at least " + MinCardsAmountInDeck + " cards long, instead got " + abbreviations.Length);
            }
            foreach (string a in abbreviations)
            {
                Deck.Add(new Card(a));
            }
        }

        /// <summary>
        /// Incases high rank in color line of cards which was successfully played
        /// </summary>
        /// <param name = "color">Line of which color need to be changed</param>
        private void IncraseLine(Card.Colors color)
        {
            if (!ColorSequences.ContainsKey(color))
            {
                throw new ArgumentException("Failed to find this color in current sequences dictrionary: " + color);
            }
            if (ColorSequences[color] == RankLimit)
            {
                throw new InvalidOperationException("Cannot increase color sequence " + color + ", maximum rank " + RankLimit + " is reached!");
            }
            ColorSequences[color]++;
        }

        /// <summary>
        /// Parsing string into array, containg card numbers
        /// </summary>
        /// <param name = "cardNumbersString">String containg one or multiple card numbers</param>
        /// <returns>Array, containg one or multiple card numbers</returns>
        private int[] ParseCardNumbers(string cardNumbersString)
        {
            if (string.IsNullOrEmpty(cardNumbersString))
            {
                throw new GameCommandException("Card number string expected");
            }
            string[] stringsArray = cardNumbersString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int[] result = new int[stringsArray.Length];
            if (stringsArray.Length < 1)
            {
                throw new GameCommandException("Expected at least one card number");
            }
            for (int i = 0; i < stringsArray.Length; i++)
            {
                int n;
                if (!int.TryParse(stringsArray[i], out n))
                {
                    throw new GameCommandException("Card number must be an integer: " + stringsArray[i]);
                }
                if (n < 0 || n >= NumberOfCardsInPlayersHand)
                {
                    throw new GameCommandException("Card number out of range: " + n);
                }
                result[i] = n;
            }
            return result;
        }

#endregion
#region Public Methods

        // todo: turn into protected
        /// <summary>
        /// Remove top card from the deck and return it
        /// </summary>
        /// <returns>Deck's first card</returns>
        public Card TakeCardFromDeck()
        {
            if (Deck.Count < 1)
            {
                throw new InvalidOperationException("Can't take new card from deck, deck is empty!");
            }
            Card card = Deck[0];
            Deck.RemoveAt(0);
            return card;
        }

        /// <summary>
        /// Type of game move. Current active player takes card from his hands and puts it on the table, then takes a new card. If deck is empty - game ends
        /// </summary>
        /// <param name = "cardString">String, that contains card number</param>
        public void PlayCard(string cardString)
        {
            if (!GameIsActive) return;
            int cardNumber = ParseCardNumbers(cardString)[0];
            Card card = currentActivePlayer.CardsOnHand[cardNumber];
            if (CheckMoveAvaliability(card.Color, card.Rank))
            {
                SuccessfullyPlayedCards++;
                IncraseLine(card.Color);
                // todo: check move for risk
                if (false)
                {
#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(">>>Now was risky move, played card {0} {1}", card.Color, card.Rank);
                    Console.ResetColor();
#endif
                    MovesWithRisk++;
                }
                currentActivePlayer.DropAndTryTakeNewCard(cardNumber);
                if (SuccessfullyPlayedCards == MaxCardsAmountOnTable) GameOver("Sequences are finished!");
                else if (Deck.Count == 0) GameOver("Deck ended");
                else ChangeMove();
            }
            else
            {
                GameOver("Card cannot be played");
            }
        }

        /// <summary>
        /// Type of game move. Current active player removes card from his hands and takes a new card. If deck is empty - game ends
        /// </summary>
        /// <param name = "cardString">String that contains card number</param>
        public void DropCard(string cardString)
        {
            if (!GameIsActive) return;
            int cardNumber = ParseCardNumbers(cardString)[0];
            currentActivePlayer.DropAndTryTakeNewCard(cardNumber);
            if (Deck.Count > 0) ChangeMove();
            else GameOver("Deck ended");
        }

        /// <summary>
        /// Type of game move. Current active player tells to current passive player which of his cards have specific color
        /// </summary>
        /// <param name = "colorName">Name of the color</param>
        /// <param name = "cardNumbersString">String that contains card numbers</param>
        public void TellColor(string colorName, string cardNumbersString)
        {
            if (!GameIsActive) return;
            Card.Colors color = Card.ColorParse(colorName);
            int[] numbers = ParseCardNumbers(cardNumbersString);
            if (currentPassivePlayer.ReceiveColorInfo(color, numbers)) ChangeMove();
            else GameOver("Player told wrong cards"); // Maybe "Wrong cards were hinted" - more appropriate?
        }

        /// <summary>
        /// Type of game move. Current active player tells to current passive player which of his cards have specific rank
        /// </summary>
        /// <param name = "rankString">String that contains rank number</param>
        /// <param name = "cardNumbersString">String that contains card numbers</param>
        public void TellRank(string rankString, string cardNumbersString)
        {
            if (!GameIsActive) return;
            int rank = Card.RankParse(rankString);
            int[] numbers = ParseCardNumbers(cardNumbersString);
            // Try send info to currentPassivePlayer
            if (currentPassivePlayer.TryReceiveRankInfo(rank, numbers)) ChangeMove();
            else GameOver("Player told wrong cards");
        }

#endregion
    }
}
