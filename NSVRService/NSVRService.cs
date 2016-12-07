using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace NSVRService
{
	public partial class NSVRService : ServiceBase
	{
		private IntPtr _ptr;

		public NSVRService()
		{
			InitializeComponent();
			/*if (!System.Diagnostics.EventLog.SourceExists("MySource"))
			{
				System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
			}

			eventLog1.Source = "MySource";
			eventLog1.Log = "MyNewLog";
			*/
		}

		internal void TestStartupAndStop(string[] args)
		{
			this.OnStart(args);
			Console.ReadLine();
			this.OnStop();
		}
		
		protected override void OnStart(string[] args)
		{
			//System.Diagnostics.Debugger.Launch();


			_ptr = Interop.NSEngine_Create();
			Interop.NSEngine_StartThread(_ptr);
		

		}

		
		protected override void OnStop()
		{
		
	
			Interop.NSEngine_Shutdown(_ptr);
			Thread.Sleep(100);
			Interop.NSEngine_Destroy(_ptr);
		}
	}
}
