using System.ComponentModel;
using System.IO.Ports;

namespace ESP_Flasher.UIBinders
{
    public class SerialPortBinder
    {
        private readonly ComboBox _comboBoxSerialPort;
        private readonly ComboBox _comboBoxBaudRate;

        // Configurable list of baud rates and default baud rate
        public List<int> BaudRates { get; set; } = new List<int> { 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        public int DefaultBaudRate { get; set; } = 921600;

        // Configurable list of serial ports and default serial port
        public List<string> SerialPorts { get; set; } = SerialPort.GetPortNames().ToList();
        public string DefaultSerialPort { get; set; } = "COM30";

        public SerialPortBinder(ComboBox comboBoxSerialPort, ComboBox comboBoxBaudRate)
        {
            _comboBoxSerialPort = comboBoxSerialPort;
            _comboBoxBaudRate = comboBoxBaudRate;

            SetupBaudRates();
            LoadSerialPorts();
        }

        // Method to load available serial ports
        public void LoadSerialPorts()
        {
            _comboBoxSerialPort.Items.Clear();

            // Load the configured or detected serial ports
            foreach (string port in SerialPorts)
            {
                _comboBoxSerialPort.Items.Add(port);
            }

            // Set the default port if it exists in the list
            if (SerialPorts.Contains(DefaultSerialPort))
            {
                _comboBoxSerialPort.SelectedItem = DefaultSerialPort;
            }
            else if (_comboBoxSerialPort.Items.Count > 0)
            {
                // Otherwise, select the first available port
                _comboBoxSerialPort.SelectedIndex = 0;
            }
        }

        // Method to set up common baud rates
        private void SetupBaudRates()
        {
            _comboBoxBaudRate.Items.Clear();

            // Load the configured baud rates
            foreach (int rate in BaudRates)
            {
                _comboBoxBaudRate.Items.Add(rate);
            }

            // Set the default baud rate if it's in the list
            if (BaudRates.Contains(DefaultBaudRate))
            {
                _comboBoxBaudRate.SelectedItem = DefaultBaudRate;
            }
            else
            {
                // Otherwise, select the first available rate
                _comboBoxBaudRate.SelectedIndex = 0;
            }
        }

        // Get the selected baud rate, defaulting to the pre-configured default if none is selected
        public int SelectedBaudRate
        {
            get
            {
                if (_comboBoxBaudRate.SelectedItem != null && int.TryParse(_comboBoxBaudRate.SelectedItem.ToString(), out int baudRate))
                {
                    return baudRate;
                }
                return DefaultBaudRate;
            }
        }
        public string SelectedSerialPortName => _comboBoxSerialPort.SelectedItem?.ToString() ?? DefaultSerialPort;
    }
}

