using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Measure
{
    class Program
    {
        private static string aspxBaseline = "http://localhost/BenchmarkAspx/Content/Tidy_Blog/css/style.css";
        private static string aspxPath = "http://localhost/BenchmarkAspx/Home/Index.aspx";
        private static string sparkBaseline = "http://localhost/BenchmarkSpark/Content/Tidy_Blog/css/style.css";
        private static string sparkPath = "http://localhost/BenchmarkSpark/Home/Index.castle";
        private static string velocityBaseline = "http://localhost/BenchmarkVelocity/Content/Tidy_Blog/css/style.css";
        private static string velocityPath = "http://localhost/BenchmarkVelocity/Home/Index.castle";

        static void Main(string[] args)
        {
            var urls = new[] { aspxBaseline, aspxPath, sparkBaseline, sparkPath, velocityBaseline, velocityPath };
            for (int warmup = 0; warmup != 3; ++warmup)
            {
                for (int index = 0; index != urls.Length; ++index)
                {
                    Pull(urls[index]);
                }
            }

            for (int pass = 0; pass != 3; ++pass)
            {
                Console.WriteLine("----------");
                var ticks = new List<List<long>>();
                var sizes = new List<List<long>>();
                for (int index = 0; index != urls.Length; ++index)
                {
                    ticks.Add(new List<long>());
                    sizes.Add(new List<long>());
                }

                for (int loop = 0; loop != 10; ++loop)
                {
                    for (int index = 0; index != urls.Length; ++index)
                    {
                        var time = Stopwatch.StartNew();
                        int totalSize = 0;
                        for (int sample = 0; sample != 100; ++sample)
                            totalSize += Pull(urls[index]);
                        time.Stop();

                        ticks[index].Add(time.ElapsedTicks);
                        sizes[index].Add(totalSize);
                    }
                }

                for (int index = 0; index != urls.Length; ++index)
                {
                    Console.WriteLine("{0}", urls[index]);
                    Console.Write("{0} k, {1} seconds :",
                                  sizes[index].Sum() / 1024,
                                  (ticks[index].Sum() * 1000) / Stopwatch.Frequency / (double)1000);
                    foreach (var subtotal in ticks[index])
                    {
                        Console.Write(" {0}", (subtotal * 1000) / Stopwatch.Frequency / (double)1000);
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            Console.Write("Press [enter] to exit:");
            Console.ReadLine();
        }

        private static int Pull(string path)
        {
            var request = WebRequest.Create(path);
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd().Length;
                }
            }
        }
    }
}
