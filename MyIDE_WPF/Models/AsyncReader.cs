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
    public class TextReceivedEventArgs : EventArgs
    {
        public string Text { get; private set; }

        public TextReceivedEventArgs(string data)
        {
            this.Text = data;
        }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public Message Message { get; private set; }

        public MessageReceivedEventArgs(Message message)
        {
            this.Message = message;
        }
    }

    class AsyncReader
    {
        private Stream stream;
        TimeSpan bufferTimeout;
        private bool detectMessages;

        private const char MessageStartDelimiter = (char)17;
        private char MessageEndDelimiter = (char)18;

        public AsyncReader(Stream stream, TimeSpan bufferTimeout, bool detectMessages)
        {
            this.stream = stream;
            this.bufferTimeout = bufferTimeout;
            this.detectMessages = detectMessages;
        }

        public event EventHandler<TextReceivedEventArgs> TextReceived;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private enum State
        {
            Text,
            Message
        }

        private void SendText(StringBuilder buffer)
        {
            if (buffer.Length > 0)
            {
                string text = buffer.ToString();
                buffer.Clear();
                OnTextReceived(text);
            }
        }

        private void SendMessage(StringBuilder buffer)
        {
            Message message = Message.Parse(buffer.ToString());
            buffer.Clear();
            OnMessageReceived(message);
        }

        public void BeginRead()
        {
            Task.Run(async () =>
            {
                byte[] rawBuffer = new byte[100000];
                StringBuilder textBuffer = new StringBuilder(rawBuffer.Length);
                StringBuilder messageBuffer = new StringBuilder(1000);
                State state = State.Text;

                // We'll collect text until end-of-line or until the stream dries up
                // At that point we'll send the text collected so far to the consumer
                // This means the IDE can show lines that are part complete, for example
                // when we're waiting for an Input
                while (true)
                {
                    // Read as much from the stream as possible (up to the buffer size)
                    // If there's nothing available, count will be zero
                    int count = await stream.ReadAsync(rawBuffer, 0, rawBuffer.Length);

                    if (count == 0)
                    {
                        // Nothing in the stream
                        // Do we have anything already buffered we could send?
                        if (textBuffer.Length > 0)
                        {
                            // Send the text from the buffer, then clear it
                            SendText(textBuffer);
                        }
                        else
                        {
                            // Input stream has dried up
                            // Pause slightly before checking again (prevents high CU usage)
                            await Task.Delay(bufferTimeout);
                        }
                    }
                    else
                    {
                        // We picked up some characters from the input stream
                        // Process each character in turn
                        for (int i = 0; i < count; i++)
                        {
                            char c = Convert.ToChar(rawBuffer[i]);

                            // Modify how the character is handled if we're part-way through a message
                            switch (state)
                            {
                                case State.Text:
                                    if (c == MessageStartDelimiter && detectMessages)
                                    {
                                        // We've received the start-of-message signal
                                        state = State.Message;
                                    }
                                    else if (c == '\r')
                                    {
                                        // End of line will be \r\n
                                        // We'll handle the \n and ignore the \r
                                        // So we don't double up newlines
                                    }
                                    else if (c == '\n')
                                    {
                                        // End of line
                                        // Send the text from the buffer, then clear it
                                        textBuffer.Append(Environment.NewLine);
                                    }
                                    else
                                    {
                                        // Keep adding the text to our buffer
                                        // until end of line, or until the stream dries up
                                        textBuffer.Append(c);
                                    }
                                    break;

                                case State.Message:
                                    if (c == MessageEndDelimiter)
                                    {
                                        // We've seen the end-of-message marker
                                        // Send the current message to the consumer
                                        state = State.Text;
                                        SendMessage(messageBuffer);
                                    }
                                    else
                                    {
                                        messageBuffer.Append(c);
                                    }
                                    break;
                            }
                        }

                        SendText(textBuffer);
                    }
                }
            });
        }

        private void OnMessageReceived(Message message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageReceivedEventArgs(message));
            }
        }

        private void OnTextReceived(string text)
        {
            if (TextReceived != null)
            {
                TextReceived(this, new TextReceivedEventArgs(text));
            }
        }
    }
}
