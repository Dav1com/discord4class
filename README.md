# Discord4Class

A bot for easy online class management, makes Discord, a free to
use plataform, into a decent option for online education

## Features

### Multilangual
Use, eg. `;lang es-es` to change the bot language to Spanish, currently only
supports `es-us` and `es-es`, you are a native speaker and want to help me add
translations?

### Teams making
Teachers can create and manage teams, create then manually giving the
appropiate role your students or create then randomly, every team has a private
voice and text channel, move them from the main class voice channles into their
private channels easily, and viceversa
Use `;help teams` for more information

### Questions
Don't let your students valuable question get lost in the chat, your students
can use the `;q` command to send them directly to the teachers

### LatexMath
Use `;math <equation>`, eg. `;math c_1^2 + c_2^2 = h^2`, and the bot will send
an image with the equation phrased

### Privacy
The bot doesn't save any data of the members of the guild, nor messages

### Request Data Deletion
You don't need the bot anymore?, use `;destroy` and all the information stored
will be deleted

## Usage
I'm already hosting the bot, just create a guild and [invite the bot]
If you want to host the bot yourself, you should check [Build And Run](#Build-And-Run)

## Getting Started
First [invite the bot] to your server, after that just use `;lang <lang-tag>`
to change the bot language, then use `;init` and is all done, remember to give
the 'Teacher' role to the rest of teachers so they can use the commands.

You can use `;help` on any moment to receive the full list of commands
though DM.

### Have any Questions?
[Join the support server]!

## Self Hosting
If you are using Windows or Linux, I provide binaries (you still need to have
the .NET 5.0 Runtime installed), you can download them from [Releases]

Or you can [Build](#building) the project yourself

Once you have the binaries, read [Running](#running)

## Building
### Requirements
1) .NET 5.0 SDK

### Install .NET 5.0 SDK
#### Windows
Go to [.NET Download], under `.NET 5.0` click on
`Download .NET SDK`

#### MacOS
Go to [.NET Download], click on `macOS`, under `.NET 5.0`
click on `Download .NET SDK`

#### GNU/Linux
Follow this guide [Install .NET on Linux](https://docs.microsoft.com/es-es/dotnet/core/install/linux)

Take this in count:
- `{product}` is "dotnet"
- `{type}` is "sdk"
- `{version}` is "5.0"

### Clone the repo
Run the command
```
git clone https://github.com/Dav1com/discord4class.git
cd discord4class
```

### Build
Run these commands
#### For Development
```
dotnet build -c Debug
cd bin/Debug/net5.0
```
#### For Common Usage
If you what the binary to run on machines without .NET 5.0 SDK installed, change
`--self-contained false` to `--self-cotained true`
```
dotnet publish -c Release -o ./bin/output --self-contained false
cd ./bin/output
```

### MongoDB Setup
#### Instalation and Running
Go to [MongoDB Community Edition Installation Tutorials](https://docs.mongodb.com/manual/installation/#mongodb-community-edition-installation-tutorials), and select the apropiate
tutorial for your system

#### Configuration
Follow [Enable Authentication](https://docs.mongodb.com/manual/tutorial/enable-authentication/)
guide, when you reach step 6, do the following:
```
db.createUser(
    {
        user: "discord4class",
        pwd: passwordPrompt(),
        roles: [ { role: "readWrite", db: "Discord4Class"} ]
    }
)
```

## Running
### Configuration
Use a text editor to open the file `config.ini`, there you must edit these values:

- `BotToken` : Here you must put your bot token
- `DbUri` : Here write the db URI, if you follow the guide adobe, then it's
probably `mongodb://discord4class:{PasswordGoesHere}@localhost:27017?retryWrites=true&w=majority`

For more extensive customization, read the messages on the `config.ini` file

### Run
Just do `./Discord4Class`, then read [Getting Started](#getting-started)

[invite the bot]: https://discord.com/oauth2/authorize?client_id=782369699849437194&permissions=289795152&scope=bot
[join the support server]: https://discord.com/invite/qbgbWqTrRe
[.NET Download]: https://dotnet.microsoft.com/download
[Releases]: https://github.com/dav1com/discord4class/releases/
