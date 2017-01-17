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

    class AsyncReader
    {
        private Stream stream;
        TimeSpan bufferTimeout;

        public AsyncReader(Stream stream, TimeSpan bufferTimeout)
        {
            this.stream = stream;
            this.bufferTimeout = bufferTimeout;
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public void BeginReadData()
        {
            Task.Run(async () =>
            {
                byte[] buffer = new byte[200000];
                StringBuilder sb = new StringBuilder(200000);

                while (true)
                {
                    int count = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            char c = Convert.ToChar(buffer[i]);

                            if (c == '\r')
                            {
                                sb.Append(Environment.NewLine);
                                OnDataReceived(sb.ToString());
                                sb.Clear();
                                continue;
                            }
                            else if (c == '\n')
                            {
                                continue;
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(bufferTimeout);
                    }

                    if (sb.Length > 0)
                    {
                        OnDataReceived(sb.ToString());
                        sb.Clear();
                    }
                }
            });
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
