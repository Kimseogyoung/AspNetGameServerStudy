using System;
using System.IO;

namespace ClientCore
{
    public static class DeviceKeyHelper
    {
        // 새 DeviceKey 생성 (Guid 기반)
        public static string GenerateKey()
        {
            return Guid.NewGuid().ToString();
        }

        // 파일에 저장된 키 로드, 없거나 비어 있으면 생성 후 저장 (Unity 등 외부 호출용)
        public static string LoadOrCreateKey(string filePath)
        {
            if (File.Exists(filePath))
            {
                var saved = File.ReadAllText(filePath).Trim();
                if (!string.IsNullOrEmpty(saved))
                {
                    return saved;
                }
            }

            var key = GenerateKey();
            File.WriteAllText(filePath, key);
            return key;
        }

        // 기존 키 교체 후 새 키 생성·저장 (재발급)
        public static string RegenerateKey(string filePath)
        {
            var key = GenerateKey();
            File.WriteAllText(filePath, key);
            return key;
        }
    }
}
