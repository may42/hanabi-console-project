using System;
using System.Linq;
using System.Collections.Generic;

namespace my_console_project
{
    class Player
    {
        #region Fields

        private readonly List<Card> cardsOnHand;

        /// <summary>Readonly list of players cards</summary>
        /// <remarks>All card manipulations are made through specialized player action methods</remarks>
        public readonly IReadOnlyList<Card> CardsOnHand;

        #endregion Fields
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
            if (initialCards.Any(card => card == null))
            {
                throw new ArgumentException("Player initial cards can't be null");
            }
            // Shallow copying is safe, because Card instances are unchangeable.
            cardsOnHand = initialCards.ToList();
            CardsOnHand = cardsOnHand.AsReadOnly();
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
        /// <returns>Concatenation of all card abbreviations, separeted with spaces</returns>
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
            foreach (bool cardHasColor in reportedCards)
            {
                if (cardHasColor)
                {
                    // todo: add determined-color information to risk calculation
                    bool determine = true;
                }
                else
                {
                    // todo: add illiminated-color information to risk calculation
                    bool illiminate = true;
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
            foreach (bool cardHasRank in reportedCards)
            {
                if (cardHasRank)
                {
                    // todo: add determined-rank information to risk calculation
                    bool determine = true;
                }
                else
                {
                    // todo: add illiminated-rank information to risk calculation
                    bool illiminate = true;
                }
            }
            return true;
        }

        #endregion
    }
}