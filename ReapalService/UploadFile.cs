using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace ReapalService
{

    /// <summary>
    /// 上传文件
    /// </summary>
    public class FileItem
    {

        public FileItem()
        {
            ContentType = "application/octet-stream";
        }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }

        public string GetMimeType(string fileName)
        {
            string suffix = Path.GetExtension(fileName).ToUpper();
            string mimeType;
            switch (suffix)
            {
                case ".JPG": mimeType = "image/jpeg"; break;
                case ".GIF": mimeType = "image/gif"; break;
                case ".PNG": mimeType = "image/png"; break;
                case ".BMP": mimeType = "image/bmp"; break;
                default: mimeType = "application/octet-stream"; break;
            }
            return mimeType;
        }
    }
}
