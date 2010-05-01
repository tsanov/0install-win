﻿/*
 * Copyright 2006-2010 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.IO;
using NUnit.Framework;

namespace Common.Storage
{
    /// <summary>
    /// Contains test methods for <see cref="XmlStorage"/>.
    /// </summary>
    [TestFixture]
    public class XmlStorageTest
    {
        /// <summary>
        /// A data-structure used to test serialization.
        /// </summary>
        public class TestData
        {
            public string Data { get; set; }
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.Save{T}(string,T,System.Reflection.MemberInfo[])"/> and <see cref="XmlStorage.Load{T}(string,System.Reflection.MemberInfo[])"/> work correctly.
        /// </summary>
        [Test]
        public void TestFile()
        {
            TestData testData1, testData2;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Write and read file
                testData1 = new TestData { Data = "Hello" };
                XmlStorage.Save(tempFile, testData1);
                testData2 = XmlStorage.Load<TestData>(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(testData1.Data, testData2.Data);
        }

        /// <summary>
        /// Ensures <see cref="XmlStorage.ToString{T}"/> and <see cref="XmlStorage.FromString{T}"/> work correctly.
        /// </summary>
        [Test]
        public void TestString()
        {
            var testData1 = new TestData {Data = "Hello"};

            // Serialize and deserialize
            string xml = XmlStorage.ToString(testData1);
            var testData2 = XmlStorage.FromString<TestData>(xml);

            Assert.AreEqual(testData1.Data, testData2.Data);
        }
    }
}
