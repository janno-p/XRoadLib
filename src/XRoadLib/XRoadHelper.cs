using System;
using System.Security.Cryptography;

namespace XRoadLib
{
    public static class XRoadHelper
    {
        public static string GenerateRequestID()
        {
            const int randomLength = 32;
            const int nonceLength = (int)(4.0d / 3.0d * randomLength);

            var random = new byte[randomLength];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(random);

            var nch = new char[nonceLength + 2];
            Convert.ToBase64CharArray(random, 0, randomLength, nch, 0);

            return new string(nch);
        }
    }
}