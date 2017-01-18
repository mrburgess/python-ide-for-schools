using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyIDE_WPF.Models
{
    public class RunnerOutputEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public RunnerOutputEventArgs(string text)
        {
            this.Text = text;
        }
    }

    public class RunnerErrorMessageEventArgs : EventArgs
    {
        public string ErrorMessage { get; private set; }

        public RunnerErrorMessageEventArgs(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }
    }

    public class RunnerInputEventArgs : EventArgs
    {
        public string Prompt { get; private set; }

        public RunnerInputEventArgs(string prompt)
        {
            this.Prompt = prompt;
        }
    }

    public class PythonRunner
    {
        public event EventHandler<RunnerErrorMessageEventArgs> Error;
        public event EventHandler<RunnerOutputEventArgs> Output;
        public event EventHandler<EventArgs> Terminated;
        public event EventHandler<RunnerInputEventArgs> Input;

        private Process pythonProcess;

        public TimeSpan BufferTimeout { get; set; }

        public PythonRunner()
        {
            BufferTimeout = TimeSpan.FromMilliseconds(100);
        }

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

        private string InstrumentCode(string originalCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("import sys");
            sb.AppendLine();
            sb.AppendLine("def my_input(prompt=''):");
            sb.AppendLine("  print(chr(17) + 'INPUT:' + prompt + chr(18), end='')");
            sb.AppendLine("  sys.stdout.flush()");
            sb.AppendLine("  return sys.stdin.readline()");
            sb.AppendLine();
            sb.AppendLine("__builtins__.input = my_input");
            sb.AppendLine();
            sb.Append(originalCode);

            return sb.ToString();
        }

        public void BeginRun(string programCode)
        {
            string instrumentedCode = InstrumentCode(programCode);

            File.WriteAllText("temp.py", instrumentedCode);

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

            var dataAndCommandReader = new AsyncReader(
                pythonProcess.StandardOutput.BaseStream, BufferTimeout, (char)0x11, (char)0x12);

            dataAndCommandReader.DataReceived += OutputAndCommandReader_DataReceived;
            dataAndCommandReader.CommandReceived += DataAndCommandReader_CommandReceived;
            dataAndCommandReader.BeginRead();

            var errorReader = new AsyncReader(pythonProcess.StandardError.BaseStream, BufferTimeout);
            errorReader.DataReceived += ErrorReader_DataReceived;
            errorReader.BeginRead();
        }

        public void SubmitInput(string input)
        {
            pythonProcess.StandardInput.WriteLine(input.Trim(Environment.NewLine.ToCharArray()));
        }

        Regex commandRegex = new Regex(@"^(?<command>\w+?):(?<argument>.*)$", RegexOptions.Singleline);

        private void DataAndCommandReader_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            Match match = commandRegex.Match(e.Command);
            if (match.Success)
            {
                string command = match.Groups["command"].Value;
                string argument = match.Groups["argument"].Value;

                if (command == "INPUT")
                {
                    OnInput(argument);
                }
            }
        }

        private void PythonProcess_Exited(object sender, EventArgs e)
        {
            OnTerminated();
        }

        private void ErrorReader_DataReceived(object sender, DataReceivedEventArgs e)
        {
            OnError(e.Data);
        }

        private void OutputAndCommandReader_DataReceived(object sender, DataReceivedEventArgs e)
        {
            OnOutput(e.Data);
        }

        private void OnError(string errorMessage)
        {
            if (Error != null)
            {
                Error(this, new RunnerErrorMessageEventArgs(errorMessage));
            }
        }

        private void OnOutput(string text)
        {
            if (Output != null)
            {
                Output(this, new RunnerOutputEventArgs(text));
            }
        }

        private void OnInput(string prompt)
        {
            if (Input != null)
            {
                Input(this, new RunnerInputEventArgs(prompt));
            }
        }
    }
}
