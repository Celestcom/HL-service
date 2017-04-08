using System;
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

	public class MyCustomApplicationContext : ApplicationContext
	{
		private Timer _checkStatusTimer;
		private NotifyIcon trayIcon;
		private ServiceController sc;
		private IntPtr _plugin;
		private Timer sendHapticDelayed;
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
			Interop.NSVR_Timeline_Create(ref _testEffectData, _plugin);
			float offset = 0.0f;
			foreach (var flag in order)
			{
				IntPtr myEvent = IntPtr.Zero;
				Interop.NSVR_Event_Create(ref myEvent, Interop.NSVR_EventType.Basic_Haptic_Event);

				Interop.NSVR_Event_SetFloat(myEvent, "duration", 0.0f);
				Interop.NSVR_Event_SetFloat(myEvent, "strength", 1.0f);
				Interop.NSVR_Event_SetInteger(myEvent, "area", (int)flag);
				Interop.NSVR_Event_SetInteger(myEvent, "effect", (int) Interop.NSVR_Effect.Click); 
				Interop.NSVR_Event_SetFloat(myEvent, "time", offset);
				Interop.NSVR_Timeline_AddEvent(_testEffectData, myEvent);

				Interop.NSVR_Event_Release(ref myEvent);
				offset += 0.1f;
			}

		



			sc = new ServiceController();
			sc.ServiceName = "NullSpace VR Runtime";

			var myMenu = new ContextMenu(new MenuItem[] {
					new MenuItem("Enable Suit", StartService),
					new MenuItem("Disable Suit", StopService),
					new MenuItem("Test Suit", TestSuit),
					new MenuItem("Version info", VersionInfo),

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
					Interop.NSVR_DeviceInfo deviceinfo = new Interop.NSVR_DeviceInfo();

					if (Interop.NSVR_SUCCESS(Interop.NSVR_System_GetDeviceInfo(_plugin, ref deviceinfo))) {
						trayIcon.Icon = Properties.Resources.TrayIconServiceOnSuitConnected;
					} else
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

			Interop.NSVR_Timeline_Transmit( _testEffectData, playbackHandle);
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
			
			VersionInfo v = new VersionInfo();

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
