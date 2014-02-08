using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace doraemon
{
    public class Bot
    {
        private string orientation;
        private int[] position;
        private Image<Bgr, Byte> backTemplate = new Image<Bgr, Byte>("");
        private Image<Bgr, Byte> frontTemplate = new Image<Bgr, Byte>("");
        private Image<Bgr, Byte> leftTemplate = new Image<Bgr, Byte>("");
        private Image<Bgr, Byte> rightTemplate = new Image<Bgr, Byte>("");
        public int[] getPosition(ColorImageFrame frame)
        {
            this.orientation = getOrientation(frame);
            return this.position;
        }

        public string getOrientation(ColorImageFrame frame)
        {
            Image<Bgr, Byte> img = ImageProc.colorFrameToImage(frame);
            int[] posBack = ImageProc.matchImages(img, backTemplate);
            int[] posRight = ImageProc.matchImages(img, rightTemplate);
            int[] posLeft = ImageProc.matchImages(img, leftTemplate);
            int[] posFront = ImageProc.matchImages(img, frontTemplate);
            if ( posBack!= null)
            {
                this.orientation = "back";
                this.position = posBack;
                return this.orientation;
            }

            else if (posRight != null)
            {
                this.orientation = "right";
                this.position = posRight;
                return this.orientation;
            }

            else if (posLeft != null)
            {
                this.orientation = "left";
                this.position = posLeft;
                return this.orientation;
            }

            else if (posFront != null)
            {
                this.orientation = "front";
                this.position = posFront;
                return this.orientation;
            }

            else
            {
                return null;
            }
        }

        public void setOrientation()
        {
            // Set the orientation to any of these ['front', 'left', 'right', 'back']
        }

        protected void traverse()
        {
            // Move the bot in forward direction
        }

        protected void turnRight()
        {
            // Turn the bot right
        }

        protected void turnLeft()
        {
            // Turn the bot left
        }

        protected void align()
        {
            // Aligns the bot facing back to Kinect
        }

        protected void stop()
        {
            // Stops the bot
        }
    }
}
