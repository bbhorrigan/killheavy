using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ProcessControlApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void KillHeavyProcessesButton_Click(object sender, EventArgs e)
        {
            // Kill heavy and hung processes
            KillProcesses(p => !p.Responding && p.PrivateMemorySize64 > 50000000);
        }

        private void StopDefenderSymantecButton_Click(object sender, EventArgs e)
        {
            // Stop Defender/Symantec processes
            StopProcessesByName("defender.exe");
            StopProcessesByName("symantec.exe");
        }

        private void StopChildProcessesButton_Click(object sender, EventArgs e)
        {
            // Stop child processes
            KillProcesses(p => p.Parent() == Process.GetCurrentProcess());
        }

        private void KillProcesses(Func<Process, bool> condition)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (condition(process))
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void StopProcessesByName(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}
