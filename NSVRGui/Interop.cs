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

		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr NSVR_Create();


		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern int NSVR_PollStatus(IntPtr value);

		
		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void NSVR_Delete(IntPtr value);

		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr NSVR_GetError(IntPtr value);

		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void NSVR_FreeError(IntPtr value);


		//[DllImport("NSVRPlugin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		//public static extern void NSVR_HandleCommand(IntPtr value, uint handle, short command);

		//[DllImport("NSVRPlugin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		//public static extern void NSVR_CreateHaptic(IntPtr value, uint handle, byte[] data, uint size);

	}
}
