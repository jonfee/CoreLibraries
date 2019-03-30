using System;
using System.Security.Cryptography;

namespace JF.Common
{
    /// <summary>
    /// 随机数生成类
    /// </summary>
    public static class RandomGenerator
    {
		#region 常量定义

		private const string Digits = "0123456789ABCDEFGHJKMNPRSTUVWXYZ";
		private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
		
		#endregion

		#region 公共方法

		public static byte[] Generate(int length)
		{
			if(length < 1)
				throw new ArgumentOutOfRangeException("length");

			var bytes = new byte[length];
			_random.GetBytes(bytes);
			return bytes;
		}

		public static int GenerateInt32()
		{
			var bytes = new byte[4];
			_random.GetBytes(bytes);
			return BitConverter.ToInt32(bytes, 0);
		}

		public static long GenerateInt64()
		{
			var bytes = new byte[8];
			_random.GetBytes(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}

		public static string GenerateString()
		{
			return GenerateString(8);
		}

		public static string GenerateString(int length, bool digitOnly = false)
		{
			if(length < 1 || length > 128)
				throw new ArgumentOutOfRangeException("length");

			var result = new char[length];
			var data = new byte[length];

			_random.GetBytes(data);

			//确保首位字符始终为数字字符
			result[0] = Digits[data[0] % 10];

			for(int i = 1; i < length; i++)
			{
				result[i] = Digits[data[i] % (digitOnly ? 10 : 32)];
			}

			return new string(result);
		}

        #endregion

        #region 正态分布、泊松分布、指数分布以及负指数分布随机数

        /// <summary>
        /// 产生(min,max)之间均匀分布的随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetAverageRandom(int min, int max)
        {
            int exp = 10000;

            int MINnteger = (int)(min * exp);
            int MAXnteger = (int)(max * exp);
            int resultInteger = new Random(Guid.NewGuid().GetHashCode()).Next(MINnteger, MAXnteger);
            return (int)resultInteger / exp;
        }

        /// <summary>
        /// 产生(min,max)之间均匀分布的随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double GetAverageRandom(double min, double max)
        {
            int exp = 10000;

            int MINnteger = (int)(min * exp);
            int MAXnteger = (int)(max * exp);
            int resultInteger = new Random(Guid.NewGuid().GetHashCode()).Next(MINnteger, MAXnteger);
            return resultInteger / (exp * 1.0);
        }

        /// <summary>
        /// 正态分布概率密度函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="miu"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double GetNormal(double x, double miu, double sigma)
        {
            return 1.0 / (x * Math.Sqrt(2 * Math.PI) * sigma) * Math.Exp(-1 * (Math.Log(x) - miu) * (Math.Log(x) - miu) / (2 * sigma * sigma));
        }

        /// <summary>
        /// 产生正态分布随机数
        /// </summary>
        /// <param name="miu"></param>
        /// <param name="sigma"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double GetRandomNormal(double miu, double sigma, double min, double max)
        {
            double x;
            double dScope;
            double y;
            do
            {
                x = GetAverageRandom(min, max);
                y = GetNormal(x, miu, sigma);
                dScope = GetAverageRandom(0, GetNormal(miu, miu, sigma));
            } while (dScope > y);
            return x;
        }

        /// <summary>
        /// 指数分布随机数
        /// </summary>
        /// <param name="const_a"></param>
        /// <returns></returns>
        public static double GetRandExp(double const_a)//const_a是指数分布的参数λ
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            double p;
            double temp;
            if (const_a != 0)
                temp = 1 / const_a;
            else
                throw new System.InvalidOperationException("除数不能为零！不能产生参数为零的指数分布！");
            double randres;
            while (true) //用于产生随机的密度，保证比参数λ小
            {
                p = rand.NextDouble();
                if (p < const_a)
                    break;
            }
            randres = -temp * Math.Log(temp * p, Math.E);
            return randres;
        }

        private static double ngtIndex(Random ran, double lam)
        {
            double dec = ran.NextDouble();
            while (dec == 0)
                dec = ran.NextDouble();
            return -Math.Log(dec) / lam;
        }

        /// <summary>
        /// 泊松分布产生
        /// </summary>
        /// <param name="lam">参数</param>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static double GetRandomPoisson(double lam, double time)
        {
            Random ran = new Random(Guid.NewGuid().GetHashCode());

            int count = 0;
            while (true)
            {
                time -= ngtIndex(ran, lam);
                if (time > 0)
                    count++;
                else
                    break;
            }
            return count;
        }

        #endregion
    }
}
