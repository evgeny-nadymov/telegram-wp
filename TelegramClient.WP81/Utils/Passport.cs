// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using TelegramClient.Utils.SecureCredentials;
using TelegramClient.ViewModels.Passport;
using Buffer = Windows.Storage.Streams.Buffer;
using File = TelegramClient.Utils.SecureCredentials.File;

namespace TelegramClient.Utils
{
    static class Passport
    {
        public static bool IsValidPostCode(string postcode)
        {
            foreach (var symbol in postcode)
            {
                if (!IsValidPostCodeSymbol(symbol)) return false;
            }

            return true;
        }

        private static bool IsValidPostCodeSymbol(char symbol)
        {
            if (symbol >= 'a' && symbol <= 'z'
                || symbol >= 'A' && symbol <= 'Z'
                || symbol >= '0' && symbol <= '9'
                || symbol == '-')
            {
                return true;
            }

            return false;
        }

        public static bool IsValidName(string postcode)
        {
            foreach (var symbol in postcode)
            {
                if (!IsValidNameSymbol(symbol)) return false;
            }

            return true;
        }

        private static bool IsValidNameSymbol(char symbol)
        {
            if (symbol >= 'a' && symbol <= 'z'
                || symbol >= 'A' && symbol <= 'Z'
                || symbol >= '0' && symbol <= '9'
                || symbol == '-'
                || symbol == '.'
                || symbol == ','
                || symbol == '/'
                || symbol == '&'
                || symbol == '\''
                || symbol == ' ')
            {
                return true;
            }

            return false;
        }

        private static TLString DoFinal(bool forEncryption, TLString data, byte[] hash)
        {
            var secretKey = hash.SubArray(0, 32);
            var iv = hash.SubArray(32, 16);

            var cipher = CipherUtilities.GetCipher("AES/CBC/NOPADDING");
            var param = new KeyParameter(secretKey);
            cipher.Init(forEncryption, new ParametersWithIV(param, iv));
            var result = cipher.DoFinal(data.Data);

            return TLString.FromBigEndianData(result);
        }

        private static TLString EncryptSecureData(TLString data, TLString valueSecret, TLString hash)
        {
            var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            var secretHash = sha512.HashData(TLUtils.Combine(valueSecret.Data, hash.Data).AsBuffer()).ToArray();

            var result = DoFinal(true, data, secretHash);

            return result;
        }

        private static TLString DecryptSecureData(TLString data, TLString valueSecret, TLString hash)
        {
            var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            var secretHash = sha512.HashData(TLUtils.Combine(valueSecret.Data, hash.Data).AsBuffer()).ToArray();

            var result = DoFinal(false, data, secretHash);

            return result;
        }

        public static TLString EncryptValueSecret(TLString valueSecret, TLString secret, TLString hash)
        {
            var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            var secretHash = sha512.HashData(TLUtils.Combine(secret.Data, hash.Data).AsBuffer()).ToArray();

            return DoFinal(true, valueSecret, secretHash);
        }

        public static TLString DecryptValueSecret(TLString secureValueSecret, TLString secret, TLString hash)
        {
            var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            var secretHash = sha512.HashData(TLUtils.Combine(secret.Data, hash.Data).AsBuffer()).ToArray();

            return DoFinal(false, secureValueSecret, secretHash);
        }

        public static TLSecureValue EncryptSecureValue(TLSecureValue secureValue, TLString innerData, TLString valueSecret, TLString secret)
        {
            var length = innerData.Data.Length % 16;
            var paddingLength = 16 - length + 32;
            var padding = new byte[paddingLength];

            var randomGenerator = new SecureRandom(TLString.Empty.Data);
            randomGenerator.NextBytes(padding);

            padding[0] = (byte)paddingLength;
            var decryptedData = TLString.FromBigEndianData(TLUtils.Combine(padding, innerData.Data));

            secureValue.Data = new TLSecureData();

            var dataHash = Telegram.Api.Helpers.Utils.ComputeSHA256(decryptedData.Data);
            secureValue.Data.DataHash = TLString.FromBigEndianData(dataHash);

            var data = EncryptSecureData(
                decryptedData,
                valueSecret,
                secureValue.Data.DataHash);
            secureValue.Data.Data = data;

            var secureValueSecret = EncryptValueSecret(
                valueSecret,
                secret,
                secureValue.Data.DataHash);
            secureValue.Data.Secret = secureValueSecret;

            return secureValue;
        }

        public static TLSecureCredentialsEncrypted EncryptSecureCredentials(TLString innerData, TLString credentialsSecret)
        {
            var credentials = new TLSecureCredentialsEncrypted();

            var length = innerData.Data.Length % 16;
            var paddingLength = 16 - length + 32;
            var padding = new byte[paddingLength];

            var randomGenerator = new SecureRandom(TLString.Empty.Data);
            randomGenerator.NextBytes(padding);

            padding[0] = (byte)paddingLength;
            var decryptedData = TLString.FromBigEndianData(TLUtils.Combine(padding, innerData.Data));

            var dataHash = Telegram.Api.Helpers.Utils.ComputeSHA256(decryptedData.Data);
            credentials.Hash = TLString.FromBigEndianData(dataHash);

            var data = EncryptSecureData(
                decryptedData,
                credentialsSecret,
                credentials.Hash);
            credentials.Data = data;

            return credentials;
        }

        public static TLString DecryptSecureValue(TLSecureValue secureValue, TLString secret)
        {
            var value = DecryptValueSecret(
                secureValue.Data.Secret,
                secret,
                secureValue.Data.DataHash);

            if (!IsGoodSecret(value)) return null;

            var data = DecryptSecureData(
                secureValue.Data.Data,
                value,
                secureValue.Data.DataHash);

            if (data.Data.Length == 0) return null;

            var dataHash = Telegram.Api.Helpers.Utils.ComputeSHA256(data.Data);
            if (!TLUtils.ByteArraysEqual(secureValue.Data.DataHash.Data, dataHash)) return null;

            var length = data.Data[0];
            if (length < 32 || length > 255) return null;

            var innerData = data.Data.SubArray(length, data.Data.Length - length);

            return TLString.FromBigEndianData(innerData);
        }

        public static TLString GenerateSecret(TLString secretRandom)
        {
            var randomGenerator = new SecureRandom(secretRandom.Data);
            var secret = new byte[32];
            randomGenerator.NextBytes(secret);

            var sum = secret.Sum(x => x) % 255;
            var diff = secret[secret.Length - 1] + 239 - sum;
            secret[secret.Length - 1] = (byte)(diff % 255);

            return TLString.FromBigEndianData(secret);
        }

        public static bool IsGoodSecret(TLString secret)
        {
            if (secret == null) return false;
            if (secret.Data.Length != 32) return false;

            var sum = secret.Data.Sum(x => x) % 255;
            if (sum != 239) return false;

            return true;
        }

        public static void EncryptSecret(out TLString secureSecret, out TLString secureSalt, out TLLong secureHash, TLString secret, TLString secretRandom, TLSecurePasswordKdfAlgoBase algoBase, TLString password)
        {
            // only this algo is possible to encrypt secret
            var algo2 = algoBase as TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000;
            if (algo2 != null)
            {
                // generate secure hash
                var secureSecretHash = Telegram.Api.Helpers.Utils.ComputeSHA256(secret.Data);
                var secureSecretHash2 = secureSecretHash.SubArray(0, 8);
                secureHash = new TLLong(BitConverter.ToInt64(secureSecretHash2, 0));

                // generate secure salt
                var randomGenerator = new SecureRandom(secretRandom.Data);
                var random = new byte[8];
                randomGenerator.NextBytes(random);
                var newSecureSaltData = TLUtils.Combine(algo2.Salt.Data, random);
                secureSalt = TLString.FromBigEndianData(newSecureSaltData);

                // generate secure secret
                var passwordHash = PBKDF2.GetHash(password.Data.AsBuffer(), newSecureSaltData.AsBuffer()).ToArray();
                secureSecret = DoFinal(true, secret, passwordHash);

                return;
            }

            var algo1 = algoBase as TLSecurePasswordKdfAlgoSHA512;
            if (algo1 != null)
            {
                secureSecret = null;
                secureSalt = null;
                secureHash = null;

                return;

                // generate secure hash
                var secureSecretHash = Telegram.Api.Helpers.Utils.ComputeSHA256(secret.Data);
                var secureSecretHash2 = secureSecretHash.SubArray(0, 8);
                secureHash = new TLLong(BitConverter.ToInt64(secureSecretHash2, 0));

                // generate secure salt
                var randomGenerator = new SecureRandom(secretRandom.Data);
                var random = new byte[8];
                randomGenerator.NextBytes(random);
                var newSecureSaltData = TLUtils.Combine(algo1.Salt.Data, random);
                secureSalt = TLString.FromBigEndianData(newSecureSaltData);

                // generate secure secret
                var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
                var passwordHash = sha512.HashData(TLUtils.Combine(newSecureSaltData, password.Data, newSecureSaltData).AsBuffer()).ToArray();
                secureSecret = DoFinal(true, secret, passwordHash);

                return;
            }

            secureSecret = null;
            secureSalt = null;
            secureHash = null;
        }

        public static TLString DecryptSecureSecret(TLString secureSecret, TLString password, TLSecurePasswordKdfAlgoBase algoBase)
        {
            var algo1 = algoBase as TLSecurePasswordKdfAlgoSHA512;
            if (algo1 != null)
            {
                var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
                var passwordHash = sha512.HashData(TLUtils.Combine(algo1.Salt.Data, password.Data, algo1.Salt.Data).AsBuffer()).ToArray();

                var secret = DoFinal(false, secureSecret, passwordHash);

                if (!IsGoodSecret(secret)) return null;

                return secret;
            }

            var algo2 = algoBase as TLSecurePasswordKdfAlgoPBKDF2HMACSHA512iter100000;
            if (algo2 != null)
            {
                var passwordHash = PBKDF2.GetHash(password.Data.AsBuffer(), algo2.Salt.Data.AsBuffer()).ToArray();

                var secret = DoFinal(false, secureSecret, passwordHash);

                if (!IsGoodSecret(secret)) return null;

                return secret;
            }

            return null;
        }

        private static async Task<Tuple<StorageFile, byte[]>> DoFinal(bool forEncryption, string fileName, StorageFile file, byte[] hash, byte[] padding)
        {
            var localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var inputStream = await file.OpenStreamForReadAsync();
            var outputStream = await localFile.OpenStreamForWriteAsync();

            var secretKey = hash.SubArray(0, 32);
            var iv = hash.SubArray(32, 16);

            var cipher = CipherUtilities.GetCipher("AES/CBC/NOPADDING");
            var param = new KeyParameter(secretKey);
            cipher.Init(forEncryption, new ParametersWithIV(param, iv));

            var processPadding = true;
            var bytesRead = 0;
            var bytesProcessed = 0;
            var input = new byte[1 * 1024 * 1024];
            var output = new byte[1 * 1024 * 1024];

            if (processPadding && forEncryption)
            {
                processPadding = false;

                // add random padding [32..255]
                Array.Copy(padding, input, padding.Length);
                bytesRead = inputStream.Read(input, padding.Length, input.Length - padding.Length) + padding.Length;
                bytesProcessed = cipher.ProcessBytes(input, 0, bytesRead, output, 0);
                outputStream.Write(output, 0, bytesProcessed);
            }

            while ((bytesRead = inputStream.Read(input, 0, input.Length)) > 0)
            {
                // process padding
                if (processPadding && !forEncryption)
                {
                    processPadding = false;

                    // remove random padding [32..255]
                    bytesProcessed = cipher.ProcessBytes(input, 0, bytesRead, output, 0);
                    var paddingLength = output[0];
                    padding = new byte[paddingLength];
                    Array.Copy(output, padding, paddingLength);
                    outputStream.Write(output, paddingLength, bytesProcessed - paddingLength);
                }
                else
                {
                    bytesProcessed = cipher.ProcessBytes(input, 0, bytesRead, output, 0);
                    outputStream.Write(output, 0, bytesProcessed);
                }
            }

            bytesProcessed = cipher.DoFinal(output, 0);
            outputStream.Write(output, 0, bytesProcessed);

            outputStream.Flush();
            outputStream.Dispose();
            inputStream.Dispose();

            return new Tuple<StorageFile, byte[]>(localFile, padding);
        }

        public static async Task<StorageFile> EncryptFile(string fileName, StorageFile file, TLString fileSecret, TLString fileHash, byte[] padding)
        {
            var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            var secretHash = sha512.HashData(TLUtils.Combine(fileSecret.Data, fileHash.Data).AsBuffer()).ToArray();

            var result = await DoFinal(true, fileName, file, secretHash, padding);

            return result.Item1;
        }

        public static async Task<Tuple<StorageFile, byte[]>> DecryptFile(string fileName, StorageFile file, TLString fileSecret, TLString fileHash)
        {
            var sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            var secretHash = sha512.HashData(TLUtils.Combine(fileSecret.Data, fileHash.Data).AsBuffer()).ToArray();

            var result = await DoFinal(false, fileName, file, secretHash, null);

            return result;
        }

        public static async Task<byte[]> GetSha256(byte[] padding, StorageFile file)
        {
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var inputStream = await file.OpenSequentialReadAsync();
            uint capacity = 10 * 1024 * 1024;
            var buffer = new Buffer(capacity);
            var hash = provider.CreateHash();

            if (padding != null)
            {
                hash.Append(padding.AsBuffer());
            }

            while (true)
            {
                await inputStream.ReadAsync(buffer, capacity, InputStreamOptions.None);
                if (buffer.Length > 0)
                    hash.Append(buffer);
                else
                    break;
            }

            var hashValue = hash.GetValueAndReset();

            inputStream.Dispose();

            return hashValue.ToArray();
        }

        public static TLSecureCredentialsEncrypted GenerateSecureCredentialsEncrypted(IList<SecureRequiredType> requiredTypes, TLAuthorizationForm authorizationForm, TLString credentialsSecret, TLString secret)
        {
            var @switch = new Dictionary<Type, Action<SecureData, TLSecureRequiredType, TLSecureValue85>>
            {
                {
                    typeof(TLSecureValueTypePersonalDetails), 
                    (obj, requiredType, value) =>
                    {
                        obj.personal_details = new PersonalDetails
                        {
                            data =
                                new Data
                                {
                                    data_hash = Convert.ToBase64String(value.Data.DataHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(value.Data.Secret, secret, value.Data.DataHash).Data)
                                }
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypePassport), 
                    (obj, requiredType, value) =>
                    {
                        File frontSide = null;
                        var secureFile = value.FrontSide as TLSecureFile;
                        if (secureFile != null)
                        {
                            frontSide = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        File selfie = null;
                        secureFile = value.Selfie as TLSecureFile;
                        if (requiredType.SelfieRequired && secureFile != null)
                        {
                            selfie = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.passport = new SecureCredentials.Passport
                        {
                            data =
                                new Data
                                {
                                    data_hash = Convert.ToBase64String(value.Data.DataHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(value.Data.Secret, secret, value.Data.DataHash).Data)
                                },
                            front_side = frontSide,
                            selfie = selfie,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeInternalPassport), 
                    (obj, requiredType, value) =>
                    {
                        File frontSide = null;
                        var secureFile = value.FrontSide as TLSecureFile;
                        if (secureFile != null)
                        {
                            frontSide = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        File selfie = null;
                        secureFile = value.Selfie as TLSecureFile;
                        if (requiredType.SelfieRequired && secureFile != null)
                        {
                            selfie = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.internal_passport = new InternalPassport
                        {
                            data =
                                new Data
                                {
                                    data_hash = Convert.ToBase64String(value.Data.DataHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(value.Data.Secret, secret, value.Data.DataHash).Data)
                                },
                            front_side = frontSide,
                            selfie = selfie,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeDriverLicense), 
                    (obj, requiredType, value) =>
                    {
                        File frontSide = null;
                        var secureFile = value.FrontSide as TLSecureFile;
                        if (secureFile != null)
                        {
                            frontSide = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        File reverseSide = null;
                        secureFile = value.ReverseSide as TLSecureFile;
                        if (secureFile != null)
                        {
                            reverseSide = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        File selfie = null;
                        secureFile = value.Selfie as TLSecureFile;
                        if (requiredType.SelfieRequired && secureFile != null)
                        {
                            selfie = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.driver_license = new DriverLicense
                        {
                            data =
                                new Data
                                {
                                    data_hash = Convert.ToBase64String(value.Data.DataHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(value.Data.Secret, secret, value.Data.DataHash).Data)
                                },
                            front_side = frontSide,
                            reverse_side = reverseSide,
                            selfie = selfie,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeIdentityCard), 
                    (obj, requiredType, value) =>
                    {
                        File frontSide = null;
                        var secureFile = value.FrontSide as TLSecureFile;
                        if (secureFile != null)
                        {
                            frontSide = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        File reverseSide = null;
                        secureFile = value.ReverseSide as TLSecureFile;
                        if (secureFile != null)
                        {
                            reverseSide = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        File selfie = null;
                        secureFile = value.Selfie as TLSecureFile;
                        if (requiredType.SelfieRequired && secureFile != null)
                        {
                            selfie = new File
                            {
                                file_hash = Convert.ToBase64String(secureFile.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(secureFile.Secret, secret, secureFile.FileHash).Data)
                            };
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.identity_card = new IdentityCard
                        {
                            data =
                                new Data
                                {
                                    data_hash = Convert.ToBase64String(value.Data.DataHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(value.Data.Secret, secret, value.Data.DataHash).Data)
                                },
                            front_side = frontSide,
                            reverse_side = reverseSide,
                            selfie = selfie,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeAddress), 
                    (obj, requiredType, value) =>
                    {
                        obj.address = new Address
                        {
                            data =
                                new Data
                                {
                                    data_hash = Convert.ToBase64String(value.Data.DataHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(value.Data.Secret, secret, value.Data.DataHash).Data)
                                }
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeUtilityBill), 
                    (obj, requiredType, value) =>
                    {
                        var files = new List<File>();
                        foreach (var file in value.Files.OfType<TLSecureFile>())
                        {
                            files.Add(new File
                            {
                                file_hash = Convert.ToBase64String(file.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                            });
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.utility_bill = new UtilityBill
                        {
                            files = files,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeBankStatement), 
                    (obj, requiredType, value) =>
                    {
                        var files = new List<File>();
                        foreach (var file in value.Files.OfType<TLSecureFile>())
                        {
                            files.Add(new File
                            {
                                file_hash = Convert.ToBase64String(file.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                            });
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.bank_statement = new BankStatement
                        {
                            files = files,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeRentalAgreement), 
                    (obj, requiredType, value) =>
                    {
                        var files = new List<File>();
                        foreach (var file in value.Files.OfType<TLSecureFile>())
                        {
                            files.Add(new File
                            {
                                file_hash = Convert.ToBase64String(file.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                            });
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.rental_agreement = new RentalAgreement
                        {
                            files = files,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypePassportRegistration), 
                    (obj, requiredType, value) =>
                    {
                        var files = new List<File>();
                        foreach (var file in value.Files.OfType<TLSecureFile>())
                        {
                            files.Add(new File
                            {
                                file_hash = Convert.ToBase64String(file.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                            });
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.passport_registration = new PassportRegistration
                        {
                            files = files,
                            translation = translation
                        };
                    }
                },
                {
                    typeof(TLSecureValueTypeTemporaryRegistration), 
                    (obj, requiredType, value) =>
                    {
                        var files = new List<File>();
                        foreach (var file in value.Files.OfType<TLSecureFile>())
                        {
                            files.Add(new File
                            {
                                file_hash = Convert.ToBase64String(file.FileHash.Data),
                                secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                            });
                        }

                        List<File> translation = null;
                        if (requiredType.TranslationRequired)
                        {
                            translation = new List<File>();
                            foreach (var file in value.Translation.OfType<TLSecureFile>())
                            {
                                translation.Add(new File
                                {
                                    file_hash = Convert.ToBase64String(file.FileHash.Data),
                                    secret = Convert.ToBase64String(DecryptValueSecret(file.Secret, secret, file.FileHash).Data)
                                });
                            }
                        }

                        obj.temporary_registration = new TemporaryRegistration
                        {
                            files = files,
                            translation = translation
                        };
                    }
                }
            };

            var secureData = new SecureData();

            foreach (var requiredType in requiredTypes)
            {
                if (requiredType.DataValue != null)
                {
                    var type = requiredType.DataValue.Type.GetType();

                    if (@switch.ContainsKey(type))
                    {
                        @switch[type](secureData, requiredType.DataRequiredType, requiredType.DataValue);
                    }
                }
                if (requiredType.DataProofValue != null)
                {
                    var type = requiredType.DataProofValue.Type.GetType();

                    if (@switch.ContainsKey(type))
                    {
                        @switch[type](secureData, requiredType.SelectedDataProofRequiredType, requiredType.DataProofValue);
                    }
                }
            }

            var scopeRootObject = JsonUtils.FromJSON<ScopeRootObject>(authorizationForm.Scope.Data);

            var rootObject = new RootObject
            {
                payload = scopeRootObject == null ? authorizationForm.Payload.ToString() : null,
                nonce = scopeRootObject != null && scopeRootObject.v == 1 && authorizationForm.Payload != null ? authorizationForm.Payload.ToString() : null,
                secure_data = secureData
            };

            var data = JsonUtils.ToJSON(rootObject);

            var secureCredentials = EncryptSecureCredentials(
                new TLString(data),
                credentialsSecret);

            var secureCredentialsSecret = RSAEncryption.EncryptWithPublic(credentialsSecret.Data, authorizationForm.PublicKey.ToString());
            secureCredentials.Secret = TLString.FromBigEndianData(secureCredentialsSecret);

            return secureCredentials;
        }

        public static TLVector<TLSecureValueHash> GenerateValueHashes(IList<SecureRequiredType> requiredTypes)
        {
            var result = new TLVector<TLSecureValueHash>();

            foreach (var requiredType in requiredTypes)
            {
                if (requiredType.DataValue != null)
                {
                    result.Add(new TLSecureValueHash { Type = requiredType.DataValue.Type, Hash = requiredType.DataValue.Hash });
                }
                if (requiredType.DataProofValue != null)
                {
                    result.Add(new TLSecureValueHash { Type = requiredType.DataProofValue.Type, Hash = requiredType.DataProofValue.Hash });
                }
            }

            return result;
        }

        public static async Task<byte[]> GenerateRandomPadding(StorageFile decryptedFile)
        {
            byte[] padding;
            var basicProperties = await decryptedFile.GetBasicPropertiesAsync();
            var inputLength = basicProperties.Size % 16;
            var paddingLength = 16 - inputLength + 32;
            padding = new byte[paddingLength];

            var randomGenerator = new SecureRandom(TLString.Empty.Data);
            randomGenerator.NextBytes(padding, 0, padding.Length);
            padding[0] = (byte)padding.Length;

            return padding;
        }
    }

    namespace SecureCredentials
    {
        [DataContract]
        public class Data
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public string data_hash { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public string secret { get; set; }
        }

        [DataContract]
        public class File
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public string file_hash { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public string secret { get; set; }
        }

        [DataContract]
        public class PersonalDetails
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public Data data { get; set; }
        }

        [DataContract]
        public class Passport
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public Data data { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public File front_side { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public File selfie { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 4)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class InternalPassport
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public Data data { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public File front_side { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public File selfie { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 4)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class DriverLicense
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public Data data { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public File front_side { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public File reverse_side { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 4)]
            public File selfie { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 5)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class IdentityCard
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public Data data { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public File front_side { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public File reverse_side { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 4)]
            public File selfie { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 5)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class Address
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public Data data { get; set; }
        }

        [DataContract]
        public class UtilityBill
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public List<File> files { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class BankStatement
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public List<File> files { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class RentalAgreement
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public List<File> files { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class PassportRegistration
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public List<File> files { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class TemporaryRegistration
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public List<File> files { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public List<File> translation { get; set; }
        }

        [DataContract]
        public class SecureData
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public PersonalDetails personal_details { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public Passport passport { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public InternalPassport internal_passport { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 4)]
            public DriverLicense driver_license { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 5)]
            public IdentityCard identity_card { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 6)]
            public Address address { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 7)]
            public UtilityBill utility_bill { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 8)]
            public BankStatement bank_statement { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 9)]
            public RentalAgreement rental_agreement { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 10)]
            public PassportRegistration passport_registration { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 11)]
            public TemporaryRegistration temporary_registration { get; set; }
        }

        [DataContract]
        public class RootObject
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public SecureData secure_data { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public string payload { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public string nonce { get; set; }
        }

        [DataContract]
        public class ScopeRootObject
        {
            [DataMember(EmitDefaultValue = false, Order = 1)]
            public List<object> data { get; set; }

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public int v { get; set; }
        }
    }

    public static class RSAEncryption
    {
        public static byte[] EncryptWithPublic(byte[] bytesToEncrypt, string publicKey)
        {
            var encryptEngine = new OaepEncoding(new RsaEngine(), new Sha1Digest());

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter);
            }

            var encrypted = encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            return encrypted;

        }

        public static byte[] EncryptWithPrivate(byte[] bytesToEncrypt, string privateKey)
        {
            var encryptEngine = new OaepEncoding(new RsaEngine(), new Sha1Digest());

            using (var txtreader = new StringReader(privateKey))
            {
                var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyPair.Private);
            }

            var encrypted = encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            return encrypted;
        }
    }

    public static class JsonUtils
    {
        public static string ToJSON(object rootObject)
        {
            try
            {
                var stream = new MemoryStream();

                var ser = new DataContractJsonSerializer(rootObject.GetType());
                ser.WriteObject(stream, rootObject);
                stream.Position = 0;
                var sr = new StreamReader(stream);

                return sr.ReadToEnd();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T FromJSON<T>(byte[] data) where T : class
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var stream = new MemoryStream(data))
                {
                    return serializer.ReadObject(stream) as T;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}