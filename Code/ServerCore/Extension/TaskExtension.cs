using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ServerCore.Extension
{
    public static class TaskExtension
    {
        // 의도적으로 await하지 않는 Task의 실패를 로그로 남긴다 (unobserved exception 방지).
        public static void FireAndForget(this Task task,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _ = task.ContinueWith(t =>
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                Core.Logger.LogError(t.Exception, $"UNHANDLED_EXCEPTION {fileName}.{memberName}:{lineNumber}");
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
