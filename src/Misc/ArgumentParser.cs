using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using src.ConsoleBackup.IO;

namespace src.ConsoleBackup.Misc
{
    public class ArgumentParser
    {
        private static Action exitEvent;
        private static List<string> errors = new List<string>();
        public static ImmutableList<string> Errors{get => errors.ToImmutableList<string>(); }

        /// <summary> ArgumentOptionData </summary>
        private class ArgumentOptionAttribute:Attribute 
        {
            public string[] Aliases{get; set;}
            public string Description{get; set;}
            public bool Required{get; set;}

            public override bool Equals(object obj) => !(Aliases is null) && Aliases.Contains(obj.ToString());

            public override int GetHashCode() => base.GetHashCode();
        }

        /// <summary> Program argument options events container </summary>
        private class ArgumentOptions
        {
            [ArgumentOption(Aliases = new[]{ "h", "help"}, Description = "Show a list of all argument options")]
            public static void ShowArgumentOptions() 
                => Console.WriteLine("Argument options:\n" + string.Join("\n", GetAllOptions()
                                                                              .Select(i => $"{string.Join(" ", i.Key.Aliases.Select(j => ($"--{j}"))), -30}{i.Key.Description}")));
                                                                                                                                       

            [ArgumentOption(Aliases = new[]{ "s", "silent"}, Description = "Disable the CLI visiblity")]
            public static void SetSilent()  => Program.settings.Silent = true;

            [ArgumentOption(Aliases = new string[]{"f", "filters"}, Description ="Filter directory names")]
            public static void AddFilters(IEnumerable<string> filters) => Program.settings.Filters = filters.Concat(Program.settings.Filters).ToArray();

            [ArgumentOption(Aliases = new string[]{"c", "capped"}, Description =  "Set limit to 2 and keep first backup and current backup of the day")]
            public static void SetDailyCap() => Program.settings.DailyCap = true;
                                                                              
            [ArgumentOption(Aliases = new[]{ "v", "verbose"}, Description = "Verbose/aggresive logging")]
            public static void SetVerbose()  => Program.settings.Verbose = true;


            [ArgumentOption(Aliases = new[]{ "o", "output"}, Description = "Verbose/aggresive logging")]
            public static void SetOutputPath(params string[] path)
            {
                if(path.Length == 1)
                {
                    FileAttributes fileAttr = File.GetAttributes(path.First());

                    Program.settings.OutputPath = path.First();
                }
            } 
                            
            [ArgumentOption(Aliases = new string[]{"sp", "saveprofile"}, Description = "Save current settings to a profile for later use")]
            public static void SaveProfile(params string[] name)
            {
                if(name.Length == 1)
                {
                    Program.settings.SaveProfile = name[0];
                    exitEvent = () => {
                        DirectoryInfo targetDir = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, "Profiles"));
                        if(!targetDir.Exists) targetDir.Create();
                        FileInfo targetFile = new FileInfo(Path.Combine(targetDir.FullName, $"{Program.settings.SaveProfile}.json"));
                        using(FileStream stream = File.Open(targetFile.FullName, !targetFile.Exists ? FileMode.CreateNew : FileMode.Open))
                            stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Program.settings)));
                    };
                }
            }


            [ArgumentOption(Aliases = new string[]{"lp", "loadprofile"}, Description = "Load previous used settings of a profile.")]
            public static void LoadProfile(params string[] name)
            {
                if(name.Length == 1)
                {
                    DirectoryInfo targetDir = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, "Profiles"));
                    if(!targetDir.Exists) 
                        throw new DirectoryNotFoundException("Failed to find profiles directory");

                    FileInfo targetFile = new FileInfo(Path.Combine(targetDir.FullName, $"{name[0]}.json"));
                    if(!targetFile.Exists)
                        throw new FileNotFoundException($"Attempted to load profile \"{name[0]}\" but profile does not exist");
                        
                    Program.settings = JsonConvert.DeserializeObject<BackupSettings>(File.ReadAllText(targetFile.FullName));
                }
            }
            
            [ArgumentOption(Aliases = new string[]{"t", "target"}, Description = "Set target directories to backup")]
            public static void AddDirectories(params string[] directories)
            {
                if(directories.Count() == 0) return;
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
                
                Program.settings.Directories = directorieSet.Select(i => i.FullName).ToArray();
            }


            public static ImmutableDictionary<ArgumentOptionAttribute, MethodInfo> GetAllOptions() => typeof(ArgumentOptions).GetMethods()
                                                                         .Where(i => !(i.GetCustomAttribute<ArgumentOptionAttribute>(false) is null))
                                                                         .Select(i => (i.GetCustomAttribute<ArgumentOptionAttribute>(false), i))
                                                                         .ToImmutableDictionary(
                                                                             k => k.Item1,
                                                                             v => v.i
                                                                         );
        }
        
        /// <summary> Parse program arguments </summary>
        /// <param name="args">program arguments </param>
        /// <returns>Succesfully parsed?</returns>
        public static bool Parse(params string[] args)
        {
            errors.Clear();
            
            string argPrefix = "--";
            string argString = string.Join(" ", args);

            // Normalize arguments.
            args = Regex.Split(argString, $"(?!^)(?={argPrefix})").Select(i => i.StartsWith(argPrefix) ? i.Substring(argPrefix.Length) : i).ToArray();
            ImmutableDictionary<ArgumentOptionAttribute, MethodInfo> options = ArgumentOptions.GetAllOptions();
            foreach(string arg in args)
            {
                string prefix = arg.IndexOf(" ") < 0 ? arg : arg.Substring(0, arg.IndexOf(" "));
                string[] suffix = arg.IndexOf(" ") < 0 ? new string[0] :
                                                        Regex.Matches(arg.Substring(arg.IndexOf(" ")+1), "(((?=^)|(?<=\")|(?<=,?\\s))(.*?)((?=$)|(?=\")|(?=,)))")
                                                        .Where(i => i.Length > 0)
                                                        .Select(i => i.Value.Trim()).ToArray();
                
                KeyValuePair<ArgumentOptionAttribute, MethodInfo> match = options.Where(i => i.Key.Equals(prefix)).FirstOrDefault();
                if(!(match.Key is null) && !(match.Value is null))
                {
                    ParameterInfo[] parameters = match.Value.GetParameters();
                    try
                    {
                        switch(parameters.Length)
                        {   
                            case 0: match.Value.Invoke(null, null); break;
                            case 1: match.Value.Invoke(null, new object[]{Convert.ChangeType(suffix, parameters.First().ParameterType)}); break;
                        }
                    } catch(Exception ex)
                    {
                        // Is an invokation so need to fetch the exception inside of the exception as the root exception is the invokation exception.
                        if(!(ex.InnerException is null))
                            errors.Add(ex.InnerException.Message);
                        else errors.Add(ex.Message);
                    }
                } else errors.Add($"Argument option {prefix} was not found, refere to help section for a list of all commands(--help)");
            }

            if(!(exitEvent is null)) exitEvent.Invoke();

            return Errors.Count == 0;
        }
    }
}