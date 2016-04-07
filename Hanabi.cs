using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace my_console_project
{
    class Hanabi
    {
        #region Delegates & Events

        public delegate void GameMethodContainer<T>(T param);

        public delegate void GameMethodContainer();

        /// <summary>Fires when game is finished without errors</summary>
        /// <remarks>Empty delegate avoids the need to check events for null</remarks>
        public event GameMethodContainer<string> GameOver = delegate { };

        /// <summary>Fires when risky move is played</summary>
        /// <param>Provides additional info about the risky move as a parameter</param>
        public event GameMethodContainer<string> RiskyMove = delegate { };

        /// <summary>Fires when the player action is performed successfully</summary>
        /// <remarks>That includes not only "Play card", but also "Drop card", "Tell color" and "Tell rank"</remarks>
        public event GameMethodContainer MovePerformed = delegate { };

        #endregion
        #region Fields

        private readonly Player firstPlayer, secondPlayer;
        private Player currentActivePlayer, currentPassivePlayer;
        private readonly List<Card> deck;
        private readonly Dictionary<Card.Colors, int> sequencesOnTheTable;

        public readonly int MaxCardsAmountOnTable, MinCardsAmountInDeck, MaxCardsOnHands = 5;
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
            MaxCardsAmountOnTable = Card.RankLimit * Card.NumberOfColors;
            MinCardsAmountInDeck = MaxCardsOnHands*2 + 1;
            deck = CreateDeck(abbreviations);
            sequencesOnTheTable = Enum.GetValues(typeof (Card.Colors))
                                      .Cast<Card.Colors>()
                                      .Distinct()
                                      .ToDictionary(color => color, c => 0);
            currentActivePlayer = firstPlayer = new Player(deck.Take(MaxCardsOnHands));
            currentPassivePlayer = secondPlayer = new Player(deck.Skip(MaxCardsOnHands)
                                                                 .Take(MaxCardsOnHands));
            deck = deck.Skip(MaxCardsOnHands*2).ToList();
            Deck = new ReadOnlyCollection<Card>(deck);
            SequencesOnTheTable = new ReadOnlyDictionary<Card.Colors, int>(sequencesOnTheTable);
            GameOver += CloseGame;
            MovePerformed += ChangeMove;
        }

        #endregion
        #region Event Handlers

        /// <summary>Performs last game actions, and closes the game</summary>
        /// <param name = "info">Unused parameter, containing GameOver reason</param>
        private void CloseGame(string info)
        {
            GameIsFinished = true;
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

        /// <summary>Check if it's possible to play card of given color and rank</summary>
        /// <param name = "color">Color, which need to be checked</param>
        /// <param name = "rank">Rank, which need to be checked</param>
        /// <returns><value>true</value> if such card can be legally played, <value>false</value> otherwise</returns>
        private bool CheckMoveAvaliability(Card.Colors color, int rank)
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

        /// <summary>Filling deck with cards, written in input text</summary>
        /// <param name = "abbreviationsString">String, that contains cards abbreviations, separated by spaces</param>
        private List<Card> CreateDeck(string abbreviationsString)
        {
            if (string.IsNullOrEmpty(abbreviationsString))
            {
                throw new GameCommandException("Card abbreviations expected");
            }
            List<Card> cards = Regex.Split(abbreviationsString, @" +")
                                    .Where(abbreviation => abbreviation != "")
                                    .Select(abbreviation => new Card(abbreviation))
                                    .ToList();
            if (cards.Count < MinCardsAmountInDeck)
            {
                throw new GameCommandException(
                    $"Expected at least {MinCardsAmountInDeck} cards, instead got {cards.Count}");
            }
            return cards;
        }

        /// <summary>Remove top card from the deck and return it</summary>
        /// <returns>Top card of the deck</returns>
        private Card GetNewCardFromDeck()
        {
            if (deck.Count == 0)
            {
                throw new InvalidOperationException("Can't take cards from empty deck");
            }
            Card card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        /// <summary>Increases one of the color sequences on the table</summary>
        /// <param name = "color">Color of the sequence that need to be increased</param>
        private void IncreaseSequence(Card.Colors color)
        {
            if (!SequencesOnTheTable.ContainsKey(color))
            {
                throw new ArgumentException($"Failed to find {color}-color sequence on the table");
            }
            if (SequencesOnTheTable[color] == Card.RankLimit)
            {
                throw new InvalidOperationException(
                    $"Can't increase {color}-color sequence, maximum rank {Card.RankLimit} is reached");
            }
            sequencesOnTheTable[color]++;
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
                throw new GameCommandException("Expected at least one card number");
            }
            return cardNumbers;
        }

        /// <summary>Checks whether it safe or risky to play a certain card for the current active player</summary>
        /// <param name = "cardNumber">Position of the card in players hands</param>
        /// <returns><value>true</value> if the move is guaranteed safe for the current active player,
        /// <value>false</value> if the move is risky</returns>
        private bool CheckMoveSafety(int cardNumber)
        {
            Card playedCard = currentActivePlayer.CardsOnHand[cardNumber];
            return currentActivePlayer.RankIsDetermined(cardNumber) &&
                   currentActivePlayer.GiveColorGuesses(cardNumber)
                                      .All(color => CheckMoveAvaliability(color, playedCard.Rank));
        }

        /// <summary>Temporary method for detailed risk processing</summary>
        private void ProcessRisk(int cardNumber)
        {
            Card card = currentActivePlayer.CardsOnHand[cardNumber];
            bool rankIsDetermined = currentActivePlayer.RankIsDetermined(cardNumber);
            if (!rankIsDetermined)
            {
                MovesWithRisk++;
                RiskyMove($"{card.Color} {card.Rank} - player wasn't certain about the rank");
                return;
            }
            List<Card.Colors> possibleColors = currentActivePlayer.GiveColorGuesses(cardNumber);
            int numberOfMatching = possibleColors.Count(color => CheckMoveAvaliability(color, card.Rank));
            if (numberOfMatching == possibleColors.Count) return;
            MovesWithRisk++;
            RiskyMove($"{card.Color} {card.Rank} - only {numberOfMatching} out of " +
                      $"{possibleColors.Count} possible colors matched the sequences");
        }

        #endregion
        #region Public Methods

        /// <summary>Type of game move. Current active player puts one of his card
        /// on the table and takes a new one. If deck is empty - game ends</summary>
        /// <param name = "cardString">String, that contains card number</param>
        public void PlayCard(string cardString)
        {
            if (GameIsFinished) return;
            int cardNumber = ParseCardNumber(cardString);
            Card card = currentActivePlayer.CardsOnHand[cardNumber];
            if (!CheckMoveAvaliability(card.Color, card.Rank))
            {
                GameOver("Card cannot be played");
                return;
            }
            //ProcessRisk(cardNumber);
            if (!CheckMoveSafety(cardNumber))
            {
                MovesWithRisk++;
                RiskyMove($"{card.Color} {card.Rank}");
            }
            SuccessfullyPlayedCards++;
            IncreaseSequence(card.Color);
            currentActivePlayer.DropCard(cardNumber);
            currentActivePlayer.TakeNewCard(GetNewCardFromDeck());
            if (SuccessfullyPlayedCards == MaxCardsAmountOnTable) GameOver("Sequences are finished!");
            else if (deck.Count == 0) GameOver("Deck ended");
            else MovePerformed();
        }

        /// <summary>Type of game move.
        /// Current active player drops one of his cards and takes a new one.
        /// If deck is empty - game ends</summary>
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

        /// <summary>Type of game move.
        /// Current active player tells to current passive player which of his cards have specific color</summary>
        /// <param name = "colorName">Name of the color</param>
        /// <param name = "cardNumbersString">String that contains card numbers</param>
        public void TellColor(string colorName, string cardNumbersString)
        {
            if (GameIsFinished) return;
            Card.Colors color = Card.ParseColor(colorName);
            int[] cardNumbers = ParseMultipleCardNumbers(cardNumbersString);
            if (currentPassivePlayer.ReceiveColorHint(color, cardNumbers))
            {
                MovePerformed();
            }
            else GameOver("Player told wrong cards");
        }

        /// <summary>Type of game move.
        /// Current active player tells to current passive player which of his cards have specific rank</summary>
        /// <param name = "rankString">String that contains rank number</param>
        /// <param name = "cardNumbersString">String that contains card numbers</param>
        public void TellRank(string rankString, string cardNumbersString)
        {
            if (GameIsFinished) return;
            int rank = Card.ParseRank(rankString);
            int[] cardNumbers = ParseMultipleCardNumbers(cardNumbersString);
            if (currentPassivePlayer.ReceiveRankNumberHint(rank, cardNumbers)) MovePerformed();
            else GameOver("Player told wrong cards");
        }

#endregion
    }
}
