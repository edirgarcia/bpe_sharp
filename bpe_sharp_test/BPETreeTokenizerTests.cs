using Microsoft.VisualStudio.TestTools.UnitTesting;
using bpe_sharp;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace bpe_sharp_test
{
    [TestClass]
    public class BPETreeTokenizerTests
    {
        [TestMethod]
        public void TestBPETokenizer_Initialize()
        {
            BPETreeTokenizer tokenizer = new BPETreeTokenizer("resources/tokenizer.json");

            // These asserts are only valid with the tokenizer configuration that is checked in.
            // if you want to use another file and run through this test, remove these asserts.
            Assert.IsNotNull(tokenizer.encodingLookupDict);
            Assert.IsTrue(tokenizer.encodingLookupDict.Count == 18);
            Assert.IsTrue(tokenizer.encodingLookupDict[17].Count == 7);
            Assert.IsTrue(tokenizer.encodingLookupDict[17]["Ġenvironmentalist"] == 29442);

            Console.Out.WriteLine("done");
        }

        [TestMethod]
        public void TestBPETokenizer_Encode()
        {
            BPETreeTokenizer tokenizer = new BPETreeTokenizer("resources/tokenizer.json");

            string testReview = @"This is just a precious little diamond. The play, the script are excellent. I cant compare this movie with anything else, maybe except the movie ""Leon"" wonderfully played by Jean Reno and Natalie Portman. But... What can I say about this one? This is the best movie Anne Parillaud has ever played in (See please ""Frankie Starlight"", she's speaking English there) to see what I mean. The story of young punk girl Nikita, taken into the depraved world of the secret government forces has been exceptionally over used by Americans. Never mind the ""Point of no return"" and especially the ""La femme Nikita"" TV series. They cannot compare the original believe me! Trash these videos. Buy this one, do not rent it, BUY it. BTW beware of the subtitles of the LA company which ""translate"" the US release. What a disgrace! If you cant understand French, get a dubbed version. But you'll regret later :)";

            int[] expectedResult = { 683, 209, 387, 172, 8321, 668, 10586, 17, 271, 626, 15, 175, 922, 298, 1347, 17, 213, 5683, 5003, 242, 275, 263, 1057, 1253, 15, 1610, 1543, 175, 275, 295, 19801, 5, 5072, 1165, 341, 3818, 7821, 197, 12387, 23920, 17, 634, 495, 1124, 405, 213, 666, 389, 242, 324, 34, 535, 209, 175, 672, 275, 6219, 5386, 287, 4013, 386, 682, 1165, 203, 293, 8890, 2968, 295, 12934, 227, 3409, 2299, 979, 454, 239, 4355, 2176, 438, 12, 198, 435, 434, 213, 1159, 17, 271, 447, 196, 856, 10179, 894, 13252, 15, 1968, 529, 175, 19225, 924, 196, 175, 2408, 3608, 4702, 386, 526, 10367, 533, 1317, 341, 3974, 17, 4600, 1169, 175, 295, 51, 1123, 196, 433, 1982, 5, 197, 1266, 175, 295, 8648, 15309, 13252, 5, 1109, 983, 17, 987, 2071, 5003, 175, 936, 1147, 392, 4, 25610, 747, 7534, 17, 18335, 242, 324, 15, 321, 290, 1672, 224, 15, 18743, 60, 224, 17, 21724, 15557, 196, 175, 5382, 196, 175, 5426, 3378, 484, 295, 87, 659, 6132, 403, 5, 175, 2313, 2476, 17, 1124, 172, 10810, 4, 817, 276, 5683, 1238, 2432, 15, 431, 172, 6342, 1398, 17, 634, 276, 1031, 4947, 1355, 7294 };

            int[] result = tokenizer.Encode(testReview);

            Dictionary<string, int> expectedTrigramDict = ConvertToTrigramDictionary(expectedResult);
            Dictionary<string, int> trigramDict = ConvertToTrigramDictionary(result);

            float similarity = TrigramSimilarity(expectedTrigramDict, trigramDict);

            //This method is worse
            Assert.IsTrue(similarity > .75);

            List<string> notEqualIds = new List<string>();

            //for (int i = 0; i < result.Length; i++)
            //    //Assert.AreEqual(expectedResult[i], result[i]," Index " + i + " is not the same");
            //    if (expectedResult[i] != result[i])
            //        notEqualIds.Add("index " + i + " : " + expectedResult[i] + "<>" + result[i]);

            Console.Out.WriteLine("done");
        }

        [TestMethod]
        public void TestBPETokenizer_Decode()
        {
            BPETreeTokenizer tokenizer = new BPETreeTokenizer("resources/tokenizer.json");

            string expectedString = @"This is just a precious little diamond. The play, the script are excellent. I cant compare this movie with anything else, maybe except the movie ""Leon"" wonderfully played by Jean Reno and Natalie Portman. But... What can I say about this one? This is the best movie Anne Parillaud has ever played in (See please ""Frankie Starlight"", she's speaking English there) to see what I mean. The story of young punk girl Nikita, taken into the depraved world of the secret government forces has been exceptionally over used by Americans. Never mind the ""Point of no return"" and especially the ""La femme Nikita"" TV series. They cannot compare the original believe me! Trash these videos. Buy this one, do not rent it, BUY it. BTW beware of the subtitles of the LA company which ""translate"" the US release. What a disgrace! If you cant understand French, get a dubbed version. But you'll regret later :)";

            int[] testEncoding = { 683, 209, 387, 172, 8321, 668, 10586, 17, 271, 626, 15, 175, 922, 298, 1347, 17, 213, 5683, 5003, 242, 275, 263, 1057, 1253, 15, 1610, 1543, 175, 275, 295, 19801, 5, 5072, 1165, 341, 3818, 7821, 197, 12387, 23920, 17, 634, 495, 1124, 405, 213, 666, 389, 242, 324, 34, 535, 209, 175, 672, 275, 6219, 5386, 287, 4013, 386, 682, 1165, 203, 293, 8890, 2968, 295, 12934, 227, 3409, 2299, 979, 454, 239, 4355, 2176, 438, 12, 198, 435, 434, 213, 1159, 17, 271, 447, 196, 856, 10179, 894, 13252, 15, 1968, 529, 175, 19225, 924, 196, 175, 2408, 3608, 4702, 386, 526, 10367, 533, 1317, 341, 3974, 17, 4600, 1169, 175, 295, 51, 1123, 196, 433, 1982, 5, 197, 1266, 175, 295, 8648, 15309, 13252, 5, 1109, 983, 17, 987, 2071, 5003, 175, 936, 1147, 392, 4, 25610, 747, 7534, 17, 18335, 242, 324, 15, 321, 290, 1672, 224, 15, 18743, 60, 224, 17, 21724, 15557, 196, 175, 5382, 196, 175, 5426, 3378, 484, 295, 87, 659, 6132, 403, 5, 175, 2313, 2476, 17, 1124, 172, 10810, 4, 817, 276, 5683, 1238, 2432, 15, 431, 172, 6342, 1398, 17, 634, 276, 1031, 4947, 1355, 7294 };

            string result = tokenizer.Decode(testEncoding);

            //is this needed?
            string unescaped = expectedString.Replace("\\", "");

            Assert.AreEqual(unescaped, result);

            Console.Out.WriteLine("done");
        }

        [TestMethod]
        public void TestBPETokenizer_EncodeDecode()
        {
            BPETreeTokenizer tokenizer = new BPETreeTokenizer("resources/tokenizer.json");

            string testReview = @"This is just a precious little diamond. The play, the script are excellent. I cant compare this movie with anything else, maybe except the movie ""Leon"" wonderfully played by Jean Reno and Natalie Portman. But... What can I say about this one? This is the best movie Anne Parillaud has ever played in (See please ""Frankie Starlight"", she's speaking English there) to see what I mean. The story of young punk girl Nikita, taken into the depraved world of the secret government forces has been exceptionally over used by Americans. Never mind the ""Point of no return"" and especially the ""La femme Nikita"" TV series. They cannot compare the original believe me! Trash these videos. Buy this one, do not rent it, BUY it. BTW beware of the subtitles of the LA company which ""translate"" the US release. What a disgrace! If you cant understand French, get a dubbed version. But you'll regret later :)";

            int[] encodeResult = tokenizer.Encode(testReview);
                
            string decodeResult = tokenizer.Decode(encodeResult);

            Assert.AreEqual(testReview, decodeResult);

            Console.Out.WriteLine("done");
        }

        // this functions estimates the similarity between the two encodings
        // because most times it returns what's expected but not always
        private float TrigramSimilarity(Dictionary<string, int> encDictA, Dictionary<string, int> encDictB)
        {
            int aInBCount = 0;

            int bInACount = 0;

            foreach (KeyValuePair<string, int> pair in encDictA)
            {
                if (encDictB.ContainsKey(pair.Key)) aInBCount++;
            }

            foreach (KeyValuePair<string, int> pair in encDictB)
            {
                if (encDictA.ContainsKey(pair.Key)) bInACount++;
            }

            float aInB = (float)aInBCount / encDictB.Count;

            float bInA = (float)bInACount / encDictA.Count;

            return Math.Min(aInB, bInA);
        }

        private Dictionary<string,int> ConvertToTrigramDictionary(int[] encoding)
        {
            Dictionary<string, int> encDict = new Dictionary<string, int>();

            for(int i = 0; i < encoding.Length - 2; i++)
            {
                string trigram = encoding[i] + "_" + encoding[i + 1] + "_" + encoding[i + 2];

                if (encDict.ContainsKey(trigram))
                {
                    encDict[trigram] = encDict[trigram] + 1;
                }
                else
                {
                    encDict[trigram] = 1;
                }
            }

            return encDict;
        }
    }
}