using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace ProcessControlApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private async void KillHeavyProcessesButton_Click(object sender, EventArgs e)
        {
            // Kill heavy and hung processes
            await KillProcessesAsync(p => !p.Responding && p.PrivateMemorySize64 > 50_000_000);
        }

        private async void StopDefenderSymantecButton_Click(object sender, EventArgs e)
        {
            // Stop Defender/Symantec processes
            await StopProcessesByNameAsync("defender");
            await StopProcessesByNameAsync("symantec");
        }

        private async void StopChildProcessesButton_Click(object sender, EventArgs e)
        {
            // Stop child processes
            var parentProcess = Process.GetCurrentProcess();
            await KillProcessesAsync(p => IsChildProcess(p, parentProcess));
        }

        private async Task KillProcessesAsync(Func<Process, bool> condition)
        {
            var processes = Process.GetProcesses().Where(condition).ToList();
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    await Task.Run(() => process.WaitForExit());
                }
                catch (Exception ex)
                {
                    LogError($"Error killing process {process.ProcessName} (ID: {process.Id}): {ex.Message}");
                }
            }
            MessageBox.Show("Operation completed.");
        }

        private async Task StopProcessesByNameAsync(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    await Task.Run(() => process.WaitForExit());
                }
                catch (Exception ex)
                {
                    LogError($"Error stopping process {process.ProcessName} (ID: {process.Id}): {ex.Message}");
                }
            }
            MessageBox.Show($"Stopped all processes named {processName}.");
        }

        private bool IsChildProcess(Process process, Process parent)
        {
            try
            {
                var query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId={process.Id}";
                var search = new ManagementObjectSearcher("root\\CIMV2", query);
                var results = search.Get().Cast<ManagementObject>().FirstOrDefault();

                if (results != null && results["ParentProcessId"] != null)
                {
                    var parentId = Convert.ToInt32(results["ParentProcessId"]);
                    return parentId == parent.Id;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error determining parent of process {process.ProcessName} (ID: {process.Id}): {ex.Message}");
            }
            return false;
        }

        private void LogError(string message)
        {
            // Log the error message to a file or other logging mechanism
            // For simplicity, we'll use a message box in this example
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
