<h1>BackupUtil - Simple yet powerfull</h1>
<h2>Arguments</h2>
<pre>
[Command(s): --o]             : Set the directory or filepath of the zip output
[Command(s): --d]             : Set target directories to backup
[Command(s): --v]             : Aggresive/verbose logging
[Command(s): --c]             : Set limit to 2 and keep first backup and current backup of the day
[Command(s): --f]             : Filter directory names
[Command(s): --s]             : Disable UI
[Command(s): --h, --help]     : Show command log
</pre>

<h2>Example usage:</h2>
<pre>
"ConsoleBackup.exe" --d C:\ --o B:\ --f Distro,.git --c --s

Backups drive C:\ to Drive B:\ Filters out the directories: Distro and .git, limits the backup to the first and last backup of the day and runs silent (without console GUI)
</pre>
