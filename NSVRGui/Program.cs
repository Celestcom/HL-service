using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Windows.Forms;

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
		private Timer _checkStatusTimer;
		private NotifyIcon trayIcon;
		private ServiceController sc;
		private IntPtr _plugin;
		private Timer sendHapticDelayed;
		private Dictionary<string, Interop.NSVR_DeviceInfo> devices;
		private ServiceVersion serviceVersion; 

		private MenuItem connectedDevices;
		bool disposed = false;

		IntPtr _testEffectData;
		override protected void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}
			if (disposing)
			{
				Interop.NSVR_Timeline_Release(ref _testEffectData);
				Interop.NSVR_System_Release(ref _plugin);
			}
			disposed = true;
			base.Dispose(disposing);
		}
		~MyCustomApplicationContext()
		{
			Dispose(false);
		}
		public MyCustomApplicationContext()
		{
			serviceVersion = new ServiceVersion();
			devices = new Dictionary<string, Interop.NSVR_DeviceInfo>();
			connectedDevices = new MenuItem("Plugins", new MenuItem[] { });

			if (Interop.NSVR_FAILURE(Interop.NSVR_System_Create(ref _plugin)))
			{
				//bad news
			}

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
			Interop.NSVR_Timeline_Create(ref _testEffectData);
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

		



			sc = new ServiceController();
			sc.ServiceName = "NullSpace VR Runtime";

			var myMenu = new ContextMenu(new MenuItem[] {
					new MenuItem("Enable Runtime", StartService),
					new MenuItem("Disable Runtime", StopService),
					new MenuItem("Test Suit", TestSuit),
					new MenuItem("Version info", VersionInfo),
					connectedDevices,
				new MenuItem("Exit", Exit)
				});

		
			// Initialize Tray Icon
			trayIcon = new NotifyIcon()
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
			sendHapticDelayed = new Timer();
			sendHapticDelayed.Interval = 500;
			sendHapticDelayed.Tick += DelayWhilePluginInitializes_Tick;

		}
		private void CheckStatusSuit(object sender, EventArgs args)
		{

			sc.Refresh();

			try
			{
				var serviceStatus = sc.Status;
				if (serviceStatus == ServiceControllerStatus.Running)
				{
					Interop.NSVR_ServiceInfo serviceInfo = new Interop.NSVR_ServiceInfo();

					
					if (Interop.NSVR_SUCCESS(Interop.NSVR_System_GetServiceInfo(_plugin, ref serviceInfo))) {
						this.serviceVersion.Major = serviceInfo.ServiceMajor;
						this.serviceVersion.Minor = serviceInfo.ServiceMinor;
					}

					Interop.NSVR_DeviceInfo info = new Interop.NSVR_DeviceInfo();

					bool anythingPresent = false;

					var newDevices = new Dictionary<string, Interop.NSVR_DeviceInfo>();
					//while (Interop.NSVR_System_GetNextDevice(_plugin, ref info) > 0)
					//{

					//	newDevices.Add(new string(info.ProductName), info);
					//	anythingPresent = true;
					//}

					foreach (var device in devices)
					{
						if (!newDevices.ContainsKey(device.Key))
						{
							connectedDevices.MenuItems.RemoveByKey(device.Key);
						}

					}
					foreach (var device in newDevices)
					{
						if (!devices.ContainsKey(device.Key))
						{
							MenuItem a = new MenuItem(device.Key);
							a.Name = device.Key;
							connectedDevices.MenuItems.Add(a);
						}
					}
					devices = newDevices;
					if (anythingPresent) {


						trayIcon.Icon = Properties.Resources.TrayIconServiceOnSuitConnected;

					}
					else
					{
						trayIcon.Icon = Properties.Resources.TrayIconServiceOn;

					}
				}
				else
				{
					trayIcon.Icon = Properties.Resources.TrayIconServiceOff;
				}
			}
			catch (System.InvalidOperationException e)
			{
				//loooks like the service was uninstalled, so we should quit the app!
				trayIcon.Visible = false;
				Application.Exit();
			}


		}

		void StartService(object sender, EventArgs e)
		{
	
			if (sc.Status == ServiceControllerStatus.Stopped)
			{
				try
				{
					sc.Start();
					sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 5));
					trayIcon.Icon = Properties.Resources.TrayIconServiceOn;

				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Could not start the NullSpace Runtime!");
				} 
				catch (System.ServiceProcess.TimeoutException)
				{
					MessageBox.Show("Took too long to start the NullSpace Runtime! Runtime is stopped.");
				}
			}
		}

		void StopService(object sender, EventArgs e)
		{
			if (sc.Status == ServiceControllerStatus.Running)
			{
				try
				{
					sc.Stop();
					sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0,0,5));
					trayIcon.Icon = Properties.Resources.TrayIconServiceOff;
				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Could not stop the NullSpace Runtime!");
				}
				catch (System.ServiceProcess.TimeoutException)
				{
					MessageBox.Show("Took too long to stop the NullSpace Runtime! ");
				}
			}
		}

		void TestSuit(object sender, EventArgs e)
		{
			if (sc.Status != ServiceControllerStatus.Running)
			{
				StartService(null, null);
				
				sendHapticDelayed.Start();
			} else
			{
				CreateAndPlayHaptic();
			}
			


		}
		private void CreateAndPlayHaptic()
		{
			IntPtr playbackHandle = IntPtr.Zero;
			Interop.NSVR_PlaybackHandle_Create(ref playbackHandle);

			Interop.NSVR_Timeline_Transmit( _testEffectData, _plugin, playbackHandle);
			Interop.NSVR_PlaybackHandle_Command(playbackHandle, Interop.NSVR_PlaybackCommand.Play);
			Interop.NSVR_PlaybackHandle_Release(ref playbackHandle);

		}
		private void DelayWhilePluginInitializes_Tick(object sender, EventArgs e)
		{
			CreateAndPlayHaptic();
			sendHapticDelayed.Stop();
		}

	

		void VersionInfo(object sender, EventArgs e)
		{
			
			VersionInfo v = new VersionInfo(this.serviceVersion);

			v.Show();
			

		}
		void Exit(object sender, EventArgs e)
		{
			_checkStatusTimer.Stop();
			
			// Hide tray icon, otherwise it will remain shown until user mouses over it
			trayIcon.Visible = false;
			StopService(null, null);
			Application.Exit();
		}
	}
}
