using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

internal static class ApsCalculationHealth
{
    internal static void Observe(string operation, Action run, Func<Task?> getTask, Func<bool> isComplete)
    {
        long startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timer = Stopwatch.StartNew();
        string runId = Guid.NewGuid().ToString("N");
        Write(operation, runId, startedAt, "running", 0);

        string status = "faulted";
        try
        {
            run();
            status = getTask()?.Status switch
            {
                TaskStatus.Canceled => "cancelled",
                TaskStatus.Faulted => "faulted",
                TaskStatus.RanToCompletion when isComplete() => "success",
                _ => "incomplete",
            };
        }
        catch (OperationCanceledException)
        {
            status = "cancelled";
            throw;
        }
        finally
        {
            Write(operation, runId, startedAt, status, timer.ElapsedMilliseconds);
        }
    }

    private static void Write(string operation, string runId, long startedAt, string status, long durationMs)
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "ApsRT", "monitoring");
            Directory.CreateDirectory(directory);
            var target = Path.Combine(directory, operation + ".json");
            var temporary = Path.Combine(directory, operation + "." + runId + ".tmp");
            var payload = new
            {
                operation,
                run_id = runId,
                status,
                started_at = startedAt,
                updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                duration_ms = durationMs,
            };
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(payload));
                File.Move(temporary, target, overwrite: true);
            }
            finally
            {
                File.Delete(temporary);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Trace.TraceWarning("APS monitoring write failed: {0}", ex.GetType().Name);
        }
    }
}
