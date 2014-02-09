using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    public class Thing
    {
        public string name;
        public int[] position;
        private string path = ""; // Set the image path here
        private Image<Bgr, Byte> template;
        public Thing(string name)
        {
            this.name = name;
            this.template = new Image<Bgr, Byte>(this.path + this.name);
        }

        public void findCoordinate(ColorImageFrame frame)
        {
            int[] position = new int[2];
            Image<Bgr, Byte> img = ImageProc.colorFrameToImage(frame);
            position = ImageProc.matchImages(img, this.template);
            this.position = position;
        }
    }
}
