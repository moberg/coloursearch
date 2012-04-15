using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ColourSearch.Handlers;
using ColourSearchEngine;
using Newtonsoft.Json;

namespace ColourSearch
{
    public class VerySimpleWebServer
    {
        private readonly int _port;
        private readonly bool _multithreaded;
        private readonly SearchEngine _searchEngine;
        private readonly HttpListener _listener = new HttpListener();

        private readonly List<Tuple<string, IHandler>> _routes
            = new List<Tuple<string, IHandler>>();

        public VerySimpleWebServer(int port, SearchEngine searchEngine, string staticFilesDir, bool multithreaded)
        {
            _port = port;
            _searchEngine = searchEngine;
            _multithreaded = multithreaded;

            _routes.Add(Tuple.Create("/search", (IHandler)new SearchHandler(_searchEngine)));
            _routes.Add(Tuple.Create("/", (IHandler)new StaticFileHandler(staticFilesDir)));
        }

        public void Run()
        {
            string uriPrefix = "http://+:" + _port + "/";
            _listener.Prefixes.Add(uriPrefix);

            Console.WriteLine("Listening on: " + uriPrefix);
            _listener.Start();

            while (true)
            {
                HttpListenerContext ctx = _listener.GetContext();

                if (_multithreaded)
                {
                    Task.Factory.StartNew(() => ProcessRequest(ctx));
                }
                else
                {
                    ProcessRequest(ctx);
                }
            }
        }

        private void ProcessRequest(HttpListenerContext ctx)
        {
            try
            {
                try
                {
                    var url = ctx.Request.Url.AbsolutePath;
                    bool processed = _routes
                        .Where(t => url.StartsWith(t.Item1))
                        .Select(t => t.Item2.ProcessRequest(ctx))
                        .FirstOrDefault();

                    if (!processed)
                    {
                        ctx.Response.StatusCode = (int) HttpStatusCode.NotFound;
                        byte[] b = Encoding.UTF8.GetBytes("404 Not found");
                        ctx.Response.ContentLength64 = b.Length;
                        ctx.Response.OutputStream.Write(b, 0, b.Length);
                    }
                }
                catch (HttpListenerException)
                {
                    Console.WriteLine("Client closed connection");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);

                    ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    byte[] b = Encoding.UTF8.GetBytes(e.StackTrace);
                    ctx.Response.ContentLength64 = b.Length;
                    ctx.Response.OutputStream.Write(b, 0, b.Length);
                }

                ctx.Response.OutputStream.Close();
            }
            catch (HttpListenerException)
            {
                Console.WriteLine("Client closed connection");
            }
        }
    }
}
