﻿using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using System.IO;

using Windows.Devices.WiFi;
using System.Threading.Tasks;
using Microsoft.Win32;

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

	public class Version
	{
		public int Major;
		public int Minor;
		public int Patch;
		
		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
		}
	}
	public class DllVersions
	{

		public Version Service;
		public Version Client;

		public DllVersions()
		{
			Service = new Version();
			Client = new Version();

		}
	}

	public class MyCustomApplicationContext : ApplicationContext
	{
		/// <summary>
		/// Used to periodically poll the plugin for the status of the service, and any new devices
		/// </summary>
		private Timer _checkStatusTimer;

		/// <summary>
		/// The little Hardlight icon in the tray. Can be changed to represent different states.
		/// </summary>
		private NotifyIcon _trayIcon;

		/// <summary>
		/// Responsible for starting and stopping the Windows Service for the Hardlight Platform
		/// </summary>
		private ServiceController _serviceController;

		/// <summary>
		/// Raw pointer to our Hardlight.dll instance, used to interact with the system
		/// </summary>
		private unsafe Interop.HLVR_System* _pluginPtr;
 
		/// <summary>
		/// How long we delay before playing the test routine. This is needed because
		/// the plugin takes a few moments to initialize under the hood, so a test routine submitted too quickly
		/// will not run.
		/// </summary>
		private Timer _hapticsDelay;

		/// <summary>
		/// Our cached list of devices present in the system. Key is the device name.
		/// </summary>
		private Dictionary<uint, Interop.HLVR_DeviceInfo> _cachedDevices;

		/// <summary>
		/// Cached service version information, used to display in the Version Info window.
		/// </summary>
		private DllVersions _cachedServiceVersion; 

		/// <summary>
		/// The little menu of devices when you right click on the tray icon
		/// </summary>
		private MenuItem _deviceMenuList;

		private MenuItem _updateMenu;


		private MenuItem _audioMenu;

		private HashSet<string> _wirelessAps;
		private MenuItem _wirelessDeviceMenu;
		private MenuItem _wirelessMenu;

		/// <summary>
		/// Needed for IDisposable support (because of the interop with native Hardlight.dll)
		/// </summary>
		private bool _disposed = false;

		private string _updaterModulePath;


		private string _installPath;

		override protected void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			if (disposing)
			{
				unsafe
				{
					Interop.HLVR_System_Destroy(_pluginPtr);
				}
			}
			_disposed = true;
			base.Dispose(disposing);
		}

		~MyCustomApplicationContext()
		{
			Dispose(false);
		}

		public MyCustomApplicationContext()
		{
			unsafe
			{
				fixed (Interop.HLVR_System** ptr = &_pluginPtr)
				{
					if (Interop.FAIL(Interop.HLVR_System_Create(ptr)))
					{
						MessageBox.Show("Failed to create the Hardlight plugin! Application will now exit.", "Hardlight Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
						Exit();
					}
				}
			}
			_updaterModulePath = Path.Combine(Application.StartupPath, "updater.exe");

			_cachedServiceVersion = new DllVersions();
			uint clientVersion = Interop.HLVR_Version_Get();
			_cachedServiceVersion.Client.Minor = (int) (clientVersion & 0x00FF0000) >> 16;
			_cachedServiceVersion.Client.Major = (int) (clientVersion & 0xFF000000) >> 24;
			_cachedServiceVersion.Client.Patch = (int) (clientVersion & 0x0000FFFF);
			_cachedDevices = new Dictionary<uint, Interop.HLVR_DeviceInfo>();
			_deviceMenuList = new MenuItem("Devices", new MenuItem[] { });
			_wirelessDeviceMenu = new MenuItem("Connect", new MenuItem[] { });

			_serviceController = new ServiceController();
			_serviceController.ServiceName = "Hardlight VR Runtime";

			_wirelessAps = new HashSet<string>();

			_updateMenu = new MenuItem("Updates", new MenuItem[] {
				new MenuItem("Options", UpdateConfiguration),
				new MenuItem("Check now", CheckUpdates),
			});

			_audioMenu = new MenuItem("Audio", new MenuItem[]
			{
				new MenuItem("Enable audio to haptics", EnableAudio),
				new MenuItem("Disable audio to haptics", DisableAudio)
			});

			_wirelessMenu = new MenuItem("Wireless", new MenuItem[]
			{
				new MenuItem("Scan", WifiAsync),
				 _wirelessDeviceMenu
			});
			var myMenu = new ContextMenu(new MenuItem[] {
					new MenuItem("Enable Runtime", StartService),
					new MenuItem("Disable Runtime", StopService),
					new MenuItem("Test Everything", TestSuit),
					_audioMenu,
					_updateMenu,
					new MenuItem("Version info", VersionInfo),
					_deviceMenuList,
					_wirelessMenu,
				new MenuItem("Exit", new EventHandler((object o, EventArgs e) => { Exit(); }))
			});

		
			// Initialize Tray Icon
			_trayIcon = new NotifyIcon()
			{
				Icon = Properties.Resources.TrayIconServiceOff,
				ContextMenu = myMenu,
				Visible = true
			};
			
			_checkStatusTimer = new Timer();
			_checkStatusTimer.Interval = 125;
			_checkStatusTimer.Tick += new EventHandler(CheckStatusSuit);
			_checkStatusTimer.Start();

			
			_hapticsDelay = new Timer();
			_hapticsDelay.Interval = 1000;
			_hapticsDelay.Tick += DelayWhilePluginInitializes_Tick;

			RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Hardlight VR\\Service", false);
			if (rk != null)
			{
				_installPath = (string)rk.GetValue("InstallPath");
			}

		}
		private unsafe bool isServiceActuallyConnected()
		{
			Interop.HLVR_RuntimeInfo serviceInfo = new Interop.HLVR_RuntimeInfo();

			if (Interop.OK(Interop.HLVR_System_GetRuntimeInfo(_pluginPtr, ref serviceInfo)))
			{
				_cachedServiceVersion.Service.Major = serviceInfo.MajorVersion;
				_cachedServiceVersion.Service.Minor = serviceInfo.MinorVersion;
				_cachedServiceVersion.Service.Patch = serviceInfo.PatchVersion;
				return true;
			}

			return false;
		}

		private unsafe Dictionary<uint, Interop.HLVR_DeviceInfo> fetchKnownDevices()
		{
			Interop.HLVR_DeviceIterator iter = new Interop.HLVR_DeviceIterator();
			Interop.HLVR_DeviceIterator_Init(ref iter);

			var newDevices = new Dictionary<uint, Interop.HLVR_DeviceInfo>();
			while (Interop.OK(Interop.HLVR_DeviceIterator_Next(ref iter, _pluginPtr)))
			{
				newDevices.Add(iter.DeviceInfo.Id, iter.DeviceInfo);
			}

			return newDevices;
		}

		private unsafe List<UInt32> fetchAllNodes(UInt32 device_id)
		{
			List<UInt32> nodes = new List<uint>();
			Interop.HLVR_NodeIterator iter = new Interop.HLVR_NodeIterator();
			Interop.HLVR_NodeIterator_Init(ref iter);

			while (Interop.OK(Interop.HLVR_NodeIterator_Next(ref iter, device_id, _pluginPtr)))
			{
				nodes.Add(iter.NodeInfo.Id);
			}

			return nodes;
		}

		private void forgetAllDevices()
		{
			_deviceMenuList.MenuItems.Clear();
			_cachedDevices.Clear();
		}
		private void removeUnrecognizedDevicesFromMenu(Dictionary<uint, Interop.HLVR_DeviceInfo> newDevices)
		{
			foreach (var device in _cachedDevices)
			{
				if (!newDevices.ContainsKey(device.Key))
				{
					string menuKey = deviceToStringKey(device.Value);

					_deviceMenuList.MenuItems.RemoveByKey(menuKey);
				}
			}
		}
		private string deviceToStringKey(Interop.HLVR_DeviceInfo device)
		{
			return string.Format("{0} [{1}]", new string(device.Name), device.Id);
		}

		private void addRecognizedDevicesToMenu(Dictionary<uint, Interop.HLVR_DeviceInfo> newDevices)
		{
			foreach (var device in newDevices)
			{
				if (!_cachedDevices.ContainsKey(device.Key))
				{
					string menuKey = deviceToStringKey(device.Value);
					MenuItem a = new MenuItem(menuKey);
					a.Name = menuKey;
					a.MenuItems.Add(new MenuItem("Test haptics", (object sender, EventArgs e) =>{ CreateAndPlayHaptic(device.Value.Id); }));
					a.MenuItems.Add(new MenuItem("Enable tracking", (object sender, EventArgs e) => { EnableTracking(device.Value.Id); }));
					a.MenuItems.Add(new MenuItem("Disable tracking", (object sender, EventArgs e) => { DisableTracking(device.Value.Id); }));

					_deviceMenuList.MenuItems.Add(a);
				}
			}
		}

		private async Task<List<string>> GetWirelessSuits()
		{
			

			var access = await WiFiAdapter.RequestAccessAsync();
			var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
			if (result.Count >= 1)
			{
				var nwAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);
				await nwAdapter.ScanAsync();
				//because spelling errors
				var nw = nwAdapter.NetworkReport.AvailableNetworks.Where(network => network.Ssid.Contains("NSVR") || network.Ssid.Contains("NVSR:"));

				return nw.Select(suit => suit.Ssid).ToList();
				//if (nw.Count() > 0)
				//{
				//	var suitAP = nw.First();
				//	var pass = SsidToPassword(suitAP.Ssid);
				//	var conn = await nwAdapter.ConnectAsync(suitAP, WiFiReconnectionKind.Automatic, new Windows.Security.Credentials.PasswordCredential("none", "none", pass));

				//	return new Tuple<WiFiConnectionStatus, string>(conn.ConnectionStatus, pass);

				//}
			}

			return new List<string>();
		}
		private async Task<Tuple<Windows.Devices.WiFi.WiFiConnectionStatus, string>> ConnectWifiAndGetPassword(string ssid)
		{

			var access = await WiFiAdapter.RequestAccessAsync();
			var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
			if (result.Count >= 1)
			{
				var nwAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);
				await nwAdapter.ScanAsync();
				//because spelling errors
				var nw = nwAdapter.NetworkReport.AvailableNetworks.Where(network => network.Ssid.Contains(ssid));
				if (nw.Count() > 0)
				{
					var suitAP = nw.First();
					var pass = SsidToPassword(suitAP.Ssid);
					var conn = await nwAdapter.ConnectAsync(suitAP, WiFiReconnectionKind.Automatic, new Windows.Security.Credentials.PasswordCredential("none", "none", pass));

					return new Tuple<WiFiConnectionStatus, string>(conn.ConnectionStatus, pass);

				}
			}
		
			return new Tuple<WiFiConnectionStatus, string>(WiFiConnectionStatus.NetworkNotAvailable, "");
			
		}
		private async void ConnectToWifi(string ap)
		{
			var connectionResult = await ConnectWifiAndGetPassword(ap);

			if (connectionResult.Item1 == WiFiConnectionStatus.Success)
			{
				StopService(null, null);

				string hardlightRuntimeDir = Path.Combine(_installPath, "plugins/hardlight/");

				string json = "{\n\"host\" : \"192.168.4.1\",\n\"port\" : \"23\",\n\"password\" : \"" + connectionResult.Item2 + "\"\n}";



				using (StreamWriter DestinationWriter = File.CreateText(hardlightRuntimeDir + "Wifi.json"))
				{
					await DestinationWriter.WriteAsync(json);
				}

				StartService(null, null);
			}
		}
		private async void WifiAsync(object sender, EventArgs args)
		{
			_wirelessAps.Clear();
			var aps = await GetWirelessSuits();
			foreach (var ap in aps) { _wirelessAps.Add(ap); }
			_wirelessDeviceMenu.MenuItems.Clear();

			foreach (var ap in _wirelessAps) {
				MenuItem a = new MenuItem(ap, (object s, EventArgs e) => { ConnectToWifi(ap);  });
				

				_wirelessDeviceMenu.MenuItems.Add(a);
			}
			

			

		}
		private static string SsidToPassword(string ssid)
		{
			var hexPassword = ssid.Substring(4).Replace(":", String.Empty);
			return CalculatePassword(hexPassword);
		}
		private static byte[] HexStringToHex(string inputHex)
		{
			var resultantArray = new byte[inputHex.Length / 2];
			for (var i = 0; i < resultantArray.Length; i++)
			{
				resultantArray[i] = System.Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
			}
			return resultantArray;
		}
		private static string CalculatePassword(string hexPassword)
		{
			return Convert.ToBase64String(HexStringToHex(hexPassword)).ToLower();
		}

		private void EnableTracking(uint device_id)
		{
			unsafe
			{
				Interop.HLVR_System_Tracking_Enable(_pluginPtr, device_id);
			}
		}
		private void DisableTracking(uint device_id)
		{
			unsafe
			{
				Interop.HLVR_System_Tracking_Disable(_pluginPtr, device_id);
			}
		}
		private void EnableAudio(object sender, EventArgs args)
		{
			unsafe
			{
				UInt32[] chests = new UInt32[2] { 4000000, 5000000 };
				Interop.HLVR_Event* enable = null;
				Interop.HLVR_Event_Create(&enable, Interop.HLVR_EventType.BeginAnalogAudio);
				Interop.HLVR_Event_SetUInt32s(enable, Interop.HLVR_EventKey.Target_Regions_UInt32s, chests, (uint)chests.Length);
				Interop.HLVR_System_PushEvent(_pluginPtr, enable);
				Interop.HLVR_Event_Destroy(enable);
			}
		}

		private void DisableAudio(object sender, EventArgs args)
		{
			unsafe
			{
				UInt32[] chests = new UInt32[2] { 4000000, 5000000  };
				Interop.HLVR_Event* enable = null;
				Interop.HLVR_Event_Create(&enable, Interop.HLVR_EventType.EndAnalogAudio);
				Interop.HLVR_Event_SetUInt32s(enable, Interop.HLVR_EventKey.Target_Regions_UInt32s, chests, (uint)chests.Length);
				Interop.HLVR_System_PushEvent(_pluginPtr, enable);
				Interop.HLVR_Event_Destroy(enable);
			}
		}

		private void UpdateConfiguration(object sender, EventArgs args)
		{
			Process process = Process.Start(_updaterModulePath, "/configure");
			process.Close();
		}

		private void CheckUpdates(object sender, EventArgs args)
		{
			Process process = Process.Start(_updaterModulePath, "/checknow");
			process.Close();
		}

		private void CheckStatusSuit(object sender, EventArgs args)
		{
			if (!isServiceActuallyConnected())
			{
				//_serviceController.Refresh();
				//try
				//{
				//	var serviceStatus = _serviceController.Status;

				//	if (serviceStatus != ServiceControllerStatus.Running)
				//	{
				//		StartService(null, null);
				//		return;
				//	}
				//}
				//catch (System.InvalidOperationException)
				//{
				//	//Perhaps the service was uninstalled..?
				//	_trayIcon.Visible = false;
				//	Exit();
				//}
				_trayIcon.Icon = Properties.Resources.TrayIconServiceOff;
				forgetAllDevices();
			} else
			{
				
				var newDevices = fetchKnownDevices();
				removeUnrecognizedDevicesFromMenu(newDevices);
				addRecognizedDevicesToMenu(newDevices);

				_cachedDevices = newDevices;

				bool anythingConnected = _cachedDevices.Any(kvp => kvp.Value.Status == Interop.HLVR_DeviceStatus.Connected);


				_trayIcon.Icon = anythingConnected ?
						Properties.Resources.TrayIconServiceOnSuitConnected :
						Properties.Resources.TrayIconServiceOn;
			
			}



			
		}

		void StartService(object sender, EventArgs e)
		{
	
			if (_serviceController.Status == ServiceControllerStatus.Stopped)
			{
				try
				{
					_serviceController.Start();
					_serviceController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 5));
					_trayIcon.Icon = Properties.Resources.TrayIconServiceOn;

				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Could not start the Hardlight Runtime!");
				} 
				catch (System.ServiceProcess.TimeoutException)
				{
					MessageBox.Show("Took too long to start the Hardlight Runtime! Runtime is stopped.");
				}
			}
		}

		void StopService(object sender, EventArgs e)
		{
			if (_serviceController.Status == ServiceControllerStatus.Running)
			{
				try
				{
					_serviceController.Stop();
					_serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0,0,5));
					_trayIcon.Icon = Properties.Resources.TrayIconServiceOff;
				}
				catch (InvalidOperationException)
				{
					MessageBox.Show("Could not stop the Hardlight Runtime!");
				}
				catch (System.ServiceProcess.TimeoutException)
				{
					MessageBox.Show("Took too long to stop the Hardlight Runtime! ");
				}
			}
		}



		void TestSuit(object sender, EventArgs e)
		{
			if (_serviceController.Status != ServiceControllerStatus.Running)
			{
				StartService(null, null);
				_hapticsDelay.Start();
			} else
			{
				CreateAndPlayHaptic(0);
			}
		}

		private unsafe void CreateAndPlayHaptic(UInt32 deviceId)
		{
			var nodes = fetchAllNodes(deviceId);
			float timeOffset = 0.15f;
			Interop.HLVR_Timeline* timeline = null;
			Interop.HLVR_Timeline_Create(&timeline);

			for (int i = 0; i < nodes.Count; i++)
			{
				UInt32[] singleLoc = new UInt32[1] { nodes[i] };
				Interop.HLVR_Event* haptic = null;
				Interop.HLVR_Event_Create(&haptic, Interop.HLVR_EventType.DiscreteHaptic);
				Interop.HLVR_Event_SetUInt32s(haptic, Interop.HLVR_EventKey.Target_Nodes_UInt32s, singleLoc, 1);
				Interop.HLVR_Event_SetInt(haptic, Interop.HLVR_EventKey.DiscreteHaptic_Waveform_Int, (int)Interop.HLVR_Waveform.Click);
				Interop.HLVR_Timeline_AddEvent(timeline, timeOffset * i, haptic);
				Interop.HLVR_Event_Destroy(haptic);
			}


			Interop.HLVR_Effect* effect = null;
			Interop.HLVR_Effect_Create(&effect);

			Interop.HLVR_Timeline_Transmit(timeline, _pluginPtr, effect);
			Interop.HLVR_Effect_Play(effect);

			Interop.HLVR_Timeline_Destroy(timeline);
			Interop.HLVR_Effect_Destroy(effect);
		}

		private void DelayWhilePluginInitializes_Tick(object sender, EventArgs e)
		{
			CreateAndPlayHaptic(0);
			_hapticsDelay.Stop();
		}

		private void VersionInfo(object sender, EventArgs e)
		{
			VersionInfo v = new VersionInfo(this._cachedServiceVersion);
			v.Show();
		}
		private void Exit()
		{
			_checkStatusTimer.Stop();
			
			// Hide tray icon, otherwise it will remain shown until user mouses over it
			_trayIcon.Visible = false;
			StopService(null, null);
			Application.Exit();
		}

		private void TestDevice(UInt32 id)
		{
			throw new NotImplementedException();
		}

	}
}
