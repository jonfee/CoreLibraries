using JF.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoreTestProject
{
    public class MemoryCacheUnitTest
    {
        [Fact]
        public void CacheTest1()
        {
            CachingFactory.GetOrCreateMemoryCache(10, InitUserCache);

            var cache = CachingFactory.GetInstance<UserCacheModel>();

            var users = cache.Values();

            var user = cache.FindAt(300);

            Assert.True(user.Name == "����");

            cache.Update(new UserCacheModel { Id = 300, Name = "������" });

            user = cache.FindAt(300);

            Assert.True(user.Name == "������");

            cache.RemoveAt(300);

            user = cache.FindAt(300);

            Assert.True(user == null);
        }

        public IEnumerable<UserCacheModel> InitUserCache()
        {
            List<UserCacheModel> list = new List<UserCacheModel>();
            list.Add(new UserCacheModel { Id = 100, Name = "����" });
            list.Add(new UserCacheModel { Id = 200, Name = "����" });
            list.Add(new UserCacheModel { Id = 300, Name = "����" });
            list.Add(new UserCacheModel { Id = 400, Name = "����" });
            list.Add(new UserCacheModel { Id = 500, Name = "����" });

            return list;
        }


        public class UserCacheModel : ICacheModel
        {
            public string Key => this.Id.ToString();

            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
