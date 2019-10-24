using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Runtime.InteropServices;
using System.IO;

namespace usbGuard3
{
	public partial class Form1 : Form
	{
		const int MF_BYCOMMAND = 0;
		const int MF_DISABLED = 2;
		const int SC_CLOSE = 0xF060;

		public Form1() {
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			closePopup();

			WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
			WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
			ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
			ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
			insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
			removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
			insertWatcher.Start();
			removeWatcher.Start();

			String CID = Environment.UserName + "-" + Environment.MachineName;
			log(CID,"LOGIN");
		}

		private void Form1_OnClosing(object sender, EventArgs e) {
			String CID = Environment.UserName + "-" + Environment.MachineName;
			log(CID,"LOGOUT");
		}
		
		private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e) {
			ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
			String CID = Environment.UserName + "-" + Environment.MachineName + "-" + instance.Properties["PNPDeviceID"].Value;
			log(CID,"INSERTED");
			openPopup(CID);
		}

		private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e) {
			ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
			String CID = Environment.UserName + "-" + Environment.MachineName + "-" + instance.Properties["PNPDeviceID"].Value;
			log(CID, "REMOVED");
			closePopup();
		}

		private void buttonClose_Click(object sender, EventArgs e) {
			closePopup();
		}

		private void openPopup(string cid)
		{
			BeginInvoke(new MethodInvoker(delegate
			{
				this.ShowInTaskbar = false;
				this.Visible = false;
				this.Visible = true;
				this.TopMost = true;
				this.CenterToScreen();
				labelCID.Text = cid;
			}));
		}
		private void closePopup()
		{
			BeginInvoke(new MethodInvoker(delegate {
				this.TopMost = false;
				this.Visible = false;
				notifyIcon1.Visible = true;
			}));
		}

		private void log(string text,string cat) {
			string log = "["+DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")+"] ["+ cat + "] " + text;
			Console.WriteLine(log);
			
			try
			{
				string path = @"c:\temp\usbGuardLog.txt";

				if (!File.Exists(path))
				{
					using (StreamWriter sw = File.CreateText(path)) { sw.WriteLine("[FIRST]"); }
				}

				using (StreamWriter sw = File.AppendText(path))
				{
					sw.WriteLine(log);
				}

			} catch(Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

	}
}
