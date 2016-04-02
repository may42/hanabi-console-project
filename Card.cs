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

        private int _rank;

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

        public int Rank
        {
            get
            {
                return _rank;
            }
            private set
            {
                if (value < 1 || value > RankLimit)
                {
                    throw new ArgumentException("Card rank out of range: " + value);
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

        public Card(string abbreviation)
        {
            if (abbreviation.Length != 2)
            {
                throw new ArgumentException("Incorrect card abbreviation length: " + abbreviation.Length);
            }
            Color = ColorParse(abbreviation[0]);
            Rank = RankParse(abbreviation.Substring(1));
        }

        public Card(Card card)
        {
            Color = card.Color;
            Rank = card.Rank;
        }

    #endregion
    #region Methods

        /// <summary>
        /// Parsing string into color
        /// </summary>
        /// <param name = "colorAbbreviation">One-symbol abbreviation of color, kind of: <value>'R'</value>, <value>'Y'</value>, <value>'W'</value>...</param>
        /// <returns>Card color</returns>
        public static Colors ColorParse(char colorAbbreviation)
        {
            if (!ColorsByFirstLetter.ContainsKey(colorAbbreviation))
            {
                throw new ArgumentException("Incorrect card color abbreviation: " + colorAbbreviation);
            }
            return ColorsByFirstLetter[colorAbbreviation];
        }

        /// <summary>
        /// Parsing string into color
        /// </summary>
        /// <param name = "color">Full color name. Kind of: <value>"Yellow"</value>, <value>"Red"</value>, <value>"White"</value>...</param>
        /// <returns>Card color</returns>
        public static Colors ColorParse(string color)
        {
            if (string.IsNullOrEmpty(color))
            {
                throw new ArgumentException("Color string expected");
            }
            return ColorParse(color[0]);
        }

        /// <summary>
        /// Parsing string into rank integer
        /// </summary>
        /// <param name = "rank">String containg card rank integer</param>
        /// <returns>Card rank integer</returns>
        public static int RankParse(string rank)
        {
            if (string.IsNullOrEmpty(rank))
            {
                throw new ArgumentException("Rank string expected");
            }
            try
            {
                int r = int.Parse(rank);
                if (r < 1 || r > RankLimit)
                {
                    throw new ArgumentException("Card rank out of range: " + r);
                }
                return r;
            }
            catch (FormatException e)
            {
                throw new ArgumentException("Incorrect card rank: " + rank, e);
            }
        }

    #endregion
    }
}