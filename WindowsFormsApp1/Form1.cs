using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        VideoCapture cap;
        Mat img = new Mat();
        Mat dst = new Mat();
        Mat res = new Mat();
        CascadeClassifier cascade = new
        CascadeClassifier("../../../haarcascade_frontalface_alt.xml");
        Boolean camera = false;
        Boolean picture = false;

        public Form1()
        {
            InitializeComponent();
            cap = new VideoCapture(0);
            //listbox
            String[] msg = { "None","Gray", "Otsu","Binary", "GaussianBlur", "MedianBlur","Canny","Kao"};
            for (int i=0;i<msg.Length;i++)listBox1.Items.Add(msg[i]);
            listBox1.SetSelected(0, true);
            timer1.Enabled = true;
            button2.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }
        //タイマー
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (camera) {
                cap.Read(img);
                res = img.Clone();
            }
            if (!picture&&!camera) return;
            sentaku();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void PictureBoxIpl1_Click(object sender, EventArgs e)
        {
            
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        //画像読み込み
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Jpg 画像|*.jpg|Bmp 画像|*.bmp|Png 画像|*.png ";
            file.FilterIndex = 0;
            file.RestoreDirectory = true;
            file.CheckPathExists = true;
            file.ShowDialog();
            if (file.FileName != string.Empty)
            {
                try
                {
                    img=Cv2.ImRead(file.FileName); 
                    res = img.Clone();
                    picture = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            button2.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = true;
            button5.Enabled = true;
        }
        //カメラを開く
        private void Button1_Click(object sender, EventArgs e)
        {
            camera = true;
            button2.Enabled = false;
            button1.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = false;
        }
        //カメラを閉じる
        private void button3_Click(object sender, EventArgs e)
        {
            camera = false;
            pictureBoxIpl1.ImageIpl = null;
            button2.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }
        //画像保存
        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog();
            file.Filter = "Jpg 画像|*.jpg|Bmp 画像|*.bmp|Png 画像|*.png ";
            file.FilterIndex = 0;
            file.RestoreDirectory = true;
            file.CheckPathExists = true;
            file.FileName = System.DateTime.Now.ToString("yyyyMMddHHmmss");//保存ファイル名
            if (file.ShowDialog() == DialogResult.OK)
            {
                pictureBoxIpl1.Image.Save(file.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);//保存
                MessageBox.Show(this, "保存完了！", "メッセージ");
            }
        }
        //リセット
        private void button5_Click(object sender, EventArgs e)
        {
            picture = false;
            pictureBoxIpl1.ImageIpl = null;
            button2.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }
        private void sentaku()
        {
            String mode = listBox1.Text.Trim().ToString();
            switch (mode)
            {
                case "Gray":
                    Gray();
                    break;
                case "Otsu":
                    Otsu();
                    break;
                case "Binary":
                    Binary();
                    break;
                case "GaussianBlur":
                    GaussianBlur();
                    break;
                case "MedianBlur":
                    MedianBlur();
                    break;
                case "Canny":
                    Canny();
                    break;
                case "Kao":
                    Kao();
                    break;
                default:
                    res = img.Clone();
                    break;
            }
            
            pictureBoxIpl1.ImageIpl = res;
        }
        //gray
        private void Gray()
        {
            Byte[] lut = new Byte[256];
            for (int i = 0; i < 256; i++)
            { //ルックアップテーブルの設定                
                lut[i] = (Byte)(i >= 128 ? 255 : 0); //入力が128以上なら出力を255に、未 満なら0に   
            }
            Cv2.CvtColor(img, res, ColorConversionCodes.RGB2GRAY);
        }
        //二値化（大津の手法） 
        private void Otsu()
        {
            Gray();
            dst = res.Clone();
            Cv2.Threshold(dst, res, 128, 255, ThresholdTypes.Otsu);
        }
        //適応的二値化
        private void Binary()
        {
            Gray();
            dst = res.Clone();
            Cv2.AdaptiveThreshold(dst, res, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 9, 12);
        }
        //Gaussian フィルタ 
        private void GaussianBlur()
        {
            OpenCvSharp.Size ksize = new OpenCvSharp.Size(9, 9); //フィルタサイズの指定 
            Cv2.GaussianBlur(img, res, ksize, 0);
        }
        //Median フィルタ 
        private void MedianBlur()
        {
            Cv2.MedianBlur(img, res, 15);
        }
        //Canny 
        private void Canny()
        {
            Gray();
            dst = res.Clone();
            Cv2.Canny(dst, res, 100, 200);
        }
        //顔認識
        private void Kao()
        {
            res = img.Clone();
            Mat gray = new Mat(img.Rows, img.Cols, MatType.CV_8UC1);
            Cv2.CvtColor(img, gray, ColorConversionCodes.RGB2GRAY);
            Cv2.EqualizeHist(gray, gray);
            Rect[] faces = cascade.DetectMultiScale(gray);
            for (int i = 0; i < faces.Length; i++)
            {
                Cv2.Rectangle(res,
                    new OpenCvSharp.Point(faces[i].X, faces[i].Y),
                     new OpenCvSharp.Point(faces[i].X + faces[i].Width, faces[i].Y + faces[i].Height),
                      new Scalar(0, 200, 0)
                      );
            }
        }
    }    
}

