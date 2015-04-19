#New-Hgrc

##Instant .hgrc
New-Hgrc is a handy PowerShell mod for drumming up a new basic hgrc file.

##Installation
It's as easy as 1[-2]-3!

> You can skip to 3 by installing with [PsGet](https://github.com/psget/psget)

###1.
Drop the binary into your Modules dir. 

+ For local user account, `~\Documents\WindowsPowerShell\Modules`
+ For machine, `C:\Windows\System32\WindowsPowerShell\v1.0\Modules`

###2.
Add the following to your PowerShell profile `~\Documents\WindowsPowerShell\Microsoft.PowerShell_profile.ps1`:

````
Import-Module New-Hgrc
````

###3.
Add the following to your `%HOMEPATH%\mercurial.ini`:

````
[hooks]
post-init = powershell New-Hgrc
````

Life is easy if keyring is enabled -- check `mercurial.ini`:

````
[extensions]
mercurial_keyring=
````
##Details
+ With the `post-init` hook, New-Hgrc automatically fires every time you `hg init` a new repo.
+ You can also call this in the root of any initted Hg repo and it will build (overwriting any existing) a new hgrc.
+ Looks first to your `mercurial.ini` file for your username. You can also choose to use a different one.
+ Supports both `[auth]` section for pickup by keyring and `[ui]` for display in clients like TortoiseHg.
+ Thanks to Ricardo Amores Hernández for his [ini-parser](https://github.com/rickyah/ini-parser)
