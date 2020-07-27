#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.AspNetCore.Services
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

    /// <summary>
    /// 計算処理をするサービス予定は未定。
    /// </summary>
    public interface ICalcSerivce : IService<ICalcSerivce>
    {
        UnaryResult<string> DumpAsync(int x);

        /// <summary>
        /// ダミーです1。
        /// </summary>
        UnaryResult<string> Dump1Async(int x, int y);
        /// <summary>
        /// ダミーです2。
        /// </summary>
        UnaryResult<string> Dump2Async(int x, int y);
        /// <summary>
        /// 足したりします。
        /// </summary>
        /// <param name="x">多分X。</param>
        /// <param name="y">多分Y。</param>
        /// <returns>何故かString。</returns>
        UnaryResult<string> SumAsync(int x, int y);
    }

    public class UnaryService : ServiceBase<ICalcSerivce>, ICalcSerivce
    {
        public UnaryResult<string> Dump1Async(int x, int y)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<string> Dump2Async(int x, int y)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<string> DumpAsync(int x, int y)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<string> DumpAsync(int x)
        {
            throw new NotImplementedException();
        }

        [MyFirstFilter]
        public async UnaryResult<string> SumAsync(int x, int y)
        {
            return (x + y).ToString();
        }
    }

    public class MyFirstFilter : MagicOnionFilterAttribute
    {
        public override ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            try
            {
                Console.WriteLine("BF");
                return next(context);
            }
            finally
            {
                Console.WriteLine("AF");
            }
        }
    }
}

