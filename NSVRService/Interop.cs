using System;
using System.Runtime.InteropServices;

namespace NSVRService
{
	public static class Interop
	{

		[DllImport("NSVREngine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr NSVR_Driver_Create();

		[DllImport("NSVREngine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void NSVR_Driver_Destroy(IntPtr ptr);

	
		[DllImport("NSVREngine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool NSVR_Driver_Shutdown(IntPtr ptr);

		[DllImport("NSVREngine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern void NSVR_Driver_StartThread(IntPtr ptr);

		[DllImport("NSVREngine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint NSVR_Driver_GetVersion(IntPtr ptr);

		[DllImport("NSVREngine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Driver_IsCompatibleDLL(IntPtr ptr);

	}
}
