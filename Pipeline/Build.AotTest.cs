using System.Runtime.InteropServices;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Solutions;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable UnusedMember.Local
// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Project AotSmokeTestProject => Solution.Tests.aweXpect_Mockolate_Aot;

	/// <summary>
	///     Runs the smoke tests from the compiled output and again published with Native AOT for the current platform.
	/// </summary>
	/// <remarks>
	///     The trim analyzer checks annotations, not behaviour, so this is the only place that verifies that an
	///     expectation returns the same result in a trimmed application as under the JIT.
	/// </remarks>
	Target AotSmokeTests => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			DotNetRun(s => s
				.SetProjectFile(AotSmokeTestProject)
				.SetConfiguration(Configuration)
				.EnableNoBuild());

			AbsolutePath output = ArtifactsDirectory / "Aot" / AotSmokeTestProject.Name;
			// Outside the solution build, packing the referenced library would miss the README and leave a package
			// for `Pack` to pick up.
			DotNetPublish(s => s
				.SetProject(AotSmokeTestProject)
				.SetConfiguration(Configuration)
				.SetRuntime(RuntimeInformation.RuntimeIdentifier)
				.SetProperty("GeneratePackageOnBuild", false)
				.SetOutput(output));

			string executable = output / (AotSmokeTestProject.Name + (EnvironmentInfo.IsWin ? ".exe" : ""));
			ProcessTasks.StartProcess(executable, string.Empty, output)
				.AssertZeroExitCode();
		});
}
