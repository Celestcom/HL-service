using System;
using System.Collections.Generic;
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
		private Timer _myTimer;
		private NotifyIcon trayIcon;
		private ServiceController sc;
		private NamedPipeClientStream _pipeClient;

		private Timer _pipeConnectTimer;
		public MyCustomApplicationContext()
		{
			//Start monitoring for when our service opens its pipe
			//_pipeConnectTimer = new Timer();
			//_pipeConnectTimer.Interval = 500;
			//_pipeConnectTimer.Tick += new EventHandler(ConnectToPipe);
			//_pipeConnectTimer.Start();

			//_pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.In, PipeOptions.Asynchronous);


			//_myTimer = new Timer();

			sc = new ServiceController();
			sc.ServiceName = "NullSpace VR Runtime";
	
			// Initialize Tray Icon
			trayIcon = new NotifyIcon()
			{
				Icon = Properties.Resources.TrayIconServiceOff,
				ContextMenu = new ContextMenu(new MenuItem[] {
					new MenuItem("Enable Suit", StartService),
					new MenuItem("Disable Suit", StopService),
				//	new MenuItem("Test Suit"),
					new MenuItem("Exit", Exit)
				}),
				Visible = true
			};

		
		}
/*
		private void ConnectToPipe(object sender, EventArgs e)
		{
			try
			{
				
				_pipeClient.Connect(400);
				_pipeConnectTimer.Stop();
				var a = new byte[50];
				int x = _pipeClient.Read(a, 0, 10);
			}
			catch (System.TimeoutException)
			{

			} catch (System.IO.IOException)
			{

			}
		}
		*/
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
			}
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
