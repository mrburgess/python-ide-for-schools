using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyIDE_WPF.Models
{
    public class DataReceivedEventArgs : EventArgs
    {
        public string Data { get; private set; }

        public DataReceivedEventArgs(string data)
        {
            this.Data = data;
        }
    }

    public class CommandReceivedEventArgs : EventArgs
    {
        public string Command { get; private set; }

        public CommandReceivedEventArgs(string command)
        {
            this.Command = command;
        }
    }

    class AsyncReader
    {
        private Stream stream;
        TimeSpan bufferTimeout;
        private bool processCommands;
        private char startCommandChar;
        private char endCommandChar;

        public AsyncReader(Stream stream, TimeSpan bufferTimeout)
        {
            this.stream = stream;
            this.bufferTimeout = bufferTimeout;
            this.processCommands = false;
        }

        public AsyncReader(Stream stream, TimeSpan bufferTimeout, char startCommandChar, char endCommandChar)
        {
            this.stream = stream;
            this.bufferTimeout = bufferTimeout;
            this.startCommandChar = startCommandChar;
            this.endCommandChar = endCommandChar;
            this.processCommands = true;
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event EventHandler<CommandReceivedEventArgs> CommandReceived;

        public void BeginRead()
        {
            Task.Run(async () =>
            {
                byte[] buffer = new byte[100000];
                StringBuilder sbOutput = new StringBuilder(buffer.Length);
                StringBuilder sbCommand = new StringBuilder(1000);

                bool inCommandMode = false;

                while (true)
                {
                    int count = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            char c = Convert.ToChar(buffer[i]);

                            if (c == startCommandChar && processCommands)
                            {
                                inCommandMode = true;
                                continue;
                            }
                            else if (c == endCommandChar && processCommands)
                            {
                                OnCommandReceived(sbCommand.ToString());
                                sbCommand.Clear();
                                inCommandMode = false;
                                continue;
                            }
                            else if (inCommandMode)
                            {
                                sbCommand.Append(c);
                                continue;
                            }
                            else if (c == '\r')
                            {
                                sbOutput.Append(Environment.NewLine);
                                OnDataReceived(sbOutput.ToString());
                                sbOutput.Clear();
                            }
                            else if (c == '\n')
                            {
                                // Just eat it
                            }
                            else
                            {
                                sbOutput.Append(c);
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(bufferTimeout);
                    }

                    if (sbOutput.Length > 0)
                    {
                        OnDataReceived(sbOutput.ToString());
                        sbOutput.Clear();
                    }
                }
            });
        }

        private void OnCommandReceived(string command)
        {
            if (CommandReceived != null)
            {
                CommandReceived(this, new CommandReceivedEventArgs(command));
            }
        }

        private void OnDataReceived(string data)
        {
            if (DataReceived != null)
            {
                DataReceived(this, new DataReceivedEventArgs(data));
            }
        }
    }
}
