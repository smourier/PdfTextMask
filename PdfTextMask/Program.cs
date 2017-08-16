using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup;
using PdfTextMask.Utilities;

namespace PdfTextMask
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                SafeMain(args);
                return;
            }

            try
            {
                SafeMain(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void SafeMain(string[] args)
        {
            Console.WriteLine("PdfTextMask - Copyright © 2016-" + DateTime.Now.Year + " Simon Mourier. All rights reserved.");
            Console.WriteLine(Context.AssemblyDisplayName);
            Console.WriteLine();
            if (CommandLine.HelpRequested || args.Length < 3)
            {
                Help();
                return;
            }

            string input = CommandLine.GetNullifiedArgument(0);
            string output = CommandLine.GetNullifiedArgument(1);
            var texts = Conversions.SplitToList<string>(CommandLine.GetNullifiedArgument(2), '|');
            var cc = new ColorConverter();
            var color = (Color)cc.ConvertFromString(CommandLine.GetNullifiedArgument("color", "black"));
            Console.WriteLine("Input    : " + input);
            Console.WriteLine("Output   : " + output);
            Console.WriteLine("Texts    : " + string.Join(", ", texts));
            Console.WriteLine("Color    : " + color.Name);
            Console.WriteLine();

            using (var reader = new PdfReader(input))
            {
                var locs = new List<PdfCleanUpLocation>();
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    Console.WriteLine("Page #" + i);
                    foreach (var text in texts)
                    {
                        int count = 0;
                        var strategy = new TextExtractionStrategy(text);
                        PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                        foreach (var rect in strategy.Rectangles)
                        {
                            locs.Add(new PdfCleanUpLocation(i, rect, new BaseColor(color)));
                            count++;
                        }

                        Console.WriteLine(" Number of occurrences of '" + text + "': " + count);
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("Saving '" + output + "'...");
                using (var fs = File.OpenWrite(output))
                {
                    using (var stamper = new PdfStamper(reader, fs))
                    {
                        var cleanup = new PdfCleanUpProcessor(locs, stamper);
                        cleanup.CleanUp();
                    }
                }
            }
        }

        static void Help()
        {
            Console.WriteLine(Assembly.GetEntryAssembly().GetName().Name.ToUpperInvariant() + " <infile> <outfile> \"<texts to mask>\" [options]");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("    This tool is used to mask texts in a PDF file. Texts to mask must be separated by the | character.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("    /color:code        The color code for the mask. Default is 'black'.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    " + Assembly.GetEntryAssembly().GetName().Name.ToUpperInvariant() + " input.pdf output.pdf \"whatever|something\"");
            Console.WriteLine("    Will mask all occurrences of 'whatever' and 'something' texts.");
            Console.WriteLine();
        }
    }
}
