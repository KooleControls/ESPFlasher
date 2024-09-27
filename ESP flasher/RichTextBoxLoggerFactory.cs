using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace ESP_Flasher.Logging
{
    public class RichTextBoxLoggerFactory : ILoggerFactory
    {
        private readonly RichTextBox _richTextBox;
        private readonly ConcurrentDictionary<string, RichTextBoxLogger> _loggers = new ConcurrentDictionary<string, RichTextBoxLogger>();

        public RichTextBoxLoggerFactory(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new RichTextBoxLogger(_richTextBox, name));
        }

        public void AddProvider(ILoggerProvider provider)
        {
            // Optional: For extending with external providers
        }

        public void Clear()
        {
            _richTextBox.Clear();
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
