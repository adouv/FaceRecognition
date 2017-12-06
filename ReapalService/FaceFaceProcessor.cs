using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ReapalService
{
   public class FaceFaceProcessor:BaseProcessor<BaseView>
    {
        protected override string RequestAddress { get => "v1/face/face"; }
        protected override string ServiceAddress { get => "http://10.168.200.47:8888/"; }
        protected override Dictionary<string,FileItem> FileList { get=>buffers; }

        private string face_id;

        private Dictionary<string,FileItem> buffers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="faceId"></param>
        public FaceFaceProcessor(string faceId,Dictionary<string,FileItem> buffer)
        {
            face_id = faceId;
            buffers = buffer;
        }
        protected override Dictionary<string, object> PrepareRequestCore()
        {
            var dicResult = new Dictionary<string, object>();
            dicResult.Add("face_id", face_id);
            return dicResult;
        }
    }
}
