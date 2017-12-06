using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using System.Threading;
using Emgu.CV.Structure;
using AmFaceVerify;
using System.Diagnostics;
using System.IO;
using ReapalService;
using System.Threading.Tasks;

namespace ArcsoftFaceTest
{
    public partial class Form2 : Form
    {
        private Capture capture;

        private Thread displayImage;

        private int maxFaceCount = 10;

        private int width = 0;

        private int height = 0;

        private int pitch = 0;

        //摄像头是否加载成功
        private bool _stop;

        //人脸跟踪引擎
        IntPtr traceEngine = IntPtr.Zero;

        //构造函数
        public Form2()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);

            this.UpdateStyles();
        }

        //页面初始化
        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                #region 初始化人脸跟踪引擎

                int detectSize = 40 * 1024 * 1024;

                IntPtr pMem = Marshal.AllocHGlobal(detectSize);

                //1-1
                //string appId = "4tnYSJ68e8wztSo4Cf7WvbyMZduHwpqtThAEM3obMWbE";

                //1-1
                // string sdkKey = "Cgbaq34izc8PA2Px26x8qqWLFNmGNL4oukYspKsbMz3z";

                //1-n
                string appId = "34D48RFbU22vTdGVYXncnwTkk3EN6bcW2RYfJc9i9EKE";

                //1-n
                string sdkKey = "9tjAT5hQJcSvzzDaEPqC7rmgqA9FkJFSq7yG2tzSLLzC";

                //人脸跟踪引擎初始化
                int retCode = AmFaceVerify.AFT_FSDK_InitialFaceEngine(appId, sdkKey, pMem, detectSize, ref traceEngine, 5, 16, maxFaceCount);

                Console.WriteLine("  Init Result:" + retCode);

                //获取人脸跟踪引擎版本
                IntPtr versionPtr = AmFaceVerify.AFT_FSDK_GetVersion(traceEngine);

                AFT_FSDK_Version version = (AFT_FSDK_Version)Marshal.PtrToStructure(versionPtr, typeof(AFT_FSDK_Version));

                Console.WriteLine("lCodebase:{0} lMajor:{1} lMinor:{2} lBuild:{3} Version:{4} BuildDate:{5} CopyRight:{6}", version.lCodebase, version.lMajor, version.lMinor, version.lBuild, Marshal.PtrToStringAnsi(version.Version), Marshal.PtrToStringAnsi(version.BuildDate), Marshal.PtrToStringAnsi(version.CopyRight));

                #endregion
                this.capture = new Capture();

                displayImage = new Thread(new ThreadStart(this.DisplayAddProcess));

                displayImage.Start();

                displayImage.IsBackground = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                LogHelper.WriteErrorLog("Form2_Load", ex.Message + "\n" + ex.StackTrace);
            }

        }

        //把图片转成byte[]
        private byte[] getBGR(Bitmap image, ref int width, ref int height, ref int pitch)
        {
            //Bitmap image = new Bitmap(imgPath);

            const PixelFormat PixelFormat = PixelFormat.Format24bppRgb;

            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat);

            IntPtr ptr = data.Scan0;

            int ptr_len = data.Height * Math.Abs(data.Stride);

            byte[] ptr_bgr = new byte[ptr_len];

            Marshal.Copy(ptr, ptr_bgr, 0, ptr_len);

            width = data.Width;

            height = data.Height;

            pitch = Math.Abs(data.Stride);

            int line = width * 3;

            int bgr_len = line * height;

            byte[] bgr = new byte[bgr_len];

            for (int i = 0; i < height; ++i)
            {
                Array.Copy(ptr_bgr, i * pitch, bgr, i * line, line);
            }

            pitch = line;

            image.UnlockBits(data);

            return bgr;
        }

        private bool IsPhoto = true;
        //加载摄像头图像
        public void DisplayAddProcess()
        {
            try
            {
                while (!this._stop)
                {
                    Image<Bgr, byte> frame = this.capture.QueryFrame();

                    if (frame != null)
                    {
                        using (Bitmap im = frame.Clone().ToBitmap())
                        {
                            im.RotateFlip(RotateFlipType.RotateNoneFlipX);

                            Bitmap tempBitmap = im.Clone(new RectangleF(0, 0, im.Width, im.Height), PixelFormat.Format24bppRgb);

                            //this.setPictureBoxControlImage(this.pictureBox1, new Bitmap(tempBitmap));

                            byte[] imageData = getBGR(tempBitmap, ref width, ref height, ref pitch);

                            IntPtr imageDataPtr = Marshal.AllocHGlobal(imageData.Length);

                            Marshal.Copy(imageData, 0, imageDataPtr, imageData.Length);

                            ASVLOFFSCREEN offInput = new ASVLOFFSCREEN();

                            offInput.u32PixelArrayFormat = 513;

                            offInput.ppu8Plane = new IntPtr[4];

                            offInput.ppu8Plane[0] = imageDataPtr;

                            offInput.i32Width = width;

                            offInput.i32Height = height;

                            offInput.pi32Pitch = new int[4];

                            offInput.pi32Pitch[0] = pitch;

                            AFT_FSDK_FACERES faceRes = new AFT_FSDK_FACERES();

                            IntPtr offInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf(offInput));

                            Marshal.StructureToPtr(offInput, offInputPtr, false);

                            IntPtr faceResPtr = Marshal.AllocHGlobal(Marshal.SizeOf(faceRes));

                            //人脸检测
                            int detectResult = AmFaceVerify.AFT_FSDK_FaceFeatureDetect(traceEngine, offInputPtr, ref faceResPtr);

                            if (detectResult != 0)
                            {
                                this.setPictureBoxControlImage(this.pictureBox1, new Bitmap(tempBitmap));

                                continue;
                            }

                            //Console.WriteLine("  detect Result:" + detectResult);

                            object obj = Marshal.PtrToStructure(faceResPtr, typeof(AFT_FSDK_FACERES));

                            faceRes = (AFT_FSDK_FACERES)obj;
                            //Console.WriteLine("  Face Count:{0}", faceRes.nFace);

                            if (faceRes.nFace > maxFaceCount)
                            {
                                this.setPictureBoxControlImage(this.pictureBox1, new Bitmap(tempBitmap));

                                continue;
                            }

                            Rectangle[] faces = new Rectangle[faceRes.nFace];

                            for (int i = 0; i < faceRes.nFace; i++)
                            {
                                MRECT rect = (MRECT)Marshal.PtrToStructure(faceRes.rcFace + Marshal.SizeOf(typeof(MRECT)) * i, typeof(MRECT));

                                //Console.WriteLine("    left:{0} top:{1} right:{2} bottom:{3}", rect.left, rect.top, rect.right, rect.bottom);

                                faces[i] = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);





                            }

                            if (faceRes.nFace > 0)
                            {
                                this.setPictureBoxControlImage(this.pictureBox1, drawFaces(tempBitmap, faces));

                                //  this.setPictureBoxControlImage(this.pictureBox2, drawFaces(tempBitmap, faces));
                                #region xhz

                                if (IsPhoto)
                                {
                                    Task task1 = Task.Run(() =>
                                    {
                                        pictureBox2.Image = drawFaces((Bitmap)tempBitmap.Clone(), (Rectangle[])faces.Clone());

                                        UploadPicture();
                                    });
                                   // task1.Wait();

                                    // Bitmap jpg=new Bitmap((Image)pictureBox2.Image.Clone());
                                    //pictureBox2.Image = CutFace(tempBitmap, faces[0].X,faces[0].Y,faces[0].Width,faces[0].Height);


                                    IsPhoto = false;
                                }
                                //  Image image = CutFace(bitmap, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);

                                #endregion
                            }
                            else
                            {
                                this.setPictureBoxControlImage(this.pictureBox1, new Bitmap(tempBitmap));
                            }

                            Marshal.FreeHGlobal(offInputPtr);

                            imageData = null;

                            Marshal.FreeHGlobal(imageDataPtr);

                            offInput = new ASVLOFFSCREEN();

                            faceRes = new AFT_FSDK_FACERES();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                LogHelper.WriteErrorLog("DisplayAddProcess", ex.Message + "\n" + ex.StackTrace);
            }
        }

        //页面关闭
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();

            if (this.capture != null)
            {
                this.capture.Dispose();
            }

            this._stop = true;
        }

        //多线程设置PictureBox的图像
        private void setPictureBoxControlImage(PictureBox control, Bitmap value)
        {
            control.Invoke(new Action<PictureBox, Bitmap>((ct, v) => { ct.Image = v; }), new object[] { control, value });
        }

        //提取识别出的人脸
        public static Bitmap drawFaces(Bitmap srcImage, Rectangle[] faces)
        {
            if (srcImage == null)
            {
                return null;
            }

            try
            {
                Bitmap bmpOut = new Bitmap(srcImage);

                Graphics g = Graphics.FromImage(bmpOut);

                g.DrawRectangles(Pens.Blue, faces);

                g.Dispose();

                return bmpOut;
            }
            catch
            {
                return null;
            }
        }

        //提取识别出的人脸
        public static Bitmap CutFace(Bitmap srcImage, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (srcImage == null)
            {
                return null;
            }

            int w = srcImage.Width;

            int h = srcImage.Height;

            if (StartX >= w || StartY >= h)
            {
                return null;
            }
            if (StartX + iWidth > w)
            {
                iWidth = w - StartX;
            }
            if (StartY + iHeight > h)
            {
                iHeight = h - StartY;
            }
            try
            {
                Bitmap bmpOut = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);

                Graphics g = Graphics.FromImage(bmpOut);

                g.DrawImage(srcImage, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);

                g.Dispose();

                return bmpOut;
            }
            catch
            {
                return null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IsPhoto = true;
        }


        private void Post(FileItem file)
        {
            string faceid = Guid.NewGuid().ToString();
            Dictionary<string, FileItem> dicCreate = new Dictionary<string, FileItem>();
            dicCreate.Add("image_cert", file);
            FaceCreateProcessor processor = new FaceCreateProcessor("432924197810103874", "李国松", faceid, dicCreate);
            var result = processor.Execute();
            if (result.Success)
            {
                if (file==null)
                {
                    Dictionary<string, FileItem> dicFace = new Dictionary<string, FileItem>();
                    dicFace.Add("image_face", file);
                    FaceFaceProcessor faceFaceProcessor = new FaceFaceProcessor(faceid, dicFace);
                    var faceRsult = faceFaceProcessor.Execute();
                    if (faceRsult.Success)
                    {
                        MessageBox.Show("匹配成功");
                    }
                    else
                    {
                        MessageBox.Show(faceRsult.Message);
                    }
                }
                else
                {
                    MessageBox.Show("匹配成功");
                }
               
            }
            else
            {
                MessageBox.Show(result.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

            Bitmap bitMap = new Bitmap(pictureBox2.Image);
            MemoryStream ms = new MemoryStream();
            //bitMap.Save(ms, ImageFormat.Jpeg);

            byte[] buffer = new byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(buffer, 0, buffer.Length); 

            var ft = DateTime.Now.ToFileTime();
            string name = ft + ".jpg";
            string fileName = Path.Combine("D:\\", name);
            //bitMap.Save(fileName);
            var file = new FileItem()
            {
                FileName = fileName,
                Content = buffer
            };

            Post(file);
        }

        private void UploadPicture()
        {
            Bitmap bitMap = new Bitmap(pictureBox2.Image);
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(buffer, 0, buffer.Length);
            var ft = DateTime.Now.ToFileTime();
            string name = ft + ".jpg";
            string fileName = Path.Combine("E:\\", name);
           // bitMap.Save(fileName);
            var file = new FileItem()
            {
                FileName = fileName,
                Content = buffer
            };

            Post(file);
        }
    }
}
