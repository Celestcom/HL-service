using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NSVRGui
{
	public static class Interop
	{

		public enum NSVR_HandleCommand
		{
			PLAY = 0, PAUSE, RESET, RELEASE
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct NSVR_System_Status
		{
			public int ConnectedToService;
			public int ConnectedToSuit;
		};

		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr NSVR_System_Create();

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void NSVR_System_Release(IntPtr value);

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern uint NSVR_GetVersion();

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int NSVR_IsCompatibleDLL();

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern uint NSVR_System_GenerateHandle(IntPtr system);

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]

		public static extern void NSVR_System_DoHandleCommand(IntPtr ptr, uint handle, NSVR_HandleCommand command);


		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]

		public static extern int  NSVR_System_PollStatus(IntPtr ptr, ref NSVR_System_Status status);


		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr NSVR_System_GetError(IntPtr value);

		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void NSVR_FreeError(IntPtr value);


	

	}
}
