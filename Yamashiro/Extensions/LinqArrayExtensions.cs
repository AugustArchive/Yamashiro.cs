using System.Collections.Generic;
using System.Linq;

namespace Yamashiro.Extensions
{
    public static class LinqArrayExtensions
    {
        // CREDIT: https://stackoverflow.com/a/18987751
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float) array.Length / size; i++) yield return array.Skip(i * size).Take(size);
        }
    }
}