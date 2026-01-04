using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.Win32;
using ThreadState = System.Diagnostics.ThreadState;
using System.ServiceProcess;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;

namespace GameOptimizer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokPriv1Luid
    {
        public int Count;
        public long Luid;
        public int Attr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SYSTEM_CACHE_INFORMATION
    {
        public uint CurrentSize;
        public uint PeakSize;
        public uint PageFaultCount;
        public uint MinimumWorkingSet;
        public uint MaximumWorkingSet;
        public uint Unused1;
        public uint Unused2;
        public uint Unused3;
        public uint Unused4;
    }

    //SYSTEM_CACHE_INFORMATION_64_BIT
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SYSTEM_CACHE_INFORMATION_64_BIT
    {
        public long CurrentSize;
        public long PeakSize;
        public long PageFaultCount;
        public long MinimumWorkingSet;
        public long MaximumWorkingSet;
        public long Unused1;
        public long Unused2;
        public long Unused3;
        public long Unused4;
    }

    public partial class GameOptimizerForm : Form
    {

        [DllImport("ntdll.dll", CharSet = CharSet.None, ExactSpelling = false)]
        internal static extern uint NtSetSystemInformation(int infoClass, IntPtr info, int length);

        [DllImport("ntdll.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint ActualResolution);

        [DllImport("ntdll.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern int NtSetTimerResolution(uint DesiredResolution, bool SetResolution, out uint CurrentResolution);
        Process[] ProcessArray;

        public List<ProcessList> ListItems { get; private set; }

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }
        public class SelectedCoreCount
        {
            public static readonly Int32 CoreTwo = 0x0002;
            public static readonly Int32 SingleCore = 0x0001;
            public static readonly Int32 DualCore = 0x0005;
            public static readonly Int32 TripleCore = 0x0015;
            public static readonly Int32 QuadCoreNoSMT = 0x0055;
            public static readonly Int32 HexaCoreSMT = 0x0FFF;
            public static readonly Int32 HexaCoreNoSMT = 0x0555;
            public static readonly Int32 OctaCoreNoSMT = 0x5555;
            public static readonly Int64 OctoCoreSMT = 0xFFFF;
            public static readonly Int64 DecaHexaCore = 0xAAAAA555;
            public static readonly Int32 CoreOneToFour = 0x000F;
            public static readonly Int32 CoreOneToSix = 0x003F;
            public static readonly Int32 CoreOneToEight = 0x00FF;

            // New masks for 24-core CPUs
            public static readonly Int64 TwentyFourCoreSmt = 0xFFFFFFFF;
            public static readonly Int64 TwentyFourCoreSmtNoSMT = 0xFFFF5555; // 13900HX: 8 physical P-cores + 16 E-cores
            public static readonly Int64 TwentyFourCoreSmtHalfPhysical = 0x000F5555; // 13900HX: 8 physical P + 4 E = 12 physical
            public static readonly Int64 TwentyFourCoreNoSmt = 0x00FFFFFF; // 275HX: 24 physical cores (no SMT)
            public static readonly Int64 TwentyFourCoreNoSmtHalfPhysical = 0x00000FFF; // 275HX: 12 physical cores
        }
        public class ProcessList
        {
            public bool Check { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public int Priority { get; set; }


        }
        static bool firstExecution = true;
        static bool firstExecution2 = true;
        static string GamePath;
        static string FileName;
        NetworkInterface Adapter { get; set; }

        private PropertyCleanup _cleanUpMemory;

        static string PlainName { get; set; }
        private string TcpIpKeyPath;
        List<Process> GameProcessList;
        static byte ThreadCount;
        static byte Cores;
        private uint DefaultResolution;
        private uint MininumResolution;
        private uint MaximumResolution;
        private float MinimumTimerResolution;
        private float MaximumTimerResolution;
        private uint CurrentResolution;
        Process GameProcess;
        private bool ExistKey { get { return (Registry.LocalMachine.GetValue(Key, "Priority") != null); } }
        private string Key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games";
        private PingReply status;
        private Task<bool> secondaryTask;
        private Task mainTask;
        private Task memoryTask;
        private bool failedInternetStartupCheck;
        private bool isOnline;

        private enum ThreadPriorityLevel
        {
            Normal = 0,
            AboveNormal = 1,
            Highest = 2,
            TimeCritical = 15,
        }

        public GameOptimizerForm()
        {
            Adapter =  System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && n.Description.Contains("Ethernet") || n.OperationalStatus == OperationalStatus.Up && n.Description.Contains("MediaTek Wi-Fi 6 MT7921 Wireless LAN Card"));
            _cleanUpMemory = new PropertyCleanup { WasCleaned = false };
            InitializeComponent();
            GetProcessorCount();
            ClearStandByList();
            GetCurrentTimerResolution();
            StopServicesAndProcess();
            EmptyWorkingSet();
            InitializeRegistryCheckboxes();
            StartUpCheck();
            InitializeTimer();
            SetTimerTimeSpan();
        }

        private void SetTimerTimeSpan()
        {
            cmbTimeSpanTimer.Items.Add("5 minutes");
            cmbTimeSpanTimer.Items.Add("10 minutes");
            cmbTimeSpanTimer.Items.Add("15 minutes");
            cmbTimeSpanTimer.Items.Add("20 minutes");
        }

        private void InitializeTimer()
        {
            tmrRam.Interval = 900000;
            tmrRam.Enabled = true;
            tmrRam.Start();
        }

        const int SE_PRIVILEGE_ENABLED = 2;
        const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        const string SE_PROFILE_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
        const int SystemFileCacheInformation = 0x0015;
        const int SystemMemoryListInformation = 0x0050;
        const int MemoryPurgeStandbyList = 4;
        const int MemoryEmptyWorkingSets = 2;

        //Import of DLL's (API) and the necessary functions 
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

       

        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmptyWorkingSet()
        {
            //Declaration of variables
            string ProcessName = string.Empty;
            Process[] allProcesses = Process.GetProcesses();
            List<string> successProcesses = new List<string>();
            List<string> failProcesses = new List<string>();

            //Cycle through all processes
            for (int i = 0; i < allProcesses.Length; i++)
            {
                Process p = new Process();
                p = allProcesses[i];
                //Try to empty the working set of the process, if succesfull add to successProcesses, if failed add to failProcesses with error message
                try
                {
                    ProcessName = p.ProcessName;
                    EmptyWorkingSet(p.Handle);
                    successProcesses.Add(ProcessName);
                }
                catch (Exception ex)
                {
                    failProcesses.Add(ProcessName + ": " + ex.Message);
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopServicesAndProcess()
        {
            string path = "processes.txt";
            if (File.Exists(path))
            {
                var result = File.ReadLines(path).ToList();
                var ListProcess = new List<string>();
                var ServiceList = new List<string>();
                bool foundService = false;
                bool enabled = true;
                if (result.Count() > 0)
                {
                    StreamReader sr = new StreamReader(path);
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!foundService)
                        {
                            foundService = line.Contains("services");
                            if (!foundService && enabled)
                            {
                                if (!string.IsNullOrEmpty(line))
                                    ListProcess.Add(line[line.Length - 1] == ' ' ? line.Substring(0, line.Length - 2) : line);
                            }
                            else
                            {
                                enabled = false;
                                if (!string.IsNullOrEmpty(line) && !line.Contains("services:"))
                                    ServiceList.Add(line);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(line))
                                ServiceList.Add(line);
                        }
                    }
                }

                if (ListProcess.Any())
                {
                    foreach (var str in ListProcess)
                    {
                        var process = Process.GetProcessesByName(str).FirstOrDefault();

                        if (process != null)
                            KillProcess(process.Id);
                    }
                }
                if (ServiceList.Any())
                {
                    foreach (var servicename in ServiceList)
                        StopService(servicename);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ServiceExist(string serviceName, string machineName)
        {
            ServiceController controller = null;
            try
            {
                controller = new ServiceController(serviceName, machineName);
                _ = controller.Status;
                controller.Close();
                controller.Dispose();
                return true;
            }
            catch (InvalidOperationException)
            {
                controller.Close();
                controller.Dispose();
                return false;
            }
           
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopService(string name)
        {

            ServiceController sc = new ServiceController(name);
            sc.MachineName = ".";
            if (ServiceExist(name, "."))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    try
                    {
                        sc.Stop();
                    }
                    catch (Exception ex)
                    {
                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c sc stop {name}",
                                Verb = "runas",
                                CreateNoWindow = false,

                            };
                            Process.Start(startInfo).WaitForExit();
                        }
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EditRegistryKey()
        {
            var gpuPriority = (int)Registry.GetValue(Key, "Priority", null);
            var priority = (int)Registry.GetValue(Key, "GPU Priority", null);
            if (ExistKey)
            {
                //64 Bit
                if (Environment.Is64BitOperatingSystem)
                {
                    if (gpuPriority != 6 || priority != 8)
                    {
                        var localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                        var key = localMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", true);
                        key.SetValue("Priority", 6);
                        key.SetValue("GPU Priority", 8);
                        key.SetValue("Scheduling Category", "High");
                        key.Close();


                    }
                    else
                    {
                        RegistryKey localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                        RegistryKey regKey = localMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", true);
                        regKey.SetValue("Scheduling Category", "High", RegistryValueKind.String);
                        regKey.Close();

                    }
                }
                else
                {
                    if (gpuPriority != 6 || priority != 8)
                    {
                        var localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                        var key = localMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", true);
                        key.SetValue("Priority", 6);
                        key.SetValue("GPU Priority", 8);
                        key.SetValue("Scheduling Category", "High");
                        key.Close();

                        //Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", true);

                    }
                    else
                    {
                        RegistryKey localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                        RegistryKey regKey = localMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", true);
                        regKey.SetValue("Scheduling Category", "High", RegistryValueKind.String);
                        regKey.Close();

                    }
                }

            }
            else
            {
                if (Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", true) == null)
                {
                    RegistryKey systemProfileKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", true);
                    systemProfileKey.CreateSubKey("Tasks");
                    systemProfileKey.Close();

                    if (Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks", true) == null)
                    {
                        RegistryKey gameTaskKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks", true);
                        gameTaskKey.CreateSubKey("Games");
                        gameTaskKey.Close();

                        RegistryKey gameModeKey = Registry.LocalMachine.OpenSubKey(Key);
                        gameModeKey.SetValue("Affinity", 0);
                        gameModeKey.SetValue("Background Only", false);
                        gameModeKey.SetValue("Clock Rate", 0x2710);
                        gameModeKey.SetValue("GPU Priority", 8);
                        gameModeKey.SetValue("Priority", 6);
                        gameModeKey.SetValue("Scheduling Category", "High");
                        gameModeKey.SetValue("SFIO Priority", "Normal");
                        gameModeKey.Close();

                    }
                }
                else
                {
                    if (Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks", true) == null)
                    {
                        RegistryKey gameTaskKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks", true);
                        gameTaskKey.CreateSubKey("Games");
                        gameTaskKey.Close();

                        RegistryKey gameModeKey = Registry.LocalMachine.OpenSubKey(Key);
                        gameModeKey.SetValue("Affinity", 0);
                        gameModeKey.SetValue("Background Only", false);
                        gameModeKey.SetValue("Clock Rate", 0x2710);
                        gameModeKey.SetValue("GPU Priority", 8);
                        gameModeKey.SetValue("Priority", 6);
                        gameModeKey.SetValue("Scheduling Category", "High");
                        gameModeKey.SetValue("SFIO Priority", "Normal");
                        gameModeKey.Close();

                    }

                }

            }

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisableNaggleAlgorithm()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"))
            {
                if (key != null)
                {
                    var subKeyArray = key.GetSubKeyNames();
                    var subKeyList = new List<string>();
                    foreach (var keyName in subKeyArray)
                    {
                        subKeyList.Add(string.Concat(@"System\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\", keyName));
                    }
                    foreach (var subkeyName in subKeyList)
                    {
                        using (RegistryKey subKey = Registry.LocalMachine.OpenSubKey(subkeyName))
                        {
                            var subkeyNamePath = string.Concat(@"HKEY_LOCAL_MACHINE\", subkeyName);

                            if (Registry.GetValue(subkeyNamePath, "DhcpIPAddress", null) != null)
                            {
                                if (Registry.GetValue(subkeyNamePath, "DhcpIPAddress", null).ToString() != "0.0.0.0")
                                {
                                    TcpIpKeyPath = string.Concat(@"HKEY_LOCAL_MACHINE\", subkeyName);
                                    if (Registry.GetValue(TcpIpKeyPath, "TcpAckFrequency", null) != null && chckbxNaggleAlgo.Checked)
                                    {
                                        Registry.SetValue(TcpIpKeyPath, "TcpAckFrequency", 1);
                                        Registry.SetValue(TcpIpKeyPath, "TCPNoDelay", 1);
                                        Registry.SetValue(TcpIpKeyPath, "TcpDelAckTicks", 0);

                                    }
                                    else if (Registry.GetValue(TcpIpKeyPath, "TcpAckFrequency", null) != null && !chckbxNaggleAlgo.Checked)
                                    {
                                        Registry.SetValue(TcpIpKeyPath, "TcpAckFrequency", 0);
                                        Registry.SetValue(TcpIpKeyPath, "TCPNoDelay", 0);
                                        Registry.SetValue(TcpIpKeyPath, "TcpDelAckTicks", 1);
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetCurrentTimerResolution()
        {
            NtQueryTimerResolution(out this.MaximumResolution, out this.MininumResolution, out this.DefaultResolution);
            //float DefaultResolution = (float)((float)this.DefaultResolution) / 10000f;
            //float defaultResolution = (float)((float)this.DefaultResolution) / 10000f;
            //lblrestmr.Text = string.Concat(DefaultResolution.ToString(), "ms");
            //MaximumTimerResolution = (float)((float)this.MaximumResolution) / 10000f;
            //defaultResolution = (float)((float)this.MaximumResolution) / 10000f;
            //MinimumTimerResolution = (float)((float)this.MininumResolution) / 10000f;
            //defaultResolution = (float)((float)this.MininumResolution) / 10000f;
            lblrestmr.Text = textBox2.Text = (DefaultResolution/10000).ToString(CultureInfo.InvariantCulture);
            lblrestmr.Text += "ms";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SelectPath_Click(object sender, EventArgs e)
        {

            if (this.textBox1.Text != String.Empty/* && !firstExecution*/)
            {
                GamePath = textBox1.Text.ToString();
                firstExecution = false;
              
            }
            else
            {
                if (dataGridView1.SelectedRows != null && dataGridView1.SelectedRows.Count > 0 || dataGridView1.SelectedCells.Count==1)
                {
                    if (dataGridView1.SelectedCells.Count == 1)
                    {
                        GamePath = dataGridView1.CurrentCell.Value.ToString();
                        if (GamePath == "True")
                        {
                            GamePath = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex + 2].Value.ToString();
                        }
                        if (Process.GetProcessesByName(GamePath).Any())
                            textBox1.Text = GamePath;
                        else
                        {
                            MessageBox.Show($"{GamePath} is not a running process.. select another cell");
                            dataGridView1.ClearSelection();
                            GamePath = "";
                            textBox1.Text = "";
                        }
                    }
                    else
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.Selected)
                            {
                                textBox1.Text = row.Cells[2].Value.ToString();
                                GamePath = textBox1.Text;
                                break;
                            }
                        }
                    }

                    firstExecution = false;
                }
                else
                {
                    DialogResult result = openFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        GamePath = openFileDialog1.FileName;
                        textBox1.Text = GamePath;
                        firstExecution = false;
                    }
                }
            }

          
            if (GamePath.Contains(@"\"))
                FileName = GamePath.Substring(GamePath.LastIndexOf('\\') + 1, GamePath.LastIndexOf('.') - (GamePath.LastIndexOf('\\') + 1)) + ".exe";
            else
                FileName = Path.GetFileName(GamePath);


            if (GamePath != "")
                StartProcess();
            else
            {
                MessageBox.Show("Process Path is Empty");
                textBox1.Focus();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetProcessorCount()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                byte.TryParse(queryObj["NumberOfCores"].ToString(), out Cores);
                byte.TryParse(queryObj["NumberOfLogicalProcessors"].ToString(), out ThreadCount);
            }

            if (ThreadCount == 8 && Cores == 8)
                this.cmbxAffinityCount.Items.Remove("8c/16t");
            else if (Cores == 4 && ThreadCount == 4)
            {
                this.cmbxAffinityCount.Items.Remove("4c/8t");
                this.cmbxAffinityCount.Items.Remove("8c/8t");
                this.cmbxAffinityCount.Items.Remove("8c/16t");
            }
            else if (Cores == 4 && ThreadCount == 8)
            {
                this.cmbxAffinityCount.Items.Remove("8c/8t");
                this.cmbxAffinityCount.Items.Remove("8c/16t");
            }
            else if (ThreadCount == 4 && Cores == 2)
            {
                this.cmbxAffinityCount.Items.Remove("4c/4t");
                this.cmbxAffinityCount.Items.Remove("4c/8t");
                this.cmbxAffinityCount.Items.Remove("8c/8t");
                this.cmbxAffinityCount.Items.Remove("8c/16t");

            }
            else if (Cores==2 && ThreadCount == 2)
            {
                this.cmbxAffinityCount.Items.Remove("2c/4t");
                this.cmbxAffinityCount.Items.Remove("4c/4t");
                this.cmbxAffinityCount.Items.Remove("4c/8t");
                this.cmbxAffinityCount.Items.Remove("8c/8t");
                this.cmbxAffinityCount.Items.Remove("8c/16t");
            }
            else if (ThreadCount == 1)
            {
                this.cmbxAffinityCount.Items.Clear();
            }

            if (Cores == 24 && ThreadCount == 32)
            {
                if (!this.cmbxAffinityCount.Items.Contains("24c/32t"))
                    this.cmbxAffinityCount.Items.Add("24c/32t");
            }
            if (Cores == 24 && ThreadCount == 24)
            {
                if (!this.cmbxAffinityCount.Items.Contains("24c/24t"))
                    this.cmbxAffinityCount.Items.Add("24c/24t");
            }
        }
        async Task StartUpCheck()
        {
            try
            {
                status = IsInternetConnectionAvailable().Result;
                secondaryTask = Task.FromResult(status.Status.Equals(IPStatus.Success));

                if (Adapter != null)
                {
                   
                    mainTask = new Task(async () =>
                    {
                        await GetNetworkSpeed();
                    });

                    mainTask.Start();
                }
            }
            catch (Exception ex)
            {
                failedInternetStartupCheck = true;
                isOnline = false;
                lblInternet.Text = "Sin Conexion";
            }
        }
        async Task<PingReply> IsInternetConnectionAvailable()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "8.8.8.8";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return reply;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private void SetAffinity(ref Process GameProcess)
        {
            if (!GameProcess.HasExited)
            {
                if (rdoAuto.Checked)
                {
                    if (Cores == 8 || Cores == 4 && ThreadCount == 8)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.CoreOneToEight;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity=(IntPtr)SelectedCoreCount.CoreOneToEight;
                    }
                    if (Cores == 6 && ThreadCount == 12)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreSMT;
                    }
                    if (Cores == 8 && ThreadCount == 16)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.OctoCoreSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.OctoCoreSMT;
                    }
                    if (Cores == 24 && ThreadCount == 32)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmt;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmt;
                    }
                    if (Cores == 24 && ThreadCount == 24)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                    }

                    GameProcess.Refresh();
                }
                if (rdoNoSMT.Checked)
                {
                    if (Cores == 4 && ThreadCount == 8)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                            }
                            catch (Exception ex)
                            { 
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                    }
                    if (Cores == 6 && ThreadCount == 12)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreNoSMT;

                    }
                    if (Cores == 8 && ThreadCount == 16)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.OctaCoreNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.OctaCoreNoSMT;
                    }
                    if (Cores == 24 && ThreadCount == 32)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtNoSMT;
                    }
                    if (Cores == 24 && ThreadCount == 24)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                    }
                    GameProcess.Refresh();
                }
                if (rdoHalfPhysical.Checked)
                {
                    if (Cores == 4 && ThreadCount == 4 || Cores == 4 && ThreadCount == 8)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.DualCore;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.DualCore;
                    }
                    if (Cores == 6 && ThreadCount == 6 || Cores == 6 && ThreadCount == 12)
                    {

                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TripleCore;
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TripleCore;
                    }
                    if (Cores == 8 && ThreadCount == 16)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                    }
                    if (Cores == 24 && ThreadCount == 32)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtHalfPhysical;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtHalfPhysical;
                    }
                    if (Cores == 24 && ThreadCount == 24)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmtHalfPhysical;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmtHalfPhysical;
                    }
                    GameProcess.Refresh();
                }
            }
            else
            {
                GameProcess = Process.GetProcessesByName(PlainName).Where(x => x.HasExited == false).FirstOrDefault();
                if (rdoAuto.Checked)
                {
                    if (Cores == 8 || Cores == 4 && ThreadCount == 8)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.CoreOneToEight;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.CoreOneToEight;
                    }
                    if (Cores == 6 && ThreadCount == 12)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreSMT;
                    }
                    if (Cores == 8 && ThreadCount == 16)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.OctoCoreSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.OctoCoreSMT;
                    }
                    if (Cores == 24 && ThreadCount == 32)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmt;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmt;
                    }
                    if (Cores == 24 && ThreadCount == 24)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                    }

                    GameProcess.Refresh();
                }
                if (rdoNoSMT.Checked)
                {
                    if (Cores == 4 && ThreadCount == 8)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                    }
                    if (Cores == 6 && ThreadCount == 12)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreNoSMT;
                    }
                    if (Cores == 8 && ThreadCount == 16)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.OctaCoreNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.OctaCoreNoSMT;
                    }
                    if (Cores == 24 && ThreadCount == 32)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtNoSMT;
                    }
                    if (Cores == 24 && ThreadCount == 24)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;
                    }
                    GameProcess.Refresh();
                }
                if (rdoHalfPhysical.Checked)
                {
                    if (Cores == 4 && ThreadCount == 4 || Cores == 4 && ThreadCount == 8)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.DualCore;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.DualCore;
                    }
                    if (Cores == 6 && ThreadCount == 6 || Cores == 6 && ThreadCount == 12)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TripleCore;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TripleCore;
                    }
                    if (Cores == 8 && ThreadCount == 16)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                    }
                    if (Cores == 24 && ThreadCount == 32)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtHalfPhysical;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtHalfPhysical;
                    }
                    if (Cores == 24 && ThreadCount == 24)
                    {
                        for (int i = 0; i < GameProcess.Threads.Count; i++)
                        {
                            try
                            {
                                GameProcess.Threads[i].ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmtHalfPhysical;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        GameProcess.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmtHalfPhysical;
                    }
                    GameProcess.Refresh();
                }

            }

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetAffinity()
        {
            GameProcessList = Process.GetProcessesByName(PlainName).Where(x => x.HasExited == false).ToList();
            foreach (var process in GameProcessList)
            {
                if (rdoAuto.Checked)
                {
                    if (Cores == 8 || Cores == 4 && ThreadCount == 8)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.CoreOneToEight;
                    if (Cores == 6 && ThreadCount == 12)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreSMT;
                    if (Cores == 8 && ThreadCount == 16)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.OctoCoreSMT;
                    if (Cores == 24 && ThreadCount == 32)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmt;
                    if (Cores == 24 && ThreadCount == 24)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;

                    process.Refresh();
                }
                if (rdoNoSMT.Checked)
                {
                    if (Cores == 4 && ThreadCount == 8)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                    if (Cores == 6 && ThreadCount == 12)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.HexaCoreNoSMT;
                    if (Cores == 8 && ThreadCount == 16)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.OctaCoreNoSMT;
                    if (Cores == 24 && ThreadCount == 32)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtNoSMT;
                    if (Cores == 24 && ThreadCount == 24)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmt;

                    process.Refresh();
                }
                if (rdoHalfPhysical.Checked)
                {
                    if (Cores == 4 && ThreadCount == 4 || Cores == 4 && ThreadCount == 8)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.DualCore;
                    if (Cores == 6 && ThreadCount == 6 || Cores == 6 && ThreadCount == 12)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.TripleCore;
                    if (Cores == 8 && ThreadCount == 16)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.QuadCoreNoSMT;
                    if (Cores == 24 && ThreadCount == 32)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreSmtHalfPhysical;
                    if (Cores == 24 && ThreadCount == 24)
                        process.ProcessorAffinity = (IntPtr)SelectedCoreCount.TwentyFourCoreNoSmtHalfPhysical;

                    process.Refresh();
                }


            } 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPriority(ref Process GameProcess, bool running)
        {
            Process GameProcess1 = null ;
            if (!running)
            {
                Thread.Sleep(60000);
                
                if (!GameProcess.HasExited)
                {
                    for (int i = 0; i < GameProcess.Threads.Count; i++)
                    {
                        try
                        {
                            if (GameProcess.Threads[i].ThreadState != (ThreadState)4)
                                GameProcess.Threads[i].PriorityLevel = System.Diagnostics.ThreadPriorityLevel.TimeCritical;
                            GameProcess.Threads[i].PriorityBoostEnabled = true;

                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    GameProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
                    GameProcess.Refresh();
                }
            }
            else
            {

                if (!GameProcess.HasExited)
                {
                    for (int i = 0; i < GameProcess.Threads.Count; i++)
                    {
                        try
                        {
                            GameProcess.Threads[i].PriorityLevel = System.Diagnostics.ThreadPriorityLevel.TimeCritical;
                            GameProcess.Threads[i].PriorityBoostEnabled = true;
                        }
                        catch (Exception ex)
                        { 
                        
                        }

                    }
                    GameProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
                    GameProcess.Refresh();
                }
                else
                {
                    GameProcess1 = Process.GetProcessesByName(PlainName).Where(x => x.HasExited == false).FirstOrDefault();


                    for (int i = 0; i < GameProcess1.Threads.Count; i++)
                    {
                        try
                        {
                            GameProcess1.Threads[i].PriorityLevel = System.Diagnostics.ThreadPriorityLevel.TimeCritical;
                            GameProcess1.Threads[i].PriorityBoostEnabled = true;
                        }
                        catch (Exception ex)
                        {
                         
                        }

                    }
                    GameProcess1.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
                    GameProcess1.Refresh();
                    GameProcess = GameProcess1;
                    GameProcess.Refresh();
                }
            }
            if (GameProcess != null)
            {
                if (!GameProcess.HasExited)
                    SetAffinity(ref GameProcess);
                else if (!GameProcess1.HasExited)
                    SetAffinity(ref GameProcess1);
            }
            if (chckboxPriority.Checked)
            {
                if (!tmrPriority.Enabled)
                {
                    tmrPriority.Enabled = true;
                    tmrPriority.Start();
                }
            }
            else
            {

                tmrPriority.Enabled = false;
                tmrPriority.Stop();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartProcess()
        {
            Process[] processArr = Process.GetProcesses();
            var name  = textBox1.Text.IndexOf('.') > 0 ?  Path.GetFileName(textBox1.Text).Substring(0, Path.GetFileName(textBox1.Text).IndexOf('.')): textBox1.Text;
            PlainName = name;
            if (processArr.Any(x => x.ProcessName == name) && firstExecution2)
            {
                GameProcess = Process.GetProcesses().Where(x =>x.ProcessName ==name && !x.HasExited).Select(x => x).First();
                GetProcessorCount();
                SetPriority(ref GameProcess,true);
                firstExecution2 = false;
            }
            else
            {
                try
                {

                    GameProcess = new Process();
                    GameProcess.StartInfo.UseShellExecute = true;
                    GameProcess.StartInfo.FileName = GamePath;
                    GameProcess.Start();
                    GameProcess.PriorityBoostEnabled = true;
                    //GameProcessList = Process.GetProcessesByName(FileName).Where(x => x.HasExited == false).ToList();
                    SetPriority(ref GameProcess,false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearStandByList()
        {
            int systemInfoLength = Marshal.SizeOf<int>(4);
            GCHandle gcHandle = GCHandle.Alloc(4, GCHandleType.Pinned);
            if (GameOptimizerForm.NtSetSystemInformation(80, gcHandle.AddrOfPinnedObject(), systemInfoLength) != 0)
            {
                var str = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                //this.BeginInvoke(new MethodInvoker(delegate{
                //    lblStandByRAM.Text = str.Substring(str.LastIndexOf(':') + 2, str.Length - (str.LastIndexOf(':') + 2)).ToString();
                //}));
                
            }
            gcHandle.Free();
        }

        private float GetCurrentMemoryUsage()
        {
            return GetTotalMemoryInMiB() - GetPhysicalAvailableMemoryInMiB();
        }
        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }

        }

        public static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void tmrRam_Tick(object sender, EventArgs e)
        {
            if (enableClnStandby.Checked)
            {
                if (chkThresholdRAM.Checked)
                {
                    if (GetCurrentMemoryUsage() > (float)nudAutRamFreeup.Value)
                    {
                        ClearFileSystemCache(true);
                        ClearStandByList();
                        EmptyWorkingSet();
                    }
                }
                else
                {
                    ClearFileSystemCache(true);
                    ClearStandByList();
                }
            }

            

            if (chckbxTimerRes.Checked)
            {
                btnTmrResTime_Click(sender, e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMaxTimerResolution(uint TimerValue)
        {
            NtSetTimerResolution(TimerValue, true, out CurrentResolution);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void btnTmrResTime_Click(object sender, EventArgs e)
        {
            Double defaultTimer = Double.Parse(textBox2.Text.ToString(), CultureInfo.InvariantCulture);
            defaultTimer *= 10000;
            uint value = (uint)defaultTimer;
            SetMaxTimerResolution(value);
            GetCurrentTimerResolution();
        }


        private void chckbxNaggleAlgo_CheckedChanged(object sender, EventArgs e)
        {
            DisableNaggleAlgorithm();
        }

        private void chckBoxGamePriority_CheckedChanged(object sender, EventArgs e)
        {
            EditRegistryKey();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChckbxTimerRes_CheckedChanged(object sender, EventArgs e)
        {
            if (tmrRam.Enabled)
            {
                if (chckbxTimerRes.Checked)
                {
                    tmrRam.Interval = 900000;
                    tmrRam.Enabled = true;
                    tmrRam.Start();
                }
                else
                {
                    chckbxTimerRes.Checked = false;
                }
            }
            else
            {
                if (chckbxTimerRes.Checked)
                {
                    tmrRam.Interval = 900000;
                    tmrRam.Enabled = true;
                    tmrRam.Start();
                }

            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
                if (comboBox1.SelectedIndex == 1)
                {
                    string algo = "netsh int tcp set supplemental internet congestionprovider = newreno";
                    var stInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {algo}",
                        Verb = "runas",
                        CreateNoWindow = false,

                    };
                    Process.Start(stInfo).WaitForExit();
                }
                else if (comboBox1.SelectedIndex == 2)
                {
                    string algo = "netsh int tcp set supplemental internet congestionprovider = ctcp";
                    var stInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {algo}",
                        Verb = "runas",
                        CreateNoWindow = false,

                    };
                    Process.Start(stInfo).WaitForExit();
                }
                else if (comboBox1.SelectedIndex == 3)
                {
                    string algo = "netsh int tcp set supplemental internet congestionprovider = dctcp";
                    var stInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {algo}",
                        Verb = "runas",
                        CreateNoWindow = false,

                    };
                     Process.Start(stInfo).WaitForExit();
                }
                else if (comboBox1.SelectedIndex == 4)
                {
                    string algo = "netsh int tcp set supplemental internet congestionprovider = cubic";
                    var stInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {algo}",
                        Verb = "runas",
                        CreateNoWindow = false,

                    };
                    Process.Start(stInfo).WaitForExit();
                }
           
        }

        private void CmbxTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAffinity();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TmrPriority_Tick(object sender, EventArgs e)
        {
            if (chckboxPriority.Checked)
            {
                if (!tmrPriority.Enabled)
                {
                    tmrPriority.Enabled = true;
                    tmrPriority.Start();
                }
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    if (GameProcess == null)
                    {

                        GameProcess = new Process();
                        GameProcess.StartInfo.UseShellExecute = true;
                        GameProcess.StartInfo.FileName = GamePath;
                        GameProcess.Start();
                        GameProcess.PriorityBoostEnabled = true;
                        SetPriority(ref GameProcess,false);
                    }
                    else
                    {
                        GameProcess = Process.GetProcessesByName(PlainName).Where(x => x.HasExited == false).FirstOrDefault();
                        GetProcessorCount();
                        SetPriority(ref GameProcess,true);
                    }
                }
            }
            else
            {

                tmrPriority.Enabled = false;
                tmrPriority.Stop();
            }


        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChckboxPriority_CheckedChanged(object sender, EventArgs e)
        {
            if (chckboxPriority.Checked)
            {
                if (!tmrPriority.Enabled)
                {
                    tmrPriority.Enabled = true;
                    tmrPriority.Start();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void btnProcess_Click(object sender, EventArgs e)
        {
            this.ClientSize = new System.Drawing.Size(890, 452);
            this.dataGridView1.Enabled = true;
            this.dataGridView1.Visible = true;
            
            ProcessArray= Process.GetProcesses();
             ListItems = ProcessArray.Select(
                x => new ProcessList 
                { 
                 Name = x.ProcessName, 
                 ID = x.Id, 
                 Check = false, 
                 Priority = x.BasePriority,
                }).ToList();

            dataGridView1.DataSource = ListItems;
            
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void KillProcess(int pid)
        {
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ($"Select * From Win32_Process Where ParentProcessID={pid}");
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcess(Convert.ToInt32(mo["ProcessID"]));
            }
            Process proc=null;
            try
            {
                proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (Exception ex)
            {
                if (proc != null)
                {
                    proc = Process.GetProcessById(pid);
                    if (proc != null)
                        proc.Close();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void btnKill_Click(object sender, EventArgs e)
        {
            var xx = dataGridView1.Rows[0];
            var local = new List<ProcessList>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if ((bool)row.Cells[0].Value)
                    local.Add(new ProcessList
                    {
                        Name = (string)row.Cells[2].Value,
                        ID = (int)row.Cells[1].Value,
                        Check = true
                    });
            }
            try
            {
                local.ForEach((ProcessList p) => { KillProcess(p.ID); });

                ProcessArray = Process.GetProcesses();
                ListItems = ProcessArray.Select(
                   x => new ProcessList
                   {
                       Name = x.ProcessName,
                       ID = x.Id,
                       Check = false,
                       Priority = x.BasePriority,
                   }).ToList();

                dataGridView1.DataSource = ListItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (enableClnStandby.Checked)
            {
                if (!tmrRam.Enabled)
                {
                    tmrRam.Enabled = true;
                    tmrRam.Start();
                }
                ClearStandByList();
            }
            else
            {
                tmrRam.Enabled = false;
                tmrRam.Stop();
            }
        }

        private void chkThresholdRAM_CheckedChanged(object sender, EventArgs e)
        {
            if (chkThresholdRAM.Checked)
            {
                // Check every 30 seconds if threshold is enabled
                tmrRam.Interval = 30000;
            }
            else
            {
                // Return to 15 minutes if only automatic clearing is enabled
                tmrRam.Interval = 900000;
            }
        }

        private void tmrResSet_Tick(object sender, EventArgs e)
        {
            if(chckbxTimerRes.Checked)
            btnTmrResTime_Click(sender, e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetIncreasePrivilege(string privilegeName)
        {
            using (WindowsIdentity current = WindowsIdentity.GetCurrent(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges))
            {
                TokPriv1Luid newst;
                newst.Count = 1;
                newst.Luid = 0L;
                newst.Attr = SE_PRIVILEGE_ENABLED;

                //Retrieves the LUID used on a specified system to locally represent the specified privilege name
                if (!LookupPrivilegeValue(null, privilegeName, ref newst.Luid))
                    throw new Exception("Error in LookupPrivilegeValue: ", new Win32Exception(Marshal.GetLastWin32Error()));

                //Enables or disables privileges in a specified access token
                int num = AdjustTokenPrivileges(current.Token, false, ref newst, 0, IntPtr.Zero, IntPtr.Zero) ? 1 : 0;
                if (num == 0)
                    throw new Exception("Error in AdjustTokenPrivileges: ", new Win32Exception(Marshal.GetLastWin32Error()));
                return num != 0;
            }

        }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ClearFileSystemCache(bool ClearStandbyCache)
            {
                try
                {   
                    //Check if privilege can be increased
                    if (SetIncreasePrivilege(SE_INCREASE_QUOTA_NAME))
                    {
                        uint num1;
                        var SystemInfoLength = -1;
                        GCHandle gcHandle;
                        
                        if (!(Marshal.SizeOf(typeof(IntPtr)) == 8))
                        {
                            SYSTEM_CACHE_INFORMATION cacheInformation = new SYSTEM_CACHE_INFORMATION();
                            cacheInformation.MinimumWorkingSet = uint.MaxValue;
                            cacheInformation.MaximumWorkingSet = uint.MaxValue;
                            SystemInfoLength = Marshal.SizeOf(cacheInformation);
                            gcHandle = GCHandle.Alloc(cacheInformation, GCHandleType.Pinned);
                            num1 = NtSetSystemInformation(SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                            gcHandle.Free();
                        }
                        else
                        {
                            SYSTEM_CACHE_INFORMATION_64_BIT information64Bit = new SYSTEM_CACHE_INFORMATION_64_BIT();
                            information64Bit.MinimumWorkingSet = -1L;
                            information64Bit.MaximumWorkingSet = -1L;
                            SystemInfoLength = Marshal.SizeOf(information64Bit);
                            gcHandle = GCHandle.Alloc(information64Bit, GCHandleType.Pinned);
                            num1 = NtSetSystemInformation(SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                            gcHandle.Free();
                        }
                        if (num1 != 0)
                            throw new Exception("NtSetSystemInformation(SYSTEMCACHEINFORMATION) error: ", new Win32Exception(Marshal.GetLastWin32Error()));
                    }

                //If passes paramater is 'true' and the privilege can be increased, then clear standby lists through MemoryPurgeStandbyList
                    if (ClearStandbyCache && SetIncreasePrivilege(SE_PROFILE_SINGLE_PROCESS_NAME))
                     {
                        int SystemInfoLength = Marshal.SizeOf(MemoryPurgeStandbyList);
                        GCHandle gcHandle = GCHandle.Alloc(MemoryPurgeStandbyList, GCHandleType.Pinned);
                        uint num2 = NtSetSystemInformation(SystemMemoryListInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                        gcHandle.Free();
                        if (num2 != 0)
                            throw new Exception("NtSetSystemInformation(SYSTEMMEMORYLISTINFORMATION) error: ", new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                }
                catch (Exception ex)
                {
                   MessageBox.Show(ex.ToString());
                }
            }
        class PropertyCleanup
        {
            public bool WasCleaned { get; set; }
        }
        bool GetIfConnected(bool connected, IPStatus? status = IPStatus.Unknown)
        {
            BeginInvoke(new MethodInvoker(delegate
            {
                this.lblInternet.Text = status == IPStatus.Success? "Conectado" : $"Sin Conexion ({status})";
            }));
            return connected;
        }
        async Task AutomaticMemoryCleanup( PropertyCleanup clean, Stopwatch sw1, long limit)
        {

            if (GetCurrentMemoryUsage() > (float)nudAutRamFreeup.Value)
            {
                if (!clean.WasCleaned)
                {
                    int tries = 0;
                    await Task.Run(() =>
                    {
                        while (tries < 7)
                        {
                            EmptyWorkingSet();
                            ClearStandByList();
                            tries++;
                        }
                    });
                }
                if (!sw1.IsRunning)
                {
                    sw1.Start();
                    clean.WasCleaned = true;
                }
                if (sw1.ElapsedMilliseconds >= limit)
                {
                    sw1.Stop();
                    clean.WasCleaned = false;
                }
            }
        }
        async Task GetNetworkSpeed()
        {
            IEnumerable<Double> reads = new List<double>();
            var sw = new Stopwatch();
            var lastBr = Adapter.GetIPv4Statistics().BytesReceived;
            Stopwatch sw1 = new Stopwatch();
            long limit = 600000;
            await Task.Run(async () =>
            {
                ulong i = 0;
                while (true)
                {

                    await AutomaticMemoryCleanup(_cleanUpMemory, sw1, limit);
                    sw.Restart();
                    var RES = (await IsInternetConnectionAvailable());
                    isOnline = (RES?.Status ?? IPStatus.Unknown).Equals(IPStatus.Success);

                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        if (!GetIfConnected(isOnline, RES?.Status))
                        {
                            if (!timer.Enabled)
                            {
                                timer.Enabled = true;
                                timer.Start();
                            }
                        }
                    }));
                    //Thread.Sleep(10);
                    var elapsed = sw.Elapsed.TotalSeconds;
                    var br = Adapter.GetIPv4Statistics().BytesReceived;

                    double local = (br - lastBr) / elapsed;
                    lastBr = br;

                    // Keep last 20, ~2 seconds
                    reads = new[] { local }.Concat(reads).Take(20);

                    if (i % (ulong)10 == (ulong)0)
                    { // ~1 second
                        var bSec = reads.Sum() / reads.Count();

                        var kbs = ((bSec * 8) / 1024) / 8;

                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            lblDlSpeed.Text = "Kb/s " + kbs;
                            lblInternet.Text = $"{(RES?.Status == IPStatus.Success  ? "Conectado" : $"Sin Conexion ({RES?.Status})")}";
                            lblRamUsage.Text = $"RAM: {GetCurrentMemoryUsage()}/{GetTotalMemoryInMiB()} MB";
                        }));
                    }

                    if (i == ulong.MaxValue)
                        i = 0;

                    i++;
                }

            });
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ClearStandByList();
        }

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        static extern UInt32 DnsFlushResolverCache();

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCacheEntry_A")]
        public static extern int DnsFlushResolverCacheEntry(string hostName);

        public static void FlushCache()
        {
            DnsFlushResolverCache();
        }

        public static void FlushCache(string hostName)
        {
            DnsFlushResolverCacheEntry(hostName);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void btnRenewIP_Click(object sender, EventArgs e)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                //Need to determine which adapter here with some kind of if() statement
                
                objMO.InvokeMethod("ReleaseDHCPLease", null, null);

                objMO.InvokeMethod("RenewDHCPLease", null, null);


            }

            ManagementClass mClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection mObjCol = mClass.GetInstances();
            foreach (ManagementObject mObj in mObjCol)
            {
                if ((bool)mObj["IPEnabled"])
                {
                    ManagementBaseObject mboDNS = mObj.GetMethodParameters("SetDNSServerSearchOrder");
                    if (mboDNS != null)
                    {
                        mboDNS["DNSServerSearchOrder"] = null;
                        mObj.InvokeMethod("SetDNSServerSearchOrder", mboDNS, null);
                    }
                }
            }



            List<string> commands = new List<string>()
            { 
                "ipconfig/release", 
                "ipconfig/renew", 
                "ipconfig/flushdns",
                "ipconfig/registerdns", 
                "netsh dump", 
                "nbtstat -R", 
                "netsh int ip reset reset.log", 
                "netsh winsock reset", 
                "ipconfig /flushdns", 
                "NETSH winsock reset catalog", 
                "NETSH int ipv4 reset reset.log", 
                "NETSH int ipv6 reset reset.log" 
            };
           
            for (int i = 0; i < commands.Count(); i++)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{commands[i]}\"",
                    Verb = "runas"
                };
                Process.Start(startInfo).WaitForExit();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void btnClearWorkingSet_Click(object sender, EventArgs e)
        {
            EmptyWorkingSet();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                
                notifyIcon1.Visible = true;
                this.notifyIcon1.ShowBalloonTip(1000);
                Hide();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void chckOverlayTstMode_CheckedChanged(object sender, EventArgs e)
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\Dwm", true))
                    {
                        if (key != null)
                        {
                            if (chckOverlayTstMode.Checked)
                            {
                                key.SetValue("OverlayTestMode", 5, RegistryValueKind.DWord);
                            }
                            else
                            {
                                // Set to 0 or delete to disable. Let's set it to 0 as it's safer.
                                key.SetValue("OverlayTestMode", 0, RegistryValueKind.DWord);
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Administrator privileges are required to modify this registry key.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    chckOverlayTstMode.Checked = !chckOverlayTstMode.Checked; // Revert change
                }
                catch (Exception ex)
                {
                  MessageBox.Show("An error occurred while modifying the registry: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void chkHwSchMode_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", true))
                {
                    if (key != null)
                    {
                        if (chkHwSchMode.Checked)
                        {
                            key.SetValue("HwSchMode", 2, RegistryValueKind.DWord);
                        }
                        else
                        {
                            key.SetValue("HwSchMode", 1, RegistryValueKind.DWord);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Administrator privileges are required to modify GPU scheduling settings.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chkHwSchMode.Checked = !chkHwSchMode.Checked; // Revert change
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while modifying the registry: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkBcdTweaks_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkBcdTweaks.Checked)
                {
                    RunBcdCommand("/set disabledynamictick yes");
                    RunBcdCommand("/set useplatformtick yes");
                    RunBcdCommand("/set tscsyncpolicy Enhanced");
                }
                else
                {
                    RunBcdCommand("/deletevalue disabledynamictick");
                    RunBcdCommand("/deletevalue useplatformtick");
                    RunBcdCommand("/deletevalue tscsyncpolicy");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying BCD tweaks: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunBcdCommand(string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo("bcdedit", arguments)
            {
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            try
            {
                Process.Start(psi).WaitForExit();
            }
            catch { }
        }
        
        private void InitializeRegistryCheckboxes()
        {
            // Temporarily remove event handlers to avoid triggering them during initialization
            this.chckOverlayTstMode.CheckedChanged -= new System.EventHandler(this.chckOverlayTstMode_CheckedChanged);
            this.chkHwSchMode.CheckedChanged -= new System.EventHandler(this.chkHwSchMode_CheckedChanged);
            this.chkBcdTweaks.CheckedChanged -= new System.EventHandler(this.chkBcdTweaks_CheckedChanged);

            // Initialize OverlayTestMode CheckBox
            if (Environment.OSVersion.Version.Major >= 10)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\Dwm"))
                    {
                        if (key != null)
                        {
                            var value = key.GetValue("OverlayTestMode");
                            if (value != null && value is int intValue && intValue == 5)
                                chckOverlayTstMode.Checked = true;
                            else
                                chckOverlayTstMode.Checked = false;
                        }
                    }
                }
                catch { }
            }

            // Initialize Hardware GPU Scheduling CheckBox
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("HwSchMode");
                        if (value != null && value is int intValue && intValue == 2)
                            chkHwSchMode.Checked = true;
                        else
                            chkHwSchMode.Checked = false;
                    }
                }
            }
            catch { }

            // Initialize BCD Tweaks CheckBox (Simple check by running bcdedit and checking output)
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("bcdedit", "/enum {current}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    if (output.Contains("disabledynamictick    Yes") && output.Contains("useplatformtick       Yes"))
                    {
                        chkBcdTweaks.Checked = true;
                    }
                }
            }
            catch { }

            // Re-hook event handlers
            this.chckOverlayTstMode.CheckedChanged += new System.EventHandler(this.chckOverlayTstMode_CheckedChanged);
            this.chkHwSchMode.CheckedChanged += new System.EventHandler(this.chkHwSchMode_CheckedChanged);
            this.chkBcdTweaks.CheckedChanged += new System.EventHandler(this.chkBcdTweaks_CheckedChanged);
        }

        private void cmbTimeSpanTimer_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbTimeSpanTimer.SelectedItem.ToString())
            {
               
                case "5 minutes":
                    tmrRam.Interval = 300_000;
                    break;
                case "10 minutes":
                    tmrRam.Interval = 600_000;
                    break;
                case "15 minutes":
                    tmrRam.Interval = 900_000;
                    break;
                case "20 minutes":
                    tmrRam.Interval = 1200_000;
                    break;
                default:
                    tmrRam.Interval = 900_000;
                    break;
            }
        }
    }
}
