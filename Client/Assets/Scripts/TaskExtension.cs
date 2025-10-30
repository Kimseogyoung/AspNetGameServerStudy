using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TaskExtension
{
    public static async void FireAndForget(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            LOG.E($"Task Exception Msg({ex.Message}) StackTrace({ex.StackTrace})");
        }
    }
}
