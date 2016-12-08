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
		private NamedPipeServerStream _pipeServer;
		private Thread _monitorThread;
		private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
		public NSVRService()
		{
			InitializeComponent();
		//	_pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.Out, 1, //PipeTransmissionMode.Message, PipeOptions.Asynchronous);
			
			//_pipeServer.BeginWaitForConnection(HandlePipeConnection, null);
		//	System.Diagnostics.Debugger.Launch();
		}
/*
		private void HandlePipeConnection(IAsyncResult ar)
		{
			//	_pipeServer.EndWaitForConnection(ar);
			StreamWriter sw = new StreamWriter(_pipeServer);
			
				if (_pipeServer.CanWrite)
				{
					
				System.Threading.Thread.Sleep(500);
				sw.WriteLine("HELLO WORLD!");

			}

		}*/
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

		//	_monitorThread = new Thread(MonitorPipe);
		//	_monitorThread.IsBackground = true;
		//	_monitorThread.Name = "Monitor Thread";
		//	_monitorThread.Start();
		}
		
		/*private void MonitorPipe()
		{
			StreamWriter sw = new StreamWriter(_pipeServer);
			sw.AutoFlush = true;
			while (!_shutdownEvent.WaitOne(0))
			{
				try
				{
					sw.WriteLine("Hi");
				}
				catch(IOException e)
				{
					//couldn't write
				}
			}
		}
		*/
		protected override void OnStop()
		{
	
			Interop.NSEngine_Shutdown(_ptr);
			Thread.Sleep(300);
			Interop.NSEngine_Destroy(_ptr);

			_shutdownEvent.Set();
			//if (!_monitorThread.Join(300))
			//{
				//_monitorThread.Abort();
			//}

		}
	}
}
