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

//final project, named this as I copied my cs hw4 file since it was a good start from that 
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

        //red line max and min values, same as yellow but now red line 
        int hueMinRed, satMinRed, valMinRed = 0;
        int hueMaxRed = 179;
        int satMaxRed, valMaxRed = 255;

        //bool value for start and stop button later 
        bool start = false;

        //setup 
        public Form1()
        {
            InitializeComponent();
        }

        //more setup 
        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(0); //0 is onboard camera, 1 is offboard camera 
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();

            robot = new robot("COM5"); //serial com port  
        }

        //resize the picturebox function 
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

                //serial char for output later on onto the UI, default is s for stop 
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
                //set new frame, and convert that from blue,green,red values to gray 
                //then threshold it to type binary 
                //set new picture box to final binary type 
                Mat grayFrame = new Mat();
                CvInvoke.CvtColor(originalFrame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayFrame, grayFrame, _threshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                binaryPictureBox.Image = grayFrame.ToBitmap();

                
                //HSV Threshold 1 -- yellow line 
                //converting this new hue frame from bgr, to hue sat val ranges 
                Mat hsvFrameYellow = new Mat();
                CvInvoke.CvtColor(originalFrame, hsvFrameYellow, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                Mat[] hsvChannels = hsvFrameYellow.Split();

                //hue part
                Mat hueFilterYellow = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(hueMin), new ScalarArray(hueMax), hueFilterYellow);
                Invoke(new Action(() => { huePictureBox.Image = hueFilterYellow.ToBitmap(); }));

                //saturation part
                Mat saturationFilterYellow = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(saturationMin), new ScalarArray(saturationMax), saturationFilterYellow);
                Invoke(new Action(() => { saturationPictureBox.Image = saturationFilterYellow.ToBitmap(); }));

                //value part
                Mat valueFilterYellow = new Mat();
                CvInvoke.InRange(hsvChannels[2], new ScalarArray(valueMin), new ScalarArray(valueMax), valueFilterYellow);
                Invoke(new Action(() => { valuePictureBox.Image = valueFilterYellow.ToBitmap(); }));

                //merge all the images into one final image for output onto the screen
                //takes all the values from hue sat and val and combines them into one output
                //example would be the yellow line, each frame combines into the final yellow line 
                Mat mergedImageYellow = new Mat();
                CvInvoke.BitwiseAnd(hueFilterYellow, saturationFilterYellow, mergedImageYellow);
                CvInvoke.BitwiseAnd(mergedImageYellow, valueFilterYellow, mergedImageYellow);
                Invoke(new Action(() => { hsvPictureBoxMerged.Image = mergedImageYellow.ToBitmap(); }));

                

                //HSV Frame 2 -- for red line picture boxes 
                Mat hsvFrameRed = new Mat();
                CvInvoke.CvtColor(originalFrame, hsvFrameRed, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                Mat[] hsvChannels2 = hsvFrameRed.Split();

                //hue 2 
                Mat hueFilterRed = new Mat();
                CvInvoke.InRange(hsvChannels2[0], new ScalarArray(hueMinRed), new ScalarArray(hueMaxRed), hueFilterRed);
                Invoke(new Action(() => { huePictureBox2.Image = hueFilterRed.ToBitmap(); }));

                //sat 2 
                Mat saturationFilterRed = new Mat();
                CvInvoke.InRange(hsvChannels2[1], new ScalarArray(satMinRed), new ScalarArray(satMaxRed), saturationFilterRed);
                Invoke(new Action(() => { satPictureBox2.Image = saturationFilterRed.ToBitmap(); }));

                //val 2 
                Mat valueFilterRed = new Mat();
                CvInvoke.InRange(hsvChannels2[2], new ScalarArray(valMinRed), new ScalarArray(valMaxRed), valueFilterRed);
                Invoke(new Action(() => { valPictureBox2.Image = valueFilterRed.ToBitmap(); }));

                //merge image for red line detection 
                //same as yellow but for red now 
                Mat mergedImageRed = new Mat();
                CvInvoke.BitwiseAnd(hueFilterRed, saturationFilterRed, mergedImageRed);
                CvInvoke.BitwiseAnd(mergedImageRed, valueFilterRed, mergedImageRed);
                Invoke(new Action(() => { hsvMergedPictureBoxRed.Image = mergedImageRed.ToBitmap(); }));


                //resize final yellow image 
                //for a bigger final image to see the line easier 
                resizeFinal_YellowImage(mergedImageYellow);
         

                //erode and dialate the image to make things smoother 
                //dialte increases the pixel sizes and then erosion gets rid of all the extra pixels not needed
                //makes final image smooth with less random pixels all around 
                Mat kernal = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(2, 2), new Point(1, 1));
                CvInvoke.Erode(mergedImageYellow, mergedImageYellow, kernal, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                CvInvoke.Dilate(mergedImageYellow, mergedImageYellow, kernal, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

                Mat kernal2 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(2, 2), new Point(1, 1));
                CvInvoke.Erode(mergedImageRed, mergedImageRed, kernal2, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
                CvInvoke.Dilate(mergedImageRed, mergedImageRed, kernal2, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());

                //gaussian blur 
                //this is done to make the image more blurry but also takes out glare
                //only issue is that the image cannot see as far in front of it so placement matters much more now 
                Mat gaussFrameYellow = mergedImageYellow.Clone();
                CvInvoke.GaussianBlur(gaussFrameYellow, gaussFrameYellow, new Size(5, 5), 1.5);

                Mat gaussFrameRed = mergedImageRed.Clone();
                CvInvoke.GaussianBlur(gaussFrameRed, gaussFrameRed, new Size(5, 5), 1.5);


                //* -------------------------- Pixel Counts Red and Yellow ----------------------------- *// 

                Image<Gray, byte> image = mergedImageYellow.ToImage<Gray, byte>();
                Image<Gray, byte> image2 = mergedImageRed.ToImage<Gray, Byte>();

                //yellow 
                //set up 5 slices for the final image - middle, left and right and far left and right 
                //this is done to get largest pixel count in whatever slice to turn the robot later on while going down the track 
                //divide by 5 in this since I am using 5 slices
                //hard left 
                for (int x = 0; x < mergedImageYellow.Width / 5; x++) //get width and divide by 5 (5 slices)
                {
                    for (int y = 0; y < mergedImageYellow.Height; y++) // get height
                    {
                        if (image.Data[y, x, 0] == 255) 
                        {
                            yellowPixelsHardLeft++; //add one iteration to the yellowPixelsHardLeft count for later on
                        }
                    }
                }

                //soft left 
                //times by two (because 2nd slice from left to right) on width then multiply by 5 
                for (int x = (mergedImageYellow.Width / 5); x < (2 * (mergedImageYellow.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImageYellow.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsLeft++;
                        }

                    }
                }

                //middle 
                //times by three since its the middle slice 
                for (int x = (2 * (mergedImageYellow.Width / 5)); x < (3 * (mergedImageYellow.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImageYellow.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsMiddle++;
                        }

                    }
                }

                //soft right 
                //times by 4 since its the soft right slice, fourth in the line 
                for (int x = (3 * (mergedImageYellow.Width / 5)); x < (4 * (mergedImageYellow.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImageYellow.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsRight++;
                        }

                    }
                }

                //hard right 
                //times by 5 for furthest right slice in the sequence 
                for (int x = ((mergedImageYellow.Width / 5) * 4); x < ((mergedImageYellow.Width / 5) * 5); x++)
                {
                    for (int y = 0; y < mergedImageYellow.Height; y++)
                    {
                        if (image.Data[y, x, 0] == 255)
                        {
                            yellowPixelsHardRight++;
                        }

                    }
                }

                //this outputs the value of the pixels in each slice to the UI 
                Invoke(new Action(() =>
                {
                    yPixHLeft.Text = $"{yellowPixelsHardLeft}";
                    yPixLeft.Text = $"{yellowPixelsLeft}";
                    yPixMiddle.Text = $"{yellowPixelsMiddle}";
                    yPixRight.Text = $"{yellowPixelsRight}";
                    yPixHRight.Text = $"{yellowPixelsHardRight}";
                }));




                //same thing as yellow but with red image instead 
                //hard left 
                for (int x = 0; x < mergedImageRed.Width / 5; x++)
                {
                    for (int y = 0; y < mergedImageRed.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsHardLeft++;
                        }
                    }
                }

                //soft left 
                for (int x = (mergedImageRed.Width / 5); x < (2 * (mergedImageRed.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImageRed.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsLeft++;
                        }

                    }
                }

                //middle 
                for (int x = (2 * (mergedImageRed.Width / 5)); x < (3 * (mergedImageRed.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImageRed.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsMiddle++;
                        }

                    }
                }

                //soft right 
                for (int x = (3 * (mergedImageRed.Width / 5)); x < (4 * (mergedImageRed.Width / 5)); x++)
                {
                    for (int y = 0; y < mergedImageRed.Height; y++)
                    {
                        if (image2.Data[y, x, 0] == 255)
                        {
                            redPixelsRight++;
                        }

                    }
                }

                //hard right 
                for (int x = ((mergedImageRed.Width / 5) * 4); x < ((mergedImageRed.Width / 5) * 5); x++)
                {
                    for (int y = 0; y < mergedImageRed.Height; y++)
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

                //this did not work as it was too general and the robot would mess up in corners slighly 
                //if the pixels were close enough or some glare within the other slices would mess up 
                //causing the robot to go a different way than intended or switch states too late and go outside the white lines

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
                //this is done so that the serial buffer tube does not get overran with data bytes
                //plus it is easier for the computer to get the data and then send a serial character based on that at a later time  
                //and then make sure the switch statement can only be one case at a time 
                //set serialCharcommand to the index of the largest slice
                //then take that and output it with robot.move command 
                //this is much better than my method before with the if statements
                //since it was more detailed in finding the largest slice 

                //largestSliceArray is all the slices from above put into an array 
                int[] largestSliceArray = { yellowPixelsMiddle, yellowPixelsLeft, yellowPixelsRight, yellowPixelsHardLeft, yellowPixelsHardRight, redPixelsMiddle };
                int largestSlice_ofArray = largestSliceArray[0];
                int serialCharacterCommand = 0; //index of the array later on that represents the cases and what slice 

                for (int i = 0; i < largestSliceArray.Length; i++) //get the array index total length 
                {
                    if (largestSliceArray[i] > largestSlice_ofArray)
                    {
                        largestSlice_ofArray = largestSliceArray[i];
                        serialCharacterCommand = i; //set serial command as the index within the array 
                    }
                }

                if (start) //if statement with bool wrapped around switch statement so that the start/stop button works 
                {
                    switch (serialCharacterCommand)
                    {
                        case 0:
                            robot.Move('f');
                            serialChar = 'f'; //serial char is used to outputting onto the UI for debugging purposes 
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
               
                //outputting the serial char onto the user interface for debugging 
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

        //start and stop button for easier control when setting up the robot 
        //simply allows the program to go into the switch statements if start button is pressed 
        private void start_button_Click(object sender, EventArgs e)
        {
            start = true;
        }
        private void stop_button_Click(object sender, EventArgs e)
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
