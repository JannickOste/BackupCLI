<h1>BackupUtil - Simple yet powerful</h1>
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

<h2>Example usages:</h2>

<p>Backups drive C:\ to Drive B:\</p>
<pre>
"ConsoleBackup.exe" --d C:\ --o B:\
</pre>

<p>Backups drive C:\ to Drive B:\ but filters out Distro and .git folders</p>
<pre>
"ConsoleBackup.exe" --d C:\ --o B:\ --f Distro,.git
</pre>

<p>Backups drive C:\ to Drive B:\ but run with the CLI disabled</p>
<pre>
"ConsoleBackup.exe" --d C:\ --o B:\ --s
</pre>

<p>Now lets combine them all, Backups drive C:\ to Drive B:\ Filters out the directories: Distro and .git, limits the backup to the first and last backup of the day and runs silent (without console GUI)</p>
<pre>
"ConsoleBackup.exe" --d C:\ --o B:\ --f Distro,.git --c --s
</pre>
