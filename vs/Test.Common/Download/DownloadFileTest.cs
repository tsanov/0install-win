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

using System;
using System.IO;
using NUnit.Framework;

namespace Common.Download
{
    /// <summary>
    /// Contains test methods for <see cref="DownloadFile"/>.
    /// </summary>
    [TestFixture]
    public class DownloadFileTest
    {
        /// <summary>
        /// Downloads a small file using <see cref="DownloadFile.RunSync"/>.
        /// </summary>
        [Test]
        public void TestRunSync()
        {
            DownloadFile download;
            string fileContent;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Download the file
                download = new DownloadFile(new Uri("http://www.nano-byte.de/"), tempFile);
                download.RunSync();

                // Read the file
                fileContent = File.ReadAllText(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }
            
            // Ensure the download was successfull and the HTML file starts with a Doctype as expected
            Assert.AreEqual(DownloadState.Complete, download.State);
            Assert.IsTrue(fileContent.StartsWith("<!DOCTYPE"), fileContent);
        }

        /// <summary>
        /// Downloads a small file using <see cref="DownloadFile.Start"/> and <see cref="DownloadFile.Join"/>.
        /// </summary>
        [Test]
        public void TestThread()
        {
            DownloadFile download;
            string fileContent;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Start a background download of the file and then wait
                download = new DownloadFile(new Uri("http://www.nano-byte.de/"), tempFile);
                download.Start();
                download.Join();

                // Read the file
                fileContent = File.ReadAllText(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }

            // Ensure the download was successfull and the HTML file starts with a Doctype as expected
            Assert.AreEqual(DownloadState.Complete, download.State);
            Assert.IsTrue(fileContent.StartsWith("<!DOCTYPE"), fileContent);
        }
    }
}
