using System;

namespace src.ConsoleBackup.IO
{
    [System.Serializable]
    public struct BackupSettings
    {
        public string OutputPath{get; set;}
        
        public string[] Filters{get; set;}
        public string[] Directories{get; set;}
        
        public bool Verbose{get; set;}
        public bool Silent{get; set;}

        public bool DailyCap{get; set;}

        [NonSerialized] public string SaveProfile;

        /// <summary> Fetch the default BackupSettigs object with initialized properties.</summary>
        /// <returns> Default BackupSettings </returns>
        public static BackupSettings GetDefault() => new BackupSettings()
        {
            OutputPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            Filters = new string[]{"System Volume Information", "$RECYCLE.BIN", Logger.LogName},
            Directories = new string[0]
        };
    }

}