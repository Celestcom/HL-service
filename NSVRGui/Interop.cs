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
		public struct HLVR_System { }
		public struct HLVR_EventData { }
		public struct HLVR_Timeline { }
		public struct HLVR_PlaybackHandle { }
		public struct HLVR_Effect { }

		internal const int SUBREGION_BLOCK_SIZE = 1000000;

		public static bool OK(int result)
		{
			return result >= 0;
		}

		public static bool FAIL(int result)
		{
			return !OK(result);
		}


		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_Quaternion
		{
			public float w;
			public float x;
			public float y;
			public float z;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_TrackingUpdate
		{
			public HLVR_Quaternion chest;
			public HLVR_Quaternion left_upper_arm;
			public HLVR_Quaternion left_forearm;
			public HLVR_Quaternion right_upper_arm;
			public HLVR_Quaternion right_forearm;
		}


		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_PlatformInfo
		{
			public uint MajorVersion;
			public uint MinorVersion;
		};

		public enum HLVR_EventKey
		{
			Unknown = 0,
			/* Required keys*/
			Time_Float,

			SimpleHaptic_Duration_Float = 1000,
			SimpleHaptic_Strength_Float,
			SimpleHaptic_Effect_Int,
			SimpleHaptic_Region_UInt32s,
			SimpleHaptic_Nodes_UInt32s
		}


		public enum HLVR_EventType
		{
			Unknown = 0,
			SimpleHaptic = 1,
		};


		public enum HLVR_DeviceStatus
		{
			Unknown = 0,
			Connected = 1,
			Disconnected = 2
		}

		public enum HLVR_DeviceConcept
		{
			Unknown = 0,
			Suit,
			Controller,
			Headwear,
			Gun,
			Sword
		}

		public enum HLVR_NodeConcept
		{
			Unknown = 0,
			Haptic,
			LED,
			InertialTracker,
			AbsoluteTracker,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_DeviceInfo
		{
			public UInt32 Id;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public char[] Name;
			public HLVR_DeviceConcept Concept;
			public HLVR_DeviceStatus Status;
		};

		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_DeviceIterator
		{
			public IntPtr _internal;
			public HLVR_DeviceInfo DeviceInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_NodeInfo
		{
			public UInt32 Id;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public char[] Name;
			public HLVR_NodeConcept Concept;
		}
	
		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_NodeIterator
		{
			public IntPtr _internal;
			public HLVR_NodeInfo NodeInfo;
		}

		public enum HLVR_EffectInfo_State
		{
			Unknown = 0,
			Playing,
			Paused,
			Idle
		}

		public enum HLVR_Waveform
		{
			Unknown = 0,
			Bump = 1,
			Buzz = 2,
			Click = 3,
			Fuzz = 5,
			Hum = 6,
			Pulse = 8,
			Tick = 11,
			Double_Click = 4,
			Triple_Click = 16,

		}

	



		[StructLayout(LayoutKind.Sequential)]
		public struct HLVR_EffectInfo
		{
			public float Duration;
			public float Elapsed;
			HLVR_EffectInfo_State PlaybackState;
		};

		/* Agent functions */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_System_Create(HLVR_System** agent);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void HLVR_System_Destroy(HLVR_System** agent);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static unsafe extern int HLVR_System_SuspendEffects(HLVR_System* agent);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_System_ResumeEffects(HLVR_System* agent);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_System_CancelEffects(HLVR_System* agent);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_System_GetPlatformInfo(HLVR_System* agent, ref HLVR_PlatformInfo infoPtr);


		/* Versioning */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint HLVR_Version_Get();

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_Version_IsCompatibleDLL();


		/* Device enumeration */

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_DeviceIterator_Init(ref HLVR_DeviceIterator iter);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_DeviceIterator_Next(ref HLVR_DeviceIterator iter, HLVR_System* system);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_NodeIterator_Init(ref HLVR_NodeIterator iter);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_NodeIterator_Next(ref HLVR_NodeIterator iter, UInt32 deviceId, HLVR_System* system);

		/* Events */

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_EventData_Create(HLVR_EventData** eventData);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void HLVR_EventData_Destroy(HLVR_EventData** eventData);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_EventData_SetFloat(HLVR_EventData* eventData, HLVR_EventKey key, float value);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_EventData_SetInt(HLVR_EventData* eventData, HLVR_EventKey key, int value);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_EventData_SetUInt32s(HLVR_EventData* eventData, HLVR_EventKey key, [In, Out] UInt32[] values, uint length);

		/* Timelines */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Timeline_Create(HLVR_Timeline** timeline);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void HLVR_Timeline_Destroy(HLVR_Timeline** timeline);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Timeline_AddEvent(HLVR_Timeline* timeline, float timeOffsetSeconds, HLVR_EventData* data, HLVR_EventType eventType);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Timeline_Transmit(HLVR_Timeline* timeline, HLVR_System* agent, HLVR_Effect* effect);

		/* Playback */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Effect_Create(HLVR_Effect** effect);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void HLVR_Effect_Destroy(HLVR_Effect** effect);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Effect_Play(HLVR_Effect* effect);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Effect_Pause(HLVR_Effect* effect);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Effect_Reset(HLVR_Effect* effect);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Effect_GetInfo(HLVR_Effect* effect, ref HLVR_EffectInfo info);


		/* Experimental APIs */

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_Immediate_Set(HLVR_System* agent, [In, Out] UInt16[] intensities, [In, Out] UInt32[] areas, int length);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_BodyView_Create(ref IntPtr body);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_BodyView_Release(ref IntPtr body);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_BodyView_Poll(IntPtr body, HLVR_System* system);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_BodyView_GetNodeCount(IntPtr body, ref UInt32 outNodeCount);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_BodyView_GetNodeType(IntPtr body, UInt32 nodeIndex, ref UInt32 outType);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_BodyView_GetNodeRegion(IntPtr body, UInt32 nodeIndex, ref UInt32 outRegion);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int HLVR_BodyView_GetIntensity(IntPtr body, UInt32 nodeIndex, ref float outIntensity);



		/* Tracking */
		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_System_PollTracking(HLVR_System* agent, ref HLVR_TrackingUpdate updatePtr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_System_EnableTracking(HLVR_System* ptr);

		[DllImport("Hardlight.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int HLVR_System_DisableTracking(HLVR_System* ptr);


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
