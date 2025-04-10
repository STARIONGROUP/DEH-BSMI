﻿// -------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Starion Group S.A">
// 
//   Copyright (C) 2019-2025 Starion Group S.A.
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace DEHBSMI.Tools
{
    using System.CommandLine;
    using System.CommandLine.Builder;
    using System.CommandLine.Help;
    using System.CommandLine.Hosting;
    using System.CommandLine.Parsing;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DEHBSMI.Tools.Commands;
    using DEHBSMI.Tools.Generators;
    using DEHBSMI.Tools.Resources;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.Hosting;

    using Spectre.Console;
    using CDP4Dal;
    using STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Main entry point for the command line application
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        /// <summary>
        /// Main entry point for the command line application
        /// </summary>
        /// <param name="args">
        /// command line arguments
        /// </param>
        public static int Main(string[] args)
        {
            var commandLineBuilder = BuildCommandLine()
                .UseHost(_ => Host.CreateDefaultBuilder(args)
                        .ConfigureLogging(loggingBuilder =>
                            loggingBuilder.AddFilter<ConsoleLoggerProvider>(level =>
                                level == LogLevel.None))
                    , builder => builder
                        .ConfigureServices((services) =>
                        {
                            services.AddSingleton<ISession, Session>();
                            services.AddSingleton<IDataSourceSelector, DataSourceSelector>();
                            services.AddSingleton<IIterationReader, IterationReader>();
                            services.AddSingleton<ITraceabilityDotFileGenerator, TraceabilityDotFileGenerator>();
                            services.AddSingleton<IXlReportGenerator, XlReportGenerator>();
                            services.AddSingleton<IHtmlReportGenerator, HtmlReportGenerator>();
                        })
                        .UseCommandHandler<XlReportCommand, XlReportCommand.Handler>()
                        .UseCommandHandler<HtmlReportCommand, HtmlReportCommand.Handler>()
                        .UseCommandHandler<DotFileCommand, DotFileCommand.Handler>()
                    )
                .UseDefaults()

                .Build();

            return commandLineBuilder.Invoke(args);
        }

        /// <summary>
        /// builds the root command
        /// </summary>
        /// <returns>
        /// The <see cref="CommandLineBuilder"/> with the root command set
        /// </returns>
        private static CommandLineBuilder BuildCommandLine()
        {
            var root = CreateCommandChain();

            return new CommandLineBuilder(root)
                .UseHelp(ctx =>
                {
                    ctx.HelpBuilder.CustomizeLayout(_ =>
                        HelpBuilder.Default
                            .GetLayout()
                            .Skip(1) // Skip the default command description section.
                            .Prepend(
                                _ =>
                                {
                                    AnsiConsole.Markup($"[blue]{ResourceLoader.QueryLogo()}[/]");
                                }
                            ));
                });
        }

        /// <summary>
        /// Creates the root and sub commands
        /// </summary>
        /// <returns>
        /// returns an instance of <see cref="RootCommand"/>
        /// </returns>
        private static RootCommand CreateCommandChain()
        {
            var root = new RootCommand("uml4net Tools");

            var reportCommand = new XlReportCommand();
            root.AddCommand(reportCommand);

            var htmlReportCommand = new HtmlReportCommand();
            root.AddCommand(htmlReportCommand);

            var dotFileCommand = new DotFileCommand();
            root.AddCommand(dotFileCommand);

            return root;
        }
    }
}
