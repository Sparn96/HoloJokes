using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.ProjectOxford.SpeechRecognition;

namespace SpeechToTextDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AutoResetEvent _FinalResponseEvent;
        MicrophoneRecognitionClient _microphoneRecognitionClient;

        public MainWindow()
        {
            InitializeComponent();
            RecordButton.Content = "Start\nRecording";
            _FinalResponseEvent = new AutoResetEvent(false);
            OutputTextbox.Background = Brushes.White;
            OutputTextbox.Foreground = Brushes.Black;
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            RecordButton.Content = "Listening...";
            RecordButton.IsEnabled = false;
            OutputTextbox.Background = Brushes.Green;
            OutputTextbox.Foreground = Brushes.White;
            ConvertSpeechToText();
        }

        /// <summary>
        /// Start listening. 
        /// </summary>
        private void ConvertSpeechToText()
        {
            var speechRecognitionMode = SpeechRecognitionMode.ShortPhrase;
            string language = "en-us";
            string subscriptionKey = ConfigurationManager.AppSettings["SpeechKey"].ToString();

            _microphoneRecognitionClient
                    = SpeechRecognitionServiceFactory.CreateMicrophoneClient
                                    (
                                    speechRecognitionMode,
                                    language,
                                    subscriptionKey
                                    );

            _microphoneRecognitionClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            _microphoneRecognitionClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            _microphoneRecognitionClient.StartMicAndRecognition();

        }


        void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            string result = e.PartialResult;
            Dispatcher.Invoke(() =>
            {
                OutputTextbox.Text = (e.PartialResult);
                OutputTextbox.Text += ("\n");

            });
        }


        /// <summary>
        /// Speaker has finished speaking. Sever connection to server, stop listening, and clean up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                _FinalResponseEvent.Set();
                _microphoneRecognitionClient.EndMicAndRecognition();
                _microphoneRecognitionClient.Dispose();
                _microphoneRecognitionClient = null;
                RecordButton.Content = "Start\nRecording";
                RecordButton.IsEnabled = true;
                OutputTextbox.Background = Brushes.White;
                OutputTextbox.Foreground = Brushes.Black;

            }));
        }
    }
}
