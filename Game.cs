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
        public static readonly int CardsValueInPlayersHand = 5, MinCardsValueInDeck = 11, MaxCardsValueOnTable = 25;
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
                if (value <= MaxCardsValueOnTable)
                {
                    _successfullyPlayedCards = value;
                }
                else
                {
                    GameOver();
                }
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
            // Подписка метода OnGameOver() на событие GameOver
            GameOver += OnGameOver;
        }

    #endregion
    #region Methods
        /// <summary>
        /// Starts new Hanabi game with standart options
        /// </summary>
        public void StartNewGame()
        {
            string inputData = Console.ReadLine();
            if (inputData.StartsWith("Start new game with deck"))
            {
                // 'k' - последний символ в строке-команде "Start new game with deck"
                // Substring(1) подтирает 1-й символ (в данном случае -- пробел), с которого начинается строка
                DeckFilling(inputData.Split('k')[1].Substring(1));
                if (Deck.Count < MinCardsValueInDeck) return;
                _player1 = new Player(this);
                _player2 = new Player(this);
                MoveTransfer();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Invalid command entered. Try again.");
            }
        }

        /// <summary>
        /// Move exchange from fisrt player to second player, while game is playing
        /// </summary>
        private void MoveTransfer()
        {
            while (IsAlive)
            {
                try
                {
#if DEBUG
                    ShowStatistics();
#endif
                    PlayerMoves();
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("{0} \n{1}", ex, ex.Message);
                    continue;
                }
                WhoIsMovingNow = WhoIsMovingNow == PlayerName.Second ? PlayerName.First : PlayerName.Second;
            }
        }

        /// <summary>
        /// Regular player's move
        /// </summary>
        private void PlayerMoves()
        {
            ParseAndRunCommand(Console.ReadLine());
            if (Deck.Count == 0)
            {
                GameOver();
            }
        }

        /// <summary>
        /// Check if it's possible to play the card
        /// </summary>
        /// <param name = "card">Card, which need to be checked</param>
        /// <returns></returns>
        private bool CheckCardMoveAvaliability(Card card)
        {
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
                    throw new ArgumentException("Invalid card.");
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
            Console.WriteLine("Turn: {0}, cards played: {1}, with risk: {2}", CurrentTurn, SuccessfullyPlayedCards, MovesWithRisk);
        }

        /// <summary>
        /// Filling deck with cards, written in input text
        /// </summary>
        /// <param name = "cardsAbbreviationsPhrase">Text with cards abbreviations kind of <value>"R1"</value>, <value>"Y3"</value>, <value>"W4"</value>...</param>
        private void DeckFilling(string cardsAbbreviationsPhrase)
        {
            try
            {
                string[] cardsAbbreviations = cardsAbbreviationsPhrase.Split(' ');
                foreach (string abbreviation in cardsAbbreviations)
                {
                    Deck.Add(new Card(abbreviation));
                }
            }
            catch (ArgumentException ex)
            {
                Console.Clear();
                Console.WriteLine("Invalid card data entered. Try again. \n\nAdditional info:\n" + ex.Message);
            }
        }

        /// <summary>
        /// Parsing string to command and then running the command
        /// </summary>
        /// <param name = "command">Command text</param>
        private void ParseAndRunCommand(string command)
        {
            if (command.StartsWith("Tell color"))
            {
                TellColor(WhoIsMovingNow == PlayerName.First ? _player2 : _player1, command);
            }
            else if (command.StartsWith("Tell rank"))
            {
                TellRank(WhoIsMovingNow == PlayerName.First ? _player2 : _player1, command);
            }
            else if (command.StartsWith("Play card"))
            {
                PlayCard(WhoIsMovingNow == PlayerName.First ? _player1 : _player2, command);
            }
            else if (command.StartsWith("Drop card"))
            {
                DropCard(WhoIsMovingNow == PlayerName.First ? _player1 : _player2, command);
            }
            else
            {
                throw new ArgumentException("Invalid command.");
            }
        }

        /// <summary>
        /// Type of player action. Player takes card from his own hands and puts on table. Then takes new card, if deck is not empty
        /// </summary>
        /// <param name = "activePlayer">Player, who plays the card</param>
        /// <param name = "command">Command text</param>
        private void PlayCard(Player activePlayer, string command)
        {
            int cardNumber = int.Parse(command.Split(' ')[2]);
            if (cardNumber < 0 || cardNumber >= activePlayer.CardsOnHands.Count)
            {
                throw new ArgumentException("Invalid card rank.");
            }
            // Экземпляр, инкапсулирующий ссылку на карту в руке. Для удобства, чтоб вместо "activePlayer.CardsOnHands[cardNumber]" писать "cardOnHand"
            CardOnHand cardOnHand = activePlayer.CardsOnHands[cardNumber];
            // Экземпляр, инкапсулирующий ссылку на карту. Для удобства, чтоб вместо "activePlayer.CardsOnHands[cardNumber]" писать "cardOnHand"
            Card card = cardOnHand.Card;
            if (CheckCardMoveAvaliability(card))
            {
                IncraseLine(card.Color);
                activePlayer.DropAndTryTakeNewCard(cardNumber);
                // Если номинал карты не 1 (ей можно всегда начать ряд масти), и у игрока неполная инфа по карте -- ход рискованный
                if (card.Rank != 1 && cardOnHand.CardInfoAvaliability != CardOnHand.CardInfoAvaliabilities.All)
                {
#if DEBUG
                    Console.WriteLine(">>>\n>>>Now was risky move, played card {0}, info: {1}",
                        card.Color + card.Rank, cardOnHand.CardInfoAvaliability);
#endif
                    MovesWithRisk++;
                }
                CurrentTurn++;
                SuccessfullyPlayedCards++;
            }
            else
            {
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
            activePlayer.DropAndTryTakeNewCard(int.Parse(command.Substring(10)));
            CurrentTurn++;
        }

        /// <summary>
        /// Tells which cards of player <paramref name = "passivePlayer"/> are of specific color
        /// </summary>
        /// <param name = "passivePlayer">Player who's cards are being described</param>
        /// <param name = "command">Text command</param>
        private void TellColor(Player passivePlayer, string command)
        {
            Card.Colors color = Card.ColorParse(command.Split(' ')[2]);
            bool[] whichCardsToTell = new bool[passivePlayer.CardsOnHands.Count];
            for (int i = 0; i < whichCardsToTell.Length; i++)
            {
                if (command.Contains(i.ToString()))
                {
                    whichCardsToTell[i] = true;
                }
            }
            // Нужен для сравнения по составу экземпляра, а не по ссылке
            IStructuralEquatable equatableWhichCardsToTell = whichCardsToTell;
            // Сравнение по составу экземпляра ( Object.Equals (Object obj, StructuralComparisons.StructuralEqualityComparer) )
            if (!equatableWhichCardsToTell.Equals(GetCardsOfColor(passivePlayer, color), StructuralComparisons.StructuralEqualityComparer))
            {
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
            bool[] whichCardsAreOfRank = new bool[player.CardsOnHands.Count];
            for (int i = 0; i < player.CardsOnHands.Count; i++)
            {
                if (player.CardsOnHands[i].Card.Color == color)
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
            bool[] whichCardsToTell = new bool[passivePlayer.CardsOnHands.Count];
            // Можно заменить на нарезку по символу "пробел" (command.Split[N])
            // Tell_rank_<--(10 position)1_for_cards_1_2_3_4
            int rank = int.Parse(command.Substring(10)[0].ToString());
            // Tell_rank_1_for_cards_<--(22 position)1_2_3_4
            command = command.Substring(22);
            for (int i = 0; i < whichCardsToTell.Length; i++)
            {
                if (command.Contains(i.ToString()))
                {
                    whichCardsToTell[i] = true;
                }
            }
            IStructuralEquatable equatableWhichCardsToTell = whichCardsToTell;
            if (!equatableWhichCardsToTell.Equals(GetCardsOfRank(passivePlayer, rank), StructuralComparisons.StructuralEqualityComparer))
            {
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
            bool[] whichCardsAreOfRank = new bool[player.CardsOnHands.Count];
            for (int i = 0; i < player.CardsOnHands.Count; i++)
            {
                if (player.CardsOnHands[i].Card.Rank == rank)
                {
                    whichCardsAreOfRank[i] = true;
                }
            }
            return whichCardsAreOfRank;
        }

        /// <summary>
        /// Gives first card from deck and destroying it from deck
        /// </summary>
        /// <returns>Deck's first card</returns>
        public Card GiveCard()
        {
            Card card = Deck[0];
            Deck.RemoveAt(0);
            return card;
        }
    #endregion
    }
}