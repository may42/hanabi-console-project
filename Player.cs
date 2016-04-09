using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace my_console_project
{
    class Player
    {
        #region Nested-Classes
        
        /// <summary>Special type used by the player for storing card
        /// information, gained from the hints of the other player</summary>
        /// <remarks>This type allows to conceal properties set-accessors and sensitive methods of
        /// the <see cref="PlayerCardInfo"/> class, and safely expose it as a public field</remarks>
        public interface IPlayerCardInfo
        {
            /// <summary>List of all possible card colors, from the viewpoint
            /// of the player, except the actual color of the card</summary>
            /// <remarks>Actual color of the card is not included for the code simplicity</remarks>
            ReadOnlyCollection<Card.Colors> PossibleColors { get; }

            /// <summary>List of all possible card ranks, from the viewpoint
            /// of the player, except the actual rank of the card</summary>
            /// <remarks>Actual rank of the card is not included for the code simplicity</remarks>
            ReadOnlyCollection<int> PossibleRanks { get; }

            /// <summary>Property, that shows, whether player determined color of the card, or not</summary>
            bool ColorIsDetermined { get; }

            /// <summary>Property, that shows, whether player determined rank of the card, or not</summary>
            bool RankIsDetermined { get; }
        }

        /// <summary>Special private class used by the player for managing
        /// card information, gained from the hints of the other player</summary>
        /// <remarks>All methods and property accessors that are not declared in the
        /// <see cref="IPlayerCardInfo"/> are concealed in the <see cref="KnownCardInfo"/></remarks>
        private class PlayerCardInfo : IPlayerCardInfo
        {
            #region Fields

            /// <summary>List that contains all existing card colors</summary>
            /// <remarks>Needed for fast <see cref="PossibleColors"/> initialization</remarks>
            private static readonly List<Card.Colors> AllColors;
            /// <summary>List that contains all existing card ranks</summary>
            /// <remarks>Needed for fast <see cref="PossibleRanks"/> initialization</remarks>
            private static readonly List<int> AllRanks;
            /// <summary>Card reference, needed for not allowing actual info elimination</summary>
            private readonly Card card;

            /// <summary>List of all possible card colors, except the actual rank of the card</summary>
            public readonly List<Card.Colors> possibleColors;
            /// <summary>List of all possible card ranks, except the actual rank of the card</summary>
            public readonly List<int> possibleRanks;

            #endregion
            #region Props

            /// <summary>Wrapper for the possible card colors, will be used in <see cref="IPlayerCardInfo"/></summary>
            public ReadOnlyCollection<Card.Colors> PossibleColors { get; }
            /// <summary>Wrapper for the possible card ranks, will be used in <see cref="IPlayerCardInfo"/></summary>
            public ReadOnlyCollection<int> PossibleRanks { get; }
            /// <summary>Property, that shows, whether player determined color of the card, or not</summary>
            public bool ColorIsDetermined { get; set; }
            /// <summary>Property, that shows, whether player determined rank of the card, or not</summary>
            public bool RankIsDetermined { get; set; }

            #endregion
            #region Constructors

            /// <summary>Initializes static fields <see cref="AllRanks"/> and <see cref="AllColors"/></summary>
            static PlayerCardInfo()
            {
                AllColors = Enum.GetValues(typeof(Card.Colors))
                                .Cast<Card.Colors>()
                                .Distinct()
                                .ToList();
                AllRanks = new List<int>();
                for (int i = 1; i <= Card.RankLimit; i++)
                {
                    AllRanks.Add(i);
                }
            }

            /// <summary>Creates new PlayerCardInfo instance for the given card</summary>
            /// <param name = "card">Card, which information will be stored</param>
            /// <remarks>Throws an exception if card is null, which is handy for Player initialization</remarks>
            public PlayerCardInfo(Card card)
            {
                if (card == null)
                {
                    throw new ArgumentNullException(nameof(card));
                }
                this.card = card;
                possibleColors = AllColors.Where(color => color != card.Color)
                                          .ToList();
                possibleRanks = AllRanks.Where(rank => rank != card.Rank)
                                        .ToList();
                PossibleColors = new ReadOnlyCollection<Card.Colors>(possibleColors);
                PossibleRanks = new ReadOnlyCollection<int>(possibleRanks);
            }

            #endregion
            #region Methods

            /// <summary>Player eliminates one of the possible colors of the card</summary>
            /// <remarks>Does nothing if the color is already eliminated, Throws an
            /// error if player tries to eliminate actual color of the card</remarks>
            public void EliminateColor(Card.Colors color)
            {
                if (color == card.Color)
                {
                    throw new InvalidOperationException("Cant eliminate actual card color: " + color);
                }
                if (ColorIsDetermined) return;
                possibleColors.Remove(color);
                if (possibleColors.Count > 0) return;
                possibleColors.TrimExcess();
                ColorIsDetermined = true;
            }

            /// <summary>Player eliminates one of the possible ranks of the card</summary>
            /// <remarks>Does nothing if the rank is already eliminated, Throws an
            /// error if player tries to eliminate actual rank of the card</remarks>
            public void EliminateRank(int rank)
            {
                if (rank == card.Rank)
                {
                    throw new InvalidOperationException("Cant eliminate actual card rank: " + rank);
                }
                if (RankIsDetermined) return;
                possibleRanks.Remove(rank);
                if (possibleRanks.Count > 0) return;
                possibleRanks.TrimExcess();
                RankIsDetermined = true;
            }

            #endregion
        }

        #endregion
        #region Fields

        /// <summary>List of player cards</summary>
        private readonly List<Card> cardsOnHand;
        /// <summary>List of all the information that player knows about his cards</summary>
        private readonly List<PlayerCardInfo> knownCardInfo;

        /// <summary>Read-only list of players cards</summary>
        /// <remarks>All card manipulations are made through specialized player action methods</remarks>
        public readonly ReadOnlyCollection<Card> CardsOnHand;
        /// <summary>List of all the information that player knows about his cards</summary>
        /// <remarks>Uses covariance to upcast parameter PlayerCardInfo to its interface
        /// IPlayerCardInfo, thus, protecting its public fields</remarks>
        public readonly IReadOnlyCollection<IPlayerCardInfo> KnownCardInfo;

        #endregion
        #region Constructors

        /// <summary>Creates Player instance with given cards on hand</summary>
        /// <remarks>Player doesn't monitor number of his cards. This responsibility lies entirely on Hanabi</remarks>
        /// <param name = "initialCards">Initial player cards</param>
        public Player(IEnumerable<Card> initialCards)
        {
            if (initialCards == null)
            {
                throw new ArgumentNullException(nameof(initialCards));
            }
            // Shallow copying is safe, because Card instances are immutable.
            cardsOnHand = initialCards.ToList();
            CardsOnHand = cardsOnHand.AsReadOnly();
            knownCardInfo = cardsOnHand.Select(card => new PlayerCardInfo(card))
                                       .ToList();
            // Covariance-magic in action.
            KnownCardInfo = knownCardInfo.AsReadOnly();
        }

        #endregion
        #region Private Methods

        /// <summary>Converts array of card numbers into array, that
        /// shows whether certain card is listed or not</summary>
        /// <remarks>Duplicates are ignored</remarks>
        /// <param name = "cardNumbers">Array of card numbers</param>
        /// <returns>Array that shows whether certain card number is
        /// listed in <paramref name="cardNumbers"/> or not</returns>
        private bool[] CheckWhichNumbersAreListed(int[] cardNumbers)
        {
            if (cardNumbers == null || cardNumbers.Length == 0)
            {
                throw new ArgumentException("Expected at least one card number");
            }
            var checkedCards = new bool[cardsOnHand.Count];
            foreach (int number in cardNumbers)
            {
                if (number < 0 || number >= cardsOnHand.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(cardNumbers));
                }
                checkedCards[number] = true;
            }
            return checkedCards;
        }

        #endregion
        #region Public Methods

        /// <summary>Gives string representation of all cards on player's hand</summary>
        /// <returns>Concatenation of all card abbreviations, separated with spaces</returns>
        public override string ToString()
        {
            return string.Join(" ", cardsOnHand);
        }

        /// <summary>Tells player to drop specific card</summary>
        /// <param name = "cardNumber">Number of the card, that needs to be dropped</param>
        public void DropCard(int cardNumber)
        {
            if (cardsOnHand.Count == 0)
            {
                throw new InvalidOperationException("Can't drop any cards, player has no cards");
            }
            if (cardNumber < 0 || cardNumber >= cardsOnHand.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(cardNumber));
            }
            cardsOnHand.RemoveAt(cardNumber);
            knownCardInfo.RemoveAt(cardNumber);
        }

        /// <summary>Gives a new card to the player</summary>
        /// <param name = "newCard">New card that will be placed last in players hands</param>
        public void TakeNewCard(Card newCard)
        {
            if (newCard == null)
            {
                throw new ArgumentNullException(nameof(newCard));
            }
            cardsOnHand.Add(newCard);
            knownCardInfo.Add(new PlayerCardInfo(newCard));
        }

        /// <summary>Gives the player a hint about all cards that have certain color</summary>
        /// <remarks>If <paramref name="colorHint"/> is out of <see cref="Card.Colors"/> enumerator range -
        /// "received hint" is considered wrong and false is returned. Method can be easily modified to
        /// throw a GameCommandException instead of returning false, when wrong hint is received.</remarks>
        /// <param name = "colorHint">Reported color</param>
        /// <param name = "cardNumbers">Non-empty array of reported card numbers</param>
        /// <returns><value>true</value> if hint is accurate, <value>false</value> if hint is wrong</returns>
        public bool ReceiveColorHint(Card.Colors colorHint, int[] cardNumbers)
        {
            if (cardsOnHand.Count == 0)
            {
                throw new InvalidOperationException("Player can't get any hints, player has no cards");
            }
            bool[] reportedCards = CheckWhichNumbersAreListed(cardNumbers);
            bool[] actualCards = cardsOnHand.Select(card => card.Color == colorHint).ToArray();
            if (!reportedCards.SequenceEqual(actualCards))
            {
                return false;
            }
            for (int i = 0; i < reportedCards.Length; i++)
            {
                if (reportedCards[i])
                {
                    knownCardInfo[i].ColorIsDetermined = true;
                    knownCardInfo[i].possibleColors.Clear();
                    knownCardInfo[i].possibleColors.TrimExcess();
                }
                else
                {
                    knownCardInfo[i].EliminateColor(colorHint);
                }
            }
            return true;
        }

        /// <summary>Gives the player a hint about all cards that have certain rank</summary>
        /// <remarks>If <paramref name="rankHint"/> is out of range -
        /// "received hint" is considered wrong and false is returned</remarks>
        /// <param name = "rankHint">Reported rank</param>
        /// <param name = "cardNumbers">Non-empty array of reported card numbers</param>
        /// <returns><value>true</value> if hint is accurate, <value>false</value> if hint is wrong</returns>
        public bool ReceiveRankHint(int rankHint, int[] cardNumbers)
        {
            if (cardsOnHand.Count == 0)
            {
                throw new InvalidOperationException("Player can't get any hints, player has no cards");
            }
            bool[] reportedCards = CheckWhichNumbersAreListed(cardNumbers);
            bool[] actualCards = cardsOnHand.Select(card => card.Rank == rankHint).ToArray();
            if (!reportedCards.SequenceEqual(actualCards))
            {
                return false;
            }
            for (int i = 0; i < reportedCards.Length; i++)
            {
                if (reportedCards[i])
                {
                    knownCardInfo[i].RankIsDetermined = true;
                    knownCardInfo[i].possibleRanks.Clear();
                    knownCardInfo[i].possibleColors.TrimExcess();
                }
                else
                {
                    knownCardInfo[i].EliminateRank(rankHint);
                }
            }
            return true;
        }

        #endregion
    }
}