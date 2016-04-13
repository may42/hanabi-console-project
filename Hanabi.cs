using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace my_console_project
{
    /// <summary>Hanabi game class. Takes game moves through public methods, gives
    /// information about current game state with events and properties</summary>
    class Hanabi
    {
        #region Delegates & Events

        /// <summary>Special delegate type for game events</summary>
        /// <param name = "info">Provides additional information for the event handlers</param>
        public delegate void GameMethodContainer<T>(T info);

        /// <summary>Special delegate type for game events</summary>
        public delegate void GameMethodContainer();

        /// <summary>Fires when the player action is performed successfully</summary>
        /// <remarks>That includes not only "Play card", but also "Drop card", "Tell color" and "Tell rank"</remarks>
        public event GameMethodContainer MovePerformed = delegate { };

        /// <summary>Fires when the game is finished without errors</summary>
        /// <remarks>Empty delegate assignment avoids the need to check events for null</remarks>
        public event GameMethodContainer<string> GameOver = delegate { };

        /// <summary>Fires when risky move has been performed</summary>
        public event GameMethodContainer<string> RiskyMove = delegate { };

        #endregion
        #region Fields

        private readonly Player firstPlayer, secondPlayer;
        private Player currentActivePlayer, currentPassivePlayer;
        private readonly List<Card> deck;
        private readonly Dictionary<Card.Colors, int> sequencesOnTheTable;

        public readonly int MaxAmountOfCardsOnTable, MaxCardsOnHands = 5;
        /// <summary>Current deck</summary>
        /// <remarks>Exposing card instances is safe, because they are immutable</remarks>
        public readonly ReadOnlyCollection<Card> Deck;
        /// <summary>Current color sequences on the table</summary>
        public readonly ReadOnlyDictionary<Card.Colors, int> SequencesOnTheTable;

        #endregion
        #region Props

        public bool GameIsFinished { get; private set; }
        public int CurrentTurn { get; private set; }
        public int SuccessfullyPlayedCards { get; private set; }
        public int MovesWithRisk { get; private set; }

        /// <summary>Info about cards on hand of current active player</summary>
        public string ActivePlayerCards => currentActivePlayer.ToString();

        /// <summary>Info about cards on hand of current passive player</summary>
        public string PassivePlayerCards => currentPassivePlayer.ToString();

        /// <summary>Info about the current top cards in the color sequences on the table</summary>
        public string CardsOnTable => string.Join(" ", sequencesOnTheTable
                                                              .Select(p => p.Key.ToString()[0] + p.Value.ToString()));

        /// <summary>Info about current turn, number of played cards and risky cards</summary>
        public string Stats => $"Turn: {CurrentTurn}, cards: {SuccessfullyPlayedCards}, with risk: {MovesWithRisk}";

        /// <summary>Detailed game situation, player cards and cards on the table</summary>
        public string DetailedStats => $"Turn: {CurrentTurn}, Score: {SuccessfullyPlayedCards}, " +
                                       $"Finished: {GameIsFinished}\n  Current player: {ActivePlayerCards}\n     " +
                                       $"Next player: {PassivePlayerCards}\n           Table: {CardsOnTable}";

        #endregion
        #region Constructors

        /// <summary>Creates new Hanabi game with given cards in the deck</summary>
        /// <param name = "abbreviations">String, that contains all abbreviations of the starting deck</param>
        public Hanabi(string abbreviations)
        {
            MaxAmountOfCardsOnTable = Card.NumberOfColors * Card.RankLimit;
            deck = CreateDeck(abbreviations, MaxCardsOnHands * 2 + 1);
            sequencesOnTheTable = Enum.GetValues(typeof (Card.Colors))
                                      .Cast<Card.Colors>()
                                      .Distinct()
                                      .ToDictionary(color => color, c => 0);
            currentActivePlayer = firstPlayer = new Player(deck.Take(MaxCardsOnHands));
            currentPassivePlayer = secondPlayer = new Player(deck.Skip(MaxCardsOnHands)
                                                                 .Take(MaxCardsOnHands));
            deck = deck.Skip(MaxCardsOnHands*2).ToList();
            deck.TrimExcess();
            Deck = new ReadOnlyCollection<Card>(deck);
            SequencesOnTheTable = new ReadOnlyDictionary<Card.Colors, int>(sequencesOnTheTable);
            MovePerformed += ChangeMove;
            GameOver += CloseGame;
        }

        #endregion
        #region Event Handlers

        /// <summary>Performs last game actions, and closes the game</summary>
        /// <param name = "info">Unused parameter, containing GameOver reason</param>
        private void CloseGame(string info)
        {
            GameIsFinished = true;
            deck.TrimExcess();
            MovePerformed();
        }

        /// <summary>Increments move, switches players</summary>
        private void ChangeMove()
        {
            CurrentTurn++;
            currentActivePlayer = CurrentTurn % 2 == 0 ? firstPlayer : secondPlayer;
            currentPassivePlayer = CurrentTurn % 2 == 1 ? firstPlayer : secondPlayer;
        }

        #endregion
        #region Private Methods

        /// <summary>Filling deck with cards, written in input text</summary>
        /// <param name = "abbreviationsString">String, that contains cards abbreviations, separated by spaces</param>
        /// <param name = "minAmountOfCards">Expected minimum amount of cards in the deck</param>
        private List<Card> CreateDeck(string abbreviationsString, int minAmountOfCards)
        {
            if (string.IsNullOrEmpty(abbreviationsString))
            {
                throw new GameCommandException("Card abbreviations expected");
            }
            List<Card> cards = Regex.Split(abbreviationsString, @" +")
                                    .Where(abbreviation => abbreviation != "")
                                    .Select(abbreviation => new Card(abbreviation))
                                    .ToList();
            if (cards.Count < minAmountOfCards)
            {
                throw new GameCommandException(
                    $"Expected at least {minAmountOfCards} cards, instead got {deck.Count}");
            }
            return cards;
        }

        /// <summary>Remove top card from the deck and return it</summary>
        /// <returns>Top card of the deck</returns>
        private Card GetNewCardFromDeck()
        {
            if (deck.Count == 0)
            {
                throw new InvalidOperationException("Can't take cards from an empty deck");
            }
            Card card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        /// <summary>Parsing string, that contains single card number</summary>
        private int ParseCardNumber(string cardNumberString)
        {
            if (string.IsNullOrEmpty(cardNumberString))
            {
                throw new GameCommandException("Card number string expected");
            }
            int cardNumber;
            if (!int.TryParse(cardNumberString, out cardNumber))
            {
                throw new GameCommandException("Card number must be an integer: " + cardNumberString);
            }
            if (cardNumber < 0 || cardNumber >= MaxCardsOnHands)
            {
                throw new GameCommandException("Card number out of range: " + cardNumber);
            }
            return cardNumber;
        }

        /// <summary>Parsing string, that contains one or many card numbers</summary>
        private int[] ParseMultipleCardNumbers(string cardNumbersString)
        {
            if (string.IsNullOrEmpty(cardNumbersString))
            {
                throw new GameCommandException("Card number string expected");
            }
            int[] cardNumbers = Regex.Split(cardNumbersString, @" +")
                                     .Where(str => str != "")
                                     .Select(ParseCardNumber)
                                     .ToArray();
            if (cardNumbers.Length == 0)
            {
                throw new GameCommandException("At least one card number is expected");
            }
            return cardNumbers;
        }

        /// <summary>Checks if it's possible to play card of given color and rank</summary>
        /// <returns><value>true</value> if such card can be legally played, <value>false</value> otherwise</returns>
        private bool CheckMoveAvailability(Card.Colors color, int rank)
        {
            if (!SequencesOnTheTable.ContainsKey(color))
            {
                throw new ArgumentException("Failed to find this color in current sequences dictionary: " + color);
            }
            if (rank < 1 || rank > Card.RankLimit)
            {
                throw new ArgumentOutOfRangeException("Rank out of range: " + rank);
            }
            return SequencesOnTheTable[color] + 1 == rank;
        }

        /// <summary>Checks the safety level of a potential move of the given player</summary>
        /// <param name = "player">Player which move needs to be checked</param>
        /// <param name = "cardNumber">Position of the card in players hands</param>
        /// <returns>Safety level of the potential move</returns>
        private string CheckMoveSafety(Player player, int cardNumber)
        {
            Card card = player.CardsOnHand[cardNumber];
            if (!CheckMoveAvailability(card.Color, card.Rank))
            {
                return "Card cannot be played";
            }
            ReadOnlyCollection<int> possibleRanks = player.GiveRankGuesses(cardNumber);
            if (possibleRanks == null)
            {
                throw new NullReferenceException(nameof(possibleRanks));
            }
            if (!possibleRanks.Contains(card.Rank))
            {
                throw new InvalidOperationException("Player eliminated actual rank of the card");
            }
            if (possibleRanks.Count > 1)
            {
                return "Player wasn't certain about the rank: " + string.Join(" ", possibleRanks);
            }
            ReadOnlyCollection<Card.Colors> possibleColors = player.GiveColorGuesses(cardNumber);
            if (possibleColors == null)
            {
                throw new NullReferenceException(nameof(possibleColors));
            }
            if (!possibleColors.Contains(card.Color))
            {
                throw new InvalidOperationException("Player eliminated actual color of the card");
            }
            int numberOfMatched = possibleColors.Count(color => CheckMoveAvailability(color, card.Rank));
            if (numberOfMatched != possibleColors.Count)
            {
                return $"Only {numberOfMatched} out of {possibleColors.Count} possible colors matched the sequences";
            }
            return "Safe";
        }

        #endregion
        #region Public Methods

        /// <summary>Type of a game move.
        /// Current active player puts one of his cards on the table and takes a new one.
        /// If the deck is empty - game ends</summary>
        /// <param name = "cardString">String, that contains card number</param>
        public void PlayCard(string cardString)
        {
            if (GameIsFinished) return;
            int cardNumber = ParseCardNumber(cardString);
            Card playedCard = currentActivePlayer.CardsOnHand[cardNumber];
            string moveSafety = CheckMoveSafety(currentActivePlayer, cardNumber);
            if (moveSafety == "Card cannot be played")
            {
                GameOver("Card cannot be played");
                return;
            }
            if (moveSafety != "Safe")
            {
                MovesWithRisk++;
                RiskyMove($"{playedCard} - {moveSafety}");
            }
            SuccessfullyPlayedCards++;
            sequencesOnTheTable[playedCard.Color]++;
            currentActivePlayer.DropCard(cardNumber);
            currentActivePlayer.TakeNewCard(GetNewCardFromDeck());
            if (SuccessfullyPlayedCards == MaxAmountOfCardsOnTable) GameOver("Sequences are finished!");
            else if (deck.Count == 0) GameOver("Deck ended");
            else MovePerformed();
        }

        /// <summary>Type of a game move.
        /// Current active player drops one of his cards and takes a new one. If the deck is empty - game ends</summary>
        /// <param name = "cardString">String that contains card number</param>
        public void DropCard(string cardString)
        {
            if (GameIsFinished) return;
            int cardNumber = ParseCardNumber(cardString);
            currentActivePlayer.DropCard(cardNumber);
            currentActivePlayer.TakeNewCard(GetNewCardFromDeck());
            if (deck.Count == 0) GameOver("Deck ended");
            else MovePerformed();
        }

        /// <summary>Type of a game move.
        /// Current active player tells current passive player
        /// which of his cards have a specific color</summary>
        /// <param name = "colorName">Name of the color</param>
        /// <param name = "cardNumbersString">String that contains card numbers</param>
        public void TellColor(string colorName, string cardNumbersString)
        {
            if (GameIsFinished) return;
            Card.Colors color = Card.ParseColor(colorName);
            int[] cardNumbers = ParseMultipleCardNumbers(cardNumbersString);
            if (currentPassivePlayer.ReceiveColorHint(color, cardNumbers)) MovePerformed();
            else GameOver("Player told the wrong cards");
        }

        /// <summary>Type of a game move.
        /// Current active player tells current passive player
        /// which of his cards have a specific rank</summary>
        /// <param name = "rankString">String that contains rank number</param>
        /// <param name = "cardNumbersString">String that contains card numbers</param>
        public void TellRank(string rankString, string cardNumbersString)
        {
            if (GameIsFinished) return;
            int rank = Card.ParseRank(rankString);
            int[] cardNumbers = ParseMultipleCardNumbers(cardNumbersString);
            if (currentPassivePlayer.ReceiveRankHint(rank, cardNumbers)) MovePerformed();
            else GameOver("Player told the wrong cards");
        }

        #endregion
    }
}
