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
            // Kill all heavy processes that are hung
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Responding == false && process.PrivateMemorySize64 > 50000000) // Customize the condition for heavy and hung processes
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

        private void StopDefenderSymantecButton_Click(object sender, EventArgs e)
        {
            // Stop all Defender/Symantec processes
            StopProcessesByName("defender.exe");
            StopProcessesByName("symantec.exe");
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

        private void StopChildProcessesButton_Click(object sender, EventArgs e)
        {
            // Stop child processes
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Parent() == Process.GetCurrentProcess())
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
    }
}
