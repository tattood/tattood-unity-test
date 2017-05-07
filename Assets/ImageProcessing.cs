using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System.IO;
using UnityEngine.UI;
using OpenCvSharp.CPlusPlus;

public class ImageProcessing : MonoBehaviour {
    public Image IPElement;
    public GameObject a;
    // Use this for initialization
    public static void IP(GameObject model) {
        Application.CaptureScreenshot(Application.persistentDataPath + "/" + "inputImage.png", 2);
        var fileName = Application.persistentDataPath + "/" + "inputImage.png";

        var imageBytes = File.ReadAllBytes(fileName);
        var imageTexture = new Texture2D(Screen.width, Screen.height);
        imageTexture.LoadImage(imageBytes);
        //IPElement.sprite = Sprite.Create(imageTexture, new Rect(0,0, Screen.width, Screen.height), new Vector2());

        IplImage sourceImage = Cv.LoadImage(fileName, LoadMode.GrayScale);
        IplImage dst = Cv.CreateImage(new CvSize(sourceImage.Width, sourceImage.Height), BitDepth.U8, 1);
        IplImage thresholdDst = Cv.CreateImage(new CvSize(sourceImage.Width, sourceImage.Height), BitDepth.U8, 1);
        //Cv.ShowImage("source image", sourceImage);
        CvMat midCol = sourceImage.GetCol(sourceImage.Width/2);

        float avg = 0;
        float sum = 0;
        for (int i = 1; i < 5; i++)
        {
             sum += (float)midCol[sourceImage.Height / 2 + i];
        }
        avg = sum / 4;

        Cv.Threshold(sourceImage, thresholdDst, avg - 10, 255, ThresholdType.ToZero);
        Cv.Threshold(thresholdDst, thresholdDst, avg + 10, 255, ThresholdType.ToZeroInv);

        for (int i = 0; i < 50; i++)
        {
            //Cv.Dilate(thresholdDst, thresholdDst);
            Cv.Dilate(thresholdDst, thresholdDst);
        }
        for (int i = 0; i < 50; i++)
        {
            //Cv.Dilate(thresholdDst, thresholdDst);
            Cv.Erode(thresholdDst, thresholdDst);
        }
        //IplImage gradDst = Cv.CreateImage(new CvSize(sourceImage.Width, sourceImage.Height), BitDepth.U8, 1);
        //Cv.Sobel(thresholdDst, gradDst,  1, 1);
        //Cv.ShowImage("gradient image ", gradDst);

        //CvMat avgValues = new CvMat(dst.Size.Height, dst.Size.Width, MatrixType.U8C1);
        //CvMat eigenValues = new CvMat(dst.Size.Height, dst.Size.Width, MatrixType.U8C1);
        //CvMat eigenVectors = new CvMat(dst.Size.Height, dst.Size.Width, MatrixType.U8C1);
        //gradDst.EigenVV(eigenVectors, eigenValues);
        //Debug.Log(eigenVectors);

        //Cv.CalcPCA(gradDst, avg, eigenvalues, eigenvectors ,PCAFlag.UseAvg);

        //Cv.InRange(sourceImage, 150f, 250f, thresholdDst);

        //Cv.ShowImage("thresholded image ", thresholdDst);

        Cv.Canny(thresholdDst, dst, 200, 250);

        //Cv.ShowImage("edges", dst);


        IplImage output = new IplImage(dst.GetSize(), BitDepth.U8, 3);//image.Depth, 1);
        output.Set(CvColor.Black);
        CvSeq<CvPoint> contoursRaw;
        CvMemStorage storage = new CvMemStorage();
        Cv.FindContours(dst, storage, out contoursRaw);


        IplImage outputRGB = Cv.CreateImage(new CvSize(dst.Width, dst.Height), BitDepth.U8, 3);

        Cv.CvtColor(dst, outputRGB, ColorConversion.GrayToRgb);
        using (CvContourScanner scanner = new CvContourScanner(thresholdDst, storage, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple))
        {
            double max = double.MinValue;
            CvRect maxBoundingBox = new CvRect();

            foreach (CvSeq<CvPoint> c in scanner)
            {
                //Some contours are negative so make them all positive for easy comparison
                double area = Mathf.Abs((float)c.ContourArea());
                //Uncomment below to see the area of each contour
                //Console.WriteLine(area.ToString());
                if (area >= 0)
                {
                    List<CvPoint[]> points = new List<CvPoint[]>();
                    List<CvPoint> point = new List<CvPoint>();
                    foreach (CvPoint p in c.ToArray())
                        point.Add(p);

                    points.Add(point.ToArray());

                    //Use FillPoly instead of DrawContours as requested
                    outputRGB.FillPoly(points.ToArray(), CvColor.Red, LineType.AntiAlias);

                    //-1 means fill the polygon
                    //outputRGB.DrawContours(c, CvColor.White, CvColor.Green, 0, -1, LineType.AntiAlias);

                    //maxBoundingBox = Cv.BoundingRect(c);
                    //Cv.DrawRect(outputRGB, maxBoundingBox, CvColor.Green, 5);
                    //Cv.ShowImage("rect", outputRGB);

                    if (area > max)
                    {
                        maxBoundingBox = Cv.BoundingRect(c);
                        max = area;
                        contoursRaw = c;
                    }
                }
            }
            //Cv.DrawRect(outputRGB, maxBoundingBox, CvColor.Green, 5);
            //Cv.ShowImage("rect", outputRGB);
        }


        //Debug.Log("Heleloy" + contoursRaw.Total);

        CvBox2D rect = Cv.FitEllipse2(contoursRaw);


        print("Angle" + ((rect.Angle + 270) % 360));

        model.transform.Rotate(Vector3.forward * rect.Angle);


        //Cv.DrawContours(outputRGB, anan, CvColor.White, CvColor.Green, 0, -1, LineType.AntiAlias);

        //CvBox2D box = Cv.FitEllipse2(contoursRaw);
        //Debug.Log(box.Angle);
        CvMat edges = new CvMat(dst.Size.Height, dst.Size.Width, MatrixType.U8C1);
        Cv.Canny(sourceImage, edges, 200, 250);

        //Mat[] contours;
        //OutputArray hie = OutputArray.Create(new Mat());
        //Mat nm = new Mat(thresholdDst);


        //Cv2.FindContours(OutputArray.Create(nm),out contours, hie, ContourRetrieval.List, ContourChain.ApproxNone);

        //Debug.Log(" size:  "+ contoursRaw[0]);
        //Mat asd = new Mat(contoursRaw);
        //Mat mean = new Mat();
        //PCA pca_anaylsis = new PCA(asd, mean, PCAFlag.DataAsRow);
        //Debug.Log(pca_anaylsis.Mean.At<double>(0,0));
        //Point center = new Point(pca_anaylsis.Mean.At<double>(0, 0), pca_anaylsis.Mean.At<double>(0, 1));

        //Debug.Log("x: " + pca_anaylsis.Eigenvalues.At<double>(0, 0));
        //Debug.Log("y: " + pca_anaylsis.Eigenvalues.At<double>(0, 1));

        //List<Point2d> eigenVecs = new List<Point2d>() ;
        //List<double> eigenVals = new List<double>();

        //for (int i = 0; i < 2; i++)
        //{
        //    eigenVecs.Add(new Point2d(pca_anaylsis.Eigenvectors.At<double>(i, 0), pca_anaylsis.Eigenvectors.At<double>(i, 1)));
        //    eigenVals.Add(pca_anaylsis.Eigenvalues.At<double>(0, i));
        //}

        //Cv2.Circle(asd, center.X, center.Y, 3, new Scalar(255, 0, 255), 2);

        //Point p1 = center + new Point((int)(eigenVecs[0].X * eigenVals[0]) * 0.02, (int)(eigenVecs[0].Y * eigenVals[0]) * 0.02 ); 
        //Point p2 = center - new Point((int)(eigenVecs[1].X * eigenVals[1]) * 0.02, (int)(eigenVecs[1].Y * eigenVals[1]) * 0.02);

        //double angle = Mathf.Atan2((float)eigenVecs[0].Y , (float)eigenVecs[0].X);
        //angle = System.Math.Atan2(eigenVecs[0].Y, eigenVecs[0].X);
        //Debug.Log("Angle: " + angle);
        //float angle2 = Mathf.Atan2((float)eigenVecs[1].Y, (float)eigenVecs[1].X);

        //Debug.Log("Second Angle: " + angle2);


        //Debug.Log("x: " + pca_anaylsis.Eigenvectors.At<double>(0, 0));
        //Debug.Log("y: " + pca_anaylsis.Eigenvectors.At<double>(0, 1));

        CvMat edgesmidcol = edges.GetCol(edges.Width / 2);
        Debug.Log(edgesmidcol[200]);

        int count = 1;
        int index = edges.Height / 2;
        while (edgesmidcol[index] == edgesmidcol[index + 1])
        {
            count++;
            index++;
            if (index == edges.Height - 1)
            {
                break;
            }
        }
        if (count <= 100)
        {
            model.transform.localScale = Vector3.one * 1;
        }
        else if (count > 100 && count <= 200)
        {
            model.transform.localScale = Vector3.one * 1.5f;
        }
        else if (count > 200 && count <= 300)
        {
            model.transform.localScale = Vector3.one * 2;
        }
        else if (count > 300 && count <= 400)
        {
            model.transform.localScale = Vector3.one * 2.5f;
        }
        else
        {
            model.transform.localScale = Vector3.one * 3;
        }

        //z
        //Debug.Log(edges);

        //IplImage output = new IplImage(edges.GetSize(), BitDepth.U8, 3);//image.Depth, 1);
        //output.Set(CvColor.Black);
        //CvSeq<CvPoint> contoursRaw;
        //CvMemStorage storage = new CvMemStorage();
        //Cv.FindContours(dst, storage, out contoursRaw, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple);


        //IplImage outputRGB = Cv.CreateImage(new CvSize(sourceImage.Width, sourceImage.Height), BitDepth.U8, 3);
        //Cv.CvtColor(dst, outputRGB, ColorConversion.GrayToRgb);
        //using (CvContourScanner scanner = new CvContourScanner(edges, storage, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple))
        //{
        //    foreach (CvSeq<CvPoint> c in scanner)
        //    {
        //        //Some contours are negative so make them all positive for easy comparison
        //        double area = Mathf.Abs((float)c.ContourArea());
        //        //Uncomment below to see the area of each contour
        //        //Console.WriteLine(area.ToString());
        //        if (area >= 60)
        //        {
        //            CvRect boundingBox = Cv.BoundingRect(contoursRaw);
        //            Cv.DrawRect(outputRGB, boundingBox, CvColor.Green, 5);
        //            Debug.Log(boundingBox.Location);
        //            var distance = Mathf.Abs(boundingBox.Left - boundingBox.Right);
        //            Debug.Log(distance);
        //        }
        //    }
        //    Cv.ShowImage("rect", outputRGB);
        //}


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
