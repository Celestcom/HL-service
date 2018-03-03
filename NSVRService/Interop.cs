using System;
using System.Runtime.InteropServices;

namespace NSVRService
{
	public static class Interop
	{

		[DllImport("HardlightPlatform.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hvr_platform_create(ref IntPtr ptr);

		[DllImport("HardlightPlatform.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hvr_platform_destroy(ref IntPtr ptr);

	
		[DllImport("HardlightPlatform.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hvr_platform_shutdown(IntPtr ptr);

		[DllImport("HardlightPlatform.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hvr_platform_startup(IntPtr ptr);

		[DllImport("HardlightPlatform.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint hvr_platform_getversion(IntPtr ptr);

	

	}
}
