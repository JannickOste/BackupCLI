﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using src.ConsoleBackup.IO;
namespace src.ConsoleBackup
{
    class Program
    {
        static void Main(string[] args) 
        {
            
            if(args.Length > 0)
            { 
                if(args.Contains("--h") || args.Contains("--help"))
                {
                    ShowCommands();
                    return;
                }

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
                if(iOHandler.Errors.Count() > 0)
                {
                    iOHandler.DisplayErrors();
                    return;
                }
                else
                {
                    if(builder.Settings.Silent)
                        Kernel32.ShowWindow(Kernel32.GetConsoleWindow(), Kernel32.SW_HIDE);
                    
                    try
                    {
                        iOHandler.DumpToZip();
                    } catch(Exception ex)
                    {
                        Logger.PrintMessage(ex.ToString());
                    }
                    finally
                    {
                        iOHandler.DisplayErrors();
                    }
                }
            } else Logger.PrintMessage("Backup CLI UI not implemented yet...");
        }

        [CLIArgument(aliases: new string[]{"h", "help"}, description: "Show command log")]
        public static void ShowCommands()
        {
            Console.WriteLine("Command info:");
            foreach(CLIArgumentAttribute command in CLIArgumentAttribute.GetAll().Keys)
            {
                Console.WriteLine($"{string.Join(", ", command.Aliases.Select(i => ($"--{i}"))), -30}: {command.Description}");
            }
        }
    }
}
