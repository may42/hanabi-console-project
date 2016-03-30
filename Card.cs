using System;

namespace my_console_project
{
    class Card
    {
        public enum Colors
        {
            Red,
            Green,
            Blue,
            Yellow,
            White
        }

        private int _rank;
        
    #region Props
        public Colors Color { get; private set; }

        public int Rank
        {
            get
            {
                return _rank;
            }
            private set
            {
                if (value < 1 || value > 5)
                {
                    throw new ArgumentException("Incorrect card rank.");
                }
                _rank = value;
            }
        }

    #endregion
    #region Constructors
        public Card(int cardRank, Colors cardColor)
        {
            Color = cardColor;
            Rank = cardRank;
        }

        public Card(string cardAbbreviation)
            : this (int.Parse(cardAbbreviation[1].ToString()), ColorParse(cardAbbreviation[0]))
        {
        }

        public Card(Card card)
        {
            Color = card.Color;
            Rank = card.Rank;
        }

    #endregion
    #region Methods
        /// <summary>
        /// Parsing string into card instance
        /// </summary>
        /// <param name = "cardAbbreviation">Abbreviation, which represents a card. Kind of <value>"R1"</value>, <value>"Y3"</value>... 1-st symbol -- card color, 2-nd -- card rank</param>
        /// <returns></returns>
        public static Card Parse(string cardAbbreviation)
        {
            if (cardAbbreviation.Length == 2)
            {
                return new Card(int.Parse(cardAbbreviation[1].ToString()), ColorParse(cardAbbreviation[0]));
            }
            throw new ArgumentException("Incorrect input data");
        }

        /// <summary>
        /// Parsing string into color
        /// </summary>
        /// <param name = "colorAbbreviation">One-symbol abbreviation of color, kind of: <value>'R'</value>, <value>'Y'</value>, <value>'W'</value>...</param>
        /// <returns></returns>
        public static Colors ColorParse(char colorAbbreviation)
        {
            switch (colorAbbreviation)
            {
                case 'R':
                {
                    return Colors.Red;
                }
                case 'Y':
                {
                    return Colors.Yellow;
                }
                case 'W':
                {
                    return Colors.White;
                }
                case 'B':
                {
                    return Colors.Blue;
                }
                case 'G':
                {
                    return Colors.Green;
                }
                default:
                {
                    throw new ArgumentException("Incorrect card color abbreviation.");
                }
            }
        }

        /// <summary>
        /// Parsing string into color
        /// </summary>
        /// <param name = "color">Full color name. Kind of: <value>"Yellow"</value>, <value>"Red"</value>, <value>"White"</value>...</param>
        /// <returns></returns>
        public static Colors ColorParse(string color)
        {
            switch (color)
            {
                case "Red":
                {
                    return Colors.Red;
                }
                case "Yellow":
                {
                    return Colors.Yellow;
                }
                case "Blue":
                {
                    return Colors.Blue;
                }
                case "White":
                {
                    return Colors.White;
                }
                case "Green":
                {
                    return Colors.Green;
                }
                default:
                {
                    throw new ArgumentException("Incorrect card color abbreviation.");
                }
            }
        }

    #endregion
    }
}