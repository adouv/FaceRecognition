using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ReapalDDD
{
  public static  class NameValueCollectionExtension
    {
        /// <summary>
        /// NameValueCollection 转换成字典
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ConvNameValueCollectionToDictionary(NameValueCollection data)
        {
            return data.AllKeys.OrderBy(i => i).ToDictionary(k => k, k => data[k]);
        }
    }
}
