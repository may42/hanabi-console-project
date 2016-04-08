using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace my_console_project
{
    class Player
    {
        #region Nested-Classes

        /// todo: PlayersCard documentation
        /// <summary></summary>
        /// <remarks></remarks>
        private class PlayerCardInfo
        {
            #region Fields

            /// <summary></summary>
            /// <remarks></remarks>
            private static readonly List<int> AllRanks;
            /// <summary></summary>
            /// <remarks></remarks>
            private static readonly List<Card.Colors> AllColors;
            /// <summary></summary>
            /// <remarks></remarks>
            private readonly Card card;

            /// <summary></summary>
            /// <remarks></remarks>
            public List<int> PossibleRanks;
            /// <summary></summary>
            /// <remarks></remarks>
            public List<Card.Colors> PossibleColors;

            #endregion
            #region Props

            /// <summary></summary>
            /// <remarks></remarks>
            public bool RankIsDetermined { get; set; }

            /// <summary></summary>
            /// <remarks></remarks>
            public bool ColorIsDetermined { get; set; }

            #endregion
            #region Constructors

            /// <summary></summary>
            /// <remarks></remarks>
            static PlayerCardInfo()
            {
                AllRanks = new List<int>();
                for (int i = 1; i <= Card.RankLimit; i++)
                {
                    AllRanks.Add(i);
                }
                AllColors = Enum.GetValues(typeof(Card.Colors))
                                .Cast<Card.Colors>()
                                .Distinct()
                                .ToList();
            }

            /// <summary></summary>
            /// <remarks></remarks>
            public PlayerCardInfo(Card card)
            {
                if (card == null)
                {
                    throw new ArgumentException("Player initial cards can't be null");
                }
                this.card = card;
                PossibleColors = AllColors.Where(color => color != card.Color)
                                          .ToList();
                PossibleRanks = AllRanks.Where(rank => rank != card.Rank)
                                        .ToList();
            }

            #endregion
            #region Methods

            /// <summary></summary>
            /// <remarks>Does nothing if color is already eliminated</remarks>
            public void EliminateColor(Card.Colors color)
            {
                if (color == card.Color)
                {
                    throw new InvalidOperationException("Cant eliminate actual card color: " + color);
                }
                if (ColorIsDetermined) return;
                PossibleColors.Remove(color);
                if (PossibleColors.Count > 0) return;
                ColorIsDetermined = true;
                PossibleColors = null;
            }

            /// <summary></summary>
            /// <remarks>Does nothing if rank is already eliminated</remarks>
            public void EliminateRank(int rank)
            {
                if (rank == card.Rank)
                {
                    throw new InvalidOperationException("Cant eliminate actual card rank: " + rank);
                }
                if (RankIsDetermined) return;
                PossibleRanks.Remove(rank);
                if (PossibleRanks.Count > 0) return;
                RankIsDetermined = true;
                PossibleRanks = null;
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
                    knownCardInfo[i].PossibleColors = null;
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
        public bool ReceiveRankNumberHint(int rankHint, int[] cardNumbers)
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
                    knownCardInfo[i].PossibleRanks = null;
                }
                else
                {
                    knownCardInfo[i].EliminateRank(rankHint);
                }
            }
            return true;
        }
        
        /// <summary>Tells all of the player's guesses about the color of
        /// specific card, except the actual color of the card</summary>
        /// <param name = "cardNumber">Number of the card in player hands</param>
        /// <returns>List of all possible colors of the card, from the viewpoint of the player</returns>
        public List<Card.Colors> GiveColorGuesses(int cardNumber)
        {
            return knownCardInfo[cardNumber].PossibleColors.ToList();
        }

        /// <summary>Check if the player knows the color of a particular card with certainty</summary>
        /// <param name = "cardNumber">Number of the card, that need to be checked</param>
        /// <returns><value>true</value> if the player knows the color of the
        /// card with certainty, <value>false</value> otherwise</returns>
        public bool CheckIfColorIsDetermined(int cardNumber)
        {
            return knownCardInfo[cardNumber].ColorIsDetermined;
        }

        /// <summary>Check if the player knows the rank of a particular card with certainty</summary>
        /// <param name = "cardNumber">Number of the card, that need to be checked</param>
        /// <returns><value>true</value> if the player knows the rank of the
        /// card with certainty, <value>false</value> otherwise</returns>
        public bool CheckIfRankIsDetermined(int cardNumber)
        {
            return knownCardInfo[cardNumber].RankIsDetermined;
        }

        #endregion
    }
}