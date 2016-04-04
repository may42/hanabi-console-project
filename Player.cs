using System;
using System.Collections.Generic;

namespace my_console_project
{
    class Player
    {
        private readonly Hanabi playersGame;
        public List<Card> CardsOnHand { get; }
        
        public Player(Hanabi game)
        {
            playersGame = game;
            CardsOnHand = new List<Card>();
            //takes full hand of cards
            for (int i = 0; i < playersGame.NumberOfCardsInPlayersHand; i++)
            {
                TakeNewCard();
            }
        }

    #region Private Methods

        /// <summary>
        /// Takes new card from deck into player's hands
        /// </summary>
        private void TakeNewCard()
        {
            CardsOnHand.Add(playersGame.TakeCardFromDeck());
        }

        /// <summary>
        /// Convert array of card numbers into array, that shows whether certain card number is given or not
        /// </summary>
        /// <param name = "cardNumbers">Array of card numbers</param>
        /// <returns>Array that shows whether certain card number is given or not</returns>
        private bool[] CheckCardNumbers(int[] cardNumbers)
        {
            if (cardNumbers == null || cardNumbers.Length == 0)
            {
                throw new ArgumentException("Expected at least one card number");
            }
            bool[] checkedCards = new bool[CardsOnHand.Count];
            foreach (int number in cardNumbers)
            {
                if (number < 0 || number >= CardsOnHand.Count)
                {
                    throw new ArgumentOutOfRangeException("Card number out of range: " + number);
                }
                // Duplicates are ignored
                checkedCards[number] = true;
            }
            return checkedCards;
        }

    #endregion
    #region Public Methods

        /// <summary>
        /// Drops specific card in player's hands and takes one more from deck, if deck isn't empty
        /// </summary>
        /// <param name = "cardNumber">Number of card in hands, which is has to be dropped</param>
        public void DropAndTryTakeNewCard(int cardNumber)
        {
            // ParseCardNumbers exception duplicate
            if (cardNumber < 0 || cardNumber >= CardsOnHand.Count)
            {
                throw new ArgumentOutOfRangeException("Card number out of range: " + cardNumber);
            }
            CardsOnHand.RemoveAt(cardNumber);
            TakeNewCard();
        }

        /// <summary>
        /// Receive hint about all cards that have certain color
        /// </summary>
        /// <param name = "hintedColor">Hinted color</param>
        /// <param name = "hintedCardNumbers">Array of hinted card numbers</param>
        /// <returns><value>false</value> if hint was wrong, <value>true</value> if hint was successful</returns>
        public bool ReceiveColorInfo(Card.Colors hintedColor, int[] hintedCardNumbers)
        {
            /*if (hintedColor == null)
            {
                throw new ArgumentNullException(nameof(hintedColor));
            }
            if (!Enum.IsDefined(typeof(Card.Colors), hintedColor))
            {
                throw new ArgumentException("Wrong cards has been hinted");
            }*/
            bool[] hintedCards = CheckCardNumbers(hintedCardNumbers);
            for (int i = 0; i < CardsOnHand.Count; i++)
            {
                if (hintedCards[i] != (CardsOnHand[i].Color == hintedColor))
                {
                    //throw new GameCommandException("Wrong cards has been hinted");
                    return false;
                }
            }
            foreach (bool hasColor in hintedCards)
            {
                if (!hasColor)
                {
                    // todo: add illiminated-color information
                    bool illiminate = true;
                }
                else
                {
                    // todo: add determined-color information
                    bool determine = true;
                }
            }
            return true;
        }

        /// <summary>
        /// Receive hint about all cards that have certain rank
        /// </summary>
        /// <param name = "hintedRank">Hinted rank</param>
        /// <param name = "hintedCardNumbers">Array of hinted card numbers</param>
        /// <returns><value>false</value> if hint was wrong, <value>true</value> if hint was successful</returns>
        public bool TryReceiveRankInfo(int hintedRank, int[] hintedCardNumbers)
        {
            bool[] hintedCards = CheckCardNumbers(hintedCardNumbers);
            for (int i = 0; i < CardsOnHand.Count; i++)
            {
                if (hintedCards[i] != (CardsOnHand[i].Rank == hintedRank))
                {
                    //throw new GameCommandException("Wrong cards has been hinted");
                    return false;
                }
            }
            foreach (bool hasRank in hintedCards)
            {
                if (!hasRank)
                {
                    // todo: add illiminated-rank information
                    bool illiminate = true;
                }
                else
                {
                    // todo: add determined-rank information
                    bool determine = true;
                }
            }
            return true;
        }
        
    #endregion
    }
}