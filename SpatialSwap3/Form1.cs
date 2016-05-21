using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using System.Xml;
using System.Threading;

namespace SpatialSwap3
{
    public partial class Form1 : Form
    {
        ObservableCollection<string> _versionList = new ObservableCollection<string>();
        private string _version, _dir, _orac, _binfile, _originpath, _runpath, _arguments;
        private int _pid, _originalPid;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SwapBack();

            //checks for xml file
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "SpatialSwap3Config.xml"))
            {
                MessageBox.Show("The SpatialSwap3Config.xml file is required to run this application.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }

            LoadVersions();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            GetSettings(AppDomain.CurrentDomain.BaseDirectory + "SpatialSwap3Config.xml", cbVersion.Text);
        }

        public void LoadVersions()
        {
            //loads list of versions from xml file
            VerList(AppDomain.CurrentDomain.BaseDirectory + "SpatialSwap3Config.xml"); //can be changed if necessary
            cbVersion.DataSource = _versionList;
        }

        public ObservableCollection<string> VerList(string _filename)
        {
            if (File.Exists(_filename))
            {
                _versionList.Clear();
                XDocument _xmlDoc = XDocument.Load(_filename);
                var _verList = (from n in _xmlDoc.Descendants("Setting") select n.Element("name")).ToList().OrderBy(n => n.Element("name"));                
                _versionList.Add("---");
                foreach (string _name in _verList.ToList())
                {
                    _versionList.Add("" + _name);
                }

                _xmlDoc = XDocument.Load(_filename);
                _verList = (from n in _xmlDoc.Descendants("Setting") select n.Element("name")).ToList().OrderBy(n => n.Element("name"));                
                foreach (string _name in _verList)
                {
                    if (!_versionList.Contains(_name))
                    {
                        _versionList.Add("" + _name);
                    }
                }
                return _versionList;
            }
            return null;
        }


        public void GetSettings(string _filename, string _name)
        {
            //loads all settings for swap
            XDocument xDoc = XDocument.Load(_filename);
            var settings = (from n in xDoc.Descendants("Setting") where n.Element("name").Value == _name select n).ToList();

            foreach (var item in settings)
            {
                _version = item.Element("version").Value;
                _dir = item.Element("dir").Value;
                _orac = item.Element("orac").Value;
                _binfile = item.Element("binfile").Value;
                _originpath = item.Element("originpath").Value;
                _runpath = item.Element("runpath").Value;
                _arguments = item.Element("arguments").Value;
            }

            Environment.SetEnvironmentVariable("path", _runpath);

            //perform swap
            Swap(_version, _dir, _orac, _binfile, _arguments);

            Environment.SetEnvironmentVariable("path", _originpath);
        }

        public void Swap(string _version, string _dir, string _orac, string _binfile, string _arguments)
        {
            try
            {
                System.IO.Directory.Move("C:\\program files\\spatialinfo\\spatialnet" + _version, "C:\\program files\\spatialinfo\\spatialnet");
                File.Copy(_dir + "config" + _version + "\\tnsnames.ora", _orac + "tnsnames.ora", true);
                File.Copy(_dir + "config" + _version + "\\sqlnet.ora", _orac + "sqlnet.ora", true);
                File.Copy(_dir + "config" + _version + "\\listener.ora", _orac + "listener.ora", true);
            }
            catch
            {
                lblMsg.Text = "Error: Missing directory or file.";
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = _binfile;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = _arguments;

            try
            {                
                Process exeProcess = Process.Start(startInfo);

                //see if acad.exe is running
                Process[] pname = Process.GetProcessesByName("acad");
                _pid = pname[5].Id;
                if (pname.Length>0)
                {
                    //get process id of acad.exe
                    _originalPid = pname[0].Id;
                    MessageBox.Show("proc id:" + _originalPid.ToString());
                    //check to see if running loop
                    while (_pid == _originalPid)
                    {
                        Process[] procname = Process.GetProcessesByName("acad");
                        _pid = procname[0].Id;
                        MessageBox.Show("sleeping next");
                        Thread.Sleep(30000);
                    }
                    lblMsg.Text = "Process completed.";
                    Environment.SetEnvironmentVariable("path", _originpath); // change oracle back if fail
                    btnStart.Enabled = false;
                    Thread.Sleep(3000);
                    lblMsg.Text = "Shutting down.";
                    Thread.Sleep(1000);
                    lblMsg.Text = "Shutting down..";
                    Thread.Sleep(1000);
                    lblMsg.Text = "Shutting down...";
                    Thread.Sleep(1000);
                    lblMsg.Text = "Shutting down....";
                    Thread.Sleep(1000);
                    lblMsg.Text = "Shutting down.....";
                    Application.Exit();  //close app if oracle fails.
                }

                    
                //}
            }
            catch
            {
                lblMsg.Text = "Process did not complete.";
                btnStart.Enabled = false;
                Thread.Sleep(3000);
                lblMsg.Text = "Shutting down.";
                Thread.Sleep(1000);
                lblMsg.Text = "Shutting down..";
                Thread.Sleep(1000);
                lblMsg.Text = "Shutting down...";
                Thread.Sleep(1000);
                lblMsg.Text = "Shutting down....";
                Thread.Sleep(1000);
                lblMsg.Text = "Shutting down.....";
                Environment.SetEnvironmentVariable("path", _originpath); // change oracle back if fail
                Application.Exit();  //close app if oracle fails.
            }
        }



        public void SwapBack()
        {
            try
            {                
                _versionList.Clear();

                XDocument _xmlDoc = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "SpatialSwap3Config.xml");
                var _verList = (from n in _xmlDoc.Descendants("Setting") select n.Element("version")).ToList().OrderBy(n =>
                    n.Element("version"));
                foreach (string _name in _verList.ToList())
                {
                    if (!Directory.Exists(@"C:\\program files\\spatialinfo\\spatialnet" + _name))
                    {
                        System.IO.Directory.Move("C:\\Program Files\\spatialinfo\\spatialnet", @"C:\\program files\\spatialinfo\\spatialnet" + _name);
                    }
                }
            }
            catch
            {
                lblMsg.Text = "Swapback error. Exit this app and try again.";                
                btnStart.Enabled = false;
            }

            _arguments = "";
            _binfile = "";
            _dir = "";
            _orac = "";
            _version = "";

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This application allows the user to swap between various versions of the Oracle Spatial database being used so that " +
                "data between various clients can be managed on the same desktop PC.  If a swapback error occurs, it means that the version or versions " +
                "of Oracle Database are not installed or files are missing on this PC.  Please contact the IT department for installation.");
        }


    }
}
