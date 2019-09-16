using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JF.EventBus.Mapping
{
    /// <summary>
    /// 事件与订阅对象类型映射程序
    /// </summary>
    public sealed class EventSubscriberTypedMapping
    {
        #region 私有变量
        private static readonly object locker = new object();
        private static EventSubscriberTypedMapping current;
        /// <summary>
        /// 事件与事件订阅者类型的映射关系
        /// </summary>
        private Dictionary<Type, List<Type>> eventSubscriberMappings = new Dictionary<Type, List<Type>>();
        /// <summary>
        /// 订阅者与事件处理程序方法的映射关系
        /// </summary>
        private Dictionary<Type, IEnumerable<MethodInfo>> subscriberHandlersMappings = new Dictionary<Type, IEnumerable<MethodInfo>>();

        #endregion

        #region contructors

        private EventSubscriberTypedMapping() { }

        #endregion

        #region 公开属性

        /// <summary>
        /// 当前实例，此处使用单例
        /// </summary>
        public static EventSubscriberTypedMapping Current
        {
            get
            {
                if (current == null)
                {
                    lock (locker)
                    {
                        if (current == null)
                        {
                            current = new EventSubscriberTypedMapping();
                        }
                    }
                }

                return current;
            }
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 加载领域事件，使之生效。
        /// </summary>
        /// <param name="assemblies"></param>
        public void LoadEvents(IEnumerable<Assembly> assemblies)
        {
            this.ResolveEventSubscriberTypeMappings(assemblies);
        }

        /// <summary>
        /// 从多个程序集中解析出领域事件的订阅者，及订阅者的事件处理程序
        /// </summary>
        /// <param name="assemblies"></param>
        private void ResolveEventSubscriberTypeMappings(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || assemblies.Count() < 1) return;

            foreach (Assembly assembly in assemblies)
            {
                ResolveEventSubscriberTypeMappings(assembly);
            }
        }

        /// <summary>
        /// 从程序集中解析出领域事件的订阅者，及订阅者的事件处理程序
        /// </summary>
        /// <param name="assembly"></param>
        private void ResolveEventSubscriberTypeMappings(Assembly assembly)
        {
            if (assembly == null) return;

            IEnumerable<Type> subscriberTypes = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Count() == 0);

            foreach (var subscriberType in subscriberTypes)
            {
                // 事件处理程序（method）
                var handlers = subscriberType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(IsEventHandler);

                if (handlers == null || handlers.Count() < 1) continue;

                // add to 订阅者 的 事件处理程序集合
                subscriberHandlersMappings.Add(subscriberType, handlers);

                // 遍历事件处理程序，以找到事件对应的订阅者
                foreach (var handler in handlers)
                {
                    var eventType = handler.GetParameters().First().ParameterType;

                    //订阅者（class）
                    List<Type> subscribers = null;

                    // 从原集合中找出当前事件的订阅者
                    // 如果当前事件之前未匹配到订阅者，则初始化
                    if (!eventSubscriberMappings.TryGetValue(eventType, out subscribers))
                    {
                        subscribers = new List<Type>();
                        eventSubscriberMappings.Add(eventType, subscribers);
                    }

                    //如果没有将当前类记录为当前事件的订阅者，则添加到订阅者集合
                    if (!subscribers.Exists(t => t == subscriberType))
                    {
                        subscribers.Add(subscriberType);
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定领域事件的订阅者
        /// </summary>
        /// <typeparam name="TEvent">领域事件类型</typeparam>
        /// <returns></returns>
        public List<Type> GetSubscribersFor<TEvent>() where TEvent : IEvent
        {
            return GetSubscribersFor(typeof(TEvent));
        }

        /// <summary>
        /// 获取指定领域事件的订阅者
        /// </summary>
        /// <param name="eventType">领域事件类型</param>
        /// <returns></returns>
        public List<Type> GetSubscribersFor(Type eventType)
        {
            List<Type> subscribers = null;

            //当前事件类型的订阅者
            //return eventSubscriberMappings.Where(pair => pair.Key.IsAssignableFrom(eventType)).Select(pair => pair.Value).FirstOrDefault();

            if (!eventSubscriberMappings.TryGetValue(eventType, out subscribers))
            {
                subscribers = new List<Type>();
            }

            return subscribers;
        }

        /// <summary>
        /// 获取订阅者的事件处理程序集合
        /// </summary>
        /// <param name="subscriberType">订阅者类型</param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> GetEventHandlers(Type subscriberType)
        {
            IEnumerable<MethodInfo> handlers = null;

            if (!subscriberHandlersMappings.TryGetValue(subscriberType, out handlers))
            {
                handlers = new List<MethodInfo>();
            }

            return handlers;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检测方法是否为一个处理<see cref="IEvent"/>领域事件的方法
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private bool IsEventHandler(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            return parameters.Count() == 1 && typeof(IEvent).IsAssignableFrom(parameters[0].ParameterType);
        }

        #endregion
    }
}
