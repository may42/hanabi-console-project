using System;
using System.Collections.Generic;

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

        public static int RankLimit = 5;

        public static Dictionary<char, Colors> ColorsByFirstLetter = new Dictionary<char, Colors>
        {
            { 'R', Colors.Red },
            { 'G', Colors.Green },
            { 'B', Colors.Blue },
            { 'Y', Colors.Yellow },
            { 'W', Colors.White }
        };
        
    #region Props

        public Colors Color { get; }
        public int Rank { get; }

    #endregion
    #region Constructors

        public Card(int cardRank, Colors cardColor)
        {
            if (cardRank < 1 || cardRank > RankLimit)
            {
                throw new ArgumentOutOfRangeException("Card rank out of range: " + cardRank);
            }
            Color = cardColor;
            Rank = cardRank;
        }

        public Card(string cardAbbreviation)
        {
            if (string.IsNullOrEmpty(cardAbbreviation))
            {
                throw new GameCommandException("Card abbreviation expected");
            }
            if (cardAbbreviation.Length != 2)
            {
                throw new GameCommandException("Card abbreviation must be 2 symbols long: " + cardAbbreviation);
            }
            Color = ParseColor(cardAbbreviation[0]);
            Rank = RankParse(cardAbbreviation.Substring(1));
        }

    #endregion
    #region Methods

        /// <summary>
        /// Custom ToString method
        /// </summary>
        /// <returns>Two-letter card abbreviation</returns>
        public override string ToString()
        {
            return Color.ToString().Substring(0, 1) + Rank;
        }

        /// <summary>
        /// Parses first character of a color name
        /// </summary>
        /// <param name = "firstLetterOfAColor">One-symbol abbreviation of color, for example: <value>'R'</value>, <value>'Y'</value>, <value>'W'</value></param>
        /// <returns>Card color</returns>
        public static Colors ParseColor(char firstLetterOfAColor)
        {
            if (!ColorsByFirstLetter.ContainsKey(firstLetterOfAColor))
            {
                throw new GameCommandException("Incorrect card color abbreviation: " + firstLetterOfAColor);
            }
            return ColorsByFirstLetter[firstLetterOfAColor];
        }

        /// <summary>
        /// Parses full string name of a color
        /// </summary>
        /// <param name = "colorName">Full color name, for example: <value>"Red"</value>, <value>"Yellow"</value>, <value>"White"</value></param>
        /// <returns>Card color</returns>
        public static Colors ParseColor(string colorName)
        {
            if (string.IsNullOrEmpty(colorName))
            {
                throw new GameCommandException("Color name expected");
            }
            if (!Enum.IsDefined(typeof(Colors), colorName))
            {
                throw new GameCommandException("Unknown color name " + colorName);
            }
            return (Colors)Enum.Parse(typeof(Colors), colorName);
        }

        /// <summary>
        /// Parses rank string
        /// </summary>
        /// <param name = "rank">String containg card rank integer</param>
        /// <returns>Card rank integer</returns>
        public static int RankParse(string rank)
        {
            if (string.IsNullOrEmpty(rank))
            {
                throw new GameCommandException("Rank string expected");
            }
            int r;
            if (!int.TryParse(rank, out r))
            {
                throw new GameCommandException("Card number must be an integer: " + rank);
            }
            if (r < 1 || r > RankLimit)
            {
                throw new GameCommandException("Card rank out of range: " + r);
            }
            return r;
        }

    #endregion
    }
}