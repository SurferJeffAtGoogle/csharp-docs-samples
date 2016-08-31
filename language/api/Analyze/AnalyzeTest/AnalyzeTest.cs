﻿/*
 * Copyright (c) 2016 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xunit;

namespace GoogleCloudSamples
{
    public class AnalyzeTest
    {
        /// <summary>Runs sample with the provided arguments</summary>
        /// <returns>The console output of this program</returns>
        public string Run(params string[] arguments)
        {
            var standardOut = Console.Out;

            using (var output = new StringWriter())
            {
                Console.SetOut(output);

                try
                {
                    Analyze.Main(arguments);
                    return output.ToString();
                }
                finally
                {
                    Console.SetOut(standardOut);
                }
            }
        }

        static string text = 
            "Santa Claus Conquers the Martians is a terrible movie. "
            + "It's so bad, it's good.";

        [Fact]
        public void CommandLinePrintsUsageTest()
        {
            Assert.Equal(Analyze.Usage, Run());
            Assert.Equal(Analyze.Usage, Run("entities"));
            Assert.Equal(Analyze.Usage, Run("badcommand", "text"));
        }

        [Fact]
        public void EntitiesTest()
        {
            string output = Run("entities", text);
            Assert.Contains("Entities:", output);
            Assert.Contains("Name: Santa Claus Conquers the Martians", output);
        }

        [Fact]
        public void SyntaxTest()
        {
            string output = Run("syntax", text);
            Assert.Contains("Sentences:", output);
            Assert.Contains("0: Santa Claus Conquers the Martians is a terrible movie.", output);
            Assert.Contains("55: It's so bad, it's good.", output);
        }

        [Fact]
        public void SentimentTest()
        {
            string output = Run("sentiment", text);
            Assert.Contains("Polarity: -", output);
            Assert.Contains("Magnitude: ", output);
        }

        [Fact]
        public void EverythingTest()
        {
            string output = Run("everything", text);
            Assert.Contains("Language: en", output);
            Assert.Contains("Polarity: -", output);
            Assert.Contains("Magnitude: ", output);
            Assert.Contains("Sentences:", output);
            Assert.Contains("0: Santa Claus Conquers the Martians is a terrible movie.", output);
            Assert.Contains("55: It's so bad, it's good.", output);
            Assert.Contains("Entities:", output);
            Assert.Contains("Name: Santa Claus Conquers the Martians", output);
        }
    }
}
