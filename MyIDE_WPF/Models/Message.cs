using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyIDE_WPF.Models
{
    public class Message
    {
        public const char StartMarker = (char)17;
        public const char EndMarker = (char)18;
        public const char Separator = ':';

        public string Subject { get; set; } = String.Empty;

        public string Content { get; set; } = String.Empty;

        public Message()
        {
            Subject = String.Empty;
            Content = String.Empty;
        }

        public Message(string subject, string content)
        {
            this.Subject = subject;
            this.Content = content ?? string.Empty;
        }

        public string ToString(bool includeStartAndEndMarkers)
        {
            StringBuilder sb = new StringBuilder(Subject.Length + Content.Length + 5);

            if (includeStartAndEndMarkers)
                sb.Append(Message.StartMarker);

            if (!string.IsNullOrWhiteSpace(Subject))
            {
                sb.Append(Subject);
                sb.Append(Separator);
            }

            sb.Append(Content);

            if (includeStartAndEndMarkers)
                sb.Append(Message.EndMarker);

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(false);
        }

        static Regex regex = new Regex(@"((?<subject>.*?):)?(?<content>.*)", RegexOptions.Singleline);

        public static Message Parse(string text)
        {
            var match = regex.Match(text);
            return new Message
            {
                Subject = match.Groups["subject"]?.Value ?? string.Empty,
                Content = match.Groups["content"]?.Value ?? string.Empty
            };
        }
    }
}
