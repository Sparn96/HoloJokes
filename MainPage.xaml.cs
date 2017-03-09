using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Core;
using System.Linq;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion.Contract;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;



namespace HoloJokes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        string url = "";
        MediaCapture mediaCapture;
        bool _isPreviewing = false;
        DisplayRequest _displayRequest;
        
        public MainPage()
        {
            this.InitializeComponent();
            onRun();
            Application.Current.Suspending += Application_Suspending;
        }


        public void onRun()
        {

            StartPreviewAsync();
            //initVideo();
        }
        

        private void GetEmotion_Click(object sender, RoutedEventArgs e)
        {
              
           GetLocalEmotions();

        }
        private async void GetLocalEmotions()                    //used to grab local camera informaiton
        {
            

            var emotionServiceClient = new EmotionServiceClient(key);

            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreatePng());
         

            var capturedPhoto = await lowLagCapture.CaptureAsync();
            var PNGSTREAM = capturedPhoto.Frame.AsStream();
            await lowLagCapture.FinishAsync();

            string responseString;

            

            responseString = await MakeRequest(PNGSTREAM);
            Emotion[] em = JsonConvert.DeserializeObject<Emotion[]>(responseString);            //This might need to be tweaked

            string anger, contempt, disgust, fear, happiness, neutral, sadness, surprise;

            anger =      "Anger:     " + em[0].Scores.Anger.ToString();
            contempt =   "Contempt:  " + em[0].Scores.Contempt.ToString();
            disgust =    "Disgust:   " + em[0].Scores.Disgust.ToString();
            fear =       "Fear:      " + em[0].Scores.Fear.ToString();
            happiness =  "Happiness: " + em[0].Scores.Happiness.ToString();
            neutral =    "Neutral:   " + em[0].Scores.Neutral.ToString();
            sadness =    "Sadness:   " + em[0].Scores.Sadness.ToString();
            surprise =   "Surprise:  " + em[0].Scores.Surprise.ToString();

            listView.Items.Clear();
            listView.Items.Add(anger);
            listView.Items.Add(contempt);
            listView.Items.Add(disgust);
            listView.Items.Add(fear);
            listView.Items.Add(happiness);
            listView.Items.Add(neutral);
            listView.Items.Add(sadness);
            listView.Items.Add(surprise);


            //FaceRect.Height = 3 * em[0].FaceRectangle.Height;           //Sets the face rect? 
            //FaceRect.Width = 3 * em[0].FaceRectangle.Width;


        }


        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static async Task<string> MakeRequest(Stream IMAGE)
        {
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "INSERT YOU API KEY HERE");
            

            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;


           

            BinaryReader binaryReader = new BinaryReader(IMAGE);
            byte[] byteData = binaryReader.ReadBytes((int)IMAGE.Length);

            using (var content = new ByteArrayContent(byteData))
            {               
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");  //pass as binary stream
                response = await client.PostAsync(uri, content);
                return response.Content.ReadAsStringAsync().Result;
            }
           
        }

        private async Task StartPreviewAsync()
        {
            try
            {

                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                PreviewControl.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                _isPreviewing = true;

                _displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                System.Diagnostics.Debug.WriteLine("The app was denied access to the camera");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
            }
        }
        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (_isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    if (_displayRequest != null)
                    {
                        _displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }

        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
                //nothing
        }
    }






}
