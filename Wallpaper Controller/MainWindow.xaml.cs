using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
using static SharedLibrary.API;
using static SharedLibrary.Flags;
namespace Wallpaper_Controller
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        IntPtr engineHandle;
        IntPtr windowHandle;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void test_Click(object sender, RoutedEventArgs e)
        {
            FunctionMessage fm = new FunctionMessage();
            fm.Fun = (int)EngineFunctionFlags.PlayVideoURL;
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "미디어 파일 선택";
            ofd.Filter = "미디어 파일 | *.*";
            if (ofd.ShowDialog() == true)
            {
                fm.str = Encoding.UTF32.GetBytes(ofd.FileName);
            }
            else
            { return; }
            SendMessage(fm, typeof(FunctionMessage), engineHandle);
        }
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            windowHandle = new WindowInteropHelper(Main).Handle;
            HwndSource source = HwndSource.FromHwnd(windowHandle);
            source.AddHook(new HwndSourceHook(WndProc));
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                FunctionMessage fm = (FunctionMessage)PtrToStructure(lParam, typeof(FunctionMessage));
                if ((int)ControllerFunctionFlags.SetEngineHandle == fm.Fun)
                {
                    engineHandle = (IntPtr)fm.Handle;
                    connectedHandle.Content = "연결된 엔진 핸들 : 0x" + fm.Handle.ToString("X");
                }
            }
            return IntPtr.Zero;
        }

        private void stop_media_Click(object sender, RoutedEventArgs e)
        {
            FunctionMessage fm = new FunctionMessage();
            fm.Fun = (int)EngineFunctionFlags.StopVideo;
            SendMessage(fm, typeof(FunctionMessage), engineHandle);
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FunctionMessage fm = new FunctionMessage();
            fm.Fun = (int)EngineFunctionFlags.SetVolume;
            fm.str = Encoding.UTF32.GetBytes(((Slider)sender).Value.ToString());
            SendMessage(fm, typeof(FunctionMessage), engineHandle);
        }

        private void turnOff_Click(object sender, RoutedEventArgs e)
        {
            FunctionMessage fm = new FunctionMessage();
            fm.Fun = (int)EngineFunctionFlags.Exit;
            SendMessage(fm, typeof(FunctionMessage), engineHandle);
        }

        private void loop_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                FunctionMessage fm = new FunctionMessage();
                fm.Fun = (int)EngineFunctionFlags.SetLoopEnable;
                SendMessage(fm, typeof(FunctionMessage), engineHandle);
            }
            else
            {
                FunctionMessage fm = new FunctionMessage();
                fm.Fun = (int)EngineFunctionFlags.SetLoopDisable;
                SendMessage(fm, typeof(FunctionMessage), engineHandle);
            }
        }
    }
}
