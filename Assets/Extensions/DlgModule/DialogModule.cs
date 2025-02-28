using System;
using System.IO;
using System.Runtime.InteropServices;

public class DialogModule {
#if UNITY_STANDALONE_WIN
    [DllImport("libdlgmod.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX
    [DllImport("libdlgmod.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
    [DllImport("libdlgmod.bundle", CallingConvention = CallingConvention.Cdecl)]
#endif
    private static extern IntPtr get_open_filename(string filter, string fname);
    
#if UNITY_STANDALONE_WIN
    [DllImport("libdlgmod.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX
    [DllImport("libdlgmod.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
    [DllImport("libdlgmod.bundle", CallingConvention = CallingConvention.Cdecl)]
#endif
    private static extern IntPtr get_open_filenames(string filter, string fname);
    
#if UNITY_STANDALONE_WIN
    [DllImport("libdlgmod.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX
    [DllImport("libdlgmod.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
    [DllImport("libdlgmod.bundle", CallingConvention = CallingConvention.Cdecl)]
#endif
    private static extern IntPtr get_save_filename(string filter, string fname);

    public static string OpenFileBrowser(string filter) {
        return Marshal.PtrToStringAnsi(get_open_filename(filter, ""));
    }
    
    public static string OpenFilesBrowser(string filter) {
        return Marshal.PtrToStringAnsi(get_open_filenames(filter, ""));
    }

    public static string SaveFileBrowser(string filter, string filename) {
        return Marshal.PtrToStringAnsi(get_save_filename(filter, filename));
    }
}