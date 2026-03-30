using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Najlot.Map.Benchmarks;

public sealed class BenchmarksConfig : ManualConfig
{
	public BenchmarksConfig()
	{
		AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance));
	}
}