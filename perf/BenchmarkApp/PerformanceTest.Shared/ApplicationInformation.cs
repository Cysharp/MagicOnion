using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace PerformanceTest.Shared;

public class ApplicationInformation
{
    public static ApplicationInformation Current { get; } = new ApplicationInformation();

#if SERVER
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.Server.MagicOnionEngine).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = typeof(Grpc.AspNetCore.Server.GrpcServiceOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
#elif CLIENT
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.Client.MagicOnionClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = typeof(Grpc.Net.Client.GrpcChannel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
#else
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.UnaryResult).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = default;
#endif
    public string? MessagePackVersion { get; } = typeof(MessagePack.MessagePackSerializer).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? MemoryPackVersion { get; } = typeof(MemoryPack.MemoryPackSerializer).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    public bool IsReleaseBuild { get; }
#if RELEASE
        = true;
#else
        = false;
#endif

    public string FrameworkDescription { get; } = RuntimeInformation.FrameworkDescription;
    public string OSDescription { get; } = RuntimeInformation.OSDescription;
    public Architecture OSArchitecture { get; } = RuntimeInformation.OSArchitecture;
    public Architecture ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture;
    public string CpuModelName { get; } = CpuInformation.Current.ModelName;
    public bool IsServerGC { get; } = GCSettings.IsServerGC;
    public int ProcessorCount { get; } = Environment.ProcessorCount;
    public bool IsAttached { get; } = Debugger.IsAttached;

    public class CpuInformation
    {
        public static CpuInformation Current { get; } = new CpuInformation();
        public string ModelName { get; private set; } = "";

        private CpuInformation()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64 && System.Runtime.Intrinsics.X86.X86Base.IsSupported)
            {
                // Linux x86_64 & Windows x86_64...
                ModelName = GetX86CpuModelName();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux Arm64 will be here...
                ModelName = GetLinuxModelName();
            }
            else
            {
                // Windows Arm64 is not supported...
                ModelName = "Unsupported OS";
            }
        }

        private string GetX86CpuModelName()
        {
            var regs = new int[4 * 3]; // 0x80000002, 0x80000003, 0x80000004 with 4 registers

            // Calling __cpuid with 0x80000000 as the InfoType argument and gets the number of valid extended IDs.
            var extendedId = System.Runtime.Intrinsics.X86.X86Base.CpuId(unchecked((int)0x80000000), 0).Eax;

            // Get the information associated with each extended ID.
            if ((uint)extendedId >= 0x80000004)
            {
                int p = 0;
                for (uint i = 0x80000002; i <= 0x80000004; ++i)
                {
                    var (Eax, Ebx, Ecx, Edx) = System.Runtime.Intrinsics.X86.X86Base.CpuId((int)i, 0);
                    regs[p + 0] = Eax;
                    regs[p + 1] = Ebx;
                    regs[p + 2] = Ecx;
                    regs[p + 3] = Edx;
                    p += 4; // advance
                }
                return ConvertToString(regs);
            }

            return $"Unknown CPU Architecture (extendedId: {extendedId})";

            static string ConvertToString(int[] regs)
            {
                var sb = new System.Text.StringBuilder();
                foreach (int reg in regs)
                {
                    var bytes = BitConverter.GetBytes(reg);
                    sb.Append(System.Text.Encoding.ASCII.GetString(bytes));
                }
                return sb.ToString().Trim();
            }
        }

        private string GetLinuxModelName()
        {
            var cpuInfo = File.ReadAllText("/proc/cpuinfo");
            var lines = cpuInfo.Split('\n');
            foreach (var line in lines)
            {
                if (!line.StartsWith("model name"))
                {
                    continue;
                }
                var parts = line.Split(':');
                if (parts.Length > 1)
                {
                    var modelName = parts[1].Trim();
                    return modelName;
                }
            }
            return "Unknown";
        }
    }
}
