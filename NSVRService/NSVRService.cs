using System;
using System.ServiceProcess;
using System.Threading;

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

			_ptr = Interop.NSVR_Driver_Create();
			Interop.NSVR_Driver_StartThread(_ptr);

		}
		
	
		protected override void OnStop()
		{
	
			Interop.NSVR_Driver_Shutdown(_ptr);
			Thread.Sleep(500);
			Interop.NSVR_Driver_Destroy(_ptr);

			
		}
	}
}
