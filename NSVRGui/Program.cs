using System;
using System.Collections.Generic;
using System.Linq;
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
		private NotifyIcon trayIcon;
		private ServiceController sc;
		private System.Drawing.Icon iconImage;
		public MyCustomApplicationContext()
		{
			sc = new ServiceController();
			sc.ServiceName = "NullSpace VR Runtime";
			iconImage = Properties.Resources.TrayIcon
			// Initialize Tray Icon
			trayIcon = new NotifyIcon()
			{
				Icon = iconImage,
				ContextMenu = new ContextMenu(new MenuItem[] {
					new MenuItem("Enable Suit", StartService),
					new MenuItem("Disable Suit", StopService),
					new MenuItem("Test Suit"),
					new MenuItem("Exit", Exit)
				}),
				Visible = true
			};
		}

		void StartService(object sender, EventArgs e)
		{
			if (sc.Status == ServiceControllerStatus.Stopped)
			{
				try
				{
					sc.Start();
					sc.WaitForStatus(ServiceControllerStatus.Running);
				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Could not start the NullSpace Runtime!");
				} catch(Exception)
				{
					int i = 3;
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
					sc.WaitForStatus(ServiceControllerStatus.Stopped);
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

			Application.Exit();
		}
	}
}
