using Microsoft.VisualStudio.TestTools.UnitTesting;
using bpe_sharp;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace bpe_sharp_test
{
    [TestClass]
    public class BPETokenizerTests
    {
        [TestMethod]
        public void TestBPETokenizer_Initialize()
        {
            BPETokenizer tokenizer = new BPETokenizer("resources/tokenizer.json");

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
            BPETokenizer tokenizer = new BPETokenizer("resources/tokenizer.json");

            Int64[] result = tokenizer.Encode(TestUtils.testReview);

            for (int i = 0; i < result.Length; i++)
                Assert.AreEqual(TestUtils.encodedTestReview[i], result[i]," Index " + i + " is not the same");

            Console.Out.WriteLine("done");
        }

        [TestMethod]
        public void TestBPETokenizer_Decode()
        {
            BPETokenizer tokenizer = new BPETokenizer("resources/tokenizer.json");

            string result = tokenizer.Decode(TestUtils.encodedTestReview);

            //is this needed?
            string unescaped = TestUtils.testReview.Replace("\\", "");

            Assert.AreEqual(unescaped, result);

            Console.Out.WriteLine("done");
        }

        [TestMethod]
        public void TestBPETokenizer_EncodeDecode()
        {
            BPETokenizer tokenizer = new BPETokenizer("resources/tokenizer.json");

            Int64[] encodeResult = tokenizer.Encode(TestUtils.testReview);
                
            string decodeResult = tokenizer.Decode(encodeResult);

            Assert.AreEqual(TestUtils.testReview, decodeResult);

            Console.Out.WriteLine("done");
        }
    }
}