using System;

namespace src.ConsoleBackup.IO
{
    [System.Serializable]
    public struct BackupProfile
    {
        [NonSerialized] public string name;
        [NonSerialized] public bool saveProfile;
        
        public string OutputPath{get; set;}
        public string[] Filters{get; set;}
        public string[] Directories{get; set;}
        
        public bool Verbose{get; set;}
        public bool Silent{get; set;}
        public bool DailyCap{get; set;}

        public string sshProfile{get; set;}


        /// <summary> Fetch the default BackupSettigs object with initialized properties.</summary>
        /// <returns> Default BackupSettings </returns>
        public static BackupProfile GetDefault() => new BackupProfile()
        {
            OutputPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            Filters = new string[]{"System Volume Information", "$RECYCLE.BIN", Logger.LogName},
            Directories = new string[0]
        };
    }

}