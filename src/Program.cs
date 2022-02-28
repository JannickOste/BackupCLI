using System;
using src.ConsoleBackup.IO;
using src.ConsoleBackup.Misc;
namespace src.ConsoleBackup
{
    class Program
    {
        public static BackupSettings settings = BackupSettings.GetDefault();

        static void Main(string[] args) 
        {
            if(!ArgumentParser.Parse(args))
            {
                Console.WriteLine($"Something went wrong when attempting to parse the program arguments...\n{(string.Join("\n", ArgumentParser.Errors))}");
                return;
            }

            if(args.Length > 0) StartBackup();
            else Console.WriteLine("Backup GUI not implemented yet.");
        }

        static void StartBackup()
        {
            try
            {
                BackupUtlity.DumpToZip();
            } catch(Exception ex)
            {
                Logger.PrintMessage(ex.ToString());
            }
            finally
            {
                BackupUtlity.DisplayErrors();
            }
        }
    }
}
