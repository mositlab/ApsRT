using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniversalPack.Extensions
{
    public static class EventExtensions
    {
        /// <summary>
        /// Словарь последних вызовов
        /// </summary>
        private static readonly IDictionary<int, DateTime> _lastCalls;

        static EventExtensions()
        {
            _lastCalls = new Dictionary<int, DateTime>();
        }

        /// <summary>
        /// Вызывает событие, только если последний вызов был более <paramref name="threshold"/> времени назад
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <param name="threshold"></param>
        public static void InvokeThrottled<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs args, TimeSpan threshold)
        {
            var hash = handler.GetHashCode();

            if (!_lastCalls.ContainsKey(hash) || DateTime.Now - _lastCalls[hash] > threshold)
            {
                lock (_lastCalls)
                {
                    _lastCalls[hash] = DateTime.Now;
                }

                handler.Invoke(sender, args);
            }
        }

        /// <summary>
        /// Убирает из <paramref name="handler"/> все подписки
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="handler"></param>
        public static void Clear<TEventArgs>(this EventHandler<TEventArgs> handler)
        {
            var invocationList = handler
                .GetInvocationList()
                .OfType<EventHandler<TEventArgs>>()
                .ToList();

            foreach (var invocation in invocationList)
            {
                handler -= invocation;
            }
        }

        /// <summary>
        /// Асинхронный вызов события
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task InvokeAsync<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs args)
        {
            if (handler == null)
            {
                return;
            }

            var invocationList = handler
                .GetInvocationList()
                .OfType<EventHandler<TEventArgs>>()
                .ToList();

            if (invocationList.Any())
            {
                await Task.WhenAll(invocationList.Select(e => Task.Run(() => e.Invoke(sender, args))));
            }

            invocationList.Clear();
        }

        /// <summary>
        /// Асинхронный вызов события, только если последний вызов был более <paramref name="threshold"/> времени назад
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static async Task InvokeThrottledAsync<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs args, TimeSpan threshold)
        {
            if (handler == null)
            {
                return;
            }

            var hash = handler.GetHashCode();

            if (_lastCalls.TryGetValue(hash, out var lastCall) && DateTime.Now - lastCall < threshold)
            {
                return;
            }

            lock (_lastCalls)
            {
                _lastCalls[hash] = DateTime.Now;
            }

            var invocationList = handler
                .GetInvocationList()
                .OfType<EventHandler<TEventArgs>>()
                .ToList();

            if (invocationList.Any())
            {
                await Task.WhenAll(invocationList.Select(e => Task.Run(() => e.Invoke(sender, args))));
            }

            invocationList.Clear();
        }
    }
}
