using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace src.ConsoleBackup.IO
{
    public sealed class ZipUtility
    {
        /// <summary> Internal error stack </summary>
        private static List<string> errors = new List<string>();
        /// <summary> Error stack </summary>
        public static ImmutableList<string> Errors{ get => errors.ToImmutableList();}

        public static void CreateZipFromProfile(BackupProfile profile)
        {
            if(profile.Directories is null || profile.Directories.Length == 0)
                errors.Add("No target directories specified");
            
            string dumpPath = File.GetAttributes(profile.OutputPath).HasFlag(FileAttributes.Directory) 
                                ? Path.Combine(profile.OutputPath, $"{DateTime.Now.ToString("dd_MM_yyyy__hh_mm_ss")}.zip") : profile.OutputPath;
            
            if(!dumpPath.ToLower().EndsWith(".zip"))
                errors.Add($"Invalid output path \"{dumpPath}\"");

            if(errors.Count > 0)
            {
                DisplayErrors();
                return;
            }
            
            CreateZipFromPath(dumpPath, profile.Directories);
        }

        public static void CreateZipFromPath(string outputPath, params string[] targetPaths)
        {
            errors.Clear();
            if(!Regex.Match(outputPath, @"((.*?)\.zip)$").Success) throw new ArgumentException("output path must be a zip path");
            if(targetPaths.Length == 0) throw new ArgumentException("No target path(s) specified");

            Logger.PrintMessage($"Writing zip to path: {outputPath}");
            FileMode mode = System.IO.File.Exists(outputPath) ? FileMode.Open : FileMode.CreateNew;
            using(FileStream pathStream = new FileStream(outputPath, mode))
            {
                using(ZipArchive archive = new ZipArchive(pathStream, mode == FileMode.CreateNew ? ZipArchiveMode.Create : ZipArchiveMode.Update))
                {
                    targetPaths.ToList().ForEach(dir =>  {
                        Logger.PrintMessage($"Adding root directory {dir}");
                        AddToArchive(archive, new DirectoryInfo(dir));
                    });
                }
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
                if(Program.profile.Filters.Contains(dirInfo.Name)) return;
                entryName += "\\";
                if(Program.profile.Verbose) Logger.PrintMessage($"Created entry: {archive.CreateEntry(entryName).FullName}");
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
                if(Program.profile.Filters.Contains(info.Name)) return;
                if(Program.profile.Verbose) Logger.PrintMessage($"Adding file \"{info.Name}\" to entry: {prefix}");
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