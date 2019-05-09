using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JF.Random
{
    /// <summary>
    /// ID生成器
    /// </summary>
    public class IDGenerator
    {
        #region variables

        private static IDGenerator _default;
        private long _sequence = 0L;
        private long _lastTimestamp = -1L;

        readonly static object _lock = new Object();

        //基准时间
        const long StartStamp = 1288834974657L;

        #region 每一部分占用的位数

        //机器标识位数
        const int WorkerIdBits = 5;
        //数据标志位数
        const int DatacenterIdBits = 5;
        //序列号识位数
        const int SequenceBits = 12;

        #endregion

        #region 每一部分的最大值

        //机器ID最大值
        const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        //数据标志ID最大值
        const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);
        //序列号ID最大值
        const long SequenceMask = -1L ^ (-1L << SequenceBits);

        #endregion

        #region 每一部分偏移量

        //机器ID偏左移12位
        const int WorkerIdShift = SequenceBits;
        //数据ID偏左移17位
        const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        //时间毫秒左移22位
        const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        #endregion

        #endregion

        #region 实例化

        /// <summary>
        /// 实例化一个<see cref="IDGenerator"/>
        /// </summary>
        /// <param name="workerId">机器标识ID</param>
        /// <param name="datacenterId">数据标识ID</param>
        /// <param name="sequence">序列号</param>
        public IDGenerator(long workerId, long datacenterId, long sequence = 0L)
        {
            // 如果超出范围就抛出异常
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException(string.Format("worker Id 必须大于0，且不能大于MaxWorkerId： {0}", MaxWorkerId));
            }

            if (datacenterId > MaxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException(string.Format("region Id 必须大于0，且不能大于MaxWorkerId： {0}", MaxDatacenterId));
            }

            //先检验再赋值
            WorkerId = workerId;
            DatacenterId = datacenterId;
            _sequence = sequence;
        }

        /// <summary>
        /// 默认实例，一般用于单服务数据中心时使用。
        /// </summary>
        public static IDGenerator Default
        {
            get
            {
                if (_default == null)
                {
                    lock (_lock)
                    {
                        if (_default == null)
                        {
                            _default = new IDGenerator(1, 1);
                        }
                    }
                }
                return _default;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// 机器标识ID
        /// </summary>
        public long WorkerId { get; protected set; }
        /// <summary>
        /// 数据标识ID
        /// </summary>
        public long DatacenterId { get; protected set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public long Sequence
        {
            get { return _sequence; }
            internal set { _sequence = value; }
        }

        #endregion

        #region Behavious

        /// <summary>
        /// 读取下一个ID
        /// </summary>
        /// <returns></returns>
        public virtual long NextId()
        {
            lock (_lock)
            {
                var timestamp = TimeGen();
                if (timestamp < _lastTimestamp)
                {
                    throw new Exception(string.Format("时间戳必须大于上一次生成ID的时间戳.  拒绝为{0}毫秒生成id", _lastTimestamp - timestamp));
                }

                //如果上次生成时间和当前时间相同,在同一毫秒内
                if (_lastTimestamp == timestamp)
                {
                    //sequence自增，和sequenceMask相与一下，去掉高位
                    _sequence = (_sequence + 1) & SequenceMask;
                    //判断是否溢出,也就是每毫秒内超过1024，当为1024时，与sequenceMask相与，sequence就等于0
                    if (_sequence == 0)
                    {
                        //等待到下一毫秒
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    //如果和上次生成时间不同,重置sequence，就是下一毫秒开始，sequence计数重新从0开始累加,
                    //为了保证尾数随机性更大一些,最后一位可以设置一个随机数
                    _sequence = 0;//new Random().Next(10);
                }

                _lastTimestamp = timestamp;
                return ((timestamp - StartStamp) << TimestampLeftShift) | (DatacenterId << DatacenterIdShift) | (WorkerId << WorkerIdShift) | _sequence;
            }
        }

        /// <summary>
        /// 防止产生的时间比之前的时间还要小（由于NTP回拨等问题）,保持增量的趋势.
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        protected virtual long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取当前的时间戳
        /// </summary>
        /// <returns></returns>
        protected virtual long TimeGen()
        {
            //return TypeExtensions.CurrentTimeMillis();
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        #endregion
    }
}
