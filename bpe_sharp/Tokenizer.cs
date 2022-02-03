using System;

namespace bpe_sharp
{
    public abstract class Tokenizer
    {
        public abstract int[] Encode(string str);

        public abstract string Decode(int[] enc);

        public abstract void Initialize(string configPath);
    }
}
