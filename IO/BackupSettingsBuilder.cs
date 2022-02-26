using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ConsoleBackup.IO
{
    public sealed class BackupSettingsBuilder
    {
        private BackupSettings settings = new BackupSettings()
        {
            OutputPath = Directory.GetCurrentDirectory(),
            Filters = new string[]{"System Volume Information"}
        };

        public BackupSettings Settings {get => settings; }

        private List<string> errors = new List<string>();
        public ImmutableList<string> Errors{ get => errors.ToImmutableList();}

        public static BackupSettings BuildFromProgramArgs(string[] args)
        {
            BackupSettingsBuilder builder = new BackupSettingsBuilder();
            builder.SetArgs(args);
            return builder.Settings;
        }

        public void SetArgs(string[] args)
        {
            ImmutableDictionary<CLIArgumentAttribute, MethodInfo> parsers = CLIArgumentAttribute.GetAll(this.GetType());
            List<string> sanitzedArguments = Regex.Split(string.Join(" ", args), @"(?!^)(?=--)").ToList();
            foreach(string argument in sanitzedArguments)
            {
                string prefix = (prefix = argument.Substring(2)).IndexOf(" ") == -1 ? prefix : prefix.Substring(0, prefix.IndexOf(" "));
                string[] suffix = argument.IndexOf(" ") == -1 ? new string[0] : argument.Substring(argument.IndexOf(" ")+1).Split(", ").Select(i => i.Trim()).ToArray();
                bool set = false;
                foreach(KeyValuePair<CLIArgumentAttribute, MethodInfo> pair in parsers)
                    if((set = pair.Key.Equals(prefix)))
                    {
                        ParameterInfo[] parameters = pair.Value.GetParameters();
                        switch(parameters.Length)
                        {
                            case 0: 
                                pair.Value.Invoke(this, null);
                                break;
                            case 1:
                                if(parameters.First().ParameterType == typeof(string)) 
                                    pair.Value.Invoke(this, new object[]{suffix.FirstOrDefault()});
                                else if(parameters.First().ParameterType == typeof(IEnumerable<string>))
                                    pair.Value.Invoke(this, new object[]{suffix.AsEnumerable<string>()});
                                break;
                            default: 
                                Console.WriteLine("Failed to parse argument");
                                break;
                        }
                    }
            }
        }

        [CLIArgument(aliases: new string[]{"o"}, description: "Set the directory or filepath of the zip output")]
        public void SetOutputPath(string outputPath) => settings.OutputPath = outputPath;

        [CLIArgument(aliases: new string[]{"d"}, description: "Set target directories to backup")]
        public void AddDirectories(IEnumerable<string> directories)
        {
            Console.WriteLine("Adding directory");
            List<DirectoryInfo> directorieSet = new List<DirectoryInfo>();
            foreach(string directory in directories)
                try
                {
                    if(!File.GetAttributes(directory).HasFlag(FileAttributes.Directory))
                    {
                        errors.Add($"AddDirectory -> Path is not a directory path: {directory}");
                        continue;
                    }

                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    if(!directoryInfo.Exists)
                    {
                        errors.Add($"AddDirectory -> Directory \"{directory}\" doesnt exist");
                    } else directorieSet.Add(directoryInfo);
                } 
                catch (Exception ex)
                {
                    errors.Add($"AddDirectory -> {ex.Message}");
                }
            
            settings.Directories = directorieSet.ToArray();
        }

        [CLIArgument(aliases: new string[]{"f"}, description: "Filter directory names")]
        public void AddFilters(IEnumerable<string> filters) => settings.Filters = filters.Concat(settings.Filters).ToArray();

        [CLIArgument(aliases: new string[]{"v"}, description: "Aggresive/verbose logging")]
        public void SetVerbose() => this.settings.Verbose = true;

        [CLIArgument(aliases: new string[]{"s"}, description: "Disable UI")]
        public void SetSilent() => Kernel32.ShowWindow(Kernel32.GetConsoleWindow(), Kernel32.SW_HIDE);

    }
}