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


		public enum NSVR_EventType
		{
			BASIC_HAPTIC_EVENT = 1,
			NSVR_EventType_MAX = 65535
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

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr  NSVR_Event_Create(NSVR_EventType type);

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void  NSVR_Event_Release(IntPtr eventPtr);

		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern UInt32 NSVR_Event_SetFloat(IntPtr eventptr, string key, float value);

		[DllImport("NSVRPlugin.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern UInt32 NSVR_Event_SetInteger(IntPtr eventPtr, string key, int value);

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr NSVR_EventList_Create();

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void NSVR_EventList_Release(IntPtr listPtr);

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern UInt32 NSVR_EventList_AddEvent(IntPtr listPtr, IntPtr eventPtr);

		[DllImport("NSVRPlugin.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern UInt32  NSVR_EventList_Bind(IntPtr systemPtr, IntPtr listPtr, UInt32 handle);


		public enum AreaFlag
		{
			None,
			Forearm_Left = 1 << 0,
			Upper_Arm_Left = 1 << 1,
			Shoulder_Left = 1 << 2,
			Back_Left = 1 << 3,
			Chest_Left = 1 << 4,
			Upper_Ab_Left = 1 << 5,
			Mid_Ab_Left = 1 << 6,
			Lower_Ab_Left = 1 << 7,

			Forearm_Right = 1 << 16,
			Upper_Arm_Right = 1 << 17,
			Shoulder_Right = 1 << 18,
			Back_Right = 1 << 19,
			Chest_Right = 1 << 20,
			Upper_Ab_Right = 1 << 21,
			Mid_Ab_Right = 1 << 22,
			Lower_Ab_Right = 1 << 23,
			Forearm_Both = Forearm_Left | Forearm_Right,
			Upper_Arm_Both = Upper_Arm_Left | Upper_Arm_Right,
			Shoulder_Both = Shoulder_Left | Shoulder_Right,
			Back_Both = Back_Left | Back_Right,
			Chest_Both = Chest_Left | Chest_Right,
			Upper_Ab_Both = Upper_Ab_Left | Upper_Ab_Right,
			Mid_Ab_Both = Mid_Ab_Left | Mid_Ab_Right,
			Lower_Ab_Both = Lower_Ab_Left | Lower_Ab_Right,
			Left_All = 0x000000FF,
			Right_All = 0x00FF0000,
			All_Areas = Left_All | Right_All,
		};


		public enum NSVR_Effect
		{
			Bump = 1,
			Buzz = 2,
			Click = 3,
			Fuzz = 5,
			Hum = 6,
			Pulse = 8,
			Tick = 11,
			Double_Click = 4,
			Triple_Click = 16
		};
		


	}
}
