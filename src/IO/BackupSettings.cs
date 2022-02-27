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
    }

}