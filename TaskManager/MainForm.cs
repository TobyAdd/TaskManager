using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TaskManager
{
    public partial class MainForm : Form
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        List<string> Suspendlist = new List<string>();
        bool killed;
        public MainForm()
        {
            InitializeComponent();
            ListProcess();
        }
        void ListProcess()
        {
            Process[] proceesList = Process.GetProcesses();
            foreach (Process p in proceesList)
            {
                ProcessList.Items.Add(p.ProcessName);
            }
        }

        void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);
            
            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);
                CloseHandle(pOpenThread);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ProcessList.Items.Clear();
            ListProcess();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            killed = true;
            Process[] KillProcess = Process.GetProcessesByName(ProcessList.SelectedItem.ToString());

            foreach (Process Kill in KillProcess)
            {
                Kill.Kill();
                Kill.WaitForExit();
                Kill.Dispose();
                ProcessList.Items.Remove(ProcessList.SelectedItem);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var processid = Process.GetProcessesByName(ProcessList.SelectedItem.ToString());

            if (button2.Text == "Suspend")
            {           
                foreach (var process in processid)
                {
                    SuspendProcess(process.Id);
                    Suspendlist.Add(ProcessList.SelectedItem.ToString());
                }
                button2.Text = "Resume";
            }
            else
            {
                foreach (var process in processid)
                {
                    ResumeProcess(process.Id);
                    Suspendlist.Remove(ProcessList.SelectedItem.ToString());
                }
                button2.Text = "Suspend";
            }
        }

        private void ProcessList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (killed)
            {
                killed = false;
            }
            else
            {                
                if (Suspendlist.Contains(ProcessList.SelectedItem.ToString()))
                {
                    button2.Text = "Resume";
                }
                else
                {
                    button2.Text = "Suspend";
                }
            }


        }
    }
}
