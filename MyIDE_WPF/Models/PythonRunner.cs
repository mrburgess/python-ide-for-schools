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
    public enum ExecutionState
    {
        Stopped,
        Running,
        Paused,
        WaitingForInput
    }

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

    public class ExecutionStateChangedEventArgs : EventArgs
    {
        public ExecutionState ExecutionState { get; private set; }
        public ExecutionState OldExecutionState { get; private set; }

        public ExecutionStateChangedEventArgs(ExecutionState oldExecutionState, ExecutionState newExecutionState)
        {
            this.ExecutionState = newExecutionState;
            this.OldExecutionState = oldExecutionState;
        }
    }

    public class PythonRunner
    {
        public event EventHandler<RunnerErrorMessageEventArgs> Error;
        public event EventHandler<RunnerOutputEventArgs> Output;
        public event EventHandler<ExecutionStateChangedEventArgs> ExecutionStateChanged;

        private Process pythonProcess;

        [Obsolete("Use the ExecutionState property instead.")]
        private ExecutionState _executionState = ExecutionState.Stopped;

        public int LineNumber { get; private set; } = 0;

        public string Prompt { get; private set; } = String.Empty;

        public ExecutionState ExecutionState
        {
            get
            {
                return _executionState;
            }
            set
            {
                if (_executionState != value)
                {
                    var oldExecutionState = _executionState;
                    _executionState = value;
                    if (ExecutionStateChanged != null)
                    {
                        ExecutionStateChanged(this, new ExecutionStateChangedEventArgs(oldExecutionState, value));
                    }
                }
            }
        }

        public TimeSpan BufferTimeout { get; set; }

        public PythonRunner()
        {
            BufferTimeout = TimeSpan.FromMilliseconds(100);
        }

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

        private Stream GetResourceStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string fullName = assembly.GetName().Name + "." + name;
            return assembly.GetManifestResourceStream(fullName);
        }

        public void BeginRun(string programCode)
        {
            LineNumber = 0;
            ExecutionState = ExecutionState.Stopped;

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

            ExecutionState = ExecutionState.Running;

            var outputReader = new AsyncReader(
                pythonProcess.StandardOutput.BaseStream, BufferTimeout, detectMessages:true);

            outputReader.TextReceived += OutputReader_TextReceived;
            outputReader.MessageReceived += OutputReader_MessageReceived;
            outputReader.BeginRead();

            var errorReader = new AsyncReader(
                pythonProcess.StandardError.BaseStream, BufferTimeout, detectMessages:false);

            errorReader.TextReceived += ErrorReader_TextReceived;
            errorReader.BeginRead();
        }

        public void SubmitInput(string input)
        {
            Debug.Assert(ExecutionState == ExecutionState.WaitingForInput);

            ExecutionState = ExecutionState.Running;
            SendMessageToPython(new Message("INPUT_RESPONSE", input));
        }

        public void SendMessageToPython(Message message)
        {
            string raw = message.ToString(includeStartAndEndMarkers: false);
            pythonProcess.StandardInput.WriteLine(raw);
        }

        private void OutputReader_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            switch (e.Message.Subject)
            {
                case "INPUT":
                    Prompt = e.Message.Content;
                    ExecutionState = ExecutionState.WaitingForInput;
                    break;

                case "LINE":
                    LineNumber = int.Parse(e.Message.Content);
                    ExecutionState = ExecutionState.Paused;
                    break;
            }
        }

        public void StepNextLine()
        {
            Debug.Assert(ExecutionState == ExecutionState.Paused);

            ExecutionState = ExecutionState.Running;
            SendMessageToPython(new Message("CONTINUE", ""));
        }

        private void PythonProcess_Exited(object sender, EventArgs e)
        {
            LineNumber = 0;
            ExecutionState = ExecutionState.Stopped;
        }

        private void ErrorReader_TextReceived(object sender, TextReceivedEventArgs e)
        {
            if (Error != null)
            {
                Error(this, new RunnerErrorMessageEventArgs(e.Text));
            }
        }

        private void OutputReader_TextReceived(object sender, TextReceivedEventArgs e)
        {
            if (Output != null)
            {
                Output(this, new RunnerOutputEventArgs(e.Text));
            }
        }
    }
}
