using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
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
		private uint _startupRoutineHandle = 4294967290;
		bool disposed = false;
		override protected void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}
			if (disposing)
			{
				Interop.NSVR_Delete(_plugin);
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
			_checkStatusTimer.Interval = 180;
			_checkStatusTimer.Tick += new EventHandler(CheckStatusSuit);
			_checkStatusTimer.Start();

			_plugin = Interop.NSVR_Create();

			sendHapticDelayed = new Timer();
			sendHapticDelayed.Interval = 500;
			sendHapticDelayed.Tick += DelayWhilePluginInitializes_Tick;

		}
		private void CheckStatusSuit(object sender, EventArgs args)
		{

			_checkStatusTimer.Stop();
			sc.Refresh();

			try
			{
				var serviceStatus = sc.Status;
				if (serviceStatus == ServiceControllerStatus.Running)
				{
					int status = Interop.NSVR_PollStatus(_plugin);
					//todo: check bug for not having right status?
					trayIcon.Icon = status == 2 ? Properties.Resources.TrayIconServiceOnSuitConnected : Properties.Resources.TrayIconServiceOn;
				}
				else
				{
					trayIcon.Icon = Properties.Resources.TrayIconServiceOff;
				}
				_checkStatusTimer.Start();
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
			Interop.NSVR_CreateHaptic(_plugin, _startupRoutineHandle, Properties.Resources.StartupRoutine, (uint)Properties.Resources.StartupRoutine.Length);
			Interop.NSVR_HandleCommand(_plugin, _startupRoutineHandle, 2);
			Interop.NSVR_HandleCommand(_plugin, _startupRoutineHandle, 0);
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
			// Hide tray icon, otherwise it will remain shown until user mouses over it
			trayIcon.Visible = false;
			StopService(null, null);
			Application.Exit();
		}
	}
}
