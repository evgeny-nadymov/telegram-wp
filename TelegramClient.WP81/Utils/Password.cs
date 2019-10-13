// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Org.BouncyCastle.Security;
using Telegram.Api.TL;

namespace TelegramClient.Utils
{
    static class Password
    {
        public static TLString GetHash(TLString currentSalt, TLString password)
        {
            var passwordHash = Telegram.Api.Helpers.Utils.ComputeSHA256(TLUtils.Combine(currentSalt.Data, password.Data, currentSalt.Data));
            
            return TLString.FromBigEndianData(passwordHash);
        }

        public static TLString GetHash(TLPasswordBase passwordBase, TLString pwd)
        {
            var password83 = passwordBase as TLPassword83;
            if (password83 != null)
            {
                return GetNewHash(password83.CurrentAlgo, pwd);
            }

            var password = passwordBase as TLPassword;
            if (password != null)
            {
                return GetHash(password.CurrentSalt, pwd);
            }

            return null;
        }

        public static TLString GetOldHash(TLPasswordKdfAlgoBase kdfAlgoBase, TLString password)
        {
            var algo = kdfAlgoBase as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (algo != null)
            {
                var salt1 = algo.Salt1;

                var hash1 = GetHash(salt1, password);

                return hash1;
            }

            return null;
        }

        public static TLString GetNewHash(TLPasswordKdfAlgoBase kdfAlgoBase, TLString password)
        {
            var algo = kdfAlgoBase as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (algo != null)
            {
                var salt1 = algo.Salt1;
                var salt2 = algo.Salt2;

                var hash1 = GetHash(salt1, password);
                var hash2 = GetHash(salt2, hash1);
                var hash3 = PBKDF2.GetHash(hash2.Data.AsBuffer(), salt1.Data.AsBuffer());
                var hash4 = GetHash(salt2, TLString.FromBigEndianData(hash3.ToArray()));

                return hash4;
            }

            return null;
        }

        public static void AddClientSalt(TLPasswordKdfAlgoBase algoBase)
        {
            var algo = algoBase as TLPasswordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow;
            if (algo == null) return;

            var secureRandom = new SecureRandom();
            var clientSalt1 = new byte[32];
            secureRandom.NextBytes(clientSalt1);

            var newSalt1Data = TLUtils.Combine(algo.Salt1.Data, clientSalt1);
            var newSalt1 = TLString.FromBigEndianData(newSalt1Data);

            algo.Salt1 = newSalt1;
        }

        public static void AddRandomSecureSecret(TLPasswordInputSettings83 inputSettings, IPasswordSecret passwordSecret, TLString password)
        {
            var secret = Passport.GenerateSecret(passwordSecret.SecretRandom);

            var newSecureAlgo = passwordSecret.NewSecureAlgo as TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000;
            if (newSecureAlgo == null) return;

            TLString newSecureSalt;
            TLString newSecureSecret;
            TLLong newSecureSecretId;
            Passport.EncryptSecret(out newSecureSecret, out newSecureSalt, out newSecureSecretId, secret, passwordSecret.SecretRandom, newSecureAlgo, password);

            inputSettings.NewSecureSettings = new TLSecureSecretSettings
            {
                SecureAlgo = new TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000 { Salt  = newSecureSalt },
                SecureSecret = newSecureSecret,
                SecureSecretId = newSecureSecretId
            };

            //inputSettings.NewSecureSalt = newSecureSalt;
            //inputSettings.NewSecureSecret = newSecureSecret;
            //inputSettings.NewSecureSecretId = newSecureSecretId;
        }

        public static void AddSecureSecret(TLString secret, TLPasswordInputSettings83 inputSettings, IPasswordSecret passwordSecret, TLString password)
        {
            var newSecureAlgo = passwordSecret.NewSecureAlgo as TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000;
            if (newSecureAlgo == null) return;

            TLString newSecureSalt;
            TLString newSecureSecret;
            TLLong newSecureSecretId;
            Passport.EncryptSecret(out newSecureSecret, out newSecureSalt, out newSecureSecretId, secret, passwordSecret.SecretRandom, newSecureAlgo, password);

            inputSettings.NewSecureSettings = new TLSecureSecretSettings
            {
                SecureAlgo = new TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000 { Salt = newSecureSalt },
                SecureSecret = newSecureSecret,
                SecureSecretId = newSecureSecretId
            };

            //inputSettings.NewSecureSalt = newSecureSalt;
            //inputSettings.NewSecureSecret = newSecureSecret;
            //inputSettings.NewSecureSecretId = newSecureSecretId;
        }
    }

    public class PBKDF2
    {
        /**
         * The algorithm to use
         * @var string algorithm
         */
        private static string _algorithm = KeyDerivationAlgorithmNames.Pbkdf2Sha512;

        /**
         * Generate a PBDFK hash
         * @param string password
         * @param string salt
         * @param string algorithm
         * @param uint iterationCountIn
         * @param uint target size
         */
        public static IBuffer GetHash(IBuffer buffSecret, IBuffer buffSalt, uint targetSize = 64, uint iterationCountIn = 100000, string algorithm = null)
        {
            // Use the provide KeyDerivationAlgorithm if provided, otherwise default to PBKDF2-SHA256
            if (algorithm == null)
                algorithm = _algorithm;

            KeyDerivationAlgorithmProvider provider = KeyDerivationAlgorithmProvider.OpenAlgorithm(algorithm);

            // This is our secret password
            //IBuffer buffSecret = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);

            // Use the provided salt
            //IBuffer buffSalt = CryptographicBuffer.ConvertStringToBinary(salt, BinaryStringEncoding.Utf8);

            // Create the derivation parameters.
            KeyDerivationParameters pbkdf2Params = KeyDerivationParameters.BuildForPbkdf2(buffSalt, iterationCountIn);

            // Create a key from the secret value.
            CryptographicKey keyOriginal = provider.CreateKey(buffSecret);

            // Derive a key based on the original key and the derivation parameters.
            IBuffer keyDerived = CryptographicEngine.DeriveKeyMaterial(
                keyOriginal,
                pbkdf2Params,
                targetSize
            );

            return keyDerived;

            // Encode the key to a hexadecimal value (for display)
            // return CryptographicBuffer.EncodeToHexString(keyDerived);
        }
    }
}
