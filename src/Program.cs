using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using src.ConsoleBackup.IO;
using src.ConsoleBackup.Misc;
namespace src.ConsoleBackup
{
    class Program
    {
        public static BackupProfile profile = BackupProfile.GetDefault();

        static void Main(string[] args) 
        {
            if(!ArgumentParser.Parse(args))
            {
                Logger.PrintMessage($"Something went wrong when attempting to parse the program arguments...\n{(string.Join("\n", ArgumentParser.Errors))}");
                return;
            }

            if(args.Length > 0) 
            {
                if(profile.DailyCap) 
                StartBackup();
            }
            else Console.WriteLine("Backup GUI not implemented yet.");
        }

        /// <summary> Check or 2 zips have been made today, incase true delete last created zipfile.</summary>
        private static void CheckDailyWrites() 
        {
            List<FileInfo> writesToday = new DirectoryInfo(Program.profile.OutputPath)
                        .GetFiles()
                        .Where(i => i.Name.EndsWith(".zip") && (i.LastWriteTime.DayOfYear == DateTime.Now.DayOfYear 
                                                                && i.LastWriteTime.Year == DateTime.Now.Year))
                        .OrderByDescending(i => i.LastWriteTime).ToList();

            try
            {
                if(writesToday.Count == 2)
                    File.Delete(writesToday.First().FullName);
            } catch(Exception ex)
            {
                Logger.PrintMessage($"Something went wrong when attempting to remove last zip file.{ex}");
            }
        }
        
        static void StartBackup()
        {
            try
            {
                ZipUtility.CreateZipFromProfile(Program.profile);
            } catch(Exception ex)
            {
                Logger.PrintMessage(ex.ToString());
            }
            finally
            {
                ZipUtility.DisplayErrors();
            }
        }
    }
}
