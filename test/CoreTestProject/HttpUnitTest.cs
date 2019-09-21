using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using JF.Http;

namespace CoreTestProject
{
    public class HttpUnitTest
    {
        [Fact]
        public void PostFormTest()
        {
            var api = "http://localhost:64355/api/WebSite/AdManager/ChangeBudget";
            var formData = new
            {
                StrucType = 2,
                MediaId = 0,
                Items = new List<object> {
                    new
                    {
                        AccountId = "1619699211439765",
                        Budget = 202.0000,
                        Id = "23843207039360695",
                        PreBudget = 200.0
                    }
                }
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization","Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiemVuZ3hpMDEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3ByaW1hcnlzaWQiOiIzNzciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zaWQiOiIxIiwiaG9zdCI6ImxvY2FsaG9zdCIsIm5iZiI6MTU2OTA1MTc4MSwiZXhwIjoxNTY5MDY5NzgxLCJpc3MiOiJBTVMiLCJhdWQiOiJBTVNfV2ViIn0.zbGg6doRcBRvUd8bkFXqEU08Edyrs3X2zZ0giEUuEj0"}
            };

            var result = JF.Http.HttpRequest.PostByFormAsync<object, object>(api, formData, headers).Result;

            Assert.True(result != null);
        }
    }
}
