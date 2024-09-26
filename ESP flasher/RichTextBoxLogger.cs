using Microsoft.Extensions.Logging;

namespace ESP_Flasher.Logging
{
    public class RichTextBoxLogger : ILogger
    {
        private readonly RichTextBox _richTextBox;
        private readonly string _categoryName;

        public RichTextBoxLogger(RichTextBox richTextBox, string categoryName)
        {
            _richTextBox = richTextBox;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true; // Enable all log levels by default

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = $"{DateTime.Now} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
            if (exception != null)
            {
                message += $" Exception: {exception.Message}";
            }

            // Log the message to the RichTextBox
            WriteMessageToRichTextBox(message);
        }

        private void WriteMessageToRichTextBox(string message)
        {
            if (_richTextBox.InvokeRequired)
            {
                _richTextBox.Invoke((Action)(() => AppendText(message)));
            }
            else
            {
                AppendText(message);
            }
        }

        private void AppendText(string message)
        {
            _richTextBox.AppendText(message + Environment.NewLine);
            _richTextBox.ScrollToCaret(); // Ensure the RichTextBox scrolls to the latest message
        }
    }
}
