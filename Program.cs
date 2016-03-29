using System;
using System.Collections;
using System.Collections.Generic;

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

    class Card
    {
        #region Fields
        // Перечисление цветов
        public enum Colors { Red, Green, Blue, Yellow, White }

        // Скрытое поле для цвета карты
        private Colors _color;
        // Скрытое поле для достоинства карты
        private int _rank;
        #endregion
        #region Props
        // Открытое свойство, инкапсулирющее доступ до закрытого поля цвета карты
        public Colors Color
        {
            // Кто угодно может получить доступ к чтения свойства
            get { return _color; }
            // Только члены этого класса (Card) и сам класс могут получить доступ к записи этого свойства
            private set { _color = value; }
        }
        // Открытое свойство, инкапсулирющее доступ до закрытого поля достоинства карты
        public int Rank
        {
            // Кто угодно может получить доступ к чтения свойства
            get { return _rank; }
            // Только члены этого класса (Card) и сам класс могут получить доступ к записи этого свойства
            private set
            {
                // Если новое значение, лежит в промежутке: (0;6), записывается новое значение
                if (value > 0 && value < 6)
                {
                    // value -- зарезервированное в C# имя, означающее передаваемое свойству на запись значение
                    _rank = value;
                }
                else
                {
                    // Вброс ошибки, сообщающей, что полученное на вход число не попадает в целевой диапазон чисел возможного номинала карты.
                    throw new ArgumentException("Incorrect card rank.");
                }

            }
        }
        #endregion
        #region Constructors

        public Card(int cardRank, Colors cardColor)
        {
            Color = cardColor;
            Rank = cardRank;
        }
        // Вызов конструктора Card(int cardRank, Colors cardColor) из конструктора Card(string cardAbbreviation)
        public Card(string cardAbbreviation) : this(Int32.Parse(cardAbbreviation[1].ToString()), ColorParse(cardAbbreviation[0])) { }
        public Card(Card card)
        {
            Color = card.Color;
            Rank = card.Rank;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Parsing string into card instance
        /// </summary>
        /// <param name="cardAbbreviation">Abbreviation, which represents a card. Kind of <value>"R1"</value>, <value>"Y3"</value>... 1-st symbol -- card color, 2-nd -- card rank</param>
        /// <returns></returns>
        public static Card Parse(string cardAbbreviation)
        {
            if (cardAbbreviation.Length == 2)
            {
                return new Card(Int32.Parse(cardAbbreviation[1].ToString()), ColorParse(cardAbbreviation[0]));
            }
            else
            {
                // Вброс ошибки о неправильно введённых данных
                throw new ArgumentException("Incorrect input data");
            }
        }
        /// <summary>
        /// Parsing string into color
        /// </summary>
        /// <param name="colorAbbreviation">One-symbol abbreviation of color, kind of: <value>'R'</value>, <value>'Y'</value>, <value>'W'</value>...</param>
        /// <returns></returns>
        public static Colors ColorParse(char colorAbbreviation)
        {
            switch (colorAbbreviation)
            {
                case 'R':
                    {
                        return Colors.Red;
                    }
                case 'Y':
                    {
                        return Colors.Yellow;
                    }
                case 'W':
                    {
                        return Colors.White;
                    }
                case 'B':
                    {
                        return Colors.Blue;
                    }
                case 'G':
                    {
                        return Colors.Green;
                    }
                default:
                    {
                        // Вброс ошибки, сообщающей, что полученная на вход аббревиатура цвета некоректна.
                        throw new ArgumentException("Incorrect card color abbreviation.");
                    }
            }
        }
        /// <summary>
        /// Parsing string into color
        /// </summary>
        /// <param name="color">Full color name. Kind of: <value>"Yellow"</value>, <value>"Red"</value>, <value>"White"</value>...</param>
        /// <returns></returns>
        public static Colors ColorParse(string color)
        {
            switch (color)
            {
                case "Red":
                    {
                        return Colors.Red;
                    }
                case "Yellow":
                    {
                        return Colors.Yellow;
                    }
                case "Blue":
                    {
                        return Colors.Blue;
                    }
                case "White":
                    {
                        return Colors.White;
                    }
                case "Green":
                    {
                        return Colors.Green;
                    }
                default:
                    {
                        // Вброс ошибки, сообщающей, что полученная на вход аббревиатура цвета некоректна.
                        throw new ArgumentException("Incorrect card color abbreviation.");
                    }
            }
        }
        #endregion
    }

    class CardOnHand
    {
        #region Enums
        // Перечисление для указания количества информации, имеющейся о конкретной карте в руке игрока
        public enum CardInfoAvaliabilities { None, Color, Rank, All }
        #endregion
        #region Fields
        // Скрытое поле карты
        private Card _card;
        // Скрытое поле с информацией о доступных знаниях про карту
        private CardInfoAvaliabilities _cardInfoAbAvaliability;
        #endregion
        #region Props
        // Открытое свойство, инкапсулирющее доступ до закрытого поля карты
        public Card Card
        {
            get { return _card; }
            private set { _card = value; }
        }
        // Открытое свойство, инкапсулирющее доступ до закрытого поля с информацией о доступных знаниях про карту
        public CardInfoAvaliabilities CardInfoAvaliability
        {
            get { return _cardInfoAbAvaliability; }
            set { _cardInfoAbAvaliability = value; }
        }
        #endregion
        #region Constructors
        // Конструктор CardOnHand(Card card) вызывает конструктор CardOnHand(Card card, CardInfoAvaliabilities cardInfoAvaliability)
        public CardOnHand(Card card) : this(card, CardInfoAvaliabilities.None) { }
        public CardOnHand(Card card, CardInfoAvaliabilities cardInfoAvaliability)
        {
            Card = card;
            CardInfoAvaliability = cardInfoAvaliability;
        }
        #endregion
    }

    class Player
    {
        #region Fields
        // Скрытое поле-список карт на руках у игрока
        private List<CardOnHand> _cardsOnHands;
        // Скрытое поле, инкапсулирющее ссылку на экземпляр игры, в которой принимает участие этот игрок
        private readonly Game _playersGame;
        #endregion
        #region Props
        // Открытое свойство, обеспечивающее доступ к скрытому списку карт на руках у пользователя
        public List<CardOnHand> CardsOnHands
        {
            get { return _cardsOnHands; }
            private set { _cardsOnHands = value; }
        }
        #endregion
        #region Constructors
        public Player(Game game)
        {
            _playersGame = game;
            CardsOnHands = new List<CardOnHand>();
            //takes full hand of cards
            for (int i = 0; i < Game.CardsValueInPlayersHand; i++)
            {
                TakeNewCard();
            }
        }
        public Player(List<Card> takenCards)
        {
            if (takenCards.Count == Game.CardsValueInPlayersHand)
            {
                CardsOnHands = new List<CardOnHand>();
                foreach (var card in takenCards)
                {
                    CardsOnHands.Add(new CardOnHand(card));
                }
            }
            else
            {
                throw new ArgumentException("Invalid cards value.");
            }
        }
        #endregion
        #region Methods
        /// <summary>
        /// Refreshes accessibility level of color info about all cards
        /// </summary>
        /// <param name="cardsNumbers">Array of color access indicators</param>
        public void GetColorInfo(bool[] cardsNumbers)
        {
            if (cardsNumbers.Length == CardsOnHands.Count)
            {
                for (int i = 0; i < CardsOnHands.Count; i++)
                {
                    if (cardsNumbers[i])
                    {
                        if (CardsOnHands[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.Rank)
                        {
                            CardsOnHands[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.All;
                        }
                        else if (CardsOnHands[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.None)
                        {
                            CardsOnHands[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.Color;
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException("Invalid value of cards.");
            }
        }
        /// <summary>
        /// Refreshes accessibility level of rank info about all cards
        /// </summary>
        /// <param name="cardsNumbers">Array of color access indicators</param>
        public void GetRankInfo(bool[] cardsNumbers)
        {
            if (cardsNumbers.Length == CardsOnHands.Count)
            {
                for (int i = 0; i < CardsOnHands.Count; i++)
                {
                    if (cardsNumbers[i])
                    {
                        if (CardsOnHands[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.Color)
                        {
                            CardsOnHands[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.All;
                        }
                        else if (CardsOnHands[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.None)
                        {
                            CardsOnHands[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.Rank;
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException("Invalid value of cards.");
            }
        }
        /// <summary>
        /// Drops specific card in player's hands and takes one more from deck, if deck isn't empty
        /// </summary>
        /// <param name="cardNumber">Number of card in hands, which is has to be dropped</param>
        public void DropAndTryTakeNewCard(int cardNumber)
        {
            if (cardNumber >= 0 && cardNumber < _cardsOnHands.Count)
            {
                _cardsOnHands.RemoveAt(cardNumber);
                if (_playersGame.Deck.Count > 0)
                {
                    TakeNewCard();
                }
            }
            else
            {
                throw new ArgumentException("Invalid card number");
            }
        }
        /// <summary>
        /// Takes new card from deck into player's hands
        /// </summary>
        private void TakeNewCard()
        {
            _cardsOnHands.Add(new CardOnHand(_playersGame.GiveCard()));
        }
        #endregion
    }

    class Game
    {
        #region Enums
        // Перечисление для определения того, какой игрок сейчас ходит
        public enum PlayerName { First, Second };
        #endregion
        #region Fields
        // Скрытое поле, содержащее инфу о том, продолжается ли игра
        private bool _isAlive;
        // Скрытое поле-список с картами колоды. По сути, колода и есть
        private List<Card> _deck;
        // Скрытые поля с экземплярами игроков
        private Player _player1, _player2;
        // Скрытое поле, содержащее инфу о том, какого игрока чейчас ход
        private PlayerName _whoIsMovingNow;
        // Скрытые поля: 1) номер текущего хода; 2) количество успешно сыгранных карт; 3) количество сделанных рискованных ходов
        private int _currentTurn, _successfullyPlayedCards, _movesWithRisk,
            // Скрытые поля с достоинствами старших разыгранных (которые на столе, на линии) карт для каждой масти
            _redSequenceLine, _yellowSequenceLine, _whiteSequenceLine, _greenSequenceLine, _blueSequenceLine;
        // Открытые поля только для чтения: 1) количество карт в руках одного игрока; 2) минимально возможное количество карт в исходной колоде; 3) максимально возможное количество разыгранных карт (на столе)
        public static readonly int CardsValueInPlayersHand = 5, MinCardsValueInDeck = 11, MaxCardsValueOnTable = 25;

        // Собственный открытый тип делегата для игровых событий с сигнатурой методов void()
        public delegate void GameMethodsContainer();
        // Собственный открытый тип событий, базирующийся на типе делегатов GameMethodContainer. Это событие для завершения игры
        public event GameMethodsContainer GameOver;
        #endregion
        #region Props
        // Там, где нет комментариев -- всё как раньше: открытое чтение, скрытая запись (только для экземпляров этого класса (Game) и самого класса)

        public bool IsAlive
        {
            get { return _isAlive; }
            private set { _isAlive = value; }
        }
        public int CurrentTurn
        {
            get { return _currentTurn; }
            private set { _currentTurn = value; }
        }
        public int SuccessfullyPlayedCards
        {
            get { return _successfullyPlayedCards; }
            private set
            {
                // Количество успешно сыгранных карт не может привышать максимально возможное количество сыгранных карт
                if (value <= MaxCardsValueOnTable)
                {
                    _successfullyPlayedCards = value;
                }
                // Если привысило, значит -- конец игры
                else
                {
                    GameOver();
                }
            }
        }
        public PlayerName WhoIsMovingNow
        {
            get { return _whoIsMovingNow; }
            private set { _whoIsMovingNow = value; }
        }
        public int MovesWithRisk
        {
            get { return _movesWithRisk; }
            private set { _movesWithRisk = value; }
        }
        public int RedSequenceLine
        {
            get { return _redSequenceLine; }
            private set { _redSequenceLine = value; }
        }
        public int BlueSequenceLine
        {
            get { return _blueSequenceLine; }
            private set { _blueSequenceLine = value; }
        }
        public int GreenSequenceLine
        {
            get { return _greenSequenceLine; }
            private set { _greenSequenceLine = value; }
        }
        public int WhiteSequenceLine
        {
            get { return _whiteSequenceLine; }
            private set { _whiteSequenceLine = value; }
        }
        public int YellowSequenceLine
        {
            get { return _yellowSequenceLine; }
            private set { _yellowSequenceLine = value; }
        }
        public List<Card> Deck
        {
            get { return _deck; }
            private set { _deck = value; }
        }
        #endregion
        #region Constructors
        public Game()
        {
            IsAlive = true;
            // Выделение памяти для списка колоды
            Deck = new List<Card>();
            WhoIsMovingNow = PlayerName.First;
            // Три переменные поочерёдно (справа налево) инициализируются нолями
            CurrentTurn = SuccessfullyPlayedCards = MovesWithRisk = 0;
            // Пять переменных поочерёдно (справа налево) инициализируются нолями
            RedSequenceLine = YellowSequenceLine = WhiteSequenceLine = GreenSequenceLine = BlueSequenceLine = 0;
            // Подписка метода OnGameOver() на событие GameOver. При активации события, по очереди активируются все подписанные на наего методы (по очереди: первым выполнится тот, который был первым подписан). Подписываемые методы должны иметь сигнатуру делегата события, на которое подписываются. Отписка осуществляется при помощи оператора "-="
            GameOver += OnGameOver;
        }
        #endregion
        #region Methods

        /// <summary>
        /// Starts new Hanabi game with standart options
        /// </summary>
        public void StartNewGame()
        {
            // Введённая с консоли команда. Console.ReadLine() -- метод считывания введённого до нажатия кнопки Enter текста с консоли 
            string InputData = Console.ReadLine();
            // Если была введена команда начала игры
            if (InputData.StartsWith("Start new game with deck"))
            {
                // 'k' - последний символ в строке-команде "Start new game with deck". Substring(1) подтирает 1-й символ (в данном случае -- пробел), с которого начинается строка
                DeckFilling(InputData.Split('k')[1].Substring(1));
                // Если стартовая колода имеет больше карт, чем минимально возможно для игры
                if (Deck.Count >= MinCardsValueInDeck)
                {
                    // this передаёт экземпляр, инкапсулирующий ссылку на этот экземпляр игры
                    _player1 = new Player(this);
                    _player2 = new Player(this);
                    MoveTransfer();
                }
            }
            else
            {
                // Очистить консоль
                Console.Clear();
                // Вывести на консоль текст в кавычках. К концу текста в кавычках автомитачески добавляется служебный символ новой строки ("\n"). Чтоб этот символ не добавлялся, следует юзать метод Console.Write() 
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
                // Попробовать исполнить код. Если возникает ошибка -- выполнение программы переходит к блоку catch
                try
                {
                    // Дирректива препроцессора: если включен режим DEBUG (для этого нужно ранее написать: "#define DEBUG"), строка "ShowStatistics();" будет активным кодом, иначе -- будет игнорироваться компилятором.
#if DEBUG
                    ShowStatistics();
#endif
                    PlayerMoves();
                }
                // "Ловит" ошибки типа ArgumentException. Все остальные ошибки будут ложить программу.
                catch (ArgumentException ex)
                {
                    Console.WriteLine("{0} \n{1}", ex, ex.Message);
                    continue;
                }
                // Тернарный оператор. Эквивалентно коду: "if (_whoIsMovingNow == PlayerName.Second) { _whoIsMovingNow = PlayerName.First; } else { _whoIsMovingNow = PlayerName.Second; }
                _whoIsMovingNow = _whoIsMovingNow == PlayerName.Second ? PlayerName.First : PlayerName.Second;
            }
        }

        /// <summary>
        /// Regular player's move
        /// </summary>
        private void PlayerMoves()
        {
            ParseAndRunCommand(Console.ReadLine());
            // Если размер колоды нолевой, значит карты кончились, пора заканчивать игру
            if (Deck.Count == 0)
            {
                GameOver();
            }
        }

        /// <summary>
        /// Check if it's possible to play the card
        /// </summary>
        /// <param name="card">Card, which need to be checked</param>
        /// <returns></returns>
        private bool CheckCardMoveAvaliability(Card card)
        {
            switch (card.Color)
            {
                case Card.Colors.Blue:
                    {
                        return _blueSequenceLine + 1 == card.Rank;
                    }
                case Card.Colors.Green:
                    {
                        return _greenSequenceLine + 1 == card.Rank;
                    }
                case Card.Colors.Red:
                    {
                        return _redSequenceLine + 1 == card.Rank;
                    }
                case Card.Colors.White:
                    {
                        return _whiteSequenceLine + 1 == card.Rank;
                    }
                case Card.Colors.Yellow:
                    {
                        return _yellowSequenceLine + 1 == card.Rank;
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
        /// <param name="cardsAbbreviationsPhrase">Text with cards abbreviations kind of <value>"R1"</value>, <value>"Y3"</value>, <value>"W4"</value>...</param>
        private void DeckFilling(string cardsAbbreviationsPhrase)
        {
            try
            {
                // Массив строк, инициализируемый входной строкой, разбитой по символу пробел на массив строк
                string[] cardsAbbreviations = cardsAbbreviationsPhrase.Split(' ');
                foreach (var abbreviation in cardsAbbreviations)
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
        /// <param name="command">Command text</param>
        private void ParseAndRunCommand(string command)
        {
            if (command.StartsWith("Tell color"))
            {
                TellColor(_whoIsMovingNow == PlayerName.First ? _player2 : _player1, command);
            }
            else if (command.StartsWith("Tell rank"))
            {
                TellRank(_whoIsMovingNow == PlayerName.First ? _player2 : _player1, command);
            }
            else if (command.StartsWith("Play card"))
            {
                PlayCard(_whoIsMovingNow == PlayerName.First ? _player1 : _player2, command);
            }
            else if (command.StartsWith("Drop card"))
            {
                DropCard(_whoIsMovingNow == PlayerName.First ? _player1 : _player2, command);
            }
            else
            {
                throw new ArgumentException("Invalid command.");
            }
        }

        /// <summary>
        /// Type of player action. Player takes card from his own hands and puts on table. Then takes new card, if deck is not empty
        /// </summary>
        /// <param name="activePlayer">Player, who plays the card</param>
        /// <param name="command">Command text</param>
        private void PlayCard(Player activePlayer, string command)
        {
            // Номер карты, которой нужно ходить. Int32.Parse() -- стандартный .NET-метод для парсинга строки в Int32(int) : 4-байтовое целове число. int -- это лишь синтаксический сахар, как и double, char, string, double, decimal...
            int cardNumber = Int32.Parse(command.Split(' ')[2]);
            // Если номер карта в пределах номеров карт в руках игрока
            if (cardNumber >= 0 && cardNumber < activePlayer.CardsOnHands.Count)
            {
                // Экземпляр, инкапсулирующий ссылку на карту в руке. Для убоства, чтоб вместо "activePlayer.CardsOnHands[cardNumber]" писать "cardOnHand"
                CardOnHand cardOnHand = activePlayer.CardsOnHands[cardNumber];
                // Экземпляр, инкапсулирующий ссылку на карту. Для убоства, чтоб вместо "activePlayer.CardsOnHands[cardNumber]" (или вообще "activePlayer.CardsOnHands[cardNumber].Card" писать "cardOnHand"
                Card card = cardOnHand.Card;

                if (CheckCardMoveAvaliability(card))
                {
                    IncraseLine(card.Color);
                    activePlayer.DropAndTryTakeNewCard(cardNumber);
                    // Если номинал карты не 1 (единицей можно всегда начать ряд масти, значит такой ход не рискованный), и у игрока неполная инфа по карте -- ход рискованный
                    if (card.Rank != 1 && cardOnHand.CardInfoAvaliability != CardOnHand.CardInfoAvaliabilities.All)
                    {
                        // Дирректива препроцессора: если включен режим DEBUG (для этого нужно ранее написать: "#define DEBUG"), строка "ShowStatistics();" будет активным кодом, иначе -- будет игнорироваться компилятором.
#if DEBUG
                        Console.WriteLine(">>>\n>>>Now was risky move, played card {0}, info: {1}", card.Color + card.Rank, cardOnHand.CardInfoAvaliability);
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
            else
            {
                throw new ArgumentException("Invalid card rank.");
            }
        }

        /// <summary>
        /// Incases high rank in color line of cards which was successfully played
        /// </summary>
        /// <param name="color">Line of which color need to be changed</param>
        private void IncraseLine(Card.Colors color)
        {
            switch (color)
            {
                case Card.Colors.Blue:
                    {
                        _blueSequenceLine++;
                        // Поскольку метод имеет возвращаемый тип void, то "return;" просто прекращает работу метода (и возвращает экземпляр System.Void, что иначе сделается автоматически перед концом жизненного цикла метода)
                        return;
                    }
                case Card.Colors.Green:
                    {
                        _greenSequenceLine++;
                        return;
                    }
                case Card.Colors.Red:
                    {
                        _redSequenceLine++;
                        return;
                    }
                case Card.Colors.White:
                    {
                        _whiteSequenceLine++;
                        return;
                    }
                case Card.Colors.Yellow:
                    {
                        _yellowSequenceLine++;
                        return;
                    }
            }
        }

        /// <summary>
        /// Drops specific card from player's hand
        /// </summary>
        /// <param name="activePlayer">Player, who drops card</param>
        /// <param name="command">Text command</param>
        private void DropCard(Player activePlayer, string command)
        {
            activePlayer.DropAndTryTakeNewCard(Int32.Parse(command.Substring(10)));
            CurrentTurn++;
        }

        /// <summary>
        /// Tells which cards of player <paramref name="passivePlayer"/> are of specific color
        /// </summary>
        /// <param name="passivePlayer">Player who's cards are being described</param>
        /// <param name="command">Text command</param>
        void TellColor(Player passivePlayer, string command)
        {
            Card.Colors color = Card.ColorParse(command.Split(' ')[2]);
            // Инициализация (выделением памяти) массива экземпляров типа bool размером passivePlayer.CardsOnHands.Count
            bool[] whichCardsToTell = new bool[passivePlayer.CardsOnHands.Count];
            for (int i = 0; i < whichCardsToTell.Length; i++)
            {
                // Если текст команды содержит число, равное i. Object.ToString() -- стандартный .NET-метод, присущий ВСЕМ классам в C#, он возвращает текстовое представление заданного объекта
                if (command.Contains(i.ToString()))
                {
                    whichCardsToTell[i] = true;
                }
            }
            // Интерфейс. Нужен для того, чтоб инкапсулировать экземпляр структуры в оболочку, к которой можно будет применить сравнение не по ссылке, а по составу экземпляра
            IStructuralEquatable equatableWhichCardsToTell = whichCardsToTell;
            // Это самое сравнение по составу экземпляра ( Object.Equals (Object obj, StructuralComparisons.StructuralEqualityComparer) )
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
        /// Scanning cards of player <paramref name="player"/> for cards with color of <paramref name="color"/>
        /// </summary>
        /// <param name="player">Player, which cards should be scanned</param>
        /// <param name="color">The desired color</param>
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
        /// Tells which cards of player <paramref name="passivePlayer"/> are of specific rank
        /// </summary>
        /// <param name="passivePlayer">Player who's cards are being described</param>
        /// <param name="command"></param>
        void TellRank(Player passivePlayer, string command)
        {
            bool[] whichCardsToTell = new bool[passivePlayer.CardsOnHands.Count];
            // Можно заменить на нарезку по символу "пробел" (command.Split[N])
            // Tell_rank_<--(10 position)1_for_cards_1_2_3_4
            int rank = Int32.Parse(command.Substring(10)[0].ToString());
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
        /// Scanning cards of player <paramref name="player"/> for cards with rank of <paramref name="rank"/>
        /// </summary>
        /// <param name="player">Player, which cards should be scanned</param>
        /// <param name="rank">The desired rank</param>
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
