using RuntimeUnitTestToolkit;
using UnityEngine;
using System.Collections;

namespace MagicOinon.Tests
{
    public static class UnitTestLoader
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            UnitTest.RegisterAllMethods<ManualTest>();
        }
    }
}