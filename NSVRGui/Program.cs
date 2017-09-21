using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Linq;
namespace NSVRGui
{

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MyCustomApplicationContext());
		}
	}

	public class ServiceVersion
	{
		public uint Major;
		public uint Minor;
		public override string ToString()
		{
			return string.Format("{0}.{1}", Major, Minor);
		}
	}

	public class MyCustomApplicationContext : ApplicationContext
	{
		/// <summary>
		/// Used to periodically poll the plugin for the status of the service, and any new devices
		/// </summary>
		private Timer _checkStatusTimer;

		/// <summary>
		/// The little Hardlight icon in the tray. Can be changed to represent different states.
		/// </summary>
		private NotifyIcon _trayIcon;

		/// <summary>
		/// Responsible for starting and stopping the Windows Service for the Hardlight Platform
		/// </summary>
		private ServiceController _serviceController;

		/// <summary>
		/// Raw pointer to our Hardlight.dll instance, used to interact with the system
		/// </summary>
		private IntPtr _pluginPtr;
 
		/// <summary>
		/// How long we delay before playing the test routine. This is needed because
		/// the plugin takes a few moments to initialize under the hood, so a test routine submitted too quickly
		/// will not run.
		/// </summary>
		private Timer _hapticsDelay;

		/// <summary>
		/// Our cached list of devices present in the system. Key is the device name.
		/// </summary>
		private Dictionary<string, Interop.NSVR_DeviceInfo> _cachedDevices;

		/// <summary>
		/// Cached service version information, used to display in the Version Info window.
		/// </summary>
		private ServiceVersion _cachedServiceVersion; 

		/// <summary>
		/// The little menu of devices when you right click on the tray icon
		/// </summary>
		private MenuItem _deviceMenuList;

		/// <summary>
		/// Needed for IDisposable support (because of the interop with native Hardlight.dll)
		/// </summary>
		private bool _disposed = false;

		/// <summary>
		/// Holds NSVR_Timeline used in the test routine
		/// </summary>
		private IntPtr _testRoutineTimeline;

		override protected void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			if (disposing)
			{
				Interop.NSVR_Timeline_Release(ref _testRoutineTimeline);
				Interop.NSVR_System_Release(ref _pluginPtr);
			}
			_disposed = true;
			base.Dispose(disposing);
		}

		~MyCustomApplicationContext()
		{
			Dispose(false);
		}

		public MyCustomApplicationContext()
		{

			_pluginPtr = IntPtr.Zero;

			if (Interop.NSVR_FAILURE(Interop.NSVR_System_Create(ref _pluginPtr)))
			{
				MessageBox.Show("Failed to create the Hardlight plugin! Application will now exit.", "Hardlight Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Exit();
			}


			_cachedServiceVersion = new ServiceVersion();
			_cachedDevices = new Dictionary<string, Interop.NSVR_DeviceInfo>();
			_deviceMenuList = new MenuItem("Devices", new MenuItem[] { });



			Interop.AreaFlag[] order = {
				Interop.AreaFlag.Forearm_Left,
				Interop.AreaFlag.Upper_Arm_Left,
				Interop.AreaFlag.Shoulder_Left,
				Interop.AreaFlag.Back_Left,

				Interop.AreaFlag.Chest_Left,
				Interop.AreaFlag.Upper_Ab_Left,
				Interop.AreaFlag.Mid_Ab_Left,
				Interop.AreaFlag.Lower_Ab_Left,

				Interop.AreaFlag.Lower_Ab_Right,
				Interop.AreaFlag.Mid_Ab_Right,
				Interop.AreaFlag.Upper_Ab_Right,
				Interop.AreaFlag.Chest_Right,

				Interop.AreaFlag.Back_Right,
				Interop.AreaFlag.Shoulder_Right,
				Interop.AreaFlag.Upper_Arm_Right,
				Interop.AreaFlag.Forearm_Right

			};
			Interop.NSVR_Timeline_Create(ref _testRoutineTimeline);
			float offset = 0.0f;
			//foreach (var flag in order)
			//{
			//	IntPtr myEvent = IntPtr.Zero;
			//	Interop.NSVR_Event_Create(ref myEvent, Interop.NSVR_EventType.Basic_Haptic_Event);

			//	Interop.NSVR_Event_SetFloat(myEvent, "duration", 0.0f);
			//	Interop.NSVR_Event_SetFloat(myEvent, "strength", 1.0f);
			//	Interop.NSVR_Event_SetUInt32s(myEvent, "area", (int)flag);
			//	Interop.NSVR_Event_SetInteger(myEvent, "effect", (int) Interop.NSVR_Effect.Click); 
			//	Interop.NSVR_Event_SetFloat(myEvent, "time", offset);
			//	Interop.NSVR_Timeline_AddEvent(_testEffectData, myEvent);

			//	Interop.NSVR_Event_Release(ref myEvent);
			//	offset += 0.1f;
			//}





			_serviceController = new ServiceController();
			_serviceController.ServiceName = "Hardlight VR Runtime";

			var myMenu = new ContextMenu(new MenuItem[] {
					new MenuItem("Enable Runtime", StartService),
					new MenuItem("Disable Runtime", StopService),
					new MenuItem("Test Suit", TestSuit),
					new MenuItem("Version info", VersionInfo),
					_deviceMenuList,
				new MenuItem("Exit", new EventHandler((object o, EventArgs e) => { Exit(); }))
			});

		
			// Initialize Tray Icon
			_trayIcon = new NotifyIcon()
			{
				Icon = Properties.Resources.TrayIconServiceOff,
				ContextMenu = myMenu,
				Visible = true
			};
			
			_checkStatusTimer = new Timer();
			_checkStatusTimer.Interval = 125;
			_checkStatusTimer.Tick += new EventHandler(CheckStatusSuit);
			_checkStatusTimer.Start();

			
			StartService(null, null);
			_hapticsDelay = new Timer();
			_hapticsDelay.Interval = 500;
			_hapticsDelay.Tick += DelayWhilePluginInitializes_Tick;

		}
		private bool isServiceActuallyConnected()
		{
			Interop.NSVR_ServiceInfo serviceInfo = new Interop.NSVR_ServiceInfo();

			if (Interop.NSVR_SUCCESS(Interop.NSVR_System_GetServiceInfo(_pluginPtr, ref serviceInfo)))
			{
				_cachedServiceVersion.Major = serviceInfo.ServiceMajor;
				_cachedServiceVersion.Minor = serviceInfo.ServiceMinor;
				return true;
			}

			return false;
		}

		private Dictionary<string, Interop.NSVR_DeviceInfo> fetchKnownDevices()
		{
			Interop.NSVR_DeviceInfo_Iter iter = new Interop.NSVR_DeviceInfo_Iter();
			Interop.NSVR_DeviceInfo_Iter_Init(ref iter);

			var newDevices = new Dictionary<string, Interop.NSVR_DeviceInfo>();
			while (Interop.NSVR_DeviceInfo_Iter_Next(ref iter, _pluginPtr))
			{
				newDevices.Add(new string(iter.DeviceInfo.Name), iter.DeviceInfo);
			}

			return newDevices;
		}

		private void removeUnrecognizedDevicesFromMenu(Dictionary<string, Interop.NSVR_DeviceInfo> newDevices)
		{
			foreach (var device in _cachedDevices)
			{
				if (!newDevices.ContainsKey(device.Key))
				{
					_deviceMenuList.MenuItems.RemoveByKey(device.Key);
				}
			}
		}

		private void addRecognizedDevicesToMenu(Dictionary<string, Interop.NSVR_DeviceInfo> newDevices)
		{
			foreach (var device in newDevices)
			{
				if (!_cachedDevices.ContainsKey(device.Key))
				{
					MenuItem a = new MenuItem(device.Key);
					a.Name = device.Key;
					_deviceMenuList.MenuItems.Add(a);
				}
			}
		}
		private void CheckStatusSuit(object sender, EventArgs args)
		{
			_serviceController.Refresh();

			try
			{
				var serviceStatus = _serviceController.Status;

				if (serviceStatus != ServiceControllerStatus.Running)
				{
					_trayIcon.Icon = Properties.Resources.TrayIconServiceOff;
					return;
				}

				if (isServiceActuallyConnected())
				{
					var newDevices = fetchKnownDevices();
					removeUnrecognizedDevicesFromMenu(newDevices);
					addRecognizedDevicesToMenu(newDevices);

					_cachedDevices = newDevices;

					bool anythingConnected = _cachedDevices.Any(kvp => kvp.Value.Status == Interop.NSVR_DeviceStatus.Connected);

					_trayIcon.Icon = anythingConnected? 
							Properties.Resources.TrayIconServiceOnSuitConnected :
							Properties.Resources.TrayIconServiceOn;
				}
			}
			catch (System.InvalidOperationException e)
			{
				//Perhaps the service was uninstalled..?
				_trayIcon.Visible = false;
				Exit();
			}
		}

		void StartService(object sender, EventArgs e)
		{
	
			if (_serviceController.Status == ServiceControllerStatus.Stopped)
			{
				try
				{
					_serviceController.Start();
					_serviceController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 5));
					_trayIcon.Icon = Properties.Resources.TrayIconServiceOn;

				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Could not start the Hardlight Runtime!");
				} 
				catch (System.ServiceProcess.TimeoutException)
				{
					MessageBox.Show("Took too long to start the Hardlight Runtime! Runtime is stopped.");
				}
			}
		}

		void StopService(object sender, EventArgs e)
		{
			if (_serviceController.Status == ServiceControllerStatus.Running)
			{
				try
				{
					_serviceController.Stop();
					_serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0,0,5));
					_trayIcon.Icon = Properties.Resources.TrayIconServiceOff;
				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Could not stop the Hardlight Runtime!");
				}
				catch (System.ServiceProcess.TimeoutException)
				{
					MessageBox.Show("Took too long to stop the Hardlight Runtime! ");
				}
			}
		}

		void TestSuit(object sender, EventArgs e)
		{
			if (_serviceController.Status != ServiceControllerStatus.Running)
			{
				StartService(null, null);
				_hapticsDelay.Start();
			} else
			{
				CreateAndPlayHaptic();
			}
		}

		private void CreateAndPlayHaptic()
		{
			IntPtr playbackHandle = IntPtr.Zero;
			Interop.NSVR_PlaybackHandle_Create(ref playbackHandle);

			Interop.NSVR_Timeline_Transmit( _testRoutineTimeline, _pluginPtr, playbackHandle);
			Interop.NSVR_PlaybackHandle_Command(playbackHandle, Interop.NSVR_PlaybackCommand.Play);
			Interop.NSVR_PlaybackHandle_Release(ref playbackHandle);

		}

		private void DelayWhilePluginInitializes_Tick(object sender, EventArgs e)
		{
			CreateAndPlayHaptic();
			_hapticsDelay.Stop();
		}

		private void VersionInfo(object sender, EventArgs e)
		{
			VersionInfo v = new VersionInfo(this._cachedServiceVersion);
			v.Show();
		}
		private void Exit()
		{
			_checkStatusTimer.Stop();
			
			// Hide tray icon, otherwise it will remain shown until user mouses over it
			_trayIcon.Visible = false;
			StopService(null, null);
			Application.Exit();
		}
	}
}
