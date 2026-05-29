using System.Text;

namespace ESP_Flasher
{
    // Tiny modeless hex viewer: 16 bytes per line, offset + hex + ASCII.
    public class HexViewerForm : Form
    {
        private const int BytesPerLine = 16;

        public HexViewerForm(string title, uint baseAddress, byte[] data)
        {
            Text = $"Peek - {title} ({data.Length} bytes)";
            Width = 760;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericMonospace, 9.5f),
                WordWrap = false,
                Text = Format(baseAddress, data),
            };
            Controls.Add(textBox);
        }

        private static string Format(uint baseAddress, byte[] data)
        {
            var sb = new StringBuilder(data.Length * 4);
            for (int i = 0; i < data.Length; i += BytesPerLine)
            {
                int len = Math.Min(BytesPerLine, data.Length - i);
                sb.Append($"{baseAddress + i:X8}  ");

                for (int j = 0; j < BytesPerLine; j++)
                {
                    if (j < len) sb.Append($"{data[i + j]:X2} ");
                    else sb.Append("   ");
                    if (j == 7) sb.Append(' '); // mid-line gap
                }

                sb.Append(' ');
                for (int j = 0; j < len; j++)
                {
                    byte b = data[i + j];
                    sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
