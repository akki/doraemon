using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Microsoft.Kinect;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.IO;

namespace doraemon
{
    public static class ImageProc
    {
        public static int[] matchImages(Image<Bgr, Byte> img, Image<Bgr, Byte> tpl)
        {
            return null;
        }

        public static Image<Bgr, Byte> colorFrameToImage(ColorImageFrame frame)
        {
            byte[] pixels = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(pixels);
            int stride = frame.Width * 4;
            BitmapSource bitmapSource = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            
            MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapSource));
            enc.Save(outStream);
            
            Bitmap bitmap = new Bitmap(outStream);
            Image<Bgr, Byte> cvImg = new Image<Bgr, Byte>(bitmap);
            return cvImg;
        }
    }
}
