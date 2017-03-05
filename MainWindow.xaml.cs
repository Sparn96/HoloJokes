//Credit David Giard for the Cognitive Services Example for Speech Recognition



using System;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.ProjectOxford.SpeechRecognition;
using System.Diagnostics;
using NAudio;
using NAudio.Wave;

namespace SpeechToTextDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AutoResetEvent _FinalResponseEvent;
        MicrophoneRecognitionClient _microphoneRecognitionClient;
        string words = "";

       public WaveIn waveSource = null;
public WaveFileWriter waveFile = null;

        private int fileIndex = 0;


        public MainWindow()
        {
            InitializeComponent();
            _FinalResponseEvent = new AutoResetEvent(false);
            OutputTextbox.Background = Brushes.White;
            OutputTextbox.Foreground = Brushes.Black;

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

            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(44100, 1);

            _microphoneRecognitionClient
                    = SpeechRecognitionServiceFactory.CreateMicrophoneClient
                                    (
                                    speechRecognitionMode,
                                    language,
                                    subscriptionKey
                                    );
        
            _microphoneRecognitionClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            _microphoneRecognitionClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);
            fileIndex += 1;
            waveFile = new WaveFileWriter(@"C:/Users/Arekusanda/Documents/wav/Test" + fileIndex + ".wav", waveSource.WaveFormat);

            _microphoneRecognitionClient.StartMicAndRecognition();
            waveSource.StartRecording();

        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
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

        //process.StandardInput.WriteLine("PATH_TO_WAV$Transcripton words go here.")

        /// <summary>
        /// Speaker has finished speaking. Sever connection to server, stop listening, and clean up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
           Dispatcher.Invoke((Action)(() =>
            {
                words = OutputTextbox.Text;
                run_cmd();

                _FinalResponseEvent.Set();
                _microphoneRecognitionClient.EndMicAndRecognition();
                _microphoneRecognitionClient.Dispose();
                _microphoneRecognitionClient = null;
                /*RecordButton.Content = "Start\nRecording";
                RecordButton.IsEnabled = true;
                OutputTextbox.Background = Brushes.White;
                OutputTextbox.Foreground = Brushes.Black;    */

                waveSource_RecordingStopped(null, null);

                ConvertSpeechToText();
            }));
        }


      
   
    

        private void run_cmd()
        {
            /*
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python";
            start.Arguments = "C:\\Users\\Arekusanda\\Downloads\\JokeDetect-master(3)\\JokeDetect-master\\combined.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamWriter writer = process.StandardInput)
                {
                    writer.WriteLine(@"C:/Users/Arekusanda/Documents/wav/Test" + fileIndex + ".wav$" + words);
                }
                using (StreamReader reader = process.StandardOutput)
                {

                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }*/

            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;

            p.StartInfo = info;
            p.Start();

            using (StreamWriter sw = p.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("activate tensorflow2");
                    sw.WriteLine("cd C:\\Users\\Arekusanda\\Downloads\\JokeDetect-master(3)\\JokeDetect-master");
                    sw.WriteLine("python combined.py");
                    sw.WriteLine(@"C:/Users/Arekusanda/Documents/wav/Test" + fileIndex + ".wav$" + words);
                }
                using (StreamReader reader = p.StandardOutput)
                {

                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }

            }
        }
    }
}
