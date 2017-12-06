using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcsoftFaceTest
{
    public struct AFT_FSDK_FACERES
    {
        /// <summary>
        /// 人脸个数
        /// </summary>
        public int nFace;                     // number of faces detected
        /// <summary>
        /// 人脸角度信息
        /// </summary>
        public IntPtr lfaceOrient;                   // the angle of each face
        /// <summary>
        /// 人脸矩形框信息
        /// </summary>
        public IntPtr rcFace;                        // The bounding box of face
    }
}
