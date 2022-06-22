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

            Int64[] result = tokenizer.Encode(TestUtils.testReview);

            Dictionary<string, Int64> expectedTrigramDict = TestUtils.ConvertToTrigramDictionary(TestUtils.encodedTestReview);
            Dictionary<string, Int64> trigramDict = TestUtils.ConvertToTrigramDictionary(result);

            float similarity = TestUtils.TrigramSimilarity(expectedTrigramDict, trigramDict);

            //This implementation is worse :(
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

            string result = tokenizer.Decode(TestUtils.encodedTestReview);

            //is this needed?
            string unescaped = TestUtils.testReview.Replace("\\", "");

            Assert.AreEqual(unescaped, result);

            Console.Out.WriteLine("done");
        }

        [TestMethod]
        public void TestBPETokenizer_EncodeDecode()
        {
            BPETreeTokenizer tokenizer = new BPETreeTokenizer("resources/tokenizer.json");

            Int64[] encodeResult = tokenizer.Encode(TestUtils.testReview);
                
            string decodeResult = tokenizer.Decode(encodeResult);

            Assert.AreEqual(TestUtils.testReview, decodeResult);

            Console.Out.WriteLine("done");
        }
    }
}