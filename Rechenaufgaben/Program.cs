using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rechenaufgaben
{
    class MainClass
    {
        static Random random = null;
        static readonly string[] headers = new [] { "A", "Op", "B", "Resultat" };
        private static string basePath = @"../Aufgaben";
        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions {WriteIndented = true};

        public static async Task Main(string[] args)
        {
            try {
                if (!Directory.Exists(basePath)) {
                    Directory.CreateDirectory(basePath);
                }

                foreach (var i in Enumerable.Range(1, 20)) {
                    var file = Path.Combine(basePath, $"aufgaben_{i:00}.json");
                    Console.WriteLine(file);
                    var aufgabenFile = CreateAufgabenFile(
                        100
                        , GetPlusRechnung
                        , GetMinusRechnung
                        , GetMalrechnung);
                    await CreateAufgabenJson(aufgabenFile, file);
                }
            }
            catch(Exception ex) {
                while(ex != null) {
                    await Console.Error.WriteLineAsync($"Error: {ex.GetType().Name}: {ex.Message}");
                    ex = ex.InnerException;
                }
            }
        }

        public static async Task CreateAufgabenJson(AufgabenFile aufgabenFile, string outputFile)
        {
            using (FileStream fs = File.Create(outputFile))
            {
                await JsonSerializer.SerializeAsync(fs, aufgabenFile, serializerOptions);
            }
        }

        public static async Task CreateAufgabenTsv(AufgabenFile aufgabenFile, string outputFile)
        {
            using (var fs = new System.IO.FileStream(outputFile, FileMode.Create, FileAccess.Write) )
            using (var sw = new StreamWriter(fs))
            {
                await sw.WriteLineAsync(string.Join("\t", headers));
                foreach (var r in aufgabenFile.Aufgaben) {
                    var rechnung = $"{r.A} {r.Op} {r.B} = {r.Resultat}";
                    await sw.WriteLineAsync($"{r.A}\t{r.Op}\t{r.B}\t{r.Resultat}");
                }
            }
        }

        public static AufgabenFile CreateAufgabenFile(int anzahl, params Func<int, int, Rechnung>[] factories)
        {
            var statistik = new int[1000];
            var result = new List<Rechnung>(anzahl);
            foreach(var i in Enumerable.Range(1, anzahl)) {
                var selector = GetRandom(0, factories.Length);
                var factory = factories[selector];
                var (a, b) = GetOperatorsByRechnungtyp(factory);
                var r = factory(a, b);
                statistik[r.A - 1]++;
                statistik[r.B - 1]++;
                result.Add(r);
            }

            return new AufgabenFile {Aufgaben = result, Operandenstatistik = statistik};
        }

        private static (int A, int B) GetOperatorsByRechnungtyp(Func<int, int, Rechnung> factory)
        {
            int a;
            int b;

            if (factory == GetMalrechnung) {
                a = GetRandom(2, 10);
                b = GetRandom(2, 12);
                return (a, b);
            }
            if (factory == GetPlusRechnung) {
                a = GetRandom(2, 500);
                b = GetRandom(2, 1000-a);
                return (a, b);
            }

            // Minusrechnung
            a = GetRandom(2, 500);
            b = GetRandom(2, 1000);
            return (a, b);
        }

        public static Rechnung GetMalrechnung(int a, int b) {
            return new Rechnung(a, "*", b, a * b);
        }

        public static Rechnung GetPlusRechnung(int a, int b)
        {
            return new Rechnung(a, "+", b, a + b);
        }

        public static Rechnung GetMinusRechnung(int a, int b)
        {
            return (a > b) ? new Rechnung(a, "-", b, a - b) : new Rechnung(b, "-", a, b - a);
        }

        static int GetRandom(int start, int end) {
            if (random == null) {
                random = new Random();
            }
            return start < end ? random.Next(start, end) : random.Next(end, start);
        }
    }

    public class AufgabenFile
    {
        public List<Rechnung> Aufgaben { get; set; }

        public int[] Operandenstatistik { get; set; }
    }

    public class Rechnung
    {
        public Rechnung(int a, string op, int b, int resultat)
        {
            this.A = a;
            this.Op = op;
            this.B = b;
            this.Resultat = resultat;
        }

        public int A { get; set; }
        public string Op { get; set; }
        public int B { get; set; }
        public int Resultat { get; set; }
    }
}
