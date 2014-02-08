using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Microsoft.Kinect;

namespace doraemon
{
    public class Thing
    {
        public string name;
        public int[] position;
        public Thing(string name)
        {
            this.name = name;
        }
        public int[] fn(ColorImageFrame frame)
        {
            int[] position = new int[2];
            return position;
        }
    }
}
