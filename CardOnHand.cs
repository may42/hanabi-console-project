namespace my_console_project
{
    class CardOnHand
    {
        // Перечисление для указания количества информации, имеющейся о конкретной карте в руке игрока
        public enum CardInfoAvaliabilities
        {
            None,
            Color,
            Rank,
            All
        }

    #region Props
        public Card Card { get; private set; }

        public CardInfoAvaliabilities CardInfoAvaliability { get; set; }

    #endregion
    #region Constructors
        public CardOnHand(Card card)
            : this (card, CardInfoAvaliabilities.None)
        {
        }

        public CardOnHand(Card card, CardInfoAvaliabilities cardInfoAvaliability)
        {
            Card = card;
            CardInfoAvaliability = cardInfoAvaliability;
        }

    #endregion
    }
}