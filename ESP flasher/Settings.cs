using STDLib.Saveable;

namespace ESP_flasher
{
    public class Settings : BaseSettings<Settings>
    {
        public static bool DeveloperMode { get { return GetPar<bool>(false); } set { SetPar<bool>(value); } }
        public static bool AutoReleaseOnStart { get { return GetPar<bool>(false); } set { SetPar<bool>(value); } }
        public static string PathToFlashProjectArgs { get { return GetPar<string>(@"..\build\flash_project_args"); } set { SetPar<string>(value); } }
        public static string DefaultReleaseFolder { get { return GetPar<string>(@"..\release"); } set { SetPar<string>(value); } }
        public static string PathToVersionC { get { return GetPar<string>(@"..\main\source\version.c"); } set { SetPar<string>(value); } }
        public static string VersionRegex { get { return GetPar<string>(@"\{.*?(\d+),.+?(\d+),.+?(\d+)"); } set { SetPar<string>(value); } }
        public static string HexFileName { get { return GetPar<string>("{version} 19 KC KC1245-gateway.hex"); } set { SetPar<string>(value); } }
        public static string KCZipFileName { get { return GetPar<string>("{version} 19 KC KC1245-gateway.kczip"); } set { SetPar<string>(value); } }
    }

}


