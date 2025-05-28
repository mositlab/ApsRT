using System;

namespace UniversalPack.Common
{
    /// <summary>
    /// Таймер
    /// </summary>
    public sealed class Timer : IDisposable
    {
        private readonly DateTime _start = DateTime.Now;

        public TimeSpan Span => DateTime.Now - _start;

        public int Milliseconds => (int)Span.TotalMilliseconds;

        public int Seconds => (int)Span.TotalSeconds;

        void IDisposable.Dispose() { }
    }
}
