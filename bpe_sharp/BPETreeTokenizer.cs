
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bpe_sharp
{
    public class BPETreeTokenizer : Tokenizer
    {
        public Dictionary<int, Dictionary<string, int>> encodingLookupDict = new Dictionary<int, Dictionary<string, int>>();

        public Dictionary<int, string> decodingLookupDict = new Dictionary<int, string>();

        public Dictionary<string, List<string>> mergesTree = new Dictionary<string, List<string>>();

        public char spaceTokenChar = 'Ġ';

        public BPETreeTokenizer(string configPath)
        {
            this.Initialize(configPath);
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

        // This looks for the longest token match traversing thorugh the merges Tree
        //It's slower than the Greedy implementation, but it might match HuggingFace rust code better //We'll see
        public override Int64[] Encode(string str)
        {
            str = str.Replace(' ', spaceTokenChar);

            List<Int64> result = new List<Int64>();

            while (str!= "")
            {
                string currSearchKey = str[0] + "";
                int tokenId = GetTokenFromMerges(str, ref currSearchKey);

                result.Add(tokenId);

                int tokenLength = currSearchKey.Length;

                str = str.Substring(tokenLength, str.Length-tokenLength);
            }

            return result.ToArray();
        }

        private int GetTokenFromMerges(string str, ref string currSearchKey)
        {
            if(mergesTree.ContainsKey(currSearchKey))
            {
                //find the longest match
                List<string> possibleMatches = mergesTree[currSearchKey];
                string bestMatch = "";
                for (int i = 0; i < possibleMatches.Count; i++)
                {
                    string currPossibleMatch = possibleMatches[i];

                    if (str.StartsWith(currPossibleMatch))
                    {
                        if (bestMatch.Length < currPossibleMatch.Length)
                        {
                            bestMatch = currPossibleMatch;
                            currSearchKey = bestMatch;
                        }
                    }
                }

                if (bestMatch != "")
                    return GetTokenFromMerges(str, ref currSearchKey);

            }

            int tokenLength = currSearchKey.Length;

            //curSearchKey already has the best fit
            return encodingLookupDict[tokenLength][currSearchKey];
        }

        public override void Initialize(string configPath)
        {
            using (StreamReader sr = new StreamReader(configPath))
            {
                // HuggingFace tokenizer configs come in just one line
                string line = sr.ReadLine();

                //parse this raw, because it does not deserialize well
                string[] seps = { "\"vocab\":{" , "},\"merges\":[" };

                string[] tokens = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);

                string rawVocab = tokens[1];

                string rawMerges = tokens[2];

                CreateLookupDicts(rawVocab);

                CreateMergesTree(rawMerges); 
            }  
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

        private void CreateMergesTree(string rawMerges)
        {
            string[] seps = { "#sep#" };

            rawMerges = rawMerges.Replace("\",\"", "#sep#");

            rawMerges = rawMerges.Substring(1, rawMerges.Length - 5);
            string[] mergeTokens = rawMerges.Split( seps, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0 ; i< mergeTokens.Length; i++)
            {
                string mergeToken = mergeTokens[i];

                string[] kvp = mergeToken.Split(' ');

                if(!mergesTree.ContainsKey(kvp[0]))
                {
                    mergesTree[kvp[0]] = new List<string>();
                }
                mergesTree[kvp[0]].Add(kvp[0] + kvp[1]);
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
