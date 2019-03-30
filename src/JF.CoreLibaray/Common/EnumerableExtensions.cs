using System.Collections.Generic;
using System.Linq;

namespace JF.Common
{
    public static class EnumerableExtensions
    {
	    public static bool HasValue<T>(this IEnumerable<T> source)
	    {
		    return source != null && source.Any();
	    }
    }
}
