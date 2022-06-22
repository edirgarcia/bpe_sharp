using System;

namespace bpe_sharp
{
    public abstract class Tokenizer
    {
        public abstract Int64[] Encode(string str);

        public abstract string Decode(Int64[] enc);

        public abstract void Initialize(string configPath);
    }
}
