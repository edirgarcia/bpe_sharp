
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bpe_sharp
{
    public class BPETokenizer : Tokenizer
    {

        public Dictionary<int, Dictionary<string, int>> encodingLookupDict = new Dictionary<int, Dictionary<string, int>>();

        public Dictionary<int, string> decodingLookupDict = new Dictionary<int, string>();

        public char spaceTokenChar = 'Ġ';



        public BPETokenizer(string configPath)
        {
            this.Initialize(configPath);
        }

        public override string Decode(int[] enc)
        {
            StringBuilder sb = new StringBuilder();

            foreach(int i in enc)
            {
                sb.Append(decodingLookupDict[i]);
            }

            return sb.ToString().Replace(spaceTokenChar, ' ').Replace("\\", "");
        }

        // I'm not sure if this is the right algorithm
        // this basically tries to fit the longest possible token and continue to traverse the string in reverse
        // hugging face sometimes returns other tokens, maybe this is mitigated with large vocab sizes
        public override int[] Encode(string str)
        {
            str = str.Replace(' ', spaceTokenChar);

            int maxTokenLen = this.encodingLookupDict.Count;
            int l = str.Length - maxTokenLen;

            List<int> result = new List<int>();

            int currFitTokenLen = maxTokenLen ;
            int processedCharCount = 0;

            while (processedCharCount < str.Length)
            {
                
                string currFitToken = str.Substring(l, currFitTokenLen );


                // escape slash // escape quotes
                if (currFitToken.Contains("\\"))
                {
                    currFitToken = currFitToken.Replace("\\", "\\\\");
                    //processedCharCount++;
                }
                else if(currFitToken.Contains("\""))
                {
                    currFitToken = currFitToken.Replace("\"", "\\\"");
                    //processedCharCount++;
                }
                

                if (encodingLookupDict[currFitTokenLen].ContainsKey(currFitToken))
                {
                    int token = encodingLookupDict[currFitTokenLen][currFitToken];
                    Console.WriteLine(token);
                    result.Insert(0, token);

                    processedCharCount += currFitTokenLen;

                    l = str.Length - processedCharCount - maxTokenLen;

                    if(l<0)
                    {
                        currFitTokenLen = maxTokenLen + l;
                        l = 0;
                    }
                    else
                    {
                        currFitTokenLen = maxTokenLen;
                    }
                }
                else
                {
                    l++;
                    currFitTokenLen--;
                }
            }

            return result.ToArray();
        }

        public override void Initialize(string configPath)
        {
            using (StreamReader sr = new StreamReader(configPath))
            {
                // HuggingFace tokenizer configs come in just one line
                string line = sr.ReadLine();

                //parse this raw, because it does not deserialize well
                string[] seps = { "\"vocab\":{" , "},\"merges\"" };

                string[] tokens = line.Split(seps,StringSplitOptions.RemoveEmptyEntries);

                string rawVocab = tokens[1];

                int l = 0;
                int r = 1;

                int parsedNull = 0;

                while(r < rawVocab.Length)
                {
                    char currChar = rawVocab[r];
                    string prevChar = rawVocab[r - 1].ToString();

                    if ( currChar == ',' && int.TryParse(prevChar, out parsedNull))
                    {
                        string currToken = rawVocab.Substring(l, r - l);
                        processToken(currToken);
                        l = r + 1;
                    }
                    r++;

                }

            }
            
        }


        private void processToken(string token)
        {
            int tokenLastChar = token.Length - 1;
            int colonPtr = token.Length - 1;
            
            while(token[colonPtr] != ':')
                colonPtr--;

            string key = token.Substring(1,colonPtr-2);
            int value = Int32.Parse(token.Substring(colonPtr + 1 , tokenLastChar - colonPtr));

            int sizeKey = key.Length;

            if (key.Contains("\\"))
                sizeKey -= 1;

            if (! encodingLookupDict.ContainsKey(sizeKey))
            {
                encodingLookupDict[sizeKey] = new Dictionary<string, int>();
            }

            encodingLookupDict[sizeKey].Add(key, value);
            decodingLookupDict.Add(value, key);

        }
    }
}
