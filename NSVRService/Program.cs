using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NSVRService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// 
		static void Main(string[] args)
		{
			System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

			if (Environment.UserInteractive)
			{
				NSVRService service1 = new NSVRService();
				service1.TestStartupAndStop(args);
			}else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[]
				{
				new NSVRService()
				};
				ServiceBase.Run(ServicesToRun);
			}
		}
		
	}
}
