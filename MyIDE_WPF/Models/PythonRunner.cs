using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private Stream GetResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string fullName = assembly.GetName().Name + "." + name;
            return assembly.GetManifestResourceStream(fullName);
        }

        public void BeginRun(string programCode)
        {
            // Write the startup script to disk
            // (We do this each time so that people can't fiddle with it)
            using (Stream stream = GetResourceStream("startup.py"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    File.WriteAllText("startup.py", reader.ReadToEnd());
                }
            }

                // Write the user's code to disk
                File.WriteAllText("temp.py", programCode);

            // Launch the statup.py script, which gets everything ready
            // and then executes the user's code (via exec)
            // For now, we only support Python 3
            ProcessStartInfo pythonStartInfo = new ProcessStartInfo("py", "-3 -u startup.py temp.py")
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
            pythonProcess.StandardInput.WriteLine(input);
        }

        Regex commandRegex = new Regex(@"^(?<command>\w+?):(?<argument>.*)$", RegexOptions.Singleline);

        private void DataAndCommandReader_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            Match match = commandRegex.Match(e.Command);
            if (match.Success)
            {
                string command = match.Groups["command"].Value;
                string argument = match.Groups["argument"].Value;

                switch (command)
                {
                    case "INPUT":
                        OnInput(argument);
                        break;

                    case "LINE":
                        OnLine(int.Parse(argument));
                        break;
                }
            }
        }

        private void OnLine(int lineNumber)
        {
            // Experimental only...
            OnOutput($"*** LINE {lineNumber}{Environment.NewLine}");
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
