namespace ConsoleBackup.IO
{
    public struct BackupSettings
    {
        public string OutputPath{get; set;}
        
        public string[] Filters{get; set;}
        public System.IO.DirectoryInfo[] Directories{get; set;}
        
        public bool Verbose{get; set;}
        public bool Silent{get; set;}

        public bool DailyCap{get; set;}
    }

}