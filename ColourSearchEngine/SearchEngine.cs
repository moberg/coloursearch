using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ColourSearchEngine
{
    public class SearchEntity
    {
        public string FileName { get; set; }
        public Histogram Histogram { get; set; }

        public SearchEntity()
        {
            
        }

        public SearchEntity(string fileName, Histogram histogram)
        {
            
            FileName = Path.GetFileName(fileName);
            Histogram = histogram;
        }
    }

    public class SearchResult
    {
        public double Distance { get; set; }
        public SearchEntity Entity { get; set; }

        public SearchResult()
        {
            
        }

        public SearchResult(double distance, SearchEntity entity)
        {
            Distance = distance;
            Entity = entity;
        }
    }

    public class SearchDatabase
    {
        public string BasePath { get; set; }
        public SearchEntity[] Images { get; set; }
    }

    public class SearchEngine
    {
        SearchDatabase _db = new SearchDatabase();

        public int IndexSize
        {
            get { return _db.Images.Length; }
        }

        public void RebuildDatabase(string fileLocation)
        {
            string[] images = new[]
                                  {
                                      Directory.GetFiles(fileLocation, "*.jpg"),
                                      Directory.GetFiles(fileLocation, "*.png")
                                  }.SelectMany(x => x).ToArray();

            int i = 0;
            int total = images.Length;

            var entities = images
                    .AsParallel()
                    .Select(f =>
                                {
                                    
                                    var s = new SearchEntity(f, new Bitmap(f).GetHistograms());
                                    
                                    var current = Interlocked.Increment(ref i);
                                    Console.Write("\rRebuilding database: {0}%   ", (i / (double)total * 100).ToString("N1"));

                                    return s;
                                })
                    .ToArray();

            _db = new SearchDatabase {Images = entities, BasePath = fileLocation};

            Console.WriteLine("");
        }

        public void SaveDatabase(string file)
        {
            Console.WriteLine("Saving database");
            var serializer = new XmlSerializer(typeof(SearchDatabase));
            TextWriter textWriter = new StreamWriter(file);
            serializer.Serialize(textWriter, _db);
            textWriter.Close();
        }

        public void LoadDatabase(string file)
        {
            Console.Write("Loading database from file");

            var serializer = new XmlSerializer(typeof(SearchDatabase));

            var fs = new FileStream(file, FileMode.Open);
            XmlReader reader = new XmlTextReader(fs);

            _db = (SearchDatabase)serializer.Deserialize(reader);

            Console.WriteLine("... done");
        }

        public SearchResult[] Search(Color input, ColorSpace colorSpace, SearchMethod method)
        {
            Bitmap bmp = new Bitmap(265, 265, PixelFormat.Format32bppArgb);
            Graphics gBmp = Graphics.FromImage(bmp);
            gBmp.FillRectangle(new SolidBrush(input), 0, 0, 256, 256);


            if (colorSpace == ColorSpace.Rgb)
            {
                return SearchRgb(bmp.GetRgbHistogram(), method);
            }

            return SearchHsv(bmp.GetHsvHistogram(), method);
        }

        private SearchResult[] SearchRgb(int[][] input, SearchMethod method)
        {
            return Search(input, Histogram.RgbDistance, x => x.RgbHistogram, method);
        }

        private SearchResult[] SearchHsv(int[][] input, SearchMethod method)
        {
            return Search(input, Histogram.HsvChiSquareDistance, x => x.HsvHistogram, method);
        }

        private SearchResult[] Search(
            int[][] input, 
            Func<int[][], int[][], Func<int[], int[], int, double>, double> diff, 
            Func<Histogram, int[][]> getHistogram, 
            SearchMethod method)
        {
            Func<int[], int[], int, double> compareMethod = Histogram.ChiSquare;

            switch (method)
            {
                case SearchMethod.ChiSquare:
                    compareMethod = Histogram.ChiSquare;
                    break;

                case SearchMethod.ChiSquare2:
                    compareMethod = Histogram.ChiSquare2;
                    break;

                case SearchMethod.Correlation:
                    compareMethod = Histogram.Correlation;
                    break;
                case SearchMethod.Intersection:
                    compareMethod = Histogram.Intersection;
                    break;
            }

            var result = _db.Images
                .AsParallel()
                .Select(x => new SearchResult(diff(input, getHistogram(x.Histogram), compareMethod), x))
                ;
            
            if (method == SearchMethod.Correlation || method == SearchMethod.Intersection)
                return result.OrderByDescending(x => x.Distance).ToArray();
            
            return result.OrderBy(x => x.Distance).ToArray();
        }
    }

    public enum ColorSpace
    {
        Rgb,
        Hsv
    }

    public enum SearchMethod
    {
        ChiSquare,
        ChiSquare2,
        Correlation,
        Intersection,
    }
}
