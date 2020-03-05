﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace APOD
{
    public partial class AstronomyPictureForm : Form
    {
        public AstronomyPictureForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: Set the text in the date TextBox to today's date,
            // formatted as MM/DD/YYYY      
            DateTime today = DateTime.Today;
            txtDate.Text = $"{today:d}";
        }

        private void btnGetToday_Click(object sender, EventArgs e)
        {
            // TODO: Request the APOD picture for today

            //Create a Datetime that represents today
            DateTime today = DateTime.Today;


            //start a request for r=todays APOD picture
            GetAPOD(DateTime.Today);
        }

        private void btnGetForDate_Click(object sender, EventArgs e)
        {
            try
            {


                //TODO: Attempt to convert text in txtDate into a DateTime
                DateTime date = DateTime.Parse(txtDate.Text);

                // Make sure the date is today or in the past
                if (date > DateTime.Today)
                {
                    //Throw FormatException, to be caught in the catch block below
                    throw new FormatException("Date can`t be in the future");
                }

                // And make sure date is June 16, 1995 or later, the date APOD service started
                if (date < new DateTime(1995, 06, 16))
                {
                    //also to be caught by, and handlded by, the catch block
                    throw new FormatException("Date can`t be before June 16 1995");
                }

                //TODO: If date is a DateTime and within the allowed date range, 
                //  request APOD picture for this date 
                GetAPOD(date);
            }
            catch (FormatException err)
            {
                MessageBox.Show(err.Message, "Invalid date");
            }
        }



       
      
      
        private void GetAPOD(DateTime date)
        {
            // Clear current image and text, and disable form 
            ClearForm();
            EnableForm(false);

            // If there is not a request in progress, start fetching photo for date 
            // Long-running tasks should be delegated to background workers, otherwise user interface
            // will freeze or be unresponsive while request is in progress
            if (apodBackgroundWorker.IsBusy == false)
            {
                apodBackgroundWorker.RunWorkerAsync(date);
            }
            else   // A request is already in progress, ask user to wait.
            {
                MessageBox.Show("Please wait for previous request to complete.");
            }
        }

        private void LoadImageResponseIntoForm(APODResponse apodResponse)
        {
            //show title
            lblTitle.Text = apodResponse.Title;

            //format and show image credits
            lblCredits.Text = $"Image credit;  {apodResponse.Copyright}";


            //convert date string from response which is in the form yyy-mm-dd, 
            //into a Datetime, so it can be formatted and dsiplayed
            DateTime date = DateTime.Parse(apodResponse.Date);
            string formattedDate = $"{date:D}";
            lblDate.Text = formattedDate;

            //show explanation text
            lblDescription.Text = apodResponse.Explanation;

            //load picture, handle any image loading errors
            try
            {
                picAstronomyPicture.Image = Image.FromFile(apodResponse.FileSavePath);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error Loading Image saved for {apodResponse}'\n'{e.Message}");
            }
        }

        private void HandleResponse(APODResponse apodResponse, string error)
        {
            // TODO: if there is an error from the request, show a MessageBox 
            if (error != null)
            {
                MessageBox.Show(error, "Error");
                return;
            }

            // TODO: Make sure response is an image (not a video or other media type) 
            if (apodResponse.MediaType.Equals("image"))
            {
                LoadImageResponseIntoForm(apodResponse);
            }
            else
            {
                MessageBox.Show($"The Response is not an image . Please try another date.", "Sorry");
            }

            // TODO: If there are no errors, and the response is an image, call a method 
            //  (that you'll create) to display the info in the form

            // TODO: if APOD is not an image, display a message box, ask user to try another date
        }



        // TODO: Create new method to display data from an APODResponse in the form.

            // TODO: Show title in lblTitle

            // TODO: Format and show image credits in lblCredits

            // TODO: Convert date string from response, which is in the form yyyy-mm-dd, into a DateTime
            // TODO: format and display the date string in lblDate

            // TODO: Show explanation text in lblDescription

            // TODO: Load the image into the picAstronomyPicture PictureBox.
            // TODO: Catch any errors thrown loading the image



        private void ClearForm()
        {
            // Clear all info about a previous picture. 
            lblDate.Text = "";
            lblDescription.Text = "";
            lblTitle.Text = "";
            lblCredits.Text = "";

            picAstronomyPicture.Image?.Dispose();    // Release the image file resource, if there is one
            picAstronomyPicture.Image = null;    // Clear current image
        }


        private void EnableForm(Boolean enable)
        {
            // If the enable parameter is true, the Enabled property of Buttons and TextBox will be true
            // The progress bar visibility will be false. 
            // The user will be able to interact with the Button and TextBox controls, the progress bar will be hidden.

            // If the enable parameter is false, the Enabled property of Buttons and TextBox will be false
            // The progress bar visibility will be true. 
            // The user will not be able to interact with the Button and TextBox controls, the progress bar will be visible.

            btnGetForDate.Enabled = enable;
            btnGetToday.Enabled = enable;
            txtDate.Enabled = enable;

            progressBar.Visible = !enable;   // The opposite of whether the buttons are enabled
        }


        private void apodBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // If the argument is a DateTime, convert it to a DateTime and store in variable named dt
            if (e.Argument is DateTime dt)
            {
                APODResponse apodResponse = APOD.FetchAPOD(out string error, dt);  // Make the request!
                e.Result = (reponse: apodResponse, error);   // A tuple https://docs.microsoft.com/en-us/dotnet/csharp/tuples
                Debug.WriteLine(e.Result);
            }
            else
            {
                Debug.WriteLine("Background worker error - argument not a DateTime" + e.Argument);
                throw new Exception("Incorrect Argument type, must be a DateTime");
            }
        }

        private void apodBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // If the background worker throws an error, e.Error will have a value
                MessageBox.Show($"Unexpected Error fetching data", "Error");
                Debug.WriteLine($"Background Worker error {e.Error}");
            }
            else
            {
                try
                {
                    // Read the result from the background worker 
                    var (response, error) = ((APODResponse, string))e.Result;
                    // Update the user interface with the data returned. 
                    // This method also shows the user an error, if there is one
                    // These errors are generally things the user can fix, for example, no internet connection
                    HandleResponse(response, error);
                }
                catch (Exception err)
                {
                    // These are probably issues with the program that a user can't reasonable fix.
                    Debug.WriteLine($"Unexpected response from APOD request worker: {e.Result} causing error {err}");
                    MessageBox.Show($"Unexpected data returned from APOD request", "Error");
                }
            }

            EnableForm(true);   // In any case, enable the user interface 
        }
    }
}