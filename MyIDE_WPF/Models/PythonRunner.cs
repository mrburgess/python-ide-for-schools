using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIDE_WPF.Models
{
    public class PythonRunner
    {
        private Process pythonProcess;

        public TimeSpan BufferTimeout { get; set; }

        public PythonRunner()
        {
            BufferTimeout = TimeSpan.FromMilliseconds(100);
        }

        public event EventHandler<EventArgs> Terminated;

        public bool IsRunning { get; private set; }

        public void TerminateRun()
        {
            if (pythonProcess != null)
            {
                if (!pythonProcess.HasExited)
                {
                    pythonProcess.Kill();
                }
            }
        }

        private void OnTerminated()
        {
            IsRunning = false;

            if (Terminated != null)
            {
                Terminated(this, EventArgs.Empty);
            }
        }

        public void BeginRun(string programCode)
        {
            File.WriteAllText("temp.py", programCode);

            ProcessStartInfo pythonStartInfo = new ProcessStartInfo("python.exe", "-u temp.py")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            pythonProcess = new Process
            {
                StartInfo = pythonStartInfo,
                EnableRaisingEvents = true
            };

            pythonProcess.Exited += PythonProcess_Exited;

            pythonProcess.Start();

            IsRunning = true;

            var outputReader = new AsyncReader(pythonProcess.StandardOutput.BaseStream, BufferTimeout);
            outputReader.DataReceived += OutputReader_DataReceived;
            outputReader.BeginReadData();

            var errorReader = new AsyncReader(pythonProcess.StandardError.BaseStream, BufferTimeout);
            errorReader.DataReceived += ErrorReader_DataReceived;
            errorReader.BeginReadData();

        }

        private void PythonProcess_Exited(object sender, EventArgs e)
        {
            OnTerminated();
        }

        private void ErrorReader_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (ErrorReceived != null)
            {
                ErrorReceived(this, new DataReceivedEventArgs(e.Data));
            }
        }

        private void OutputReader_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (OutputReceived != null)
            {
                OutputReceived(this, new DataReceivedEventArgs(e.Data));
            }
        }

        public event EventHandler<DataReceivedEventArgs> ErrorReceived;

        public event EventHandler<DataReceivedEventArgs> OutputReceived;
    }
}
