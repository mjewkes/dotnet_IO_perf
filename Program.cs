using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
namespace Scratch
{
    public class TestRunner{
        static string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"random_data1.txt");
        public static void CreateAGigOfData()
        {
            var set = Enumerable.Range(0, 2_500_000).Select(i => FatTableGenerator.NewFatRow()).ToArray();
            Console.WriteLine(GC.GetTotalMemory(true)/1_000_000);
            File.WriteAllLines(dataPath, set.Select(e => JsonSerializer.Serialize(e)));

        }
        public static string[] MJReadLines(string path)
        {
            string[] lines = new string[2500000];
            const Int32 BufferSize = 65536;
            int i = 0;
            using (var fileStream = File.OpenRead(path))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    lines[i] = line;
                    i++;
                }
            }
            return lines;
        }
        public static string MJReadBytesToAGiantString(string path)
        {
            var builder = new StringBuilder(1_280_000_000);
            const Int32 BufferSize = 65536;
            char[] charBuffer = new char[4096];
            using (var fileStream = File.OpenRead(path))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                while (true)
                {
                    int l = streamReader.ReadBlock(charBuffer);
                    if (l <= 0) return builder.ToString();
                    builder.Append(charBuffer.AsSpan(0,l));
                }
            }
        }
        public static void Main()
        {

            if (!File.Exists(dataPath))
            {
                //CreateAGigOfData();
            }

            long length = new System.IO.FileInfo(dataPath).Length;
            Console.WriteLine($"{length} bytes; {length / 1_000_000} megabytes");

            var t = new System.Diagnostics.Stopwatch();
            GC.Collect();

            {
                t.Start();
                var bytes = File.ReadAllBytes(dataPath);
                t.Stop();
                Console.WriteLine($"Just so I can proove these bytes are in memory byte 100000 is {bytes[10000]}");

                Console.WriteLine($"{t.Elapsed} - bytes");
            }
            GC.Collect();
            {
                t.Restart();
                var textArr = MJReadLines(dataPath);
                t.Stop();
                Console.WriteLine($"{t.Elapsed} - custom read w/ big buffer");
            }
            GC.Collect();
            {
                t.Restart();
                var textArr = File.ReadAllLines(dataPath);
                t.Stop();
                Console.WriteLine($"{t.Elapsed} - default read all lines");
            }
            GC.Collect();
            {
                t.Restart();
                var text = File.ReadAllText(dataPath);
                t.Stop();
                Console.WriteLine($"{t.Elapsed} - default read all text");
            }
            GC.Collect();
            {
                t.Restart();
                var text = MJReadBytesToAGiantString(dataPath);
                t.Stop();
                Console.WriteLine($"{t.Elapsed} {t.ElapsedMilliseconds}- read bytes to a prealloced string buffer");
            }
        }
    }
    public class FatTable
    {
        public int A_int { get; set; }
        public int B_int { get; set; }
        public float C_float { get; set; }
        public double D_double { get; set; }

        public string E_str { get; set; }  
        public string F_str { get; set; }  
        public string G_str { get; set; }  
        public string H_str { get; set; }  
        public DateTime I_date { get; set; }  
        public DateTime J_date { get; set; }  

    }
    public class FatTableGenerator
    {
        static Random random = new Random(1337);
        static Lazy<string[]> Words = new(() =>
        {
            return File.ReadAllLines("english.txt");
        });
        static string NewWord()
        {
            return Words.Value[random.Next(0, 999)];
        }
        static DateTime RandomDay()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }

        public static FatTable NewFatRow()
        {
            var x = new FatTable();
            x.A_int = Serial();
            x.B_int = random.Next();
            x.C_float = random.NextSingle();
            x.D_double = random.NextDouble();

            x.E_str = $"5Code-{random.Next(10000, 99999)}";
            x.F_str = Guid.NewGuid().ToString();
            x.G_str = NewWord();
            x.H_str = string.Join(" ", Enumerable.Range(0, 10).Select(e => NewWord()));

            x.I_date=DateTime.Now;
            x.J_date = RandomDay();
            return x;
        }
        static int serial = 0;
        public static int Serial()
        {
            return System.Threading.Interlocked.Increment(ref serial);
        }
    }
}

