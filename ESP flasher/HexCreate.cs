using System;
using System.Diagnostics;
using System.Threading;

namespace ESP_flasher
{
    [Obsolete("This is obsolete, use 'I32HEX' instead.")]
    public class HexCreator
    {
        SemaphoreSlim busy = new SemaphoreSlim(0,1);

        public async void Create(string binFile, string hexFile)
        {
            string args = $"\"${binFile}\" -Binary -o \"${hexFile}\" -Intel";
            Start(args);
            await busy.WaitAsync();
        }


        Process proc = new Process();
        void Start(string arguments)
        {
            proc = new Process();
            proc.StartInfo.FileName = "srec_cat.exe";
            //proc.OutputDataReceived += EspTool_OutputDataReceived;
            proc.Exited += Proc_Exited;
            //proc.ErrorDataReceived += EspTool_ErrorDataReceived;

            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.EnableRaisingEvents = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }

        private void Proc_Exited(object sender, System.EventArgs e)
        {
            busy.Release();
        }
    }
}
