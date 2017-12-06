using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ReapalService
{
    public class FaceCreateProcessor : BaseProcessor<CrateFaceView>
    {
        protected override string RequestAddress { get => "v1/face/create"; }
        protected override string ServiceAddress { get => "http://10.168.200.47:8888/"; }
        protected override Dictionary<string, FileItem> FileList { get => buffers; }

        private string cret_no = "412722199310223521";
        private string cert_name = "201709306319733661130403840";
        private string face_id = "孙晨杨";

        private Dictionary<string, FileItem> buffers;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cretNo">身份证号</param>
        /// <param name="certName">人名</param>
        /// <param name="faceId">人脸id</param>
        /// <para name="buffer">图片流</para>
        public FaceCreateProcessor(string cretNo, string certName, string faceId, Dictionary<string, FileItem> buffer)
        {
            cret_no = cretNo;
            cert_name = certName;
            face_id = faceId;
            buffers = buffer;
        }
        protected override Dictionary<string, object> PrepareRequestCore()
        {
            var dicResult = new Dictionary<string, object>();
            dicResult.Add("cert_no", cret_no);
            dicResult.Add("face_id", face_id);
            dicResult.Add("cert_name", cert_name);
            return dicResult;
        }
    }
}
