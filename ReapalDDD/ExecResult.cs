using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReapalDDD
{
 public class ExecResult<T>
    {
        public  bool Success { get; set; }

        public  string Message { get; set; }

        public  string MsgCode { get; set; }

        public T Result { get; set; }
    }
}
