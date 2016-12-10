using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter.Formatters;

namespace MagicOnion.Server
{
    public class MagicOnionOptions
    {
        public Type ZeroFormatterTypeResolverType { get; private set; }

        public MagicOnionOptions()
            : this(typeof(ZeroFormatter.Formatters.DefaultResolver))
        {

        }

        public MagicOnionOptions(Type zeroFormatterTypeResolverType)
        {
            this.ZeroFormatterTypeResolverType = zeroFormatterTypeResolverType;
        }
    }
}