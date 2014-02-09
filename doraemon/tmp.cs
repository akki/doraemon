using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.GPU;

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    class tmp
    {

        public static int[] matchColor(Image<Bgr, Byte> img)
        {
            int[] pos = new int[2];

            Image<Hsv, Byte> hsvimg = img.Convert<Hsv, Byte>();
            Image<Gray, Byte>[] channels = hsvimg.Split();
            Image<Gray, Byte> imghue = channels[0];
            Image<Gray, Byte> imgsat = channels[1];
            Image<Gray, Byte> imgval = channels[2];
            Hsv lowerLimit = new Hsv(60, 100, 100);
            Hsv upperLimit = new Hsv(130, 255, 255);

            Image<Gray, byte> imageHSVDest = hsvimg.InRange(lowerLimit, upperLimit);
            Image<Gray, byte> res = imageHSVDest.Dilate(2);
            //return res;
            MemStorage stor = new MemStorage();
            Contour<Point> contours = res.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_TREE, stor);
            while (contours != null)
            {
                Rectangle rect = contours.BoundingRectangle;
                pos[0] = rect.X;
                pos[1] = rect.Y;
                return pos;
                rect.Height = (rect.Height + 20);
                rect.Width = (rect.Width + 20);

                res.Draw(rect, new Gray(100), 2);
                contours = contours.HNext;
            }
            return null;
        }

        public static Image<Bgr, Byte> Draw(Image<Gray, Byte> modelImage, Image<Gray, byte> observedImage)
        {
            HomographyMatrix homography = null;

            FastDetector fastCPU = new FastDetector(10, true);
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            Matrix<int> indices;

            Freak descriptor = new Freak(true, true, 22.0f, 4);

            Matrix<byte> mask;
            int k = 2;
            double uniquenessThreshold = 0.8;

            //extract features from the object image
            modelKeyPoints = fastCPU.DetectKeyPointsRaw(modelImage, null);
            Matrix<Byte> modelDescriptors = descriptor.ComputeDescriptorsRaw(modelImage, null, modelKeyPoints);

            // extract features from the observed image
            observedKeyPoints = fastCPU.DetectKeyPointsRaw(observedImage, null);
            Matrix<Byte> observedDescriptors = descriptor.ComputeDescriptorsRaw(observedImage, null, observedKeyPoints);
            BruteForceMatcher<Byte> matcher = new BruteForceMatcher<Byte>(DistanceType.L2);
            matcher.Add(modelDescriptors);

            indices = new Matrix<int>(observedDescriptors.Rows, k);
            using (Matrix<float> dist = new Matrix<float>(observedDescriptors.Rows, k))
            {
                matcher.KnnMatch(observedDescriptors, indices, dist, k, null);
                mask = new Matrix<byte>(dist.Rows, 1);
                mask.SetValue(255);
                Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
            }

            int nonZeroCount = CvInvoke.cvCountNonZero(mask);
            if (nonZeroCount >= 4)
            {
                nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, indices, mask, 1.5, 20);
                if (nonZeroCount >= 4)
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(
                    modelKeyPoints, observedKeyPoints, indices, mask, 2);
            }

            //Draw the matched keypoints
            Image<Bgr, Byte> result = Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
               indices, new Bgr(255, 255, 255), new Bgr(255, 255, 255), mask, Features2DToolbox.KeypointDrawType.DEFAULT);

            #region draw the projected region on the image
            if (homography != null)
            {  //draw a rectangle along the projected model
                Rectangle rect = modelImage.ROI;
                PointF[] pts = new PointF[] { 
         new PointF(rect.Left, rect.Bottom),
         new PointF(rect.Right, rect.Bottom),
         new PointF(rect.Right, rect.Top),
         new PointF(rect.Left, rect.Top)};
                homography.ProjectPoints(pts);

                result.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Bgr(Color.Red), 5);
            }
            #endregion

            return result;
        }
    }
}
