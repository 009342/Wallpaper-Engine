using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace SharedLibrary
{
    public static class API
    {
        #region SendMessageAPI
        public const int WM_COPYDATA = 0x004A;
        public const int WM_ERASEBKGND = 0x0014;
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(HandleRef hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);
        [DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        #endregion
        #region SendMessageFunctions
        public static IntPtr SendMessage(object target, Type t, IntPtr hWnd)
        {
            XmlSerializer xml = new XmlSerializer(t);
            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            using (StringWriter textWriter = new StringWriter())
            {
                xml.Serialize(textWriter, target);
                cds.lpData = textWriter.ToString();
                cds.cbData = cds.lpData.Length;
            }
            return SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);

        }
        public static object PtrToStructure(IntPtr LParam, Type t)
        {
            COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(LParam, typeof(COPYDATASTRUCT));
            XmlSerializer xml = new XmlSerializer(t);
            object result;
            using (TextReader reader = new StringReader(cds.lpData))
            {
                result = xml.Deserialize(reader);
            }
            return result;
        }
        public static IntPtr Alloc(object param, Type t)
        {
            IntPtr ret = Marshal.AllocHGlobal(Marshal.SizeOf(t));
            Marshal.StructureToPtr(param, ret, false);
            return ret;
        }
        public static void FreePtr(ref IntPtr Allocated)
        {
            if (Allocated == IntPtr.Zero) return;
            Marshal.FreeHGlobal(Allocated);
            Allocated = IntPtr.Zero;

        }
        public struct FunctionMessage
        {
            public Int64 Handle;
            public int Fun;
            public byte[] str;
        }
        #endregion
        #region WinAPI

        public delegate bool EnumWindowCallback(IntPtr hwnd, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowCallback callback, int extraData);
        [DllImport("user32.dll")]
        static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, DeviceContextValues flags);
        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20
        }
        [Flags()]
        public enum DeviceContextValues : uint
        {
            /// <summary>DCX_WINDOW: Returns a DC that corresponds to the window rectangle rather 
            /// than the client rectangle.</summary>
            Window = 0x00000001,
            /// <summary>DCX_CACHE: Returns a DC from the cache, rather than the OWNDC or CLASSDC 
            /// window. Essentially overrides CS_OWNDC and CS_CLASSDC.</summary>
            Cache = 0x00000002,
            /// <summary>DCX_NORESETATTRS: Does not reset the attributes of this DC to the 
            /// default attributes when this DC is released.</summary>
            NoResetAttrs = 0x00000004,
            /// <summary>DCX_CLIPCHILDREN: Excludes the visible regions of all child windows 
            /// below the window identified by hWnd.</summary>
            ClipChildren = 0x00000008,
            /// <summary>DCX_CLIPSIBLINGS: Excludes the visible regions of all sibling windows 
            /// above the window identified by hWnd.</summary>
            ClipSiblings = 0x00000010,
            /// <summary>DCX_PARENTCLIP: Uses the visible region of the parent window. The 
            /// parent's WS_CLIPCHILDREN and CS_PARENTDC style bits are ignored. The origin is 
            /// set to the upper-left corner of the window identified by hWnd.</summary>
            ParentClip = 0x00000020,
            /// <summary>DCX_EXCLUDERGN: The clipping region identified by hrgnClip is excluded 
            /// from the visible region of the returned DC.</summary>
            ExcludeRgn = 0x00000040,
            /// <summary>DCX_INTERSECTRGN: The clipping region identified by hrgnClip is 
            /// intersected with the visible region of the returned DC.</summary>
            IntersectRgn = 0x00000080,
            /// <summary>DCX_EXCLUDEUPDATE: Unknown...Undocumented</summary>
            ExcludeUpdate = 0x00000100,
            /// <summary>DCX_INTERSECTUPDATE: Unknown...Undocumented</summary>
            IntersectUpdate = 0x00000200,
            /// <summary>DCX_LOCKWINDOWUPDATE: Allows drawing even if there is a LockWindowUpdate 
            /// call in effect that would otherwise exclude this window. Used for drawing during 
            /// tracking.</summary>
            LockWindowUpdate = 0x00000400,
            /// <summary>DCX_USESTYLE: Undocumented, something related to WM_NCPAINT message.</summary>
            UseStyle = 0x00010000,
            /// <summary>DCX_VALIDATE When specified with DCX_INTERSECTUPDATE, causes the DC to 
            /// be completely validated. Using this function with both DCX_INTERSECTUPDATE and 
            /// DCX_VALIDATE is identical to using the BeginPaint function.</summary>
            Validate = 0x00200000,
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hWndChild);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr windowHandle,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr result);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr windowHandle,
            uint Msg,
            IntPtr wParam,
            ref COPYDATASTRUCT lParam,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr result);
        #endregion
        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        public static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        public static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        public static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion
    }
    public static class Flags
    {
        public enum ControllerFunctionFlags : int
        {
            SetEngineHandle,
            RequestNextVideoPlay //미구현
        }
        public enum EngineFunctionFlags : int
        {
            SetControllerHandle,
            PlayVideoURL,
            SetLoopEnable,
            SetLoopDisable,
            StopVideo,
            SetVolume,
            Exit
        }
    }
}
