# Changelog
All notable changes to this project will be documented in this file

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Command `ping` displays the websocket ping
- Command `config <get|set|unset> <config-name> [new-value]` manage the bot settings
- Command `init` for easy and quick server setup
- Command `destroy` for requesting data deletion
- Command `lang [new-language]` for getting and setting the bot language
- Command `prefix [new-prefix]` for getting and setting the bot prefix
- Command `help [command] [subcommand] [...]`
- Command `<q|q-student> <question>` for sending a question to the teachers
- Command `q <next|count>` displays the next question or the number of remaining questions
- Command `teams <make|size|manual|destroy|move|return> [options] [positive-integer]`
- Command `math <latex-equation>` sends an image with the latex equation rendered
- Commands rate limits per guild and per user
- Commands that are heavy with the Discord API cannot run in parallel
- `config.ini` file for the bot global configurations
- Emojis personalization, with custom emojis support
- Per guild bot language, available languages are English and Spanish
- Per guild prefix personalization
- Per guild persistence on MongoDB
