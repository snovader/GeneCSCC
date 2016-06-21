using System;
using System.Collections.Generic;

namespace Prototype.Util
{
    public class Shingle
    {
        private readonly ushort _chunkSize;
        private readonly ushort _overlapSize;

        public Shingle() : this(4, 3)
        {
        }

        public Shingle(ushort chunkSize, ushort overlapSize)
        {
            if (chunkSize <= overlapSize)
            {
                throw new ArgumentException("Chunck size must be greater than overlap size.");
            }
            _overlapSize = overlapSize;
            _chunkSize = chunkSize;
        }

        public IEnumerable<string> Tokenise(string input)
        {
            var result = new List<string>();
            var position = 0;
            while (position < input.Length - _chunkSize)
            {
                result.Add(input.Substring(position, _chunkSize));
                position += _chunkSize - _overlapSize;
            }
            return result;
        }
    }
}

