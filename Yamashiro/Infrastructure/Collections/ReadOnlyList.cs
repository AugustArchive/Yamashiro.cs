using System.Collections.Generic;
using System.Collections;
using System.Text;
using System;

// CREDIT: https://github.com/Quahu/Qommon/blob/master/src/Qommon/Collections/ReadOnlyList.cs
namespace Yamashiro.Infrastructure.Collections
{
    public sealed class ReadOnlyList<T>: IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> list;
        public int Count => list.Count;

        public ReadOnlyList(IReadOnlyList<T> _list)
        {
            if (_list is null) throw new ArgumentNullException(nameof(_list));

            list = _list;
        }

        public T this[int index] => list[index];
        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString()
        {
            var sb = new StringBuilder($"ReadOnlyList<{this.GetType().Name}>\n");
            foreach (var entry in this) sb.AppendLine($"- {entry.ToString()}");

            return sb.ToString();
        }
    }
}