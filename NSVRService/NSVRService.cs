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
using System.IO.Pipes;
using System.IO;

namespace NSVRService
{
	public partial class NSVRService : ServiceBase
	{
		private IntPtr _ptr;
		private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
		public NSVRService()
		{
			InitializeComponent();
		
		}

		internal void TestStartupAndStop(string[] args)
		{
			this.OnStart(args);
			Console.ReadLine();
			this.OnStop();
		}
		
		protected override void OnStart(string[] args)
		{
		
			_ptr = Interop.NSEngine_Create();
			Interop.NSEngine_StartThread(_ptr);

		}
		
	
		protected override void OnStop()
		{
	
			Interop.NSEngine_Shutdown(_ptr);
			Thread.Sleep(300);
			Interop.NSEngine_Destroy(_ptr);

			_shutdownEvent.Set();
			
		}
	}
}
