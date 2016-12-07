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
		private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
		private Thread _thread;
		public NSVRService()
		{
			InitializeComponent();
			if (!System.Diagnostics.EventLog.SourceExists("MySource"))
			{
				System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
			}

			eventLog1.Source = "MySource";
			eventLog1.Log = "MyNewLog";

		}

		internal void TestStartupAndStop(string[] args)
		{
			this.OnStart(args);
			Console.ReadLine();
			this.OnStop();
		}
		private void WorkerThreadFunc()
		{
			while (!_shutdownEvent.WaitOne(0))
			{
				Interop.NSEngine_Update(_ptr);
			}
		}
		protected override void OnStart(string[] args)
		{
			//System.Diagnostics.Debugger.Launch();

			eventLog1.WriteEntry("In OnStart 3.0");

			_ptr = Interop.NSEngine_Create();
			_thread = new Thread(WorkerThreadFunc);
			_thread.Name = "NSVR Engine Thread";
			_thread.IsBackground = true;
			_thread.Start();

		}

		
		protected override void OnStop()
		{
			_shutdownEvent.Set();
			if (!_thread.Join(3000))
			{
				_thread.Abort();
			}
			eventLog1.WriteEntry("In OnStop!!");
			Interop.NSEngine_Shutdown(_ptr);
			Thread.Sleep(100);
			Interop.NSEngine_Destroy(_ptr);
		}
	}
}
