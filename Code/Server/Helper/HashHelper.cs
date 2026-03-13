using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NLog;
using WebStudyServer.GAME;

namespace WebStudyServer.Helper
{
    public static class HashHelper
    {
        public static bool IsMatchHash(string targetHash, object obj, string secret, int rewardSeed = 0, bool useLog = true)
        {
            if (APP.Cfg.UseStrictValidation)
            {
                _logger.Warn("STRICT_SKIP_HASH_CHECK");
                return false;
            }

            var calHash = CalculateMD5Hash(obj, secret, rewardSeed);
            if (calHash != targetHash)
            {
                if (useLog)
                {
                    _logger.Warn("NOT_MATCHED_HASH ReqHash({ReqHash}) CalHash({CalHash}) SecretKey({SecretKey}) Data({Data})",
                        targetHash, calHash, secret, obj);
                }
                return false;
            }
            return true;
        }

        // MD5 : 빠름, 충돌 있을 수 있음.
        public static string CalculateMD5Hash(object obj, string secret = null, int randomSeed = 0)
        {
            var json = JsonSerializer.Serialize(obj);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            using var memoryStream = new MemoryStream();
            memoryStream.Write(jsonBytes, 0, jsonBytes.Length);

            // Salt
            if (!string.IsNullOrEmpty(secret))
            {
                var secretByteArr = EncryptionHelper.GetSecretByteArr(secret);
                memoryStream.Write(secretByteArr, 0, secretByteArr.Length);
            }

            if (randomSeed != 0)
            {
                var seedBytes = Encoding.UTF8.GetBytes(randomSeed.ToString());
                memoryStream.Write(seedBytes, 0, seedBytes.Length);
            }

            // 결합된 데이터를 바이트 배열로 얻기
            var combinedBytes = memoryStream.ToArray();

            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(jsonBytes);

            // 바이트 배열을 16진수 문자열로 변환
            var builder = new StringBuilder();
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        // Sha256 : 살짝 느림, 보안 강력.
        public static string CalculateSha256Hash(object obj, string secret = null, int randomSeed = 0)
        {
            var json = JsonSerializer.Serialize(obj);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            using var memoryStream = new MemoryStream();
            memoryStream.Write(jsonBytes, 0, jsonBytes.Length);

            // Salt
            if (!string.IsNullOrEmpty(secret))
            {
                var secretByteArr = EncryptionHelper.GetSecretByteArr(secret);
                memoryStream.Write(secretByteArr, 0, secretByteArr.Length);
            }

            if (randomSeed != 0)
            {
                var seedBytes = Encoding.UTF8.GetBytes(randomSeed.ToString());
                memoryStream.Write(seedBytes, 0, seedBytes.Length);
            }

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(jsonBytes);

            // 바이트 배열을 16진수 문자열로 변환
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    }
}
