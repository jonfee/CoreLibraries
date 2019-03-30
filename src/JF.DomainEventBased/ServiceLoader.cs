using JF.DomainEventBased.Mapping;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace JF.DomainEventBased
{
    /// <summary>
    /// 领域事件、命令服务加载器
    /// </summary>
    public class ServiceLoader
    {
        private static IEnumerable<Assembly> _assemblies = GetAppDomainAssemblies();

        /// <summary>
        /// 异常处理委托。
        /// </summary>
        internal static JF.ExceptionHandler.ExceptionHandler ExceptionHandler { get; private set; }

        /// <summary>
        /// 注入异常处理委托程序。
        /// </summary>
        /// <param name="handler"></param>
        public static void InjectExceptionHandler(JF.ExceptionHandler.ExceptionHandler handler)
        {
            ExceptionHandler = handler;
        }

        /// <summary>
        /// 加载所有服务处理程序。包含：Event处理程序、Command处理程序。
        /// </summary>
        /// <param name="assemblies"></param>
        public static void LoadAllHandlers(IEnumerable<Assembly> assemblies = null)
        {
            var tempAssemblies = assemblies ?? _assemblies;

            LoadEventHandlers(tempAssemblies);
            LoadCommandHandlers(tempAssemblies);
        }

        /// <summary>
        /// 加载程序集中的事件及订阅者映射关系。
        /// </summary>
        /// <param name="assemblies"></param>
        public static void LoadEventHandlers(IEnumerable<Assembly> assemblies = null)
        {
            var tempAssemblies = assemblies ?? _assemblies;

            EventSubscriberTypedMapping.Current.LoadEvents(tempAssemblies);
        }

        /// <summary>
        /// 加载程序集中的命令与处理程序映射关系。
        /// </summary>
        /// <param name="assemblies"></param>
        public static void LoadCommandHandlers(IEnumerable<Assembly> assemblies = null)
        {
            var tempAssemblies = assemblies ?? _assemblies;

            if (tempAssemblies == null) return;

            CommandSubscriberTypedMapping.Current.LoadCommands(tempAssemblies);
        }

        /// <summary>
        /// 获取当前应用程序域下的所有程序集。
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Assembly> GetAppDomainAssemblies()
        {
            var tempAssemblies = new List<Assembly>();

            var dllFiles = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll");

            foreach (var dll in dllFiles)
            {
                tempAssemblies.Add(Assembly.LoadFrom(dll));
            }

            return tempAssemblies;
        }
    }
}
