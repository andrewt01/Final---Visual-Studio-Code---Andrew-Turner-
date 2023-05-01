using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Structure;

//make some functions to handle certain things so that the display webcam section is not overrun by many many lines 

//final project
namespace CSHW4_UR1
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Thread _captureThread;
        private int _threshold = 150;
        private robot robot; 

        //need some int for hsv ScalarArray that is being created later one down below 

        //min set at zero
        //yellow line 
        int hueMin, saturationMin, valueMin = 0;

        //max set at the max values for each hsv 
        //yellow line 
        int hueMax = 179;
        int saturationMax, valueMax = 255;

        //red line 
        int hueMinRed, satMinRed, valMinRed = 0;
        int hueMaxRed = 179;
        int satMaxRed, valMaxRed = 255;

        bool start = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(0); //0 is onboard camera, 1 is offboard camera 
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();

            robot = new robot("COM20"); //serial com port  
        }

        private void resizePictureBox(Mat originalFrame)
        {
            int newHeight = (originalFrame.Size.Height * originalPictureBox.Width) / originalFrame.Size.Width;
            Size newSize = new Size(originalPictureBox.Size.Width, newHeight);
            CvInvoke.Resize(originalFrame, originalFrame, newSize);

            originalPictureBox.Image = originalFrame.ToBitmap();
        }

        //resize yellow final image 
        private void resizeFinal_YellowImage(Mat mergedImage)
        {
            int hsvYellow_newHeight = (mergedImage.Size.Height * hsvPictureBoxMerged.Width) / mergedImage.Size.Width;
            Size hsvYellow_newSize = new Size(hsvPictureBoxMerged.Size.Width, hsvYellow_newHeight);
            CvInvoke.Resize(mergedImage, mergedImage, hsvYellow_newSize);

            hsvPictureBoxMerged.Image = mergedImage.ToBitmap();
        }

        //private void resizeFinal_RedImage(Mat mergedImage2)
        //{
        //    int hsvRed_newHeight = (mergedImage2.Size.Height * hsvMergedPictureBoxRed.Width) / mergedImage2.Size.Width;
        //    Size hsvRed_newSize = new Size(hsvMergedPictureBoxRed.Size.Width, hsvRed_newHeight);
        //    CvInvoke.Resize(mergedImage2, mergedImage2, hsvRed_newSize);

        //    hsvMergedPictureBoxRed.Image = mergedImage2.ToBitmap();
        //}

        //change value in the display webcame to 1 for the external camera 
        //need two separate hue, sat, value -- one to read the yellow and the other to read the red line 
        private void DisplayWebcam()
        {
            while (_capture.IsOpened)
            {
                //pixel counts - both yellow and red 
                int yellowPixelsMiddle = 0;
                int yellowPixelsLeft = 0;
                int yellowPixelsRight = 0;
                int yellowPixelsHardLeft = 0;
                int yellowPixelsHardRight = 0; 

                int redPixelsMiddle = 0;
                int redPixelsLeft = 0;
                int redPixelsRight = 0;
                int redPixelsHardLeft = 0;
                int redPixelsHardRight = 0;

                char serialChar = 's';

                //test
                //robot.Move('f');
                //Thread.Sleep(1000);
                //robot.Move('s');


                //* ------------------------- Image Processing and HSV Frames ----------------------------- *//

                //no threshold on originalPictureBox
                Mat originalFrame = _capture.QueryFrame();

                //resize the original picture now with function 
                resizePictureBox(originalFrame);

                //binary threshold portion for binaryPictureBox
                Mat grayFrame = new Mat();
                CvInvoke.CvtColor(originalFrame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayFrame, grayFrame, _threshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                binaryPictureBox.Image = grayFrame.ToBitmap();

                
                //HSV Threshold 1 -- yellow line 
                Mat hsvFrame = new Mat();
                CvInvoke.CvtColor(originalFrame, hsvFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                Mat[] hsvChannels = hsvFrame.Split();

                //hue part
                Mat hueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(hueMin), new ScalarArray(hueMax), hueFilter);
                Invoke(new Action(() => { huePictureBox.Image = hueFilter.ToBitmap(); }));

                //saturation part
                Mat saturationFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(saturationMin), new ScalarArray(saturationMax), saturationFilter);
                Invoke(new Action(() => { saturationPictureBox.Image = saturationFilter.ToBitmap(); }));

                //value part
                Mat valueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(valueMin), new ScalarArray(valueMax), valueFilter);
                Invoke(new Action(() => { valuePictureBox.Image = valueFilter.ToBitmap(); }));

                Mat mergedImage = new Mat();
                CvInvoke.BitwiseAnd(hueFilter, saturationFilter, mergedImage);
                CvInvoke.BitwiseAnd(mergedImage, valueFilter, mergedImage);
                Invoke(new Action(() => { hsvPictureBoxMerged.Image = mergedImage.ToBitmap(); }));

                //add hue, sat, and value2 for the redline 
                Mat hsvFrame2 = new Mat();
                CvInvoke.CvtColor(originalFrame, hsvFrame2, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                Mat[] hsvChannels2 = hsvFrame2.Split();

                //hue 2 
                Mat hueFilter2 = new Mat();
                CvInvoke.InRange(hsvChannels2[0], new ScalarArray(hueMinRed), new ScalarArray(hueMaxRed), hueFilter2);
                Invoke(new Action(() => { huePictureBox2.Image = hueFilter2.ToBitmap(); }));

                //sat 2 
                Mat saturationFilter2 = new Mat();
                CvInvoke.InRange(hsvChannels2[1], new ScalarArray(satMinRed), new ScalarArray(satMaxRed), saturationFilter2);
                Invoke(new Action(() => { satPictureBox2.Image = saturationFilter2.ToBitmap(); }));

                //val 2 
                Mat valueFilter2 = new Mat();
                CvInvoke.InRange(hsvChannels2[2], new ScalarArray(valMinRed), new ScalarArray(valMaxRed), valueFilter2);
                Invoke(new Action(() => { valPictureBox2.Image = valueFilter2.ToBitmap(); }));

                //merge image 2 

                Mat mergedImage2 = new Mat();
                CvInvoke.BitwiseAnd(hueFilter2, saturationFilter2, mergedImage2);
                CvInvoke.BitwiseAnd(mergedImage2, valueFilter2, mergedImage2);
                Invoke(new Action(() => { hsvMergedPictureBoxRed.Image = mergedImage2.ToBitmap(); }));


                //resize final images
                resizeFinal_YellowImage(mergedImage);
                //resizeFinal_RedImage(mergedImage2);

                //erode and dialate the image to make things smoother 
                Mat kernal = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(2, 2), new Point(1, 1));
                CvInvoke.Erode(mergedImage, mergedImage, kernal, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                CvInvoke.Dilate(mergedImage, mergedImage, kernal, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

                Mat kernal2 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(2, 2), new Point(1, 1));
                CvInvoke.Erode(mergedImage2, mergedImage2, kernal2, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                CvInvoke.Dilate(mergedImage2, mergedImage2, kernal2, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

                //gaussian blur 
                Mat gaussFrame = mergedImage.Clone();
                CvInvoke.GaussianBlur(gaussFrame, gaussFrame, new Size(5, 5), 1.5);

                Mat gaussFrame2 = mergedImage2.Clone();
                CvInvoke.GaussianBlur(gaussFrame2, gaussFrame2, new Size(5, 5), 1.5);


                //* -------------------------- Pixel Counts Red and Yellow ----------------------------- *// 

                Image<Gray, byte> image = mergedImage.ToImage<Gray, byte>();
                Image<Gray, byte> image2 = mergedImage2.ToImage<Gray, Byte>();

                //yellow 
                //hard left 
                for (int x = 0; x < mergedImage.Width / 5; x++)
                {
                    for (int y = 0; y < mergedImage.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsHardLeft++;
                        }
                    }
                }

                //soft left 
                for (int x = (mergedImage.Width / 5); x < (2 * (mergedImage.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImage.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsLeft++;
                        }

                    }
                }

                //middle 
                for (int x = (2 * (mergedImage.Width / 5)); x < (3 * (mergedImage.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImage.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsMiddle++;
                        }

                    }
                }

                //soft right 
                for (int x = (3 * (mergedImage.Width / 5)); x < (4 * (mergedImage.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImage.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsRight++;
                        }

                    }
                }

                //hard right 
                for (int x = ((mergedImage.Width / 5) * 4); x < ((mergedImage.Width / 5) * 5); x++)
                {
                    for (int y = 0; y < mergedImage.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsHardRight++;
                        }

                    }
                }

                Invoke(new Action(() =>
                {
                    yPixHLeft.Text = $"{yellowPixelsHardLeft}";
                    yPixLeft.Text = $"{yellowPixelsLeft}";
                    yPixMiddle.Text = $"{yellowPixelsMiddle}";
                    yPixRight.Text = $"{yellowPixelsRight}";
                    yPixHRight.Text = $"{yellowPixelsHardRight}";
                }));




                //red
                //hard left 
                for (int x = 0; x < mergedImage2.Width / 5; x++)
                {
                    for (int y = 0; y < mergedImage2.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsHardLeft++;
                        }
                    }
                }

                //soft left 
                for (int x = (mergedImage2.Width / 5); x < (2 * (mergedImage2.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImage2.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsLeft++;
                        }

                    }
                }

                //middle 
                for (int x = (2 * (mergedImage2.Width / 5)); x < (3 * (mergedImage2.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImage2.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsMiddle++;
                        }

                    }
                }

                //soft right 
                for (int x = (3 * (mergedImage2.Width / 5)); x < (4 * (mergedImage2.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImage2.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsRight++;
                        }

                    }
                }

                //hard right 
                for (int x = ((mergedImage2.Width / 5) * 4); x < ((mergedImage2.Width / 5) * 5); x++)
                {
                    for (int y = 0; y < mergedImage2.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsHardRight++;
                        }

                    }
                }

                Invoke(new Action(() =>
                {
                    rPixHLeft.Text = $"{redPixelsHardLeft}";
                    rPixLeft.Text = $"{redPixelsLeft}";
                    rPixMiddle.Text = $"{redPixelsMiddle}";
                    rPixRight.Text = $"{redPixelsRight}";
                    rPixHRight.Text = $"{redPixelsHardRight}";
                }));


                //* --------------------------- Serial Communications ----------------------------- *// 

                //yellow line
                /*
                if (yellowPixelsMiddle > (yellowPixelsLeft + yellowPixelsRight + yellowPixelsHardLeft + yellowPixelsHardRight))
                {
                    robot.Move('f');
                    serialChar = 'f';
                }
                else if (yellowPixelsLeft > (yellowPixelsMiddle + yellowPixelsRight + yellowPixelsHardLeft + yellowPixelsHardRight))
                {
                    robot.Move('l');
                    serialChar = 'l';
                }
                else if (yellowPixelsRight > (yellowPixelsLeft + yellowPixelsMiddle + yellowPixelsHardLeft + yellowPixelsHardRight))
                {
                    robot.Move('r');
                    serialChar = 'r';
                }
                else if (yellowPixelsHardLeft > (yellowPixelsLeft + yellowPixelsRight + yellowPixelsMiddle + yellowPixelsHardRight))
                {
                    robot.Move('a');
                    serialChar = 'a';
                }
                else if (yellowPixelsHardRight > (yellowPixelsLeft + yellowPixelsRight + yellowPixelsHardLeft + yellowPixelsMiddle))
                {
                    robot.Move('d');
                    serialChar = 'd';
                }
                else if (redPixelsMiddle > (redPixelsLeft + redPixelsRight + redPixelsHardLeft + redPixelsHardRight))
                {
                    robot.Move('s');
                    serialChar = 's';
                }
                else
                {
                    robot.Move('s');
                    serialChar = 's';
                }

                Invoke(new Action(() =>
                {
                    serial_output_char_label.Text = $"{serialChar}";
                }));
                */

                //array to get largest slice 
                //need to have array and then find largest slice in that array from the pixel count
                //this is done so that the serial buffer tube does not get overran with data bytes, and then make sure the switch statement can only be one case at a time 
                //set serialCharcommand to the index of the largest slice, then take that and output it with robot.move command 
                int[] largestSliceArray = { yellowPixelsMiddle, yellowPixelsLeft, yellowPixelsRight, yellowPixelsHardLeft, yellowPixelsHardRight, redPixelsMiddle };
                int largestSlice_ofArray = largestSliceArray[0];
                int serialCharacterCommand = 0; 

                for (int i = 0; i < largestSliceArray.Length; i++)
                {
                    if (largestSliceArray[i] > largestSlice_ofArray)
                    {
                        largestSlice_ofArray = largestSliceArray[i];
                        serialCharacterCommand = i;
                    }
                }

                if (start)
                {
                    switch (serialCharacterCommand)
                    {
                        case 0:
                            robot.Move('f');
                            serialChar = 'f';
                            break;
                        case 1:
                            robot.Move('l');
                            serialChar = 'l';
                            break;
                        case 2:
                            robot.Move('r');
                            serialChar = 'r';
                            break;
                        case 3:
                            robot.Move('a');
                            serialChar = 'a';
                            break;
                        case 4:
                            robot.Move('d');
                            serialChar = 'd';
                            break;
                        case 5:
                            robot.Move('s');
                            serialChar = 's';
                            break;
                    }
                }
               

                Invoke(new Action(() =>
                {
                    serial_output_char_label.Text = $"{serialChar}";
                }));
            }
        }

        //trackbars to get the values of the colors on camera, values change based on where the trackbar scroll is on 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }
        private void trackBar_HueMinRed_Scroll(object sender, EventArgs e)
        {
            hueMinRed = trackBar_HueMinRed.Value;
            hueMinRed_Label.Text = $"{hueMinRed}";
        }
        private void trackBar_hueMaxRed_Scroll(object sender, EventArgs e)
        {
            hueMaxRed = trackBar_hueMaxRed.Value;
            hueMaxRed_Label.Text = $"{hueMaxRed}";
        }
        private void trackBar_SatMinRed_Scroll(object sender, EventArgs e)
        {
            satMinRed = trackBar_SatMinRed.Value;
            satMinRed_Label.Text = $"{satMinRed}";
        }
        private void trackBar_SatMaxRed_Scroll(object sender, EventArgs e)
        {
            satMaxRed = trackBar_SatMaxRed.Value;
            satMaxRed_Label.Text = $"{satMaxRed}";
        }
        private void trackBar_ValMinRed_Scroll(object sender, EventArgs e)
        {
            valMinRed = trackBar_ValMinRed.Value;
            valMinRed_Label.Text = $"{valMinRed}";
        }
        private void trackBar_ValMaxRed_Scroll(object sender, EventArgs e)
        {
            valMaxRed = trackBar_ValMaxRed.Value;
            valMaxRed_Label.Text = $"{valMaxRed}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            start = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            start = false;
        }

        private void saturationTrackBarMin_Scroll(object sender, EventArgs e)
        {
            saturationMin = saturationTrackBarMin.Value;
            satMinLabel.Text = $"{saturationMin}";
        }
        private void valueTrackBarMin_Scroll(object sender, EventArgs e)
        {
            valueMin = valueTrackBarMin.Value;
            valueMinLabel.Text = $"{valueMin}";
        }
        private void valueTrackBarMax_Scroll(object sender, EventArgs e)
        {
            valueMax = valueTrackBarMax.Value;
            valueMaxLabel.Text = $"{valueMax}";
        }
        private void saturationTrackBarMax_Scroll(object sender, EventArgs e)
        {
            saturationMax = saturationTrackBarMax.Value;
            satMaxLabel.Text = $"{saturationMax}";
        }
        private void hueTrackbarMin_Scroll(object sender, EventArgs e)
        {
            hueMin = hueTrackbarMin.Value;
            hueMinLabel.Text = $"{hueMin}";
        }
        private void hueTrackbarMax_Scroll(object sender, EventArgs e)
        {
            hueMax = hueTrackbarMax.Value;
            hueMaxLabel.Text = $"{hueMax}";
        }
    }
}
