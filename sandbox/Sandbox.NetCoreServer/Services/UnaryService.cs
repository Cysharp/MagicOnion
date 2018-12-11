#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.NetCoreServer.Services
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<Nil> ZeroAsync();
        UnaryResult<TestEnum> OneAsync(int z);
        UnaryResult<string> SumAsync(int x, int y);
        UnaryResult<OreOreResponse> OreOreAsync(OreOreRequest z);
        UnaryResult<OreOreResponse[]> OreOre2Async(OreOreRequest z);
        UnaryResult<List<OreOreResponse>> OreOre3Async(OreOreRequest z);


        Task<UnaryResult<Nil>> LegacyZeroAsync();
        Task<UnaryResult<TestEnum>> LegacyOneAsync(int z);
        Task<UnaryResult<string>> LegacySumAsync(int x, int y);
        Task<UnaryResult<OreOreResponse>> LegacyOreOreAsync(OreOreRequest z);
        Task<UnaryResult<OreOreResponse[]>> LegacyOreOre2Async(OreOreRequest z);
        Task<UnaryResult<List<OreOreResponse>>> LegacyOreOre3Async(OreOreRequest z);

        // use hub instead:)

        Task<ClientStreamingResult<int, string>> ClientStreamingSampleAsync();
        Task<ServerStreamingResult<string>> ServertSreamingSampleAsync(int x, int y, int z);
        Task<DuplexStreamingResult<int, string>> DuplexStreamingSampleAync();
    }

    public enum TestEnum
    {

    }


    public class OreOreRequest
    {

    }


    public class OreOreResponse
    {

    }

    //public class UnaryService : ServiceBase<IMyFirstService>, IMyFirstService
    //{
    //    [MyFirstFilter]
    //    public async UnaryResult<string> SumAsync(int x, int y)
    //    {
    //        return (x + y).ToString();
    //    }
    //}

    public class MyFirstFilter : MagicOnionFilterAttribute
    {
        public MyFirstFilter()
            : base(null)
        {

        }

        public MyFirstFilter(Func<ServiceContext, ValueTask> next)
            : base(next)
        {
        }

        public override ValueTask Invoke(ServiceContext context)
        {
            try
            {
                Console.WriteLine("BF");
                return Next(context);
            }
            finally
            {
                Console.WriteLine("AF");
            }
        }
    }
}

