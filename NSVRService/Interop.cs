using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NSVRService
{
	public static class Interop
	{

		[DllImport("NS_Unreal_SDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr NSEngine_Create();

		[DllImport("NS_Unreal_SDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void NSEngine_Destroy(IntPtr ptr);

		[DllImport("NS_Unreal_SDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void NSEngine_Update(IntPtr ptr);

		[DllImport("NS_Unreal_SDK", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern bool NSEngine_Shutdown(IntPtr ptr);

	}
}
