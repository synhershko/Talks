using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace StackOverflowClone.Core
{
    public static class Extensions
    {
        public static IList<T> ToRandomList<T>(this IList<T> source, int numberOfItems)
        {
            if (numberOfItems <= 0)
            {
                throw new ArgumentOutOfRangeException("numberOfItems");
            }

            IList<T> results = new List<T>();

            var rng = new RNGCryptoServiceProvider();
            var rndBytes = new byte[4];
            rng.GetBytes(rndBytes);
            int randomNumber = BitConverter.ToInt32(rndBytes, 0);


            // Based upon: http://stackoverflow.com/questions/48087/select-a-random-n-elements-from-listt-in-c
            var rand = new Random(randomNumber);
            double needed = numberOfItems;
            int available = source.Count;

            while (results.Count < numberOfItems)
            {
                if (rand.NextDouble() < needed/available)
                {
                    results.Add(source[available - 1]);
                    needed--;
                }
                available--;
            }

            return results;
        }

        public static string ToSimplifiedNumberText(this int value)
        {
            return value < 1000 ? value.ToString(CultureInfo.InvariantCulture) : string.Format("{0}k", value / 1000);
        }
    }
}