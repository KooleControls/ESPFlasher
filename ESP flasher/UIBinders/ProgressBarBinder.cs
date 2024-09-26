namespace ESP_Flasher.UIBinders
{
    public class ProgressBarBinder
    {
        private readonly ProgressBar _progressBar;

        public ProgressBarBinder(ProgressBar progressBar)
        {
            _progressBar = progressBar;

            // Ensure the progress bar is set up correctly for percentage updates
            _progressBar.Minimum = 0;
            _progressBar.Maximum = 100;
            _progressBar.Value = 0;
        }

        public IProgress<float> Bind()
        {
            return new Progress<float>(value =>
            {
                // Convert the float value (0.0 to 1.0) to a percentage (0 to 100) and update the ProgressBar.
                int progressValue = (int)(value * 100);

                // Ensure thread safety when updating the UI component
                if (_progressBar.InvokeRequired)
                {
                    _progressBar.Invoke(new Action(() =>
                    {
                        _progressBar.Value = Math.Min(Math.Max(progressValue, 0), 100); // Clamp to range 0-100
                    }));
                }
                else
                {
                    _progressBar.Value = Math.Min(Math.Max(progressValue, 0), 100); // Clamp to range 0-100
                }
            });
        }
    }




}

