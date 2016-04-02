using System;
using System.Collections.Generic;

namespace my_console_project
{
    class Player
    {
        private readonly Hanabi _playersGame;
        public List<CardOnHand> CardsOnHand { get; private set; }

    #region Constructors
        public Player(Hanabi game)
        {
            _playersGame = game;
            CardsOnHand = new List<CardOnHand>();
            //takes full hand of cards
            for (int i = 0; i < _playersGame.NumberOfCardsInPlayersHand; i++)
            {
                TakeNewCard();
            }
        }

        public Player(List<Card> takenCards)
        {
            if (takenCards.Count != _playersGame.NumberOfCardsInPlayersHand)
            {
                throw new ArgumentException("Imposible number of cards on hand: " + takenCards.Count);
            }
            CardsOnHand = new List<CardOnHand>();
            foreach (Card card in takenCards)
            {
                CardsOnHand.Add(new CardOnHand(card));
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
            if (cardsNumbers.Length != CardsOnHand.Count)
            {
                throw new ArgumentException("Color info array had wrong length: " + cardsNumbers.Length);
            }
            for (int i = 0; i < CardsOnHand.Count; i++)
            {
                if (!cardsNumbers[i]) continue;
                if (CardsOnHand[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.Rank)
                {
                    CardsOnHand[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.All;
                }
                else if (CardsOnHand[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.None)
                {
                    CardsOnHand[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.Color;
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
                if (!cardsNumbers[i]) continue;
                if (CardsOnHand[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.Color)
                {
                    CardsOnHand[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.All;
                }
                else if (CardsOnHand[i].CardInfoAvaliability == CardOnHand.CardInfoAvaliabilities.None)
                {
                    CardsOnHand[i].CardInfoAvaliability = CardOnHand.CardInfoAvaliabilities.Rank;
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
            CardsOnHand.Add(new CardOnHand(_playersGame.TakeCardFromDeck()));
        }

    #endregion
    }
}