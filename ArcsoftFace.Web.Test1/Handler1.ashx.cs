using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ArcsoftFace.Web.Test1
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class Handler1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            var xxxx = HttpContext.Current.Request.InputStream;
            xxxx.Position = 0;
            StreamReader reader = new StreamReader(xxxx);
            string text = reader.ReadToEnd();
            //context.Request.Files[0].SaveAs($@"C:\Users\xhz\Pictures\{Guid.NewGuid()}.jpg");

            //context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");


        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}