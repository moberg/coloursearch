using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ColourSearch.Handlers
{
    public class StaticFileHandler : IHandler
    {
        private readonly string _staticFilesDir;

        public StaticFileHandler(string rootDir) 
        {
            _staticFilesDir = rootDir;
        }

        public bool ProcessRequest(HttpListenerContext ctx)
        {
            string request = WebUtility.HtmlDecode(ctx.Request.RawUrl).Substring(1).Replace("/", "\\");
            if (request == "")
                request = "index.html";

            string path = Resolve(request);
            string filename = Path.GetFileName(path);

            if (!File.Exists(path))
            {
                Console.WriteLine("Client requested nonexistent file: " + filename);
                return false;
            }

            var extension = Path.GetExtension(filename);

            switch (extension.ToLower())
            {
                case ".html":
                    ctx.Response.ContentType = "text/html";
                    break;
                case ".js":
                    ctx.Response.ContentType = "application/javascript";
                    break;
                case ".css":
                    ctx.Response.ContentType = "text/css";
                    break;
                case ".png":
                    ctx.Response.ContentType = "image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    ctx.Response.ContentType = "image/jpeg";
                    break;
            }

            byte[] b = File.ReadAllBytes(path);
            ctx.Response.ContentLength64 = b.Length;
            ctx.Response.OutputStream.Write(b, 0, b.Length);

            return true;
        }

        private string Resolve(string fileName)
        {
            string ret = Path.GetFullPath(Path.Combine(_staticFilesDir, fileName));
            if (ret.StartsWith(_staticFilesDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar))
            {
                return ret;
            }
            throw new ArgumentException("Path resolved to out of accesable directroy");
        }
    }

    public interface IHandler
    {
        bool ProcessRequest(HttpListenerContext ctx);
    }
}
