using System;
using System.Collections.Generic;

namespace my_console_project
{
    class Player
    {
        private readonly Game _playersGame;
        public List<CardOnHand> CardsOnHands { get; private set; }

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
            if (takenCards.Count != Game.CardsValueInPlayersHand)
            {
                throw new ArgumentException("Invalid cards value.");
            }
            CardsOnHands = new List<CardOnHand>();
            foreach (Card card in takenCards)
            {
                CardsOnHands.Add(new CardOnHand(card));
            }
        }

    #endregion
    #region Methods
        /// <summary>
        /// Refreshes accessibility level of color info about all cards
        /// </summary>
        /// <param name = "cardsNumbers">Array of color access indicators</param>
        public void GetColorInfo(bool[] cardsNumbers)
        {
            if (cardsNumbers.Length != CardsOnHands.Count)
            {
                throw new ArgumentException("Invalid value of cards.");
            }
            for (int i = 0; i < CardsOnHands.Count; i++)
            {
                if (!cardsNumbers[i]) continue;
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

        /// <summary>
        /// Refreshes accessibility level of rank info about all cards
        /// </summary>
        /// <param name = "cardsNumbers">Array of color access indicators</param>
        public void GetRankInfo(bool[] cardsNumbers)
        {
            if (cardsNumbers.Length != CardsOnHands.Count)
            {
                throw new ArgumentException("Invalid value of cards.");
            }
            for (int i = 0; i < CardsOnHands.Count; i++)
            {
                if (!cardsNumbers[i]) continue;
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

        /// <summary>
        /// Drops specific card in player's hands and takes one more from deck, if deck isn't empty
        /// </summary>
        /// <param name = "cardNumber">Number of card in hands, which is has to be dropped</param>
        public void DropAndTryTakeNewCard(int cardNumber)
        {
            if (cardNumber < 0 || cardNumber >= CardsOnHands.Count)
            {
                throw new ArgumentException("Invalid card number");
            }
            CardsOnHands.RemoveAt(cardNumber);
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
            CardsOnHands.Add(new CardOnHand(_playersGame.GiveCard()));
        }

    #endregion
    }
}