using System.Linq;
using Fallout.Common;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Target UnitTests => _ => _
		.DependsOn(DotNetUnitTests);

	Project[] UnitTestProjects =>
	[
		Solution.Tests.aweXpect_Mockolate_Tests,
	];

	Target DotNetUnitTests => _ => _
		.Unlisted()
		.DependsOn(Compile)
		.Executes(() =>
		{
			string[] excludedFrameworks =
				EnvironmentInfo.IsWin
					? []
					: ["net48",];
			// Each run is started with `dotnet run`, so that its TRX and coverage file get a name of their own.
			DotNetRun(s => s
					.SetConfiguration(Configuration)
					.SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
					.EnableNoBuild()
					.CombineWith(
						UnitTestProjects,
						(settings, project) => settings
							.SetProjectFile(project)
							.CombineWith(
								project.GetTargetFrameworks()?.Except(excludedFrameworks),
								(frameworkSettings, framework) => frameworkSettings
									.SetFramework(framework)
									.SetProcessAdditionalArguments(
										"--",
										"--disable-logo",
										$"--results-directory \"{TestResultsDirectory}\"",
										"--report-trx",
										$"--report-trx-filename {project.Name}_{framework}.trx",
										"--coverage",
										"--coverage-output-format cobertura",
										$"--coverage-output {project.Name}_{framework}.cobertura.xml")
							)
					), completeOnFailure: true
			);
		});
}
