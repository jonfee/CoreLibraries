using JF.Common;
using JF.EventBus.Mapping;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace JF.EventBus
{
    /// <summary>
    /// 事件总线
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        #region 成员变量

        private static EventBus current = new EventBus();
        private ConcurrentQueue<IEvent> queue;

        #endregion

        #region 构造方法

        private EventBus()
        {
            queue = new ConcurrentQueue<IEvent>();

            Watching();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 当前<see cref="EventBus"/>实例
        /// </summary>
        public static EventBus Current => current;

        #endregion

        #region 公开方法

        /// <summary>
        /// 发布事件，写入事件队列
        /// </summary>
        /// <param name="event"></param>
        public void Publish(IEvent @event)
        {
            queue.Enqueue(@event);
        }

        /// <summary>
        /// 处理单个事件
        /// </summary>
        /// <param name="event"></param>
        public void Process(IEvent @event)
        {
            var subscriberTypes = EventSubscriberTypedMapping.Current.GetSubscribersFor(@event.GetType());

            if (subscriberTypes != null && subscriberTypes.Count > 0)
            {
                Task[] tasks = new Task[subscriberTypes.Count];
                int i = 0;

                foreach (var subscriberType in subscriberTypes)
                {
                    tasks[i++] = Task.Factory.StartNew(() =>
                      {
                          var rsts = HandleEvent(@event, subscriberType);
                          if (rsts != null)
                          {
                              @event.AddResults(rsts);
                          }
                      });
                }

                Task.WaitAll(tasks);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 执行事件处理程序
        /// </summary>
        /// <param name="event"></param>
        /// <param name="subscriberType"></param>
        /// <returns></returns>
        private static IEnumerable<object> HandleEvent(IEvent @event, Type subscriberType)
        {
            IList<object> results = new List<object>();

            try
            {
                var eventHandlders = EventSubscriberTypedMapping.Current.GetEventHandlers(subscriberType);

                foreach (var handler in eventHandlders)
                {
                    var subscriberObject = CreateSubscriber(subscriberType);

                    if (subscriberObject != null)
                    {
                        var result = ExecuteEventHandlder(handler, subscriberObject, @event);

                        if (result != null) results.Add(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Loader.Current.ExceptionHandler?.Invoke(ex);
            }

            return results;
        }

        /// <summary>
        /// 执行事件处理程序
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="eventSource"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        private static object ExecuteEventHandlder(MethodInfo handler, object eventSource, object @event)
        {
            object result = null;

            try
            {
                result = handler.Invoke(eventSource, new[] { @event });
            }
            catch (Exception ex)
            {
                Loader.Current.ExceptionHandler?.Invoke(ex);
            }

            return result;
        }

        private static object CreateSubscriber(Type subscriberType)
        {
            var constructor = subscriberType.GetConstructors()[0];
            var paramValues = new List<object>();

            constructor.GetParameters().ForEach((param) =>
            {
                var value = GetValueFor(param.ParameterType);

                paramValues.Add(value);
            });

            return constructor.Invoke(paramValues.ToArray());
        }

        private static object GetValueFor(Type targetType)
        {
            if (targetType.IsInterface)
            {
                return null;
            }
            else
            {
                return GetDefaultValueFor(targetType);
            }
        }

        /// <summary>
        /// 获取指定类型的默认值
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static object GetDefaultValueFor(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        private void Watching()
        {
            for (var i = 0; i < 10; i++)
            {
                Task.Factory.StartNew(DoWork);
            }
        }

        private void DoWork()
        {
            while (true)
            {
                if (queue.IsEmpty)
                {
                    System.Threading.Thread.Sleep(100);
                }
                else if (queue.TryDequeue(out IEvent @event))
                {
                    if (@event == null) continue;

                    try
                    {
                        Process(@event);
                    }
                    catch { }
                }
            }
        }

        #endregion
    }
}
