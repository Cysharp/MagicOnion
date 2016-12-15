using RuntimeUnitTestToolkit;
using UnityEngine;
using System.Collections;

namespace MagicOnion.Tests
{
    public static class UnitTestLoader
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            UnitTest.RegisterAllMethods<SimpleTest>();
            UnitTest.RegisterAllMethods<StandardTest>();
            UnitTest.RegisterAllMethods<ArgumentPatternTest>();
        }
    }
}