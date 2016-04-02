using System;
using System.Collections.Generic;

namespace my_console_project
{
    class Player
    {
        private readonly Hanabi _playersGame;
        public List<Card> CardsOnHand { get; }
        
        public Player(Hanabi game)
        {
            _playersGame = game;
            CardsOnHand = new List<Card>();
            //takes full hand of cards
            for (int i = 0; i < _playersGame.NumberOfCardsInPlayersHand; i++)
            {
                TakeNewCard();
            }
        }
        
    #region Methods
        /// <summary>
        /// Refreshes accessibility level of color info about all cards
        /// </summary>
        /// <param name = "cardsNumbers">Array of color access indicators</param>
        public void GetColorInfo(bool[] cardsNumbers)
        {
            if (cardsNumbers.Length != CardsOnHand.Count)
            {
                throw new ArgumentException("Color info array had wrong length: " + cardsNumbers.Length);
            }
            for (int i = 0; i < CardsOnHand.Count; i++)
            {
                if (!cardsNumbers[i])
                {
                    // todo: illiminate color info
                    bool illiminate = true;
                }
                else
                {
                    // todo: add color info
                    bool determine = true;
                }
            }
        }

        /// <summary>
        /// Refreshes accessibility level of rank info about all cards
        /// </summary>
        /// <param name = "cardsNumbers">Array of color access indicators</param>
        public void GetRankInfo(bool[] cardsNumbers)
        {
            if (cardsNumbers.Length != CardsOnHand.Count)
            {
                throw new ArgumentException("Rank info array had wrong length: " + cardsNumbers.Length);
            }
            for (int i = 0; i < CardsOnHand.Count; i++)
            {
                if (!cardsNumbers[i])
                {
                    // todo: illiminate rank info
                    bool illiminate = true;
                }
                else
                {
                    // todo: add rank info
                    bool determine = true;
                }
            }
        }

        /// <summary>
        /// Drops specific card in player's hands and takes one more from deck, if deck isn't empty
        /// </summary>
        /// <param name = "cardNumber">Number of card in hands, which is has to be dropped</param>
        public void DropAndTryTakeNewCard(int cardNumber)
        {
            if (cardNumber < 0 || cardNumber >= CardsOnHand.Count)
            {
                throw new ArgumentException("Invalid card number: " + cardNumber);
            }
            CardsOnHand.RemoveAt(cardNumber);
            if (_playersGame.Deck.Count > 0)
            {
                TakeNewCard();
            }
        }

        /// <summary>
        /// Takes new card from deck into player's hands
        /// </summary>
        private void TakeNewCard()
        {
            CardsOnHand.Add(new Card(_playersGame.TakeCardFromDeck()));
        }

    #endregion
    }
}