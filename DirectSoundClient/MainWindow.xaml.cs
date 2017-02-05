using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using DirectSoundLib;
using System.IO;
using System.Windows.Interop;

namespace DirectSoundClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DSLibCapture dsCapture;
        private DSLibPlayer dsPlayer;

        private FileStream fs = new FileStream("pcm", FileMode.Create);

        public MainWindow()
        {
            InitializeComponent();

            this.dsCapture = new DSLibCapture();
            this.dsCapture.AudioDataCaptured += DsCapture_AudioDataCaptured;

            this.dsPlayer = new DSLibPlayer();
        }

        private void DsCapture_AudioDataCaptured(byte[] pcmData)
        {
            fs.Write(pcmData, 0, pcmData.Length);
        }

        private void ButtonInit_Click(object sender, RoutedEventArgs e)
        {
            this.dsCapture.Initialize();
        }

        private void ButtonStartCapture_Click(object sender, RoutedEventArgs e)
        {
            this.dsCapture.Start();
        }

        private void ButtonStopCapture_Click(object sender, RoutedEventArgs e)
        {
            this.dsCapture.Stop();
        }

        private void ButtonRelease_Click(object sender, RoutedEventArgs e)
        {
            this.dsCapture.Release();
        }

        private void ButtonInitDSLibPlayer_Click(object sender, RoutedEventArgs e)
        {
            this.dsPlayer.WindowHandle = new WindowInteropHelper(this).Handle;
            this.dsPlayer.IsStreamingBuffer = true;
            this.dsPlayer.Initialize();
        }

        private void ButtonBrowserFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            if ((bool)dialog.ShowDialog())
            {
                this.dsPlayer.StreamSource = File.Open(dialog.FileName, FileMode.Open);
            }
        }

        private void ButtonStarPlay_Click(object sender, RoutedEventArgs e)
        {
            this.dsPlayer.Play();
        }

        private void ButtonStopPlay_Click(object sender, RoutedEventArgs e)
        {
            this.dsPlayer.Stop();
            this.dsPlayer.StreamSource.Seek(0, SeekOrigin.Begin);
        }

        private void ButtonReleaseDSLibPlayer_Click(object sender, RoutedEventArgs e)
        {
            this.dsPlayer.Release();
        }
    }
}