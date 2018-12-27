using System;
using System.Text;

namespace FileSearch
{
    public class StringRandomGenerator
    {
        public string Generate(int size)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            char character;

            for (int i = 0; i < size; i++)
            {
                character = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                stringBuilder.Append(character);
            }
            return stringBuilder.ToString().ToLower();
        }
    }
}
