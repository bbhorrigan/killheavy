using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Management;
using System.Threading.Tasks;

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

        private async void ListProcessesButton_Click(object sender, EventArgs e)
        {
            // List all processes
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToList();
            var processList = string.Join(Environment.NewLine, processes.Select(p => $"{p.ProcessName} (ID: {p.Id})"));
            MessageBox.Show(processList, "Running Processes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void KillSelectedProcessesButton_Click(object sender, EventArgs e)
        {
            // Allow user to select and kill specific processes
            var processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToList();
            using (var form = new SelectProcessesForm(processes))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var selectedProcesses = form.SelectedProcesses;
                    await KillProcessesAsync(p => selectedProcesses.Contains(p));
                }
            }
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

    public class SelectProcessesForm : Form
    {
        public List<Process> SelectedProcesses { get; private set; }

        private CheckedListBox checkedListBox;

        public SelectProcessesForm(List<Process> processes)
        {
            Text = "Select Processes to Kill";
            Width = 400;
            Height = 600;

            checkedListBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true
            };

            foreach (var process in processes)
            {
                checkedListBox.Items.Add(process, false);
            }

            var okButton = new Button
            {
                Text = "OK",
                Dock = DockStyle.Bottom
            };
            okButton.Click += OkButton_Click;

            Controls.Add(checkedListBox);
            Controls.Add(okButton);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            SelectedProcesses = checkedListBox.CheckedItems.Cast<Process>().ToList();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
