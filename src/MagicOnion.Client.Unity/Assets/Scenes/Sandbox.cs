using MagicOnion;
using MessagePack;
using MessagePack.Resolvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicOnionUnity
{
    public class Sandbox : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            UnsafeDirectBlitResolver.Register<Foo>();
            CompositeResolver.RegisterAndSetAsDefault(
                UnsafeDirectBlitResolver.Instance,

                BuiltinResolver.Instance

                );

            var f = new Foo { A = 10, B = 9999, C = 9999999 };
            var doudarou = MessagePackSerializer.Serialize(f, UnsafeDirectBlitResolver.Instance);
            var two = MessagePackSerializer.Deserialize<Foo>(doudarou);

           
            Debug.Log(string.Join(", ", doudarou));
            Debug.Log(two.ToString());

            var f2 = new[]{
                new Foo { A = 10, B = 9999, C = 9999999 },
                new Foo { A = 101, B = 43, C = 234 },
                new Foo { A = 20, B = 5666, C = 1111 },
            };
            var doudarou2 = MessagePackSerializer.Serialize(f2, UnsafeDirectBlitResolver.Instance);
            var two2 = MessagePackSerializer.Deserialize<Foo[]>(doudarou2);

            Debug.Log(string.Join(", ", doudarou2));
            foreach (var item in two2)
            {
                Debug.Log(item.ToString());
            }
        }

    }

    public struct Foo
    {
        public byte A;
        public long B;
        public int C;

        public override string ToString()
        {
            return $"A:{A} B:{B} C:{C}";
        }
    }

}