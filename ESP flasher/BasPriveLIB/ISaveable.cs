using System.IO;

namespace BasPriveLib
{
    public interface ISaveable
    {
        void Save(Stream stream);
        void Load(Stream stream);

    }
}

