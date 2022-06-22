using System;

namespace bpe_sharp
{
    public abstract class Tokenizer
    {
        public abstract double[] Encode(string str);

        public abstract string Decode(double[] enc);

        public abstract void Initialize(string configPath);
    }
}
