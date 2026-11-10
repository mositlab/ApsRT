using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

internal static class ApsCalculationHealth
{
    private static readonly HttpClient SentryClient = new() { Timeout = TimeSpan.FromSeconds(5) };

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
            if (status == "faulted")
                _ = ReportFailureAsync(operation, runId);
        }
    }


    private static async Task ReportFailureAsync(string operation, string runId)
    {
        try
        {
            var configuredDsn = Environment.GetEnvironmentVariable("APS_SENTRY_DSN");
            if (!Uri.TryCreate(configuredDsn, UriKind.Absolute, out var dsn)
                || dsn.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(dsn.UserInfo))
                return;

            string path = dsn.AbsolutePath.TrimEnd('/');
            int separator = path.LastIndexOf('/');
            if (separator < 0 || !uint.TryParse(path[(separator + 1)..], out uint project) || project == 0)
                return;

            string publicKey = dsn.UserInfo.Split(':', 2)[0];
            if (publicKey.Length == 0)
                return;

            string endpoint = $"https://{dsn.Authority}{path[..separator]}/api/{project}/envelope/";
            var payload = new
            {
                event_id = runId,
                timestamp = DateTimeOffset.UtcNow,
                platform = "csharp",
                level = "error",
                message = $"APS calculation failed: {operation}",
                environment = Environment.GetEnvironmentVariable("APS_SENTRY_ENVIRONMENT"),
                release = typeof(ApsCalculationHealth).Assembly
                    .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                    .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
                    .FirstOrDefault()?.InformationalVersion,
                tags = new { operation },
            };
            string envelope = JsonSerializer.Serialize(new { event_id = runId }) + "\n"
                + "{\"type\":\"event\"}\n" + JsonSerializer.Serialize(payload) + "\n";
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("X-Sentry-Auth", $"Sentry sentry_version=7, sentry_key={publicKey}");
            request.Content = new StringContent(envelope, Encoding.UTF8, "application/x-sentry-envelope");
            using var response = await SentryClient.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                Trace.TraceWarning("APS Sentry returned HTTP {0}", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("APS Sentry delivery failed: {0}", ex.GetType().Name);
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
