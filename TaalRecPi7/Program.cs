using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace TaalRecPi7
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sW = Stopwatch.StartNew();
            Console.WriteLine("Loading trigram model....");
            Dictionary<string, double> EngelsTri = Train("alengels", 3);
            Dictionary<string, double> DuitsTri = Train("alduits", 3);
            Dictionary<string, double> FransTri = Train("alfrans", 3);
            Dictionary<string, double> SpaansTri = Train("alspaans", 3);
            Dictionary<string, double> FinsTri = Train("alfins", 3);
            Dictionary<string, double> ItiliaansTri = Train("alitiliaans", 3);
            Console.WriteLine("Trigram model build.\n-----------------------------------");
            sW.Stop();
            Console.WriteLine("Elapsed time: " + sW.Elapsed.TotalMilliseconds + " Miliseconds\n===================================");

            sW = Stopwatch.StartNew();
            Console.WriteLine("\nLoading bigram model....");
            Dictionary<string, double> EngelsBi = Train("alengels", 2);
            Dictionary<string, double> DuitsBi = Train("alduits", 2);
            Dictionary<string, double> FransBi = Train("alfrans", 2);
            Dictionary<string, double> SpaansBi = Train("alspaans", 2);
            Dictionary<string, double> FinsBi = Train("alfins", 2);
            Dictionary<string, double> ItiliaansBi = Train("alitiliaans", 2);
            Console.WriteLine("Bigram model build.\n-----------------------------------");
            sW.Stop();
            Console.WriteLine("Elapsed time: " + sW.Elapsed.TotalMilliseconds + " Miliseconds\n===================================");


            Console.WriteLine("\nInput: ");
            string inputText = Console.ReadLine();

            Dictionary<string, double> MaxvalTri = new Dictionary<string, double>() { { "Engels", 0 }, { "Duits", 0 }, { "Frans", 0 }, { "Spaans", 0 }, { "Fins", 0 }, { "Italy", 0 } };
            Dictionary<string, double> MaxvalBi = new Dictionary<string, double>() { { "Engels", 0 }, { "Duits", 0 }, { "Frans", 0 }, { "Spaans", 0 }, { "Fins", 0 }, { "Italy", 0 } };
            Dictionary<string, Dictionary<string, double>> alTriLang = new Dictionary<string, Dictionary<string, double>>() { { "Engels", EngelsTri }, { "Duits", DuitsTri }, { "Frans", FransTri }, { "Spaans", SpaansTri }, { "Fins", FinsTri }, { "Italy", ItiliaansTri } };
            Dictionary<string, Dictionary<string, double>> alBiLang = new Dictionary<string, Dictionary<string, double>>() { { "Engels", EngelsBi }, { "Duits", DuitsBi }, { "Frans", FransBi }, { "Spaans", SpaansBi }, { "Fins", FinsBi }, { "Italy", ItiliaansBi } };

            Console.WriteLine("\n================================\nTri gram output:\n================================");
            Console.WriteLine("Language| Probability\n================================");
                        
            sW = Stopwatch.StartNew();
            double total;
            string Text = clean(inputText);
            foreach (var lang in alTriLang.Keys)
            {
                total = 0;
                foreach (var lang2 in alTriLang.Keys)
                {
                    total += Compare(Text, alTriLang[lang2], 3);
                }
                MaxvalTri[lang] = Compare(Text, alTriLang[lang], 3) / total * 100;
                Console.WriteLine(lang + "\t| " + MaxvalTri[lang] + "%");
            }
            var keyAndValue = MaxvalTri.OrderBy(kvp => kvp.Value).Last();
            Console.WriteLine("================================\nMost probable language:\n{0} => {1}", keyAndValue.Key, keyAndValue.Value + "%\n================================");

            sW.Stop();
            Console.WriteLine("Elapsed time: " + sW.Elapsed.TotalMilliseconds + " Miliseconds\n================================");

            Console.WriteLine("\n================================\nBi gram output:\n================================");
            Console.WriteLine("Language| Probability\n================================");
            sW = Stopwatch.StartNew();

            foreach (var lang in alBiLang.Keys)
            {
                total = 0;
                foreach (var lang2 in alBiLang.Keys)
                {
                    total += Compare(Text, alBiLang[lang2], 2);
                }
                MaxvalBi[lang] = (Compare(Text, alBiLang[lang], 2) / total * 100);
                Console.WriteLine(lang + "\t| " + (Compare(Text, alBiLang[lang], 2) / total * 100) + "%");
            }
            keyAndValue = MaxvalBi.OrderBy(kvp => kvp.Value).Last();
            Console.WriteLine("================================\nMost probable language:\n{0} => {1}", keyAndValue.Key, keyAndValue.Value + "%\n================================");

            sW.Stop();
            Console.WriteLine("Elapsed time: " + sW.Elapsed.TotalMilliseconds + " Miliseconds\n================================");
        }

        static private double Compare(string inputText, Dictionary<string, double> Dict, int ngram)
        {
            List<string> input = new List<string>();
            inputText = clean(inputText);
            for (int Char = 0; Char < (inputText.Length - 2); Char++)
            {
                if (ngram == 3)
                {
                    string tri = inputText[Char].ToString() + inputText[Char + 1].ToString() + inputText[Char + 2].ToString();
                    tri = tri.ToLower();
                    input.Add(tri);
                }
                else
                {
                    string bi = inputText[Char].ToString() + inputText[Char + 1].ToString();
                    bi = bi.ToLower();
                    input.Add(bi);
                }
            }

            double Prob = 1;
            foreach (var item in input)
            {
                Prob *= (Dict[item] / 100);
            }
            return Prob;
        }

        static private Dictionary<string, double> ngram(int ngram)
        {
            Dictionary<string, double> Template = new Dictionary<string, double>();
            string key;
            string Charset = "abcdefghijklmnopqrstuvwxyz" + //Standaard
                "àáâäãçèéêëìíîïñòóôöõùúûüýÿåæð" + //Didacties
                "ĝĉŝŭĵĥ" + //Extra Spaans
                "œ" + //extra Frans
                "ß"; //extra Duits

            var Charlistset = Charset.ToList();

            for (int val1 = 0; val1 < Charlistset.Count; val1++)
            {
                for (int val2 = 0; val2 < Charlistset.Count; val2++)
                {
                    if (ngram == 3)
                    {
                        for (int val3 = 0; val3 < Charlistset.Count; val3++)
                        {
                            key = Charlistset[val1].ToString() + Charlistset[val2].ToString() + Charlistset[val3].ToString();
                            Template.Add(key, 0.01);
                        }
                    }
                    else
                    {
                        key = Charlistset[val1].ToString() + Charlistset[val2].ToString();
                        Template.Add(key, 0.01);
                    }
                }
            }
            return Template;
        }

        static private Dictionary<string, double> Train(string datasetName, int n)
        {
            int counter = 0;
            string line;
            Dictionary<string, double> language = ngram(n);
            StreamReader file = new StreamReader(@"C:\Users\Rutger\source\repos\TaalRecPi7\TaalRecPi7\altalen\" + datasetName + ".txt");

            while ((line = file.ReadLine()) != null)
            {
                line = clean(line);
                for (int Char = 0; Char < (line.Length - 2); Char++)
                {
                    string nstring = "";
                    if (n == 3)
                    {
                        nstring = line[Char].ToString() + line[Char + 1].ToString() + line[Char + 2].ToString();
                    }
                    else if (n == 2)
                    {
                        nstring = line[Char].ToString() + line[Char + 1].ToString();
                    }
                    nstring = nstring.ToLower();
                    if (language.ContainsKey(nstring))
                    {
                        double value = language[nstring];
                        language[nstring] = value + 1;
                    }
                    else
                    {
                        Console.WriteLine(nstring + "Key Not Present");
                        Console.WriteLine(datasetName);
                        Console.WriteLine(counter);
                    }
                }
                counter++;
            }
            file.Close();
            double tot = language.Sum(x => x.Value);
            foreach (var nstring in language.Keys.ToList())
            {
                double value = language[nstring];
                double perc = value / tot * 100;
                language[nstring] = perc;
            }
            return language;
        }

        static private string clean(string line)
        {
            string Dirtchars = @"—0123456789.,!?-_+=`[]{}~<>*&^%$#@()\|/:;’ " + "\"\'" ;
            var Dirt = Dirtchars.ToList();
            foreach (var item in Dirt)
            {
                line = RemoveAny(line, item.ToString());
            }
            return line;
        }

        static private string RemoveAny(string s, string charsToRemove)
        {
            var result = "";
            foreach (var c in s)
                if (charsToRemove.Contains(c))
                {
                    continue;
                }
                else
                {
                    result += c;
                }
            return result;
        }
    }
}
