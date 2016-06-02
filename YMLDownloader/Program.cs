﻿using MarkdownLog;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

using static YMLDownloader.WorkRunner;
using static YMLDownloader.YmlStreamProcessor;

namespace YMLDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "";

            var c = new Config
            {
                ConcurrencyDegree = 10,
                FlushBufferSize = 2,
                RetryPolicy = 0,

                ProductSaver = new ProductSaver(new ConnectionFactory(connectionString)),
                ResourceProvider = new ResourceProvider(),
                XmlProductParser = new XmlProductParser(),
                YmlStreamReader = new YmlStreamReader(),

                OveralProgress = new ProgressBarOptions
                {
                    BackgroundColor = ConsoleColor.DarkGray,
                },
                SubProgress = new ProgressBarOptions
                {
                    ForeGroundColor = ConsoleColor.Cyan,
                    ForeGroundColorDone = ConsoleColor.DarkGreen,
                    ProgressCharacter = '─',
                    BackgroundColor = ConsoleColor.DarkGray,
                    CollapseWhenFinished = true
                }
            };

            var runner = new WorkRunner();
            var result = runner.Run(c).Result;

            PrintResults(result);

            Console.ReadKey();
        }

        private static void PrintResults(IEnumerable<YmlResult> ymls)
        {
            var result = ymls.Select(x => new
            {
                Elapsed = x.ElapsedMilliseconds,
                Url = x.Url.Substring(0, 20),
                Categories = x.Categories,
                Processes = x.Processed,
                Invaid = x.Invalid,
                Result = x.Exception != null ? "Failed" : "Success"
            });

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(result.ToMarkdownTable());
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }
    }
}
