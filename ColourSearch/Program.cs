using System;
using System.Collections.Generic;
using System.IO;
using ColourSearchEngine;
using Mono.Options;

namespace ColourSearch
{

    class Program
    {
        private const int Port = 9200;

        static void Main(string[] args)
        {

            bool show_help = false;
            bool rebuild = false;
            string staticFilesDir = Path.Combine(Directory.GetCurrentDirectory(), "public");
            string imagesDir = Path.Combine(staticFilesDir, "Content\\Images");
            string database = "database.xml";
            int port = 9200;
            
            var p = new OptionSet() {
                { "p|port=", "port to run the service on.", v => port = int.Parse(v) },
                { "d|database=", "database file.", v => database = v },
                { "i|images=", "images directory.", v => imagesDir = v },
                { "s|staticfiles=", "static files directory.", v => staticFilesDir = v },
                { "r|rebuild", "rebuild the database.", v => rebuild = v != null },
                { "h|help",  "show this message and exit",  v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("coloursearch: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `coloursearch --help' for more information.");
                return;
            }

            if (show_help)
            {
                ShowHelp(p);
                return;
            }

            Console.WriteLine("Starting search engine. Database: " + database);
            var searchEngine = new SearchEngine();
            
            if (rebuild || !File.Exists(database))
            {
                searchEngine.RebuildDatabase(imagesDir);
                searchEngine.SaveDatabase(database);
            }
            else
            {
                searchEngine.LoadDatabase(database);    
            }
            
            Console.WriteLine("Images loaded, {0} images", searchEngine.IndexSize);

            var service = new VerySimpleWebServer(port, searchEngine, false);
            service.Run();
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: coloursearch [OPTIONS]+ message");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}