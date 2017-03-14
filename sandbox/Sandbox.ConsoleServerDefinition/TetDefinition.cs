using MagicOnion;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.ConsoleServer
{
    public interface ITetDefinition : IService<ITetDefinition>
    {
        UnaryResult<MyEnum?> Test(List<int> l, Dictionary<int, int> d);

    }
}
