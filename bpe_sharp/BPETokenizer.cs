
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace bpe_sharp
{
    public class BPETokenizer : Tokenizer
    {
        public Dictionary<int, Dictionary<string, int>> encodingLookupDict = new Dictionary<int, Dictionary<string, int>>();

        public Dictionary<int, string> decodingLookupDict = new Dictionary<int, string>();

        public Dictionary<string, int> mergesScoreDict = new Dictionary<string, int>();

        public char spaceTokenChar = 'Ġ';

        public BPETokenizer(string configPath)
        {
            this.Initialize(configPath);
        }

        public override void Initialize(string configPath)
        {
            using (StreamReader sr = new StreamReader(configPath))
            {
                // HuggingFace tokenizer configs come in just one line
                string line = sr.ReadLine();

                //parse this raw, because it does not deserialize well
                string[] seps = { "\"vocab\":{", "},\"merges\":[" };

                string[] tokens = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);

                string rawVocab = tokens[1];

                string rawMerges = tokens[2];

                CreateLookupDicts(rawVocab);

                CreateMergesScoreDict(rawMerges);
            }
        }

        public override string Decode(Int64[] enc)
        {
            StringBuilder sb = new StringBuilder();

            foreach(int i in enc)
            {
                sb.Append(decodingLookupDict[i]);
            }

            return sb.ToString().Replace(spaceTokenChar, ' ').Replace("\\", "");
        }

        // Encode as described by
        // https://guillaume-be.github.io/2021-09-16/byte_pair_encoding#bpe-tokenization-naive
        public override Int64[] Encode(string str)
        {
            List<Int64> result = new List<Int64>();

            List<string> allSymbols = new List<string>();

            string[] preSplits =  str.Split(' ');

            for(int i = 0; i < preSplits.Length; i++)
            {
                string currPreSplit = preSplits[i];

                if(i != 0)
                {
                    currPreSplit = spaceTokenChar + currPreSplit;
                }

                //tokenize sequence here
                List<string> symbols = ConvertToStringList(currPreSplit);

                while(true)
                {
                    int bestPair = FindBestPair(symbols);

                    if (bestPair == -1)
                        break;

                    symbols = MergeBestPair(symbols, bestPair);
                }

                allSymbols.AddRange(symbols);
            }
            
            foreach (string symbol in allSymbols)
            {
                if(symbol != null && encodingLookupDict.ContainsKey(symbol.Length))
                {
                    result.Add(encodingLookupDict[symbol.Length][symbol]);
                }
            }

            return result.ToArray();
        }
        private int FindBestPair(List<string> symbols)
        {
            int bestPair = -1;
            int bestScore = 0;

            for (int i = 0; i < symbols.Count-1; i++)
            {
                string currentKey = symbols[i]+ " "+ symbols[i+1];
                int score = 0;
                
                if(mergesScoreDict.TryGetValue(currentKey, out score ))
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPair = i;
                    }
                }   
            }
            return bestPair;
        }
        private List<string> MergeBestPair(List<string> symbols, int bestPair)
        {
            symbols[bestPair]= symbols[bestPair] + symbols[bestPair +1];
            symbols.RemoveAt(bestPair + 1);

            return symbols;
        }

        private List<string> ConvertToStringList(string str)
        {
            char[] charArr = str.ToCharArray();
            List<string> result = new List<string>();   

            foreach(char c in charArr)
            {
                result.Add(c.ToString());
            }
            return result;
        }

        private void CreateLookupDicts(string rawVocab)
        {
            int l = 0;
            int r = 1;

            int parsedNull = 0;

            while (r < rawVocab.Length)
            {
                char currChar = rawVocab[r];
                string prevChar = rawVocab[r - 1].ToString();

                if (currChar == ',' && int.TryParse(prevChar, out parsedNull))
                {
                    string currToken = rawVocab.Substring(l, r - l);
                    ProcessToken(currToken);
                    l = r + 1;
                }
                r++;
            }
        }

        private void CreateMergesScoreDict(string rawMerges)
        {
            string[] seps = { "#sep#" };

            rawMerges = rawMerges.Replace("\",\"", "#sep#");

            rawMerges = rawMerges.Substring(1, rawMerges.Length - 5);
            string[] mergeTokens = rawMerges.Split( seps, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0 ; i< mergeTokens.Length; i++)
            {
                string mergeToken = mergeTokens[i].Replace("\\\"", "\"");

                mergesScoreDict.Add(mergeToken, mergeTokens.Length - i);

            }
        }

        private void ProcessToken(string token)
        {
            int tokenLastChar = token.Length - 1;
            int colonPtr = token.Length - 1;
            
            while(token[colonPtr] != ':')
                colonPtr--;

            string key = token.Substring(1,colonPtr-2);
            int value = Int32.Parse(token.Substring(colonPtr + 1 , tokenLastChar - colonPtr));

            // unescape the json "
            key = key.Replace("\\\"", "\"");

            int sizeKey = key.Length;

            if (! encodingLookupDict.ContainsKey(sizeKey))
            {
                encodingLookupDict[sizeKey] = new Dictionary<string, int>();
            }

            encodingLookupDict[sizeKey].Add(key, value);
            decodingLookupDict.Add(value, key);
        }
    }
}
