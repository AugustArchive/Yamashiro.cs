using System;

namespace Yamashiro.Extensions
{
    public static class RandomExtensions
    {
        public static int NextRange(this Random random, int min, int max) => random.Next(min, max);
    }
}