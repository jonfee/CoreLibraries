using JF.EventBus.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JF.EventBus
{
    /// <summary>
    /// 事件、命令处理程序加载器
    /// </summary>
    public class Loader
    {
        #region private variables

        private static object locker = new object();
        private static Loader current = null;

        #endregion

        #region contructors

        private Loader() { }

        #endregion

        #region singleton

        public static Loader Current
        {
            get
            {
                if (current == null)
                {
                    lock (locker)
                    {
                        if (current == null)
                        {
                            current = new Loader();
                        }
                    }
                }

                return current;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// 异常处理委托
        /// </summary>
        public JF.ExceptionHandler.ExceptionHandler ExceptionHandler { get; private set; } = null;

        #endregion

        /// <summary>
        /// 注入异常处理委托程序。
        /// </summary>
        /// <param name="handler"></param>
        public Loader Inject(JF.ExceptionHandler.ExceptionHandler handler)
        {
            ExceptionHandler = handler;

            return this;
        }

        /// <summary>
        /// 加载所有服务处理程序。包含：Event处理程序、Command处理程序。
        /// </summary>
        /// <param name="expression"></param>
        public Loader LoadHandlers(Expression<Func<Assembly, bool>> expression)
        {
            var tempAssemblies = getAppDomainAssemblies().Where(expression.Compile());
            return LoadHandlers(tempAssemblies);
        }

        /// <summary>
        /// 加载所有服务处理程序。包含：Event处理程序、Command处理程序。
        /// </summary>
        /// <param name="assemblies"></param>
        public Loader LoadHandlers(IEnumerable<Assembly> assemblies = null)
        {
            var tempAssemblies = assemblies ?? getAppDomainAssemblies();

            LoadEventHandlers(tempAssemblies);
            LoadCommandHandlers(tempAssemblies);

            return this;
        }

        /// <summary>
        /// 加载程序集中的事件及订阅者映射关系。
        /// </summary>
        /// <param name="assemblies"></param>
        public Loader LoadEventHandlers(IEnumerable<Assembly> assemblies = null)
        {
            var tempAssemblies = assemblies ?? getAppDomainAssemblies();

            if (tempAssemblies != null)
            {
                EventSubscriberTypedMapping.Current.LoadEvents(tempAssemblies);
            }

            return this;
        }

        /// <summary>
        /// 加载程序集中的命令与处理程序映射关系。
        /// </summary>
        /// <param name="assemblies"></param>
        public Loader LoadCommandHandlers(IEnumerable<Assembly> assemblies = null)
        {
            var tempAssemblies = assemblies ?? getAppDomainAssemblies();

            if (tempAssemblies != null)
            {
                CommandSubscriberTypedMapping.Current.LoadCommands(tempAssemblies);
            }

            return this;
        }

        /// <summary>
        /// 获取当前应用程序域下的所有程序集。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Assembly> getAppDomainAssemblies()
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
