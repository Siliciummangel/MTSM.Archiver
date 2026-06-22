/// <summary>
/// Entry point of the MTSM.Archiver command line interface.
///
/// Responsible for:
/// - Creating the root command.
/// - Registering all available CLI commands.
/// - Invoking the command line parser.
///
/// New commands should be registered here.
/// </summary>

using MTSM.Archiver.Cli.Commands;
using System.CommandLine;

var rootCommand = new RootCommand("MTSM.Archiver CLI");

rootCommand.Subcommands.Add(ConfigCommand.Create());
rootCommand.Subcommands.Add(VersionCommand.Create());

return await rootCommand.Parse(args).InvokeAsync();