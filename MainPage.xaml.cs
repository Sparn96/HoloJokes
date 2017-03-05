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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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
            
            url = @"http://dreamatico.com/data_images/people/people-2.jpg";
            url = textBox1.Text;
            GetEmotions(url);
            //GetLocalEmotions();

           image.Source = new BitmapImage(
    new Uri(url, UriKind.Absolute));

            

        }

        private async void GetEmotions(string imageUrl)
        {
            Windows.Storage.ApplicationDataContainer localSettings =
            Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.StorageFolder localFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;

            string key = "22da994748964c7c97432cf7c4f1695c";

            var emotionServiceClient = new EmotionServiceClient(key);
            Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(imageUrl);

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var faceNumber = 0;
            foreach (Emotion em in emotionResult)
            {
                faceNumber++;
                var scores = em.Scores;

                var anger = scores.Anger;
                var contempt = scores.Contempt;
                var disgust = scores.Disgust;
                var fear = scores.Fear;
                var happiness = scores.Happiness;
                var neutral = scores.Neutral;
                var surprise = scores.Surprise;
                var sadness = scores.Sadness;

                sb1.Append(string.Format("Face {0}\n", faceNumber));
                sb1.Append("Scores:\n");
                sb1.Append(string.Format("Anger: {0:0.000000}\n", anger));
                sb1.Append(string.Format("Contempt: {0:0.000000}\n", contempt));
                sb1.Append(string.Format("Disgust: {0:0.000000}\n", disgust));
                sb1.Append(string.Format("Fear: {0:0.000000}\n", fear));
                sb1.Append(string.Format("Happiness: {0:0.000000}\n", happiness));
                sb1.Append(string.Format("Neutral: {0:0.000000}\n", neutral));
                sb1.Append(string.Format("Surprise: {0:0.000000}\n", surprise));
                sb1.Append(string.Format("Sadness: {0:0.000000}\n", sadness));
                sb1.Append("\n");

                var emotionScoresList = new List<EmotionScore>();
                emotionScoresList.Add(new EmotionScore("anger", anger));
                emotionScoresList.Add(new EmotionScore("contempt", contempt));
                emotionScoresList.Add(new EmotionScore("disgust", disgust));
                emotionScoresList.Add(new EmotionScore("fear", fear));
                emotionScoresList.Add(new EmotionScore("happiness", happiness));
                emotionScoresList.Add(new EmotionScore("neutral", neutral));
                emotionScoresList.Add(new EmotionScore("surprise", surprise));
                emotionScoresList.Add(new EmotionScore("sadness", sadness));

                var maxEmotionScore = emotionScoresList.Max(e => e.EmotionValue);
                var likelyEmotion = emotionScoresList.First(e => e.EmotionValue == maxEmotionScore);

                // System.Drawing.Rectangle rec = new System.Drawing.Rectangle();


                string likelyEmotionText = string.Format("Face {0} is {1:N2}% likely to experiencing: {2}\n\n",
                    faceNumber, likelyEmotion.EmotionValue * 100, likelyEmotion.EmotionName.ToUpper());
                sb2.Append(likelyEmotionText);
            }
            var resultsDump = sb1.ToString();
            // save dump to container
            textBox.Text = sb2.ToString();

        }
        
        private async void GetLocalEmotions()                    //used to grab local camera informaiton
        {
            string key = "22da994748964c7c97432cf7c4f1695c";

            var emotionServiceClient = new EmotionServiceClient(key);



            

            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreatePng());
         

            var capturedPhoto = await lowLagCapture.CaptureAsync();
            var PNGSTREAM = capturedPhoto.Frame.AsStream();
            await lowLagCapture.FinishAsync();




            Emotion em = await sendMyRequest(PNGSTREAM);                //ONLY ONE FACE FOR NOW
       
            
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            var faceNumber = 0;
            
                faceNumber++;
                var scores = em.Scores;

                var anger = scores.Anger;
                var contempt = scores.Contempt;
                var disgust = scores.Disgust;
                var fear = scores.Fear;
                var happiness = scores.Happiness;
                var neutral = scores.Neutral;
                var surprise = scores.Surprise;
                var sadness = scores.Sadness;

                sb1.Append(string.Format("Face {0}\n", faceNumber));
                sb1.Append("Scores:\n");
                sb1.Append(string.Format("Anger: {0:0.000000}\n", anger));
                sb1.Append(string.Format("Contempt: {0:0.000000}\n", contempt));
                sb1.Append(string.Format("Disgust: {0:0.000000}\n", disgust));
                sb1.Append(string.Format("Fear: {0:0.000000}\n", fear));
                sb1.Append(string.Format("Happiness: {0:0.000000}\n", happiness));
                sb1.Append(string.Format("Neutral: {0:0.000000}\n", neutral));
                sb1.Append(string.Format("Surprise: {0:0.000000}\n", surprise));
                sb1.Append(string.Format("Sadness: {0:0.000000}\n", sadness));
                sb1.Append("\n");

                var emotionScoresList = new List<EmotionScore>();
                emotionScoresList.Add(new EmotionScore("anger", anger));
                emotionScoresList.Add(new EmotionScore("contempt", contempt));
                emotionScoresList.Add(new EmotionScore("disgust", disgust));
                emotionScoresList.Add(new EmotionScore("fear", fear));
                emotionScoresList.Add(new EmotionScore("happiness", happiness));
                emotionScoresList.Add(new EmotionScore("neutral", neutral));
                emotionScoresList.Add(new EmotionScore("surprise", surprise));
                emotionScoresList.Add(new EmotionScore("sadness", sadness));

                var maxEmotionScore = emotionScoresList.Max(e => e.EmotionValue);
                var likelyEmotion = emotionScoresList.First(e => e.EmotionValue == maxEmotionScore);

                // System.Drawing.Rectangle rec = new System.Drawing.Rectangle();




                string likelyEmotionText = string.Format("Face {0} is {1:N2}% likely to experiencing: {2}\n\n",
                    faceNumber, likelyEmotion.EmotionValue * 100, likelyEmotion.EmotionName.ToUpper());
                sb2.Append(likelyEmotionText);
            
            var resultsDump = sb1.ToString();
            // save dump to container
            textBox.Text = sb2.ToString();
        }
        

        private string xGetLikelyEmotion()
        {
            return "";
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public async Task<Emotion> sendMyRequest(Stream PNGSTREAM)
        {
            byte[] bytes;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "22da994748964c7c97432cf7c4f1695c");
            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;
    
            BinaryReader binaryReader = new BinaryReader(PNGSTREAM);
            bytes = binaryReader.ReadBytes((int)PNGSTREAM.Length);

            using (var content = new ByteArrayContent(bytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                return JsonConvert.DeserializeObject<Emotion>(response.Content.ReadAsStringAsync().Result);
            }
        }


        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static async void MakeRequest(string imageFilePath)
        {
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "22da994748964c7c97432cf7c4f1695c");

            string uri = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
            HttpResponseMessage response;
            string responseContent;

            // Request body. Try this sample with a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                responseContent = response.Content.ReadAsStringAsync().Result;
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




    }






}