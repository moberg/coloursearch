using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using ColourSearchEngine;
using Newtonsoft.Json;

namespace ColourSearch.Handlers
{
    public class SearchHandler : IHandler
    {
        private readonly SearchEngine _searchEngine;

        public SearchHandler(SearchEngine searchEngine)
        {
            _searchEngine = searchEngine;
        }

        public bool ProcessRequest(HttpListenerContext ctx)
        {
            string htmlColour = "#" + ctx.Request.QueryString["colour"];
            int page = int.Parse(ctx.Request.QueryString["page"]);
            int pageSize = int.Parse(ctx.Request.QueryString["pageSize"]);

            ColorSpace colorSpace;
            Enum.TryParse(ctx.Request.QueryString["colourspace"], true, out colorSpace);

            if (colorSpace != ColorSpace.Rgb)
                throw new Exception("Only RBG is supported at the moment");

            SearchMethod searchMethod;
            Enum.TryParse(ctx.Request.QueryString["comparemethod"], true, out searchMethod);

            var color = System.Drawing.ColorTranslator.FromHtml(htmlColour);

            Console.Write("Searching for color: {0} using {1} ({2})", htmlColour, searchMethod, colorSpace);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = _searchEngine.Search(color, colorSpace, searchMethod)
                .Skip(page*pageSize)
                .Take(pageSize)
                .Select(x => new ImageSearchResponse {distance = x.Distance, filename = x.Entity.FileName});

            Console.WriteLine(" {0} ms", stopwatch.ElapsedMilliseconds);

            return CreateJsonResponse(ctx, result);
        }

        public bool CreateJsonResponse(HttpListenerContext context, object result)
        {
            string json = JsonConvert.SerializeObject(result);
            string callback = context.Request.QueryString["callback"];
            json = callback + "({result:" + json + "})";
            context.Response.ContentType = "application/json";

            byte[] b = Encoding.UTF8.GetBytes(json);
            context.Response.ContentLength64 = b.Length;
            context.Response.OutputStream.Write(b, 0, b.Length);

            return true;
        }
    }

    public class ImageSearch
    {
        public string Colour { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ImageSearchResponse
    {
        public string filename { get; set; }
        public double distance { get; set; }
    }
}
