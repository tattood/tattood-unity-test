using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System.IO;
using UnityEngine.UI;
using OpenCvSharp.CPlusPlus;

public class ImageProcessing : MonoBehaviour
{
    public Image IPElement;
    public GameObject a;
    // Use this for initialization
    public static void IP(GameObject model)
    {
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
        CvMat midCol = sourceImage.GetCol(sourceImage.Width / 2);

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

    public static byte[] Filter(byte[] data)
    {
        double[] increase = {
            0.26726152,    1.26844732,    2.27254198,    3.27951485,    4.2893353,
            5.3019727,     6.3173964,     7.33557578,    8.35648019,    9.38007899,
            10.40634155,   11.43523724,   12.46673541,   13.50080544,   14.53741667,
            15.57653849,   16.61814024,   17.66219129,   18.70866102,   19.75751877,
            20.80873391,   21.86227581,   22.91811383,   23.97621734,   25.03655568,
            26.09909824,   27.16381437,   28.23067344,   29.2996448,    30.37069783,
            31.44380189,   32.51892633,   33.59604052,   34.67511384,   35.75611562,
            36.83901526,   37.92378209,   39.0103855,    40.09879484,   41.18897947,
            42.28090876,   43.37455208,   44.46987878,   45.56685823,   46.66545979,
            47.76565282,   48.86740669,   49.97069077,   51.07547441,   52.18172698,
            53.28941784,   54.39851635,   55.50899188,   56.6208138,    57.73395146,
            58.84837422,   59.96405146,   61.08095254,   62.19904681,   63.31830364,
            64.43869239,   65.56018244,   66.68274313,   67.80634384,   68.93095393,
            70.05654276,   71.18307969,   72.31053409,   73.43887532,   74.56807274,
            75.69809573,   76.82891363,   77.96049582,   79.09281165,   80.22583049,
            81.35952171,   82.49385467,   83.62879872,   84.76432324,   85.90039759,
            87.03699112,   88.17407321,   89.31161322,   90.44958051,   91.58794444,
            92.72667437,   93.86573968,   95.00510972,   96.14475386,   97.28464146,
            98.42474188,   99.56502448,  100.70545864,  101.84601371,  102.98665905,
            104.12736404,  105.26809803,  106.40883038,  107.54953046,  108.69016764,
            109.83071127,  110.97113073,  112.11139536,  113.25147454,  114.39133763,
            115.53095399,  116.67029299,  117.80932399,  118.94801635,  120.08633943,
            121.22426261,  122.36175523,  123.49878667,  124.6353263,   125.77134346,
            126.90680753,  128.04168786,  129.17595383,  130.3095748,   131.44252012,
            132.57475916,  133.70626129,  134.83699587,  135.96693226,  137.09603983,
            138.22428793,  139.35164594,  140.47808321,  141.6035691,   142.72806795,
            143.85152387,  144.97387594,  146.09506327,  147.21502491,  148.33369997,
            149.45102753,  150.56694666,  151.68139646,  152.79431601,  153.90564438,
            155.01532068,  156.12328397,  157.22947334,  158.33382788,  159.43628667,
            160.5367888,   161.63527335,  162.7316794,   163.82594603,  164.91801234,
            166.0078174,   167.09530031,  168.18040013,  169.26305596,  170.34320688,
            171.42079198,  172.49575034,  173.56802103,  174.63754316,  175.7042558,
            176.76809803,  177.82900894,  178.88692761,  179.94179313,  180.99354458,
            182.04212104,  183.0874616,   184.12950535,  185.16819136,  186.20345872,
            187.23524652,  188.26349383,  189.28813975,  190.30912335,  191.32638373,
            192.33985996,  193.34949112,  194.35521631,  195.35697461,  196.35470509,
            197.34834685,  198.33783897,  199.32312053,  200.30413062,  201.28080832,
            202.25309271,  203.22092288,  204.18423791,  205.14297688,  206.09707889,
            207.04648301,  207.99112833,  208.93095393,  209.8658989,   210.79590231,
            211.72090326,  212.64084083,  213.5556541,   214.46528216,  215.36966408,
            216.26873896,  217.16244588,  218.05072392,  218.93351216,  219.8107497,
            220.6823756,   221.54832897,  222.40854888,  223.26297441,  224.11154465,
            224.95419869,  225.7908756,   226.62151447,  227.44605439,  228.26443444,
            229.0765937,   229.88247126,  230.68200621,  231.47513761,  232.26180457,
            233.04194616,  233.81550146,  234.58240957,  235.34260956,  236.09604052,
            236.84264154,  237.58235169,  238.31511006,  239.04085574,  239.75952781,
            240.47106535,  241.17540745,  241.87249318,  242.56226165,  243.24465192,
            243.91960308,  244.58705423,  245.24694443,  245.89921277,  246.54379835,
            247.18064024,  247.80967752,  248.43084929,  249.04409462,  249.6493526,
            250.24656231,  250.83566284,  251.41659327,  251.98929269,  252.55370017,
            253.1097548,   253.65739567,  254.19656187,  254.72719246,  255.24922655,
            255.7626032
        };

        double[] decrease = {
            -2.67228320e-1,  -7.33877551e-2,   1.32520998e-1,   3.50368352e-1,
            5.80024720e-1,   8.21360515e-1,   1.07424615e0,   1.33855203e0,
            1.61414859e0,   1.90090621e0,   2.19869533e0,   2.50738636e0,
            2.82684969e0,   3.15695576e0,   3.49757497e0,   3.84857773e0,
            4.20983446e0,   4.58121557e0,   4.96259148e0,   5.35383259e0,
            5.75480931e0,   6.16539207e0,   6.58545127e0,   7.01485733e0,
            7.45348066e0,   7.90119167e0,   8.35786077e0,   8.82335838e0,
            9.29755491e0,   9.78032078e0,   1.02715264e1,   1.07710422e1,
            1.12787385e1,   1.17944858e1,   1.23181546e1,   1.28496151e1,
            1.33887378e1,   1.39353932e1,   1.44894517e1,   1.50507836e1,
            1.56192593e1,   1.61947494e1,   1.67771242e1,   1.73662540e1,
            1.79620094e1,   1.85642608e1,   1.91728785e1,   1.97877329e1,
            2.04086945e1,   2.10356337e1,   2.16684210e1,   2.23069266e1,
            2.29510210e1,   2.36005747e1,   2.42554581e1,   2.49155415e1,
            2.55806954e1,   2.62507902e1,   2.69256963e1,   2.76052841e1,
            2.82894240e1,   2.89779865e1,   2.96708419e1,   3.03678607e1,
            3.10689133e1,   3.17738701e1,   3.24826014e1,   3.31949778e1,
            3.39108696e1,   3.46301473e1,   3.53526812e1,   3.60783418e1,
            3.68069995e1,   3.75385247e1,   3.82727878e1,   3.90096592e1,
            3.97490093e1,   4.04907086e1,   4.12346275e1,   4.19806363e1,
            4.27286055e1,   4.34784055e1,   4.42299067e1,   4.49829795e1,
            4.57374944e1,   4.64933217e1,   4.72503319e1,   4.80083953e1,
            4.87673825e1,   4.95271637e1,   5.02876095e1,   5.10485901e1,
            5.18099762e1,   5.25716379e1,   5.33334459e1,   5.40952704e1,
            5.48569819e1,   5.56184508e1,   5.63795475e1,   5.71401424e1,
            5.79001060e1,   5.86593086e1,   5.94176207e1,   6.01749127e1,
            6.09310550e1,   6.16859179e1,   6.24393720e1,   6.31912877e1,
            6.39415352e1,   6.46899851e1,   6.54365078e1,   6.61809737e1,
            6.69232531e1,   6.76632166e1,   6.84007345e1,   6.91356772e1,
            6.98679152e1,   7.05973188e1,   7.13237585e1,   7.20471047e1,
            7.27672277e1,   7.34839981e1,   7.41972862e1,   7.49069625e1,
            7.56128972e1,   7.63149610e1,   7.70130241e1,   7.77069570e1,
            7.83966301e1,   7.90819646e1,   7.97630851e1,   8.04401670e1,
            8.11133855e1,   8.17829161e1,   8.24489342e1,   8.31116150e1,
            8.37711340e1,   8.44276665e1,   8.50813879e1,   8.57324735e1,
            8.63810988e1,   8.70274390e1,   8.76716696e1,   8.83139658e1,
            8.89545031e1,   8.95934569e1,   9.02310024e1,   9.08673151e1,
            9.15025704e1,   9.21369435e1,   9.27706098e1,   9.34037448e1,
            9.40365237e1,   9.46691220e1,   9.53017150e1,   9.59344780e1,
            9.65675865e1,   9.72012157e1,   9.78355411e1,   9.84707380e1,
            9.91069819e1,   9.97444479e1,   1.00383312e2,   1.01023748e2,
            1.01665933e2,   1.02310042e2,   1.02956250e2,   1.03604732e2,
            1.04255664e2,   1.04909221e2,   1.05565578e2,   1.06224912e2,
            1.06887397e2,   1.07553208e2,   1.08222521e2,   1.08895512e2,
            1.09572355e2,   1.10253227e2,   1.10938302e2,   1.11627755e2,
            1.12321763e2,   1.13020501e2,   1.13724143e2,   1.14432865e2,
            1.15146843e2,   1.15866252e2,   1.16591268e2,   1.17322065e2,
            1.18058819e2,   1.18801705e2,   1.19550900e2,   1.20306577e2,
            1.21068913e2,   1.21838083e2,   1.22614262e2,   1.23397626e2,
            1.24188350e2,   1.24986609e2,   1.25792579e2,   1.26606434e2,
            1.27428352e2,   1.28258506e2,   1.29097072e2,   1.29944226e2,
            1.30800142e2,   1.31664997e2,   1.32538965e2,   1.33422223e2,
            1.34314945e2,   1.35217306e2,   1.36129482e2,   1.37051649e2,
            1.37983982e2,   1.38926656e2,   1.39879846e2,   1.40843729e2,
            1.41818478e2,   1.42804270e2,   1.43801280e2,   1.44809684e2,
            1.45829656e2,   1.46861372e2,   1.47905008e2,   1.48960738e2,
            1.50028739e2,   1.51109185e2,   1.52202251e2,   1.53308115e2,
            1.54426949e2,   1.55558931e2,   1.56704235e2,   1.57863037e2,
            1.59035512e2,   1.60221835e2,   1.61422182e2,   1.62636728e2,
            1.63865649e2,   1.65109120e2,   1.66367316e2,   1.67640412e2,
            1.68928584e2,   1.70232008e2,   1.71550859e2,   1.72885311e2,
            1.74235541e2,   1.75601724e2,   1.76984035e2,   1.78382649e2,
            1.79797742e2,   1.81229490e2,   1.82678067e2,   1.84143649e2,
            1.85626411e2,   1.87126530e2,   1.88644179e2,   1.90179534e2
        };

        Mat mat = Cv2.ImDecode(data, LoadMode.Color);
        //Cv2.ImShow("baban", mat);

        //cv::Mat mat = cv::imread(path);

        // cv::imshow("original", mat);
        //Cv2.ImShow("original", mat);

        //cv::Mat dst = mat.clone();
        Mat dst = mat.Clone();

        //cv::GaussianBlur(mat, dst, cv::Size(3, 3), 0);
        Cv2.GaussianBlur(mat, dst, new Size(3, 3), 0);

        // cv::imshow("gaussian", dst);
        //Cv2.ImShow("Gaussian", dst);

        //std::vector<cv::Mat> rgb;
        //cv::split(dst, rgb);

        Mat[] rgb;
        Cv2.Split(dst, out rgb);

        //cv::LUT(rgb[2], cv::InputArray(decrease), rgb[2]);
        //cv::LUT(rgb[0], cv::InputArray(increase), rgb[0]);
        //cv::merge(rgb, dst);
        //cv::cvtColor(dst, dst, cv::COLOR_BGR2HSV);
        //cv::split(rgb, dst);
        //cv::LUT(rgb[1], cv::InputArray(decrease), rgb[1]);
        //cv::merge(rgb, dst);
        //cv::cvtColor(dst, dst, cv::COLOR_HSV2BGR);
        //cv::imwrite("asd.bmp", dst);

        //def render(self, img_rgb):
        //    c_r, c_g, c_b = cv2.split(img_rgb)
        //    c_r = cv2.LUT(c_r, self.decr_ch_lut).astype(np.uint8)
        //    c_b = cv2.LUT(c_b, self.incr_ch_lut).astype(np.uint8)
        //    img_rgb = cv2.merge((c_r, c_g, c_b))

        //    # decrease color saturation
        //    c_h, c_s, c_v = cv2.split(cv2.cvtColor(img_rgb, cv2.COLOR_RGB2HSV))
        //    c_s = cv2.LUT(c_s, self.decr_ch_lut).astype(np.uint8)
        //    return cv2.cvtColor(cv2.merge((c_h, c_s, c_v)), cv2.COLOR_HSV2RGB)

        //def _create_LUT_8UC1(self, x, y):
        //    spl = UnivariateSpline(x, y)
        //    return spl(range(256))

        Cv2.LUT(rgb[2], InputArray.Create(increase), rgb[2]);
        Cv2.LUT(rgb[0], InputArray.Create(decrease), rgb[0]);

        rgb[2].ConvertTo(rgb[2], MatType.CV_8UC3);
        rgb[0].ConvertTo(rgb[0], MatType.CV_8UC3);

        print(rgb[0].Size().ToString() + " " + rgb[1].Size().ToString() + " " + rgb[2].Size().ToString() + " " + mat.Size().ToString());
        print(rgb[0].Depth() + " " + rgb[1].Depth() + " " + rgb[2].Depth() + " " + dst.Depth() + " " + mat.Depth());

        Cv2.Merge(rgb, dst);

        Cv2.CvtColor(dst, dst, ColorConversion.BgrToHsv);
        Cv2.Split(dst, out rgb);

        Cv2.LUT(rgb[1], InputArray.Create(increase), rgb[1]);
        rgb[1].ConvertTo(rgb[1], MatType.CV_8UC3);

        Cv2.Merge(rgb, dst);
        Cv2.CvtColor(dst, dst, ColorConversion.HsvToBgr);
        //Cv2.ImShow("heleloy", dst);
        return dst.ToBytes();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
