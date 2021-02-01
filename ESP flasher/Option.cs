using System;



namespace CmdArgParser
{
    public class Option : Attribute
    {
        public char ShortOption { get; set; }
        public string LongOption { get; set; }
        public string HelpText { get; set; }

        public Option(char c, string s, string help = "")
        {
            ShortOption = c;
            LongOption = s;
            HelpText = help;
        }
    }

}

