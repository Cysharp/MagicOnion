using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace PerformanceTest.Shared;

public class ApplicationInformation
{
    public static ApplicationInformation Current { get; } = new ApplicationInformation();

    public bool IsLatestMagicOnion { get; } = typeof(ApplicationInformation).Assembly.GetCustomAttribute<MagicOnionIsLatestAttirbute>()?.IsLatest ?? true; // set from csproj

    public string? BenchmarkerVersion { get; } = typeof(ApplicationInformation).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

#if SERVER
    public string? TagMagicOnionVersion { get; } = RemoveHashFromVersion(typeof(MagicOnion.Server.ServiceContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.Server.ServiceContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = typeof(Grpc.AspNetCore.Server.GrpcServiceOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
#elif CLIENT
    public string? TagMagicOnionVersion { get; } = RemoveHashFromVersion(typeof(MagicOnion.Client.MagicOnionClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
    public string? MagicOnionVersion { get; } = typeof(MagicOnion.Client.MagicOnionClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public string? GrpcNetVersion { get; } = typeof(Grpc.Net.Client.GrpcChannel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
#else
    public string? TagMagicOnionVersion { get; } = RemoveHashFromVersion(typeof(MagicOnion.UnaryResult).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
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
    public string CpuModelName { get; } = CpuModel.Current.ModelName;
    public bool IsServerGC { get; } = GCSettings.IsServerGC;
    public int ProcessorCount { get; } = Environment.ProcessorCount;
    public bool IsAttached { get; } = Debugger.IsAttached;

    /// <summary>
    /// Reduct `+HASH` suffix from InformationalVersion.
    /// <br/> Example1: 1.0.0+1234567890abcdefg -> 1.0.0
    /// <br/> Example2: 1.0.0 -> 1.0.0
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    private static string? RemoveHashFromVersion(string? version)
    {
        if (string.IsNullOrEmpty(version)) return version;
        var span = version.AsSpan();
        var position = span.IndexOf('+');
        return position == -1 ? version : span[..position].ToString();
    }

    public class CpuModel
    {
        const string unknownPhrase = "Unknown";

        public static CpuModel Current { get; } = new CpuModel();
        public string ModelName { get; } = "";
        public string UnknownReason { get; } = "";

        private CpuModel()
        {
            if (System.Runtime.Intrinsics.X86.X86Base.IsSupported)
            {
                // x86_64 OS (Linux, Windows, macOS) ...
                (ModelName, UnknownReason) = GetX86CpuModelName();
            }
            else
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows Arm64 will be here...
                    (ModelName, UnknownReason) = GetWindowsModelName();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux Arm64 will be here...
                    (ModelName, UnknownReason) = GetLinuxModelName();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS will be here...
                    (ModelName, UnknownReason) = GetOSXModelname();
                }
                else
                {
                    // Unsupported platform (not Windows, Linux, or macOS)
                    (ModelName, UnknownReason) = (unknownPhrase, $"Platform not supported for... OS: {RuntimeInformation.OSDescription}, Architecture: {RuntimeInformation.OSArchitecture}");
                }
            }
        }

        private static (string modelName, string unknownReason) GetX86CpuModelName()
        {
            Span<int> regs = stackalloc int[12]; // call 3 times (0x80000002, 0x80000003, 0x80000004) for 4 registers

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
                return (ConvertToString(regs), "");
            }

            return (unknownPhrase, $"CPU Architecture not supported... extendedId: {extendedId}");

            static string ConvertToString(ReadOnlySpan<int> regs)
            {
                Span<byte> bytes = stackalloc byte[regs.Length * 4]; // int 4byte * 12
                for (int i = 0; i < regs.Length; i++)
                {
                    BitConverter.TryWriteBytes(bytes.Slice(i * 4, 4), regs[i]);
                }
                return System.Text.Encoding.ASCII.GetString(bytes).Trim();
            }
        }

        private static (string modelName, string unknownReason) GetWindowsModelName()
        {
            const string registryKey = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
            const string valueName = "ProcessorNameString";

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Not Windows OS.");

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                var value = key.GetValue(valueName);
                if (value is string model)
                {
                    return (model, "");
                }
            }
            return (unknownPhrase, "Windows Registry Key not found.");
        }

        private static (string modelName, string unknownReason) GetLinuxModelName()
        {
            try
            {
                var cpuInfo = File.ReadAllText("/proc/cpuinfo");
                var lines = cpuInfo.Split('\n');
                string vendorId = "";
                string cpuPart = "";
                string modelName = "";

                foreach (var line in lines)
                {
                    // x86/amd64 (sometimes arm64 contain this)
                    if (line.StartsWith("model name"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1)
                        {
                            modelName = parts[1].Trim();
                            return (modelName, "");
                        }
                    }
                    // ARM64
                    else if (line.StartsWith("CPU implementer"))
                    {
                        var parts = line.Split(":");
                        if (parts.Length > 1)
                        {
                            vendorId = parts[1].Trim().ToLower();
                        }
                    }
                    else if (line.StartsWith("CPU part"))
                    {
                        var parts = line.Split(":");
                        if (parts.Length > 1)
                        {
                            cpuPart = parts[1].Trim().ToLower();
                        }
                    }
                }

                // ARM64 model name resolution (0x41 is ARM Ltd.)
                if (modelName == "" && vendorId != "")
                {
                    if (cpuPart == "")
                        return (unknownPhrase, "CPU part not found for ARM CPU.");

                    var armModelName = GetArmCpuName(vendorId, cpuPart);
                    return armModelName != "Undefined"
                        ? (armModelName, "")
                        : (armModelName, $"CPU part '{cpuPart}' (vendor '{vendorId}' ({GetArmImplementerName(vendorId)})) is not mapped.");
                }

                return (unknownPhrase, "'model name' section not found.");
            }
            catch (Exception ex)
            {
                return (unknownPhrase, $"Exception occurred: {ex.Message}");
            }
        }

        private static (string modelName, string unknownReason) GetOSXModelname()
        {
            try
            {
                nint size = 0;

                // First call to get the size
                int result = sysctlbyname("machdep.cpu.brand_string", IntPtr.Zero, ref size, IntPtr.Zero, 0);
                if (result != 0)
                {
                    return (unknownPhrase, $"sysctlbyname failed to get size. Return code: {result}, errno: {Marshal.GetLastPInvokeError()}");
                }

                if (size == 0)
                {
                    return (unknownPhrase, "sysctlbyname returned size 0");
                }

                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                try
                {
                    // Second call to get the actual value
                    result = sysctlbyname("machdep.cpu.brand_string", buffer, ref size, IntPtr.Zero, 0);
                    if (result != 0)
                    {
                        return (unknownPhrase, $"sysctlbyname failed to get value. Return code: {result}, errno: {Marshal.GetLastPInvokeError()}");
                    }

                    var cpuBrand = Marshal.PtrToStringAnsi(buffer);
                    if (string.IsNullOrEmpty(cpuBrand))
                    {
                        return (unknownPhrase, "machdep.cpu.brand_string returned empty string.");
                    }

                    return (cpuBrand.Trim(), "");
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            catch (Exception ex)
            {
                return (unknownPhrase, $"Exception occurred: {ex.Message}");
            }
        }

        [DllImport("libSystem.dylib")]
        private static extern int sysctlbyname(string name, IntPtr oldp, ref nint oldlenp, IntPtr newp, nint newlen);

        // ARM CPU detection helpers
        // see: https://github.com/util-linux/util-linux/blob/v2.41.2/sys-utils/lscpu-arm.c
        // also https://github.com/fastfetch-cli/fastfetch/blob/2.55.1/src/detection/cpu/cpu_arm.h
        private enum ArmImplementers
        {
            Ampere,
            ARM,
            APM,
            Apple,
            Broadcom,
            Cavium,
            DEC,
            Faraday,
            Fujitsu,
            HiSilicon,
            Infineon,
            Intel,
            Marvell,
            Microsoft,
            Motorola,
            NVIDIA,
            Phytium,
            Qualcomm,
            Samsung,
            Unknown,
        }

        // Mapping of ARM CPU implementer IDs
        private static ArmImplementers GetArmImplementerName(string vendorId)
        {
            return vendorId switch
            {
                "0x41" => ArmImplementers.ARM,
                "0x42" => ArmImplementers.Broadcom,
                "0x43" => ArmImplementers.Cavium,
                "0x44" => ArmImplementers.DEC,
                "0x46" => ArmImplementers.Fujitsu,
                "0x48" => ArmImplementers.HiSilicon,
                "0x49" => ArmImplementers.Infineon,
                "0x4d" => ArmImplementers.Motorola,
                "0x6d" => ArmImplementers.Microsoft,
                "0x4e" => ArmImplementers.NVIDIA,
                "0x50" => ArmImplementers.APM,
                "0x51" => ArmImplementers.Qualcomm,
                "0x53" => ArmImplementers.Samsung,
                "0x56" => ArmImplementers.Marvell,
                "0x61" => ArmImplementers.Apple,
                "0x66" => ArmImplementers.Faraday,
                "0x69" => ArmImplementers.Intel,
                "0x70" => ArmImplementers.Phytium,
                "0xc0" => ArmImplementers.Ampere,
                _ => ArmImplementers.Unknown,
            };
        }

        // Mapping of ARM CPU part numbers to model names
        private static string GetArmCpuName(string vendorId, string cpuPart)
        {
            var armImpl = GetArmImplementerName(vendorId);
            return armImpl switch
            {
                ArmImplementers.ARM => cpuPart switch
                {
                    "0x810" => "ARM810",
                    "0x920" => "ARM920",
                    "0x922" => "ARM922",
                    "0x926" => "ARM926",
                    "0x940" => "ARM940",
                    "0x946" => "ARM946",
                    "0x966" => "ARM966",
                    "0xa20" => "ARM1020",
                    "0xa22" => "ARM1022",
                    "0xa26" => "ARM1026",
                    "0xb02" => "ARM11 MPCore",
                    "0xb36" => "ARM1136",
                    "0xb56" => "ARM1156",
                    "0xb76" => "ARM1176",
                    "0xc05" => "Cortex-A5",
                    "0xc07" => "Cortex-A7",
                    "0xc08" => "Cortex-A8",
                    "0xc09" => "Cortex-A9",
                    "0xc0d" => "Cortex-A17", // Originally A12
                    "0xc0f" => "Cortex-A15",
                    "0xc0e" => "Cortex-A17",
                    "0xc14" => "Cortex-R4",
                    "0xc15" => "Cortex-R5",
                    "0xc17" => "Cortex-R7",
                    "0xc18" => "Cortex-R8",
                    "0xc20" => "Cortex-M0",
                    "0xc21" => "Cortex-M1",
                    "0xc23" => "Cortex-M3",
                    "0xc24" => "Cortex-M4",
                    "0xc27" => "Cortex-M7",
                    "0xc60" => "Cortex-M0+",
                    "0xd00" => "Foundation",
                    "0xd01" => "Cortex-A32",
                    "0xd02" => "Cortex-A34",
                    "0xd03" => "Cortex-A53",
                    "0xd04" => "Cortex-A35",
                    "0xd05" => "Cortex-A55",
                    "0xd06" => "Cortex-A65",
                    "0xd07" => "Cortex-A57",
                    "0xd08" => "Cortex-A72",
                    "0xd09" => "Cortex-A73",
                    "0xd0a" => "Cortex-A75",
                    "0xd0b" => "Cortex-A76",
                    "0xd0c" => "Neoverse-N1",
                    "0xd0d" => "Cortex-A77",
                    "0xd0e" => "Cortex-A76AE",
                    "0xd0f" => "AEMv8",
                    "0xd13" => "Cortex-R52",
                    "0xd20" => "Cortex-M23",
                    "0xd21" => "Cortex-M33",
                    "0xd40" => "Neoverse-V1",
                    "0xd41" => "Cortex-A78",
                    "0xd42" => "Cortex-A78AE",
                    "0xd43" => "Cortex-A65AE",
                    "0xd44" => "Cortex-X1",
                    "0xd46" => "Cortex-A510",
                    "0xd47" => "Cortex-A710",
                    "0xd48" => "Cortex-X2",
                    "0xd49" => "Neoverse-N2",
                    "0xd4a" => "Neoverse-E1",
                    "0xd4b" => "Cortex-A78C",
                    "0xd4c" => "Cortex-X1C",
                    "0xd4d" => "Cortex-A715",
                    "0xd4e" => "Cortex-X3",
                    "0xd4f" => "Neoverse-V2",
                    "0xd80" => "Cortex-A520",
                    "0xd81" => "Cortex-A720",
                    "0xd82" => "Cortex-X4",
                    "0xd84" => "Neoverse-V3",
                    "0xd85" => "Cortex-X925",
                    "0xd87" => "Cortex-A725",
                    "0xd88" => "Cortex-A520AE",
                    "0xd89" => "Cortex-A720AE",
                    "0xd8a" => "C1-Nano",
                    "0xd8b" => "C1-Pro",
                    "0xd8c" => "C1-Ultra",
                    "0xd8e" => "Neoverse-N3",
                    "0xd8f" => "Cortex-A320",
                    "0xd90" => "C1-Premium",
                    _ => "Undefined",
                },
                ArmImplementers.Ampere => cpuPart switch
                {
                    "0xac3" => "Ampere-1",
                    "0xac4" => "Ampere-1a",
                    _ => "Undefined",
                },
                ArmImplementers.APM => cpuPart switch
                {
                    "0x000" => "X-Gene",
                    _ => "Undefined",
                },
                ArmImplementers.Apple => cpuPart switch
                {
                    "0x000" => "Swift",
                    "0x001" => "Cyclone",
                    "0x002" => "Typhoon",
                    "0x003" => "Typhoon/Capri",
                    "0x004" => "Twister",
                    "0x005" => "Twister/Elba/Malta",
                    "0x006" => "Hurricane",
                    "0x007" => "Hurricane/Myst",
                    "0x008" => "Monsoon",
                    "0x009" => "Mistral",
                    "0x00b" => "Vortex",
                    "0x00c" => "Tempest",
                    "0x00f" => "Tempest-M9",
                    "0x010" => "Vortex/Aruba",
                    "0x011" => "Tempest/Aruba",
                    "0x012" => "Lightning",
                    "0x013" => "Thunder",
                    "0x020" => "Icestorm-A14",
                    "0x021" => "Firestorm-A14",
                    "0x022" => "Icestorm-M1",
                    "0x023" => "Firestorm-M1",
                    "0x024" => "Icestorm-M1-Pro",
                    "0x025" => "Firestorm-M1-Pro",
                    "0x026" => "Thunder-M10",
                    "0x028" => "Icestorm-M1-Max",
                    "0x029" => "Firestorm-M1-Max",
                    "0x030" => "Blizzard-A15",
                    "0x031" => "Avalanche-A15",
                    "0x032" => "Blizzard-M2",
                    "0x033" => "Avalanche-M2",
                    "0x034" => "Blizzard-M2-Pro",
                    "0x035" => "Avalanche-M2-Pro",
                    "0x036" => "Sawtooth-A16",
                    "0x037" => "Everest-A16",
                    "0x038" => "Blizzard-M2-Max",
                    "0x039" => "Avalanche-M2-Max",
                    "0x046" => "Sawtooth-M11",
                    "0x048" => "Sawtooth-M3-Max",
                    "0x049" => "Everest-M3-Max",
                    _ => "Undefined",
                },
                ArmImplementers.Broadcom => cpuPart switch
                {
                    "0x0f" => "Brahma-B15",
                    "0x100" => "Brahma-B53",
                    "0x516" => "ThunderX2",
                    _ => "Undefined",
                },
                ArmImplementers.Cavium => cpuPart switch
                {
                    "0x0a0" => "ThunderX",
                    "0x0a1" => "ThunderX-88XX",
                    "0x0a2" => "ThunderX-81XX",
                    "0x0a3" => "ThunderX-83XX",
                    "0x0af" => "ThunderX2-99xx",
                    "0x0b0" => "OcteonTX2",
                    "0x0b1" => "OcteonTX2-98XX",
                    "0x0b2" => "OcteonTX2-96XX",
                    "0x0b3" => "OcteonTX2-95XX",
                    "0x0b4" => "OcteonTX2-95XXN",
                    "0x0b5" => "OcteonTX2-95XXMM",
                    "0x0b6" => "OcteonTX2-95XXO",
                    "0x0b8" => "ThunderX3-T110",
                    _ => "Undefined",
                },
                ArmImplementers.DEC => cpuPart switch
                {
                    "0x001" => "SA110",
                    "0x002" => "SA1100",
                    _ => "Undefined",
                },
                ArmImplementers.Faraday => cpuPart switch
                {
                    "0x526" => "FA526",
                    "0x626" => "FA626",
                    _ => "Undefined",
                },
                ArmImplementers.Fujitsu => cpuPart switch
                {
                    "0x001" => "A64FX",
                    "0x003" => "MONAKA",
                    _ => "Undefined",
                },
                ArmImplementers.HiSilicon => cpuPart switch
                {
                    "0xd01" => "TaiShan-v110", // used in Kunpeng-920 SoC
                    "0xd02" => "TaiShan-v120", // used in Kirin 990A and 9000S SoCs
                    "0xd40" => "Cortex-A76", // HiSilicon uses this ID though advertises A76
                    "0xd41" => "Cortex-A77", // HiSilicon uses this ID though advertises A77
                    _ => "Undefined",
                },
                ArmImplementers.Intel => cpuPart switch
                {
                    "0x200" => "i80200",
                    "0x210" => "PXA250A",
                    "0x212" => "PXA210A",
                    "0x242" => "i80321-400",
                    "0x243" => "i80321-600",
                    "0x290" => "PXA250B/PXA26x",
                    "0x292" => "PXA210B",
                    "0x2c2" => "i80321-400-B0",
                    "0x2c3" => "i80321-600-B0",
                    "0x2d0" => "PXA250C/PXA255/PXA26x",
                    "0x2d2" => "PXA210C",
                    "0x411" => "PXA27x",
                    "0x41c" => "IPX425-533",
                    "0x41d" => "IPX425-400",
                    "0x41f" => "IPX425-266",
                    "0x682" => "PXA32x",
                    "0x683" => "PXA930/PXA935",
                    "0x688" => "PXA30x",
                    "0x689" => "PXA31x",
                    "0xb11" => "SA1110",
                    "0xc12" => "IPX1200",
                    _ => "Undefined",
                },
                ArmImplementers.Marvell => cpuPart switch
                {
                    "0x131" => "Feroceon-88FR131",
                    "0x581" => "PJ4/PJ4b",
                    "0x584" => "PJ4B-MP",
                    _ => "Undefined",
                },
                ArmImplementers.Microsoft => cpuPart switch
                {
                    "0xd49" => "Azure-Cobalt-100",
                    _ => "Undefined",
                },
                ArmImplementers.NVIDIA => cpuPart switch
                {
                    "0x000" => "Denver",
                    "0x003" => "Denver 2",
                    "0x004" => "Carmel",
                    "0x010" => "Olympus",
                    _ => "Undefined",
                },
                ArmImplementers.Phytium => cpuPart switch
                {
                    "0x303" => "FTC310",
                    "0x660" => "FTC660",
                    "0x661" => "FTC661",
                    "0x662" => "FTC662",
                    "0x663" => "FTC663",
                    "0x664" => "FTC664",
                    "0x862" => "FTC862",
                    _ => "Undefined",
                },
                ArmImplementers.Qualcomm => cpuPart switch
                {
                    "0x001" => "Oryon 1",
                    "0x002" => "Oryon 2",
                    "0x00f" => "Scorpion",
                    "0x02d" => "Scorpion",
                    "0x04d" => "Krait",
                    "0x06f" => "Krait",
                    "0x201" => "Kryo",
                    "0x205" => "Kryo",
                    "0x211" => "Kryo",
                    "0x800" => "Falkor-V1/Kryo",
                    "0x801" => "Kryo-V2",
                    "0x802" => "Kryo-3XX-Gold",
                    "0x803" => "Kryo-3XX-Silver",
                    "0x804" => "Kryo-4XX-Gold",
                    "0x805" => "Kryo-4XX-Silver",
                    "0xc00" => "Falkor",
                    "0xc01" => "Saphira",
                    _ => "Undefined",
                },
                ArmImplementers.Samsung => cpuPart switch
                {
                    "0x001" => "Exynos-M1",
                    "0x002" => "Exynos-M3",
                    "0x003" => "Exynos-M4",
                    "0x004" => "Exynos-M5",
                    _ => "Undefined",
                },
                ArmImplementers.Infineon => "Undefined", // no definitions found
                ArmImplementers.Motorola => "Undefined", // no definitions found
                ArmImplementers.Unknown => "Undefined",
                _ => "Undefined",
            };
        }
    }
}
