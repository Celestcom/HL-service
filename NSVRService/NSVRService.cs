using System;
using System.ServiceProcess;
using System.Threading;

namespace NSVRService
{
	public class PlatformInstantiationException : Exception
	{
		public PlatformInstantiationException()
		{
		}

		public PlatformInstantiationException(string message)
        : base(message)
    {
		}

		public PlatformInstantiationException(string message, Exception inner)
        : base(message, inner)
    {
		}
	}

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
			if (Interop.hvr_platform_create(ref _ptr) < 0)
			{
				throw new PlatformInstantiationException("Could not instantiate the runtime!");
			}

			Interop.hvr_platform_startup(_ptr);
		}
		
	
		protected override void OnStop()
		{

			Interop.hvr_platform_shutdown(_ptr);
			Thread.Sleep(500);
			Interop.hvr_platform_destroy(ref _ptr);			
		}
	}
}
