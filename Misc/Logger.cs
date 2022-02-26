using System;
namespace ConsoleBackup
{
    public class Logger
    {
        public static void PrintMessage(string message) => Console.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss")}]: {message}");
    }
}