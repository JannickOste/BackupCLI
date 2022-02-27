using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace src.ConsoleBackup.IO
{
    public sealed class BackupSettingsBuilder
    {
        private BackupSettings settings = new BackupSettings()
        {
            OutputPath = Directory.GetCurrentDirectory(),
            Filters = new string[]{"System Volume Information", "$RECYCLE.BIN"}
        };

        public BackupSettings Settings {
            get{
                if(!(settings.SaveProfile is null) && settings.SaveProfile.Length > 0)
                {
                    DirectoryInfo targetDir = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, "Profiles"));
                    if(!targetDir.Exists) targetDir.Create();
                    FileInfo targetFile = new FileInfo(Path.Combine(targetDir.FullName, $"{settings.SaveProfile}.json"));
                    using(FileStream stream = File.Open(targetFile.FullName, !targetFile.Exists ? FileMode.CreateNew : FileMode.Open))
                    {
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings)));
                    }
                }
                return settings;
            } 
        }

        private List<string> errors = new List<string>();
        public ImmutableList<string> Errors{ get => errors.ToImmutableList();}

        public void SetArgs(string[] args)
        {
            ImmutableDictionary<CLIArgumentAttribute, MethodInfo> parsers = CLIArgumentAttribute.GetAll(this.GetType());
            List<string> sanitzedArguments = Regex.Split(string.Join(" ", args), @"(?!^)(?=--)").ToList();
            foreach(string argument in sanitzedArguments)
            {
                string prefix = (prefix = argument.Substring(2)).IndexOf(" ") == -1 ? prefix : prefix.Substring(0, prefix.IndexOf(" "));
                string[] suffix = argument.IndexOf(" ") == -1 ? new string[0] : argument.Substring(argument.IndexOf(" ")+1).Split(",").Select(i => i.Trim()).Where(i => i.Length > 0).ToArray();
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
                                else if(parameters.First().ParameterType == typeof(uint))
                                    pair.Value.Invoke(this, new object[]{uint.Parse(suffix.First())});

                                break;
                            default: 
                                Console.WriteLine("Failed to parse argument");
                                break;
                        }
                    }
            }
        }

        [CLIArgument(aliases: new string[]{"c", "capped"}, description: "Set limit to 2 and keep first backup and current backup of the day")]
        public void SetOutputPath() => settings.DailyCap = true;


        [CLIArgument(aliases: new string[]{"o", "output"}, description: "Set the directory or filepath of the zip output")]
        public void SetOutputPath(string outputPath) => settings.OutputPath = outputPath;

        [CLIArgument(aliases: new string[]{"f", "filters"}, description: "Filter directory names")]
        public void AddFilters(IEnumerable<string> filters) => settings.Filters = filters.Concat(settings.Filters).ToArray();


        [CLIArgument(aliases: new string[]{"v", "verbose"}, description: "Aggresive/verbose logging")]
        public void SetVerbose() => this.settings.Verbose = true;


        [CLIArgument(aliases: new string[]{"s", "silent"}, description: "Disable UI")]
        public void SetSilent() => Kernel32.ShowWindow(Kernel32.GetConsoleWindow(), Kernel32.SW_HIDE);


        [CLIArgument(aliases: new string[]{"sp", "saveprofile"}, description: "Save current settings to a profile for later use")]
        public void SaveProfile(string name) => settings.SaveProfile = name;


        [CLIArgument(aliases: new string[]{"lp", "loadprofile"}, description: "Load previous used settings of a profile.")]
        public void LoadProfile(string name)
        {
            DirectoryInfo targetDir = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, "Profiles"));
            if(!targetDir.Exists) 
            {
                this.errors.Add("Failed to find profiles directory");
                return;
            }

            FileInfo targetFile = new FileInfo(Path.Combine(targetDir.FullName, $"{name}.json"));
            if(!targetFile.Exists)
            {
                this.errors.Add($"Attempted to load profile \"{name}\" but profile does not exist");
                return;
            }

            this.settings = JsonConvert.DeserializeObject<BackupSettings>(File.ReadAllText(targetFile.FullName));
        }
        
        [CLIArgument(aliases: new string[]{"t", "target"}, description: "Set target directories to backup")]
        public void AddDirectories(IEnumerable<string> directories)
        {
            if(directories.Count() == 0) return;
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
            
            settings.Directories = directorieSet.Select(i => i.FullName).ToArray();
        }

    }
}