using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Unosquare.FFME;
using Unosquare.FFME.Shared;

namespace Wallpaper
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public App()
            :base()
        {
            MediaElement.FFmpegDirectory = @"e:\ffmpeg";

            // You can pick which FFmpeg binaries are loaded. See issue #28
            // Full Features is already the default.
            MediaElement.FFmpegLoadModeFlags = FFmpegLoadMode.FullFeatures;
        }
        Mutex mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            string mutexName = "GUID//" + Assembly.GetExecutingAssembly().GetType().GUID.ToString();
            try
            {
                mutex = new Mutex(false, mutexName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), mutexName);

            }
            if (mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show("월페이퍼 엔진이 이미 실행중입니다.", mutexName);
                Application.Current.Shutdown(-1);
            }

        }


    }
}
