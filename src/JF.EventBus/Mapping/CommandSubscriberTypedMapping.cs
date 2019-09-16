using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JF.EventBus.Mapping
{
    public sealed class CommandSubscriberTypedMapping
    {
        #region private variables
        private static readonly object locker = new object();
        private static CommandSubscriberTypedMapping current;
        /// <summary>
        /// 命令类型对应的处理程序实例集合
        /// </summary>
        private Dictionary<Type, object> commandHandlers;

        #endregion

        #region contructors

        private CommandSubscriberTypedMapping()
        {
            this.commandHandlers = new Dictionary<Type, object>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 当前实例，单例
        /// </summary>
        public static CommandSubscriberTypedMapping Current
        {
            get
            {
                if (current == null)
                {
                    lock (locker)
                    {
                        if (current == null)
                        {
                            current = new CommandSubscriberTypedMapping();
                        }
                    }
                }

                return current;
            }
        }

        #endregion

        /// <summary>
        /// 获取<see cref="ICommand"/>对应的处理程序。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="commandHandler"></param>
        /// <returns></returns>
        public bool TryGet(Type type, out object commandHandler)
        {
            commandHandler = null;
            bool success = this.commandHandlers.ContainsKey(type);

            if (success)
            {
                commandHandler = this.commandHandlers[type];
            }

            return success;
        }

        /// <summary>
        /// 加载命令，使之生效。
        /// </summary>
        /// <param name="assemblies"></param>
        public void LoadCommands(IEnumerable<Assembly> assemblies)
        {
            this.ResolveCommandsSubscriberTypeMappings(assemblies);
        }

        /// <summary>
        /// 从程序集中解析出领域的所有命令类型及命令处理程序实例集合
        /// </summary>
        /// <returns></returns>
        private void ResolveCommandsSubscriberTypeMappings(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || assemblies.Count() < 1) return;

            foreach (Assembly assembly in assemblies)
            {
                ResolveCommandsSubscriberTypeMappings(assembly);
            }
        }

        /// <summary>
        /// 从程序集中解析出领域的所有命令类型及命令处理程序实例集合
        /// </summary>
        /// <returns></returns>
        private void ResolveCommandsSubscriberTypeMappings(Assembly assemblie)
        {
            string ihandlerName = typeof(ICommandHandler<>).Name;

            var types = assemblie.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Count() == 0);

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var ifc in interfaces)
                {
                    if (ifc.Name != ihandlerName) continue;

                    var executeMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(m => IsCommandHandler(m) && m.Name == "Execute").FirstOrDefault();
                    if (executeMethod == null) continue;

                    //按照约定，Excute方法的第一个参数便是派生自IDomainCommand接口的命令类型
                    var commandType = executeMethod.GetParameters().FirstOrDefault()?.ParameterType;

                    if (commandType != null && typeof(ICommand).IsAssignableFrom(commandType) && !commandHandlers.ContainsKey(commandType))
                    {
                        commandHandlers.Add(commandType, Activator.CreateInstance(type));
                    }
                }
            }
        }

        #region 私有方法

        /// <summary>
        /// 检测方法是否为一个处理<see cref="ICommand"/>命令的方法
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private bool IsCommandHandler(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            return parameters.Count() == 1 && typeof(ICommand).IsAssignableFrom(parameters[0].ParameterType);
        }

        #endregion
    }
}
