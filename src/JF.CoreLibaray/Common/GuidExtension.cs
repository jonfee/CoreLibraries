using System;
using System.Collections.Generic;

namespace JF.Common
{
    /// <summary>
    /// 基于<see cref="System.Guid"/>的扩展类
    /// </summary>
    public static class GuidExtension
    {
		public static bool IsEmpty(this Guid guid)
		{
			return guid.Equals(Guid.Empty);
		}

		public static bool IsNullOrEmpty(this Guid? guid)
		{
			return guid == null || guid.Equals(Guid.Empty);
		}
	}
}