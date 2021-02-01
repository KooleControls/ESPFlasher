using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BasPriveLIB
{

    public class PropertySensitive : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetPar<T>(ref T obj, T value, [CallerMemberName] string propertyName = null)
        {
            obj = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected T GetPar<T>(T obj)
        {
            return obj;
        }

    }
}
