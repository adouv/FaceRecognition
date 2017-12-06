using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReapalService
{
    public class BaseView
    {
        public string result_code { get; set; }
        public string result_msg { get; set; }
        public string sign { get; set; }
    }
    public class ResponseView
    {
        public string encryptkey { get; set; }
        public string data { get; set; }
        public string merchant_id { get; set; }
    }
    public class CrateFaceView 
    {
       
    }
}
