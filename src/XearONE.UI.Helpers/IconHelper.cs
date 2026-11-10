using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XearONE.UI.Helpers;

public static class IconHelper
{
	private struct SHFILEINFO
	{
		public IntPtr hIcon;

		public int iIcon;

		public uint dwAttributes;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	private const uint SHGFI_ICON = 256u;

	private const uint SHGFI_LARGEICON = 0u;

	private const uint SHGFI_SMALLICON = 1u;

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool DestroyIcon(IntPtr hIcon);

	public static ImageSource GetExeIcon(string path, bool largeIcon = true)
	{
		SHFILEINFO shinfo = default;
		uint flags = 0x100u | ((!largeIcon) ? 1u : 0u);
		SHGetFileInfo(path, 0u, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
		if (shinfo.hIcon == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			return Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}
		finally
		{
			DestroyIcon(shinfo.hIcon);
		}
	}
}
