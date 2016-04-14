using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hanabi
{
    /// <summary>Common type of a Hanabi player. Stores information about their
    /// cards, receives hints and commands to drop or take new cards</summary>
    class Player
    {
        #region Nested-Classes

        /// <summary>Special class used by the player for storing and managing
        /// card information, gained from the hints of the other player</summary>
        /// <remarks>I tried to safely expose this class as a ReadOnlyCollection
        /// member, using covariance to upcast it to an interface IPlayerCardInfo,
        /// thus, concealing its public fields, but this method turned out to be unsafe:
        /// <a href="https://github.com/may42/my-console-project/issues/6">issue #6</a></remarks>
        private class PlayerCardInfo
        {
            #region Fields

            /// <summary>List that contains all existing card colors</summary>
            /// <remarks>Needed for fast <see cref="PossibleColors"/> initialization</remarks>
            private static readonly List<Card.Colors> AllColors = new List<Card.Colors>(Card.NumberOfColors);
            /// <summary>List that contains all existing card ranks</summary>
            /// <remarks>Needed for fast <see cref="PossibleRanks"/> initialization</remarks>
            private static readonly List<int> AllRanks = new List<int>(Card.RankLimit);

            /// <summary>List of all possible colors of a certain card, from the viewpoint of the player</summary>
            public readonly List<Card.Colors> PossibleColors;
            /// <summary>List of all possible ranks of a certain, from the viewpoint of the player</summary>
            public readonly List<int> PossibleRanks;

            #endregion
            #region Constructors

            /// <summary>Initializes static fields <see cref="AllRanks"/> and <see cref="AllColors"/></summary>
            static PlayerCardInfo()
            {
                AllColors.AddRange(Enum.GetValues(typeof(Card.Colors))
                                       .Cast<Card.Colors>()
                                       .Distinct());
                for (int i = 1; i <= Card.RankLimit; i++)
                {
                    AllRanks.Add(i);
                }
            }

            /// <summary>Creates new PlayerCardInfo instance, that will store info about a certain card</summary>
            public PlayerCardInfo()
            {
                PossibleColors = new List<Card.Colors>(Card.NumberOfColors);
                PossibleRanks = new List<int>(Card.RankLimit);
                // AddRange method is used to prevent the reset of the specified initial capacity of the collections.
                PossibleColors.AddRange(AllColors);
                PossibleRanks.AddRange(AllRanks);
            }

            #endregion
        }

        #endregion
        #region Fields

        /// <summary>List of player cards</summary>
        private readonly List<Card> cardsOnHand;
        /// <summary>List of all the information that player knows about their cards</summary>
        private readonly List<PlayerCardInfo> knownCardInfo;
        /// <summary>Read-only list of player cards</summary>
        public readonly ReadOnlyCollection<Card> CardsOnHand;

        #endregion
        #region Constructors

        /// <summary>Creates Player instance with given cards on hand</summary>
        /// <remarks>Player doesn't monitor the number of their cards. This responsibility lies entirely on Hanabi</remarks>
        /// <param name = "initialCards">Initial player cards</param>
        public Player(IEnumerable<Card> initialCards)
        {
            if (initialCards == null)
            {
                throw new ArgumentNullException(nameof(initialCards));
            }
            // Shallow copying is safe, because Card instances are immutable.
            cardsOnHand = initialCards.ToList();
            if (cardsOnHand.Any(card => card == null))
            {
                throw new ArgumentException("Initial player cards can't be null");
            }
            CardsOnHand = cardsOnHand.AsReadOnly();
            knownCardInfo = cardsOnHand.Select(card => new PlayerCardInfo())
                                       .ToList();
        }

        #endregion
        #region Private Methods

        /// <summary>Converts array of card numbers into array, that
        /// shows whether certain card is listed or not</summary>
        /// <remarks>Duplicates are ignored</remarks>
        /// <param name = "cardNumbers">Array of card numbers</param>
        /// <returns>Array that shows whether a certain card number is
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

        /// <summary>Gives a string representation of all cards on player's hand</summary>
        /// <returns>Concatenation of all card abbreviations, separated with spaces</returns>
        public override string ToString()
        {
            return string.Join(" ", cardsOnHand.Select(card => card.Abbreviation));
        }

        /// <summary>Tells player to drop a specific card</summary>
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
            knownCardInfo.Add(new PlayerCardInfo());
        }

        /// <summary>Gives the player a hint about all his cards that have a certain color</summary>
        /// <remarks>If <paramref name="colorHint"/> is out of <see cref="Card.Colors"/> enumerator range -
        /// received hint is considered wrong and false is returned. Method can be easily modified to
        /// throw a GameCommandException instead of returning false, when wrong hint is received</remarks>
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
                    knownCardInfo[i].PossibleColors.RemoveAll(possibleColor => possibleColor != colorHint);
                }
                else
                {
                    knownCardInfo[i].PossibleColors.Remove(colorHint);
                }
            }
            return true;
        }

        /// <summary>Gives the player a hint about all his cards that have a certain rank</summary>
        /// <remarks>If <paramref name="rankHint"/> is out of range -
        /// received hint is considered wrong and false is returned</remarks>
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
                    knownCardInfo[i].PossibleRanks.RemoveAll(possibleRank => possibleRank != rankHint);
                }
                else
                {
                    knownCardInfo[i].PossibleRanks.Remove(rankHint);
                }
            }
            return true;
        }

        /// <summary>Tells all of the player's guesses about the color of a
        /// specific card, except the actual color of the card</summary>
        /// <param name = "cardNumber">Number of the card in player hands</param>
        /// <returns>List of all possible colors of the card, from the viewpoint of the player</returns>
        public ReadOnlyCollection<Card.Colors> GiveColorGuesses(int cardNumber)
        {
            return knownCardInfo[cardNumber].PossibleColors.AsReadOnly();
        }

        /// <summary>Tells all of the player's guesses about the rank of a
        /// specific card, except the actual rank of the card</summary>
        /// <param name = "cardNumber">Number of the card in player hands</param>
        /// <returns>List of all possible ranks of the card, from the viewpoint of the player</returns>
        public ReadOnlyCollection<int> GiveRankGuesses(int cardNumber)
        {
            return knownCardInfo[cardNumber].PossibleRanks.AsReadOnly();
        }

        #endregion
    }
}