using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace src.ConsoleBackup.IO
{
    public sealed class BackupUtlity
    {
        /// <summary> Internal error stack </summary>
        private static List<string> errors = new List<string>();
        /// <summary> Error stack </summary>
        public static ImmutableList<string> Errors{ get => errors.ToImmutableList();}

        /// <summary> Dump all target directories to output zip based on Program->BackupSettings</summary>
        public static void DumpToZip()
        {
            errors.Clear();
            if(Program.settings.Directories is null || Program.settings.Directories.Length == 0)
                errors.Add("No target directories specified");
            
            string dumpPath = File.GetAttributes(Program.settings.OutputPath).HasFlag(FileAttributes.Directory) 
                                ? Path.Combine(Program.settings.OutputPath, $"{DateTime.Now.ToString("dd_MM_yyyy__hh_mm_ss")}.zip") : Program.settings.OutputPath;
            
            if(!dumpPath.ToLower().EndsWith(".zip"))
                errors.Add($"Invalid output path \"{dumpPath}\"");

            if(errors.Count > 0)
            {
                DisplayErrors();
                return;
            }
            
            if(Program.settings.DailyCap) CheckDailyWrites();
            FileMode mode = System.IO.File.Exists(dumpPath) ? FileMode.Open : FileMode.CreateNew;
            using(FileStream pathStream = new FileStream(dumpPath, mode))
            {
                Logger.PrintMessage("Starting filedump...");
                using(ZipArchive archive = new ZipArchive(pathStream, mode == FileMode.CreateNew ? ZipArchiveMode.Create : ZipArchiveMode.Update))
                {
                    Program.settings.Directories.ToList().ForEach(dir =>  {
                        Logger.PrintMessage($"Adding root directory {dir}");
                        AddToArchive(archive, new DirectoryInfo(dir));
                    });
                }
            }
        }

        /// <summary> Check or 2 zips have been made today, incase true delete last created zipfile.</summary>
        private static void CheckDailyWrites() 
        {
            List<FileInfo> writesToday = new DirectoryInfo(Program.settings.OutputPath)
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
                errors.Add($"Failed to delete last made file {ex.Message}");
            }

        }

        /// <summary> Add a file or directory to a archive </summary>
        /// <param name="archive">The target archive</param>
        /// <param name="info">FileInfo or DirectoryInfo</param>
        /// <param name="prefix">ZipPath</param>
        private static void AddToArchive(ZipArchive archive, FileSystemInfo info, string prefix = "")
        {
            string entryName = (prefix.Length > 0 ? $"{prefix}" : "")+System.Text.RegularExpressions.Regex.Replace(info.Name, $"[{string.Join("", Path.GetInvalidFileNameChars())}]", "").Replace("\\", "");
            
            if(info.GetType() == typeof(DirectoryInfo))
            {
                DirectoryInfo dirInfo = (info as DirectoryInfo);
                if(Program.settings.Filters.Contains(dirInfo.Name)) return;
                entryName += "\\";
                if(Program.settings.Verbose) Logger.PrintMessage($"Created entry: {archive.CreateEntry(entryName).FullName}");
                foreach(FileSystemInfo subDir in (dirInfo.GetDirectories() as FileSystemInfo[]).Concat(dirInfo.GetFiles()))
                {
                    try
                    {
                        AddToArchive(archive, subDir, entryName);
                    }
                    catch(AccessViolationException ex)
                    {
                        errors.Add($"Failed to access directory, ignored exception -> {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }
                }
            } 
            else if(info.GetType() == typeof(FileInfo))
            {
                if(Program.settings.Filters.Contains(info.Name)) return;
                if(Program.settings.Verbose) Logger.PrintMessage($"Adding file \"{info.Name}\" to entry: {prefix}");
                archive.CreateEntryFromFile(info.FullName, entryName);
            }
        }

        /// <summary> Display all errors if an error occured during the zip procedure or during the initialization phase. </summary>
        public static void DisplayErrors()
        {
            if(errors.Count == 0) return;
            Logger.PrintMessage("Something went wrong when initializing backup utility or dumping the zip file: ");
            foreach(string line in Errors)
                Logger.PrintMessage($"\t- {line}");
        }
    }
}