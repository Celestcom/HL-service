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

		[DllImport("NSVREngine", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr NSEngine_Create();

		[DllImport("NSVREngine", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void NSEngine_Destroy(IntPtr ptr);

		[DllImport("NSVREngine", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void NSEngine_Update(IntPtr ptr);

		[DllImport("NSVREngine", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern bool NSEngine_Shutdown(IntPtr ptr);

	}
}
