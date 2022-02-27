<h1>BackupUtil - Simple yet powerful</h1>
<h2>Arguments</h2>
<pre>
[Command(s): --s, --silent]   : Disable UI
[Command(s): --v, --verbose]  : Aggresive/verbose logging
[Command(s): --h, --help]     : Show command log
[Command(s): --c, --capped]   : Set limit to 2 and keep first backup and current backup of the day
[Command(s): --t, --target]   : Set target directories to backup
[Command(s): --f, --filters]  : Filter directory names
[Command(s): --o, --output]   : Set the directory or filepath of the zip output
</pre>

<h2>Example usages:</h2>

<p>Backups drive C:\ to Drive B:\</p>
<pre>
"ConsoleBackup.exe" --t C:\ --o B:\
</pre>

<p>Backups drive C:\ to Drive B:\ but filters out Distro and .git folders</p>
<pre>
"ConsoleBackup.exe" --t C:\ --o B:\ --f Distro,.git
</pre>

<p>Backups drive C:\ to Drive B:\ but run with the CLI disabled</p>
<pre>
"ConsoleBackup.exe" --t C:\ --o B:\ --s
</pre>

<p>Now lets combine them all, Backups drive C:\ to Drive B:\ Filters out the directories: Distro and .git, limits the backup to the first and last backup of the day and runs silent (without console GUI)</p>
<pre>
"ConsoleBackup.exe" --t C:\ --o B:\ --f Distro,.git --c --s
</pre>
