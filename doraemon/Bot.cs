using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
//using Emgu.CV;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Structure;
//using Emgu.CV.UI;
//using Emgu.CV.Util;
using System.IO.Ports;

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    public class Bot
    {
        //private string orientation;
        //private int[] position;
        //private Image<Bgr, Byte> backTemplate = new Image<Bgr, Byte>("");
        //private Image<Bgr, Byte> frontTemplate = new Image<Bgr, Byte>("");
        //private Image<Bgr, Byte> leftTemplate = new Image<Bgr, Byte>("");
        //private Image<Bgr, Byte> rightTemplate = new Image<Bgr, Byte>("");
        ////public int[] getPosition(ColorImageFrame frame)
        //{
        //    this.orientation = getOrientation(frame);
        //    return this.position;
        //}

        //public string getOrientation(ColorImageFrame frame)
        //{
        //    Image<Bgr, Byte> img = ImageProc.colorFrameToImage(frame);
        //    int[] posBack = ImageProc.matchImages(img, backTemplate);
        //    int[] posRight = ImageProc.matchImages(img, rightTemplate);
        //    int[] posLeft = ImageProc.matchImages(img, leftTemplate);
        //    int[] posFront = ImageProc.matchImages(img, frontTemplate);
        //    if (posBack != null)
        //    {
        //        this.orientation = "back";
        //        this.position = posBack;
        //        return this.orientation;
        //    }

        //    else if (posRight != null)
        //    {
        //        this.orientation = "right";
        //        this.position = posRight;
        //        return this.orientation;
        //    }

        //    else if (posLeft != null)
        //    {
        //        this.orientation = "left";
        //        this.position = posLeft;
        //        return this.orientation;
        //    }

        //    else if (posFront != null)
        //    {
        //        this.orientation = "front";
        //        this.position = posFront;
        //        return this.orientation;
        //    }

        //    else
        //    {
        //        return null;
        //    }
        //}

        public static string state = "straight";
        public static string port = "COM24";

        public static void writeSerial(string port, string data)
        {
            SerialPort portArd = new SerialPort(port, 9600);
            portArd.Open();
            portArd.Write(data);
            portArd.Close();
        }

        //public void setOrientation()
        //{
        //    // Set the orientation to any of these ['front', 'left', 'right', 'back']
        //}

        public static void traverse()
        {
            // Move the bot in forward direction
            writeSerial(port, "F");
            state = "straight";
        }

        public static void back()
        {
            // Move Back
            writeSerial(port, "B");
            state = "straight";
        }

        public static void turnRight()
        {
            // Turn the bot right
            writeSerial(port, "R");
            System.Threading.Thread.Sleep(1700);
            stop();
            state = "right";
        }

        public static void turnLeft()
        {
            // Turn the bot left
            writeSerial(port, "L");
            System.Threading.Thread.Sleep(1700);
            stop();
            state = "left";
        }

        protected static void align()
        {
            // Aligns the bot facing back to Kinect
            if (state == "right")
            {
                turnLeft();
            }
            if (state == "left")
            {
                turnRight();
            }
        }

        public static void stop()
        {
            // Stops the bot
            writeSerial(port, "S");
        }
        

        public static int moveDoraemon(int doraX, int destinationX, int doraZ, int destinationZ)
        {
            if (((doraZ - destinationZ) > 50) || ((destinationZ - doraZ) > 50))
            {
                if ((doraZ - destinationZ) > 50)
                {
                    align();
                    back();
                    return 1;
                }
                else
                {
                    align();
                    traverse();
                    return 1;
                }
            }
            else
            {
                if (((doraX - destinationX) > 50) || ((destinationX - doraX) > 50))
                {
                    if ((doraX - destinationX) > 50)
                    {
                        turnLeft();
                        return 1;
                    }
                    else
                    {
                        turnRight();
                        traverse();
                        return 1;
                    }
                }
                else
                {
                    align();
                    return 0;
                }
            }

        }
    }
}