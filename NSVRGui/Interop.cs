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

		public static bool NSVR_SUCCESS(int result )
		{
			return result >= 0;
		}

		public static bool NSVR_FAILURE(int result)
		{
			return !NSVR_SUCCESS(result);
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct NSVR_Quaternion
		{
			public float w;
			public float x;
			public float y;
			public float z;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct NSVR_TrackingUpdate
		{
			public NSVR_Quaternion chest;
			public NSVR_Quaternion left_upper_arm;
			public NSVR_Quaternion left_forearm;
			public NSVR_Quaternion right_upper_arm;
			public NSVR_Quaternion right_forearm;
		}

		public enum NSVR_HandleCommand
		{
			PLAY = 0, PAUSE, RESET, RELEASE
		};

	
	
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct NSVR_ServiceInfo
		{
			public uint ServiceMajor;
			public uint ServiceMinor;
		};
		

		public enum NSVR_EventType
		{
			Basic_Haptic_Event = 1,
			NSVR_EventType_MAX = 65535
		};

		public enum NSVR_PlaybackCommand
		{
			Play = 0,
			Pause,
			Reset
		}
		public enum NSVR_DeviceStatus
		{
			Unknown = 0,
			Connected = 1,
			Disconnected = 2
		}

		public enum NSVR_DeviceConcept
		{
			Unknown = 0,
			Suit,
			Controller,
			Headwear,
			Gun,
			Sword
		}

		//[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct NSVR_DeviceInfo
		{
			public UInt32 Id;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public char[] Name;
			public NSVR_DeviceConcept Concept;
			public NSVR_DeviceStatus Status;
		};

		public enum NSVR_NodeType
		{
			Unknown = 0,
			Haptic,
			LED,
			InertialTracker,
			AbsoluteTracker
		}

		public struct NSVR_NodeInfo
		{
			public UInt32 Id;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public char[] Name;
			public NSVR_NodeType Type;

		}
		public struct NSVR_DeviceInfo_Iter
		{
			public IntPtr _internal;
			public NSVR_DeviceInfo DeviceInfo;
		}
		public struct NSVR_NodeInfo_Iter
		{
			public IntPtr _internal;
			public NSVR_NodeInfo NodeInfo;
		}

		public enum NSVR_EventKey
		{
			Invalid = 0,
			Time_Float,

			SimpleHaptic_Duration_Float = 1000,
			SimpleHaptic_Strength_Float,
			SimpleHaptic_Effect_Int,
			SimpleHaptic_Regions_UInt32s,
			SimpleHaptic_Nodes_UInt32s,

			Max = 2147483647

		}

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_Create(ref IntPtr systemPtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void NSVR_System_Release(ref IntPtr value);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint NSVR_GetVersion();

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_IsCompatibleDLL();

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_GetServiceInfo(IntPtr systemPtr, ref NSVR_ServiceInfo infoPtr);

		/* Haptics Engine */

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_Haptics_Pause(IntPtr systemPtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_Haptics_Resume(IntPtr systemPtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_Haptics_Destroy(IntPtr systemPtr);

		/* Devices */


		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_DeviceInfo_Iter_Init(ref NSVR_DeviceInfo_Iter iter);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool NSVR_DeviceInfo_Iter_Next(ref NSVR_DeviceInfo_Iter iter, IntPtr system);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_NodeInfo_Iter_Init(ref NSVR_NodeInfo_Iter iter);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool NSVR_NodeInfo_Iter_Next(ref NSVR_NodeInfo_Iter iter, UInt32 device_id, IntPtr system);

		/* Tracking */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_Tracking_Poll(IntPtr ptr, ref NSVR_TrackingUpdate updatePtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_Tracking_Enable(IntPtr ptr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_System_Tracking_Disable(IntPtr ptr);


		/* Timeline API */

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Event_Create(ref IntPtr eventPtr, NSVR_EventType type);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void NSVR_Event_Release(ref IntPtr eventPtr);

		[DllImport("Hardlight.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Event_SetFloat(IntPtr eventPtr, NSVR_EventKey key, float value);

		[DllImport("Hardlight.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Event_SetInt(IntPtr eventPtr, NSVR_EventKey key, int value);

		[DllImport("Hardlight.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Event_SetUInt32s(IntPtr eventPtr, NSVR_EventKey key, [In, Out] UInt32[] values, uint length);

		/* Timelines */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Timeline_Create(ref IntPtr eventListPtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void NSVR_Timeline_Release(ref IntPtr listPtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Timeline_AddEvent(IntPtr list, IntPtr eventPtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_Timeline_Transmit(IntPtr timeline, IntPtr systemPtr, IntPtr handlePr);

		/* Playback */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_PlaybackHandle_Create(ref IntPtr handlePtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int NSVR_PlaybackHandle_Command(IntPtr handlePtr, NSVR_PlaybackCommand command);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void NSVR_PlaybackHandle_Release(ref IntPtr handlePtr);


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
