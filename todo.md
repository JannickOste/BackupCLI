# Add Logging implementation
LOG4Net mayby?

# Properly add daily caps. 
Currently checks or 2 files writes where done that day, if so deletes last file, keeps first and makes a new backup, needs possibility to have a higher limit and decent algoritm to select proper files .

# Improve exception handling.
Some sections still need try catch and proper handeling for them (ex: add to error list so everything doesnt crash at x point.)

# Improve argument parsing
currently uses regex implementation but isn't all to good.

# Add CLI GUI
Speaks for itself.


# (Mayby): Add backup profiles
# (Mayby): Add SSH support for external copies.