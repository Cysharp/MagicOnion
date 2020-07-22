using System.Text;

namespace MagicOnion.Utils
{
    public static class FNV1A32
    {
        public static int GetHashCode(string str)
        {
            return GetHashCode(Encoding.UTF8.GetBytes(str));
        }

        public static int GetHashCode(byte[] obj)
        {
            uint hash = 0;
            if (obj != null)
            {
                hash = 2166136261;
                for (int i = 0; i < obj.Length; i++)
                {
                    hash = unchecked((obj[i] ^ hash) * 16777619);
                }
            }

            return unchecked((int)hash);
        }
    }
}
