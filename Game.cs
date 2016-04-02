using System;
using System.Collections;
using System.Collections.Generic;

namespace my_console_project
{
    class Game
    {
        public enum PlayerName
        {
            First,
            Second
        }
        
    #region Fields
        private Player _player1, _player2;
        private int _successfullyPlayedCards;
        // todo: init those fields with constructor:
        public readonly int NumberOfCardsInPlayersHand = 5, MaxCardsAmountOnTable = 25, MinCardsAmountInDeck;
        public delegate void GameMethodsContainer();
        public event GameMethodsContainer GameOver;

    #endregion
    #region Props
        public int SuccessfullyPlayedCards
        {
            get
            {
                return _successfullyPlayedCards;
            }
            private set
            {
                // Количество успешно сыгранных карт не может привышать максимально возможное количество сыгранных карт
                if (value > MaxCardsAmountOnTable)
                {
                    throw new ArgumentException("Imposible successfullyPlayedCards value");
                }
                _successfullyPlayedCards = value;
            }
        }

        public bool IsAlive { get; private set; }
        public int CurrentTurn { get; private set; }
        public PlayerName WhoIsMovingNow { get; private set; }
        public int MovesWithRisk { get; private set; }
        public int RedSequenceLine { get; private set; }
        public int BlueSequenceLine { get; private set; }
        public int GreenSequenceLine { get; private set; }
        public int WhiteSequenceLine { get; private set; }
        public int YellowSequenceLine { get; private set; }
        public List<Card> Deck { get; private set; }

    #endregion
    #region Constructors
        public Game()
        {
            IsAlive = true;
            Deck = new List<Card>();
            WhoIsMovingNow = PlayerName.First;
            CurrentTurn = SuccessfullyPlayedCards = MovesWithRisk = 0;
            RedSequenceLine = YellowSequenceLine = WhiteSequenceLine = GreenSequenceLine = BlueSequenceLine = 0;
            // Следующая строка - для возможности изменения максимального количества карт на руках итд
            MinCardsAmountInDeck = NumberOfCardsInPlayersHand * 2 + 1;
            // Подписка метода OnGameOver() на событие GameOver
            GameOver += OnGameOver;
        }

    #endregion
    #region Methods
        /// <summary>
        /// Starts new Hanabi game with standart options
        /// </summary>
        public void StartNewGame(string startCommand)
        {
            if (string.IsNullOrEmpty(startCommand))
            {
                throw new ArgumentException("Start game command expected");
            }
            if (!startCommand.StartsWith("Start new game with deck "))
            {
                throw new ArgumentException("Expected \"Start new game with deck %ABBREVIATIONS%\", instead got: " + startCommand);
            }
            // В команде "Start new game with deck ..." данные начинаются с позиции 25
            FillDeck(startCommand.Substring(25));
            _player1 = new Player(this);
            _player2 = new Player(this);
            GameLoop();
        }

        /// <summary>
        /// Move exchange from fisrt player to second player, while game is played
        /// </summary>
        private void GameLoop()
        {
            // Так как снаружи этого игрового цикла есть код, в котором ошибки нужно обрабатывать схожим образом,
            // а так же для верной работы автоматизированных тестов - я решил ошибки выбрасывать наружу.
            // В бранче уровня сложности 1 игровой луп будет вообще вынесен из этого класса.
            while (IsAlive)
            {
#if DEBUG
                ShowStatistics();
#endif
                ParseAndRunCommand(Console.ReadLine());
                if (Deck.Count == 0) GameOver();
                WhoIsMovingNow = PlayerName.Second == WhoIsMovingNow ? PlayerName.First : PlayerName.Second;
            }
        }

        /// <summary>
        /// Check if it's possible to play the card
        /// </summary>
        /// <param name = "card">Card, which need to be checked</param>
        /// <returns></returns>
        private bool CheckCardMoveAvaliability(Card card)
        {
            // если все %color%SequenceLine заменить на Dictionary <Card.Colors, byte> CurrentTopInSequences
            // то проверка упростится до CurrentTopInSequences[card.Color] + 1 == card.Rank
            // но инициализацию Dictionary с помощью цикла я не знаю где проводить
            switch (card.Color)
            {
                case Card.Colors.Blue:
                {
                    return BlueSequenceLine + 1 == card.Rank;
                }
                case Card.Colors.Green:
                {
                    return GreenSequenceLine + 1 == card.Rank;
                }
                case Card.Colors.Red:
                {
                    return RedSequenceLine + 1 == card.Rank;
                }
                case Card.Colors.White:
                {
                    return WhiteSequenceLine + 1 == card.Rank;
                }
                case Card.Colors.Yellow:
                {
                    return YellowSequenceLine + 1 == card.Rank;
                }
                default:
                {
                    throw new ArgumentException("Сard had unknown color");
                }
            }
        }

        /// <summary>
        /// Showing game results; ending game life cycle
        /// </summary>
        private void OnGameOver()
        {
            ShowStatistics();
            IsAlive = false;
        }

        /// <summary>
        /// Writes game results to console
        /// </summary>
        private void ShowStatistics()
        {
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
                throw new ArgumentException("Card abbreviations expected");
            }
            string[] abbreviations = abbreviationsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (abbreviations.Length < MinCardsAmountInDeck)
            {
                throw new ArgumentException("Expected at least " + MinCardsAmountInDeck + " card abbreviations, instead got " + abbreviations.Length);
            }
            foreach (string a in abbreviations)
            {
                Deck.Add(new Card(a));
            }
        }

        /// <summary>
        /// Parsing string to command and then running the command
        /// </summary>
        /// <param name = "command">Command text</param>
        private void ParseAndRunCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException("Command expected");
            }
            // Может, ссылки на текущих activePlayer и passivePlayer вообще хранить в приватных полях?
            // Тогда многие методы проще станут, плюс отпадёт нужда в WhoIsMovingNow
            Player activePlayer = WhoIsMovingNow == PlayerName.First ? _player1 : _player2;
            Player passivePlayer = WhoIsMovingNow == PlayerName.First ? _player2 : _player1;
            if (command.StartsWith("Tell color "))
            {
                TellColor(passivePlayer, command);
            }
            else if (command.StartsWith("Tell rank "))
            {
                TellRank(passivePlayer, command);
            }
            else if (command.StartsWith("Play card "))
            {
                PlayCard(activePlayer, command);
            }
            else if (command.StartsWith("Drop card "))
            {
                DropCard(activePlayer, command);
            }
            else
            {
                throw new ArgumentException("Unknown command: " + command);
            }
        }

        /// <summary>
        /// Type of player action. Player takes card from his own hands and puts on table. Then takes new card, if deck is not empty
        /// </summary>
        /// <param name = "activePlayer">Player, who plays the card</param>
        /// <param name = "command">Command text</param>
        private void PlayCard(Player activePlayer, string command)
        {
            int cardNumber = CardNumberParse(command.Substring(10));
            CardOnHand cardOnHand = activePlayer.CardsOnHand[cardNumber];
            Card card = cardOnHand.Card;
            if (CheckCardMoveAvaliability(card))
            {
                IncraseLine(card.Color);
                activePlayer.DropAndTryTakeNewCard(cardNumber);
                // Если у игрока неполная инфа по карте - ход рискованный
                if (cardOnHand.CardInfoAvaliability != CardOnHand.CardInfoAvaliabilities.All)
                {
#if DEBUG
                    Console.WriteLine(">>>Now was risky move, played card {0} {1}, info: {2}",
                        card.Color, card.Rank, cardOnHand.CardInfoAvaliability);
#endif
                    MovesWithRisk++;
                }
                CurrentTurn++;
                SuccessfullyPlayedCards++;
            }
            else
            {
                // Card cannot be played - silent game over 
                CurrentTurn++;
                GameOver();
            }
        }

        /// <summary>
        /// Incases high rank in color line of cards which was successfully played
        /// </summary>
        /// <param name = "color">Line of which color need to be changed</param>
        private void IncraseLine(Card.Colors color)
        {
            // эта проверка тоже упростится если использовать Dictionary <Card.Colors, byte> CurrentTopInSequences
            switch (color)
            {
                case Card.Colors.Blue:
                {
                    BlueSequenceLine++;
                    return;
                }
                case Card.Colors.Green:
                {
                    GreenSequenceLine++;
                    return;
                }
                case Card.Colors.Red:
                {
                    RedSequenceLine++;
                    return;
                }
                case Card.Colors.White:
                {
                    WhiteSequenceLine++;
                    return;
                }
                case Card.Colors.Yellow:
                {
                    YellowSequenceLine++;
                    return;
                }
            }
        }

        /// <summary>
        /// Drops specific card from player's hand
        /// </summary>
        /// <param name = "activePlayer">Player, who drops card</param>
        /// <param name = "command">Text command</param>
        private void DropCard(Player activePlayer, string command)
        {
            int cardNumber = CardNumberParse(command.Substring(10));
            activePlayer.DropAndTryTakeNewCard(cardNumber);
            CurrentTurn++;
        }

        /// <summary>
        /// Tells which cards of player <paramref name = "passivePlayer"/> are of specific color
        /// </summary>
        /// <param name = "passivePlayer">Player who's cards are being described</param>
        /// <param name = "command">Text command</param>
        private void TellColor(Player passivePlayer, string command)
        {
            string[] words = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Card.Colors color = Card.ColorParse(words[2]);
            bool[] whichCardsToTell = new bool[NumberOfCardsInPlayersHand];
            if (words.Length < 6)
            {
                throw new ArgumentException("Expected \"Tell color %COLOR_NAME% for cards %CARD_NUMBERS%\", instead got: " + command);
            }
            for (int i = 5; i < words.Length; i++)
            {
                whichCardsToTell[CardNumberParse(words[i])] = true;
            }
            // Сравнение по составу экземпляра
            IStructuralEquatable equatableArray = whichCardsToTell;
            if (!equatableArray.Equals(GetCardsOfColor(passivePlayer, color), StructuralComparisons.StructuralEqualityComparer))
            {
                // Attempted to tell color for wrong cards
                CurrentTurn++;
                GameOver();
                return;
            }
            passivePlayer.GetColorInfo(whichCardsToTell);
            CurrentTurn++;
        }

        /// <summary>
        /// Scanning cards of player <paramref name = "player"/> for cards with color of <paramref name = "color"/>
        /// </summary>
        /// <param name = "player">Player, which cards should be scanned</param>
        /// <param name = "color">The desired color</param>
        /// <returns>Roster of predicates, which tells, if the card in player's hands is of desired color (<value>true</value>) or not <value>false</value></returns>
        private bool[] GetCardsOfColor(Player player, Card.Colors color)
        {
            bool[] whichCardsAreOfRank = new bool[player.CardsOnHand.Count];
            for (int i = 0; i < player.CardsOnHand.Count; i++)
            {
                if (player.CardsOnHand[i].Card.Color == color)
                {
                    whichCardsAreOfRank[i] = true;
                }
            }
            return whichCardsAreOfRank;
        }

        /// <summary>
        /// Tells which cards of player <paramref name = "passivePlayer"/> are of specific rank
        /// </summary>
        /// <param name = "passivePlayer">Player who's cards are being described</param>
        /// <param name = "command"></param>
        private void TellRank(Player passivePlayer, string command)
        {
            string[] words = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int rank = CardNumberParse(words[2]);
            bool[] whichCardsToTell = new bool[NumberOfCardsInPlayersHand];
            if (words.Length < 6)
            {
                throw new ArgumentException("Expected \"Tell rank %RANK_NO% for cards %CARD_NUMBERS%\", instead got: " + command);
            }
            for (int i = 5; i < words.Length; i++)
            {
                whichCardsToTell[CardNumberParse(words[i])] = true;
            }
            // Сравнение по составу экземпляра
            IStructuralEquatable equatableArray = whichCardsToTell;
            if (!equatableArray.Equals(GetCardsOfRank(passivePlayer, rank), StructuralComparisons.StructuralEqualityComparer))
            {
                // Attempted to tell color for wrong cards
                CurrentTurn++;
                GameOver();
                return;
            }
            passivePlayer.GetRankInfo(whichCardsToTell);
            CurrentTurn++;
        }

        /// <summary>
        /// Scanning cards of player <paramref name = "player"/> for cards with rank of <paramref name = "rank"/>
        /// </summary>
        /// <param name = "player">Player, which cards should be scanned</param>
        /// <param name = "rank">The desired rank</param>
        /// <returns>Roster of predicates, which tells, if the card in player's hands is of desired rank (<value>true</value>) or not <value>false</value></returns>
        private bool[] GetCardsOfRank(Player player, int rank)
        {
            bool[] whichCardsAreOfRank = new bool[player.CardsOnHand.Count];
            for (int i = 0; i < player.CardsOnHand.Count; i++)
            {
                if (player.CardsOnHand[i].Card.Rank == rank)
                {
                    whichCardsAreOfRank[i] = true;
                }
            }
            return whichCardsAreOfRank;
        }

        /// <summary>
        /// Parsing string into card number
        /// </summary>
        /// <param name = "cardNumber">String containg card number</param>
        /// <returns>Card number</returns>
        private int CardNumberParse(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
            {
                throw new ArgumentException("Card number string expected");
            }
            try
            {
                int n = int.Parse(cardNumber);
                if (n < 0 || n >= NumberOfCardsInPlayersHand)
                {
                    throw new ArgumentException("Card number out of range: " + n);
                }
                return n;
            }
            catch (FormatException e)
            {
                throw new ArgumentException("Incorrect card number: " + cardNumber, e);
            }
        }

        /// <summary>
        /// Gives first card from deck and destroying it from deck
        /// </summary>
        /// <returns>Deck's first card</returns>
        public Card TakeCardFromDeck()
        {
            Card card = Deck[0];
            Deck.RemoveAt(0);
            return card;
        }
    #endregion
    }
}