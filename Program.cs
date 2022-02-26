using System;
using System.Linq;
using ConsoleBackup.IO;

namespace ConsoleBackup
{
    class Program
    {
        static void Main(string[] args) 
        {
            
            if(args.Length > 0)
            { 
                BackupSettingsBuilder builder = new BackupSettingsBuilder();
                Logger.PrintMessage("Starting backup based on arguments");
                builder.SetArgs(args);
                if(builder.Errors.Count > 0)
                {
                    Logger.PrintMessage("Something went wrong when parsing the program arguments: ");
                    foreach(string line in builder.Errors)
                        Logger.PrintMessage($"\t- {line}");

                    ShowCommands();
                    return;
                }
                
                BackupUtlity iOHandler = new BackupUtlity(builder.Settings);
                try
                {
                    iOHandler.DumpToZip();
                } catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    iOHandler.DisplayErrors();
                }
            } else Logger.PrintMessage("Backup CLI not implemented yet...");
            
        }

        [CLIArgument(aliases: new string[]{"h", "help"}, description: "Show command log")]
        public static void ShowCommands()
        {
            Console.WriteLine("Command info:");
            
            foreach(CLIArgumentAttribute command in CLIArgumentAttribute.GetAll().Keys)
            {
                Console.WriteLine($"{("[Command(s): "+string.Join(", ", command.Aliases.Select(i => ($"--{i}")))+"]"), -30}: {command.Description}");
            }
        }
    }
}
