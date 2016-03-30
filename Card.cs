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
        /// Parsing string into color
        /// </summary>
        /// <param name = "colorAbbreviation">One-symbol abbreviation of color, kind of: <value>'R'</value>, <value>'Y'</value>, <value>'W'</value>...</param>
        /// <returns></returns>
        public static Colors ColorParse(char colorAbbreviation)
        {
            if (!ColorsByFirstLetter.ContainsKey(colorAbbreviation))
            {
                throw new ArgumentException("Incorrect card color abbreviation.");
            }
            return ColorsByFirstLetter[colorAbbreviation];
        }

        /// <summary>
        /// Parsing string into color
        /// </summary>
        /// <param name = "color">Full color name. Kind of: <value>"Yellow"</value>, <value>"Red"</value>, <value>"White"</value>...</param>
        /// <returns></returns>
        public static Colors ColorParse(string color)
        {
            return ColorParse(color[0]);
        }

    #endregion
    }
}