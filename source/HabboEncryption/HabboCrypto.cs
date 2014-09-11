using HabboEncryption.CodeProject.Utils;
using HabboEncryption.Crypto.KeyExchange;
using HabboEncryption.Hurlant.Crypto.Rsa;
using HabboEncryption.Keys;
using HabboEncryption.Utils;
using System.Text;

namespace HabboEncryption
{
    public class HabboCrypto
    {
        public static RsaKey Rsa;
        public static DiffieHellman DiffieHellman;

        public static void Initialize(RsaKeyHolder keys)
        {
            Rsa = RsaKey.ParsePrivateKey(keys.N, keys.E, keys.D);
            DiffieHellman = new DiffieHellman();
        }

        private static string DecryptRSAString(string message)
        {
            try
            {
                byte[] m = Encoding.Default.GetBytes(message);
                byte[] c = Rsa.Sign(m);

                return Converter.BytesToHexString(c);
            }
            catch
            {
                return "0";
            }
        }

        internal static string GetDHPrimeKey()
        {
            string Key = DiffieHellman.Prime.ToString(10);
            return DecryptRSAString(Key);
        }

        internal static string GetDHGeneratorKey()
        {
            string Key = DiffieHellman.Generator.ToString(10);
            return DecryptRSAString(Key);
        }

        internal static string GetDHPublicKey()
        {
            string Key = DiffieHellman.PublicKey.ToString(10);
            return DecryptRSAString(Key);
        }

        internal static BigInteger CalculateDHSharedKey(string PublicKey)
        {
            try
            {
                byte[] CBytes = Converter.HexStringToBytes(PublicKey);
                byte[] KeyBytes = Rsa.Verify(CBytes);
                string KeyString = Encoding.Default.GetString(KeyBytes);
                return DiffieHellman.CalculateSharedKey(new BigInteger(KeyString, 10));
            }
            catch
            {
                return 0;
            }
        }
    }
}
