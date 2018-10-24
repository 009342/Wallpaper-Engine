using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unosquare.FFME;
using static SharedLibrary.API;
using static SharedLibrary.Flags;
namespace Wallpaper_Engine
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        bool loop = false;
        IntPtr windowHandle;
        IntPtr ControllerHwnd;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            #region Hide
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            #endregion
            Hide();
            Main.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            Main.Height = SystemParameters.PrimaryScreenHeight;
            media.Width = Main.Width;
            media.Height = Main.Height;
            IntPtr progman = FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(progman,
                       0x052C,
                       new IntPtr(0),
                       IntPtr.Zero,
                       SendMessageTimeoutFlags.SMTO_NORMAL,
                       1000,
                       out result);
            IntPtr workerw = IntPtr.Zero;
            EnumWindows(new EnumWindowCallback((tophandle, topparamhandle) =>
            {
                IntPtr p = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (p != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                }
                return true;
            }), 0);

            windowHandle = new WindowInteropHelper(Main).Handle;

            //SetParent(FindWindow(null, "asdf"), workerw);
            SetParent(windowHandle, workerw);
            Main.Left = 0;
            Main.Top = 0;
            HwndSource source = HwndSource.FromHwnd(windowHandle);
            source.AddHook(new HwndSourceHook(WndProc));
            Thread t = new Thread(
                () =>
                {
                    while (true)
                    {
                        ControllerHwnd = FindWindow(null, "Wallpaper Controller");
                        if (ControllerHwnd != IntPtr.Zero)
                        {
                            FunctionMessage fm = new FunctionMessage();
                            fm.Handle = windowHandle.ToInt64();
                            fm.Fun = (int)ControllerFunctionFlags.SetEngineHandle;

                            SendMessage(fm, typeof(FunctionMessage), ControllerHwnd);
                        }
                        Thread.Sleep(1000);
                    }

                }
                );
            t.Start();
            media.MediaEnded += Media_MediaEnded;
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (loop)
            {
                media.Position = TimeSpan.Zero;
                media.Play();
            }
            else
            {
                FunctionMessage fm = new FunctionMessage();
                fm.Fun = (int)ControllerFunctionFlags.RequestNextVideoPlay;
                IntPtr result = SendMessage(fm, typeof(FunctionMessage), ControllerHwnd);
                if (result == IntPtr.Zero)
                {
                    Hide();
                }
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                FunctionMessage fm = (FunctionMessage)PtrToStructure(lParam, typeof(FunctionMessage));
                if (fm.Fun == (int)EngineFunctionFlags.PlayVideoURL)
                {
                    Show();
                    media.Source = new Uri(Encoding.UTF32.GetString(fm.str));
                }
                switch (fm.Fun)
                {
                    case (int)EngineFunctionFlags.SetLoopDisable:
                        loop = false;
                        break;
                    case (int)EngineFunctionFlags.SetLoopEnable:
                        loop = true;
                        break;
                    case (int)EngineFunctionFlags.StopVideo:
                        media.Stop();
                        Hide();
                        break;
                    case (int)EngineFunctionFlags.SetVolume:
                        media.Volume = double.Parse(Encoding.UTF32.GetString(fm.str));
                        break;
                    case (int)EngineFunctionFlags.Exit:
                        new Thread(() => { Thread.Sleep(1000); Environment.Exit(0); }).Start();
                        break;
                }
            }
            return IntPtr.Zero;
        }


    }
}
