using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ConsoleBackup.IO
{
    public sealed class BackupUtlity
    {
        private List<string> errors = new List<string>();
        public ImmutableList<string> Errors{ get => errors.ToImmutableList();}

        private BackupSettings Settings{get; set;}
        public BackupUtlity(BackupSettings settings) => this.Settings = settings;

        public static void DumpToZip(BackupSettings settings) => new BackupUtlity(settings).DumpToZip();

        public void DumpToZip()
        {
            if(Settings.Directories.Length == 0)
                throw new ArgumentException("No directories found to dump");

            string dumpPath = File.GetAttributes(Settings.OutputPath).HasFlag(FileAttributes.Directory) 
                                ? Path.Combine(Settings.OutputPath, $"{DateTime.Now.ToString("dd_MM_yyyy__hh_mm_ss")}.zip") : Settings.OutputPath;
            
            if(!dumpPath.ToLower().EndsWith(".zip"))
            {
                this.errors.Add($"Invalid output path \"{dumpPath}\"");
                return;
            }
            FileMode mode = System.IO.File.Exists(dumpPath) ? FileMode.Open : FileMode.CreateNew;
            using(FileStream pathStream = new FileStream(dumpPath, mode))
            {
                Logger.PrintMessage("Starting filedump...");
                using(ZipArchive archive = new ZipArchive(pathStream, mode == FileMode.CreateNew ? ZipArchiveMode.Create : ZipArchiveMode.Update))
                {
                    Settings.Directories.ToList().ForEach(dir =>  {
                        Logger.PrintMessage($"Adding root directory {dir.Name}");
                        AddToArchive(archive, dir);
                    });
                }
            }
        }

        private void AddToArchive(ZipArchive archive, FileSystemInfo info, string prefix = "")
        {
            string entryName = (prefix.Length > 0 ? $"{prefix}" : "")+System.Text.RegularExpressions.Regex.Replace(info.Name, $"[{string.Join("", Path.GetInvalidFileNameChars())}]", "").Replace("\\", "");
            
            if(info.GetType() == typeof(DirectoryInfo))
            {
                DirectoryInfo dirInfo = (info as DirectoryInfo);
                if(Settings.Filters.Contains(dirInfo.Name)) return;
                entryName += "\\";
                if(Settings.Verbose) Logger.PrintMessage($"Created entry: {archive.CreateEntry(entryName).FullName}");
                foreach(FileSystemInfo subDir in (dirInfo.GetDirectories() as FileSystemInfo[]).Concat(dirInfo.GetFiles()))
                {
                    try
                    {
                        AddToArchive(archive, subDir, entryName);
                    }
                    catch(AccessViolationException ex)
                    {
                        this.errors.Add($"Failed to access directory, ignored exception -> {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        this.errors.Add(ex.Message);
                    }
                }
            } 
            else if(info.GetType() == typeof(FileInfo))
            {
                if(Settings.Verbose) Logger.PrintMessage($"Adding file \"{info.Name}\" to entry: {prefix}");
                archive.CreateEntryFromFile(info.FullName, entryName);
            }
        }


        public void DisplayErrors()
        {
            if(errors.Count == 0) return;
            Logger.PrintMessage("Something went wrong when dumping the zipfile: ");
            foreach(string line in this.Errors)
                Logger.PrintMessage($"\t- {line}");
        }
    }
}