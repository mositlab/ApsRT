using InfimumMES.Core.Data.Contexts;
using InfimumMES.Core.Services;
using InfimumMES.Core.Services.Loader.AddCondition;
using InfimumMES.Core.Services.Loader.AltWorkCenter;
using InfimumMES.Core.Services.Loader.Container;
using InfimumMES.Core.Services.Loader.ContainerCount;
using InfimumMES.Core.Services.Loader.Dependencies;
using InfimumMES.Core.Services.Loader.Material;
using InfimumMES.Core.Services.Loader.MaxContainer;
using InfimumMES.Core.Services.Loader.Operation;
using InfimumMES.Core.Services.Loader.Rechange;
using InfimumMES.Core.Services.Loader.WorkCenter;
using InfimumMES.Core.Services.LoaderOrchestrator;
using InfimumMES.Core.Services.Log;
using InfimumMES.Core.Services.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;

namespace InfimumMES.Core.Extensions
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Создаёт экземпляр контекста БД <typeparamref name="TContext"/>
        /// </summary>
        /// <typeparam name="TContext">Тип контекста БД</typeparam>
        /// <param name="provider">Провайдер сервисов</param>
        /// <returns></returns>
        public static TContext GetDbContextInstance<TContext>(this IServiceProvider provider) where TContext : DbContext
        {
            var factory = provider.GetRequiredService<IDbContextFactory<TContext>>();

            return factory.CreateDbContext();
        }

        /// <summary>
        /// Добавляет в DI основные сервисы ядра проекта
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApsServices(this IServiceCollection services)
        {
            services.AddDbContextFactory<SqliteLogDbContext>(e => e.UseSqlite(SqliteLogDbContext.ConnectionString));

            services.AddScoped<ILogService, LogService>();

            services.AddSingleton<INotificationService, NotificationService>();

            // загрузчики
            services.AddSingleton<IOperationLoaderService, OperationLoaderService>();
            services.AddSingleton<IMaterialLoaderService, MaterialLoaderService>();
            services.AddSingleton<IMaxContainerLoaderService, MaxContainerLoaderService>();
            services.AddSingleton<IWorkCenterLoaderService, WorkCenterLoaderService>();
            services.AddSingleton<IAltWorkCenterLoaderService, AltWorkCenterLoaderService>();
            services.AddSingleton<IContainerLoaderService, ContainerLoaderService>();
            services.AddSingleton<IAddConditionLoaderService, AddConditionLoaderService>();
            services.AddSingleton<IDependenciesLoaderService, DependenciesLoaderService>();
            services.AddSingleton<IRechangeLoaderService, RechangeLoaderService>();
            services.AddSingleton<IContainerCountLoaderService, ContainerCountLoaderService>();

            // оркестратор
            services.AddSingleton<ILoaderOrchestratorService, LoaderOrchestratorService>();

            // кодировка
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return services;
        }

        /// <summary>
        /// Добавляет в DI контекст БД для вывода данных
        /// </summary>
        /// <param name="services">Провайдер сервисов</param>
        /// <param name="connectionStringLambda">Лямбда-выражение, возвращающее строку подключения</param>
        /// <returns></returns>
        public static IServiceCollection AddOutputDbContext(this IServiceCollection services, Func<string> connectionStringLambda)
        {
            services.AddDbContextFactory<InfimumOutputSqlServerDbContext>(e => e.UseSqlServer(connectionString: connectionStringLambda.Invoke()), ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Добавляет <see cref="LogWriter"/> в DI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="minSeverity"></param>
        /// <returns></returns>
        public static IServiceCollection AddLogWriter(this IServiceCollection services, LogWriter.Severity minSeverity = LogWriter.Severity.Info)
        {
            services.AddSingleton(new LogWriter(Console.Out, minSeverity));

            return services;
        }

        /// <summary>
        /// Creates instance of a service using dependencies from DI but not defined as a dependency <i>(e.g. repository or controller)</i>.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <param name="provider">Service provider.</param>
        /// <returns></returns>
        public static TService Instantiate<TService>(this IServiceProvider provider) where TService : class
        {
            var type = typeof(TService);
            var constructor = type.GetConstructors().FirstOrDefault(e => e.GetParameters().Any()) ?? throw new InvalidOperationException($"Class \"{type.FullName}\" can not be instantiated using DI");
            var argTypes = constructor.GetParameters();
            var args = argTypes.Select(e => provider.ResolveDependency(e.ParameterType)).ToArray();

            return constructor.Invoke(args) as TService;
        }

        /// <summary>
        /// Service provider service resolver middleware for <see cref="Instantiate{TService}(IServiceProvider)"/> call
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object ResolveDependency(this IServiceProvider provider, Type type)
        {
            // resolve DbContext via injected factory
            if (type.IsAssignableTo<DbContext>())
            {
                var method = typeof(DependencyInjectionExtensions)
                    .GetMethods()
                    .FirstOrDefault(e => e.Name == nameof(GetDbContextInstance) && e.IsGenericMethod)
                    .MakeGenericMethod(type);

                return method.Invoke(null, new object[] { provider });
            }

            return provider.GetRequiredService(type);
        }

        /// <summary>
        /// Выполнить миграции контекста <typeparamref name="TContext"/>
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="provider"></param>
        public static void MigrateContext<TContext>(this IServiceProvider provider)
            where TContext : DbContext
        {
            using (var context = provider.GetDbContextInstance<TContext>())
            {
                context.Database.Migrate();
            }
        }
    }
}
