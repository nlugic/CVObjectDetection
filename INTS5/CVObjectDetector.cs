using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace INTS5
{
    public class CVObjectDetector
    {
        #region Properties

        private Feature2D featureDetector,  featureDescriptor;
        private DescriptorMatcher featureMatcher;

        private Size modelImageSize;
        private VectorOfKeyPoint frameKeyPoints, modelKeyPoints;
        private UMat frameDescriptors, modelDescriptors;

        #endregion

        #region Constructors

        public CVObjectDetector()
        {
            // Brisk, KAZE, AKAZE, ORBDetector, HarrisLaplaceFeatureDetector, FastDetector
            featureDetector = new Brisk();
            // Brisk, ORBDetector, BriefDescriptorExtractor, Freak
            featureDescriptor = new Brisk();
            featureMatcher = new BFMatcher(DistanceType.L2);
        }

        #endregion

        #region Methods

        private void FindFeatures(Mat image, out VectorOfKeyPoint keypoints, out UMat descriptors)
        {
            keypoints = new VectorOfKeyPoint();
            descriptors = new UMat();

            using (UMat uImage = image.GetUMat(AccessType.Read))
            {
                featureDetector.DetectRaw(uImage, keypoints);
                featureDescriptor.Compute(uImage, keypoints, descriptors);
            }
        }

        public void AddModelImage(Mat modelImage)
        {
            modelKeyPoints?.Dispose();
            modelDescriptors?.Dispose();

            modelImageSize = modelImage.Size;
            FindFeatures(modelImage, out modelKeyPoints, out modelDescriptors);

            featureMatcher.Clear();
            featureMatcher.Add(modelDescriptors);
        }

        private void MatchKeypoints(Mat frame, out Mat mask, out Mat homography)
        {
            homography = null;
            FindFeatures(frame, out frameKeyPoints, out frameDescriptors);

            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            featureMatcher.KnnMatch(frameDescriptors, matches, 2, null);

            mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(255.0));
			
            Features2DToolbox.VoteForUniqueness(matches, 0.8, mask);
			homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
				frameKeyPoints, matches, mask, 2);

            featureMatcher.Clear();
            featureMatcher.Add(modelDescriptors);
        }

        public Mat MarkMatch(Mat frame)
        {
            if (modelKeyPoints != null)
            {
                Mat mask, homography;
                MatchKeypoints(frame, out mask, out homography);

                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, modelImageSize);

                    PointF[] fPoints = new PointF[]
                    {
                        new PointF(rect.Left, rect.Bottom),
                        new PointF(rect.Right, rect.Bottom),
                        new PointF(rect.Right, rect.Top),
                        new PointF(rect.Left, rect.Top)
                    };

                    fPoints = CvInvoke.PerspectiveTransform(fPoints, homography);

                    Point[] points = Array.ConvertAll(fPoints, Point.Round);
                    using (VectorOfPoint vPoints = new VectorOfPoint(points))
                        CvInvoke.Polylines(frame, vPoints, true, new MCvScalar(50.0, 170.0, 20.0, 255.0), 3);
                }
            }

            return frame;
        }

        public Mat MarkEdges(Mat frame)
        {
            Image<Bgr, byte> imgFrame = frame.ToImage<Bgr, byte>();

            using (Image<Gray, byte> grayFrame = frame.ToImage<Gray, byte>())
            {
                CvInvoke.GaussianBlur(grayFrame, grayFrame, new Size(9, 9), 0.0);
                CvInvoke.Canny(grayFrame, grayFrame, 40, 75);

                LineSegment2D[] detectedLines = grayFrame.HoughLinesBinary(1, Math.PI / 180.0, 45, 50.0, 1.0)[0];
                foreach (LineSegment2D line in detectedLines)
                    imgFrame.Draw(line, new Bgr(20.0, 50.0, 170.0), 3);
            }

            return imgFrame.Mat;
        }

        #endregion
    }
}
