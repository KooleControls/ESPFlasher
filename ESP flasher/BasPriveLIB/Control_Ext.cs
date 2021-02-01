using System;
using System.Windows.Forms;

namespace BasPriveLIB
{
    public static class Control_Ext
    {
        private delegate void SafeCallDelegate<T>(T c, Action<T> action);
        public static void InvokeIfRequired<T>(this T c, Action<T> action)
            where T : Control
        {
            if (c.InvokeRequired)
            {
                c.Invoke(new SafeCallDelegate<T>(InvokeIfRequired), new object[] { c, action });
            }
            else
            {
                action(c);
            }
        }
    }
}
