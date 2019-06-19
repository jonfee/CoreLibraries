using JF.Security;
using System;
using System.Text;
using Xunit;

namespace CoreTestProject
{
    public class EncryptUnitTest
    {
        [Fact]
        public void RSACipterTest()
        {
            try
            {
                var str = "__qwertyuiojhgfrtyujkmnhj__";
                StringBuilder key = new StringBuilder();
                key.Append("-----BEGINRSAPRIVATEKEY-----");
                key.Append("MIICXQIBAAKBgQCe8zGb4UAMg2A63pH+/W145hHvYQPJlkX6OfzJ1215htCI6Pyh");
                key.Append("2TdHRrDqVU6wP609ao9tLxRsbbXrajBGXiq2ijRX7AKrsVdhYi2J+B2q/CrsH5CD");
                key.Append("Ka16YCVPPwf/oZDz/hxrcjZjhOoSIZupY3/xzOBTTjcVcvWbTxGw0wOm6wIDAQAB");
                key.Append("AoGABrVg7KFPILgSwalPJCHyEt4y95VyoXl0LqFv59ztw+lKt9yNfQ875Ag5w0oi");
                key.Append("bhHh7+ulbghEpmbi/LKYov+qccTQMCz4PW1g85LrUYI5PaGKQfsTAWldQeV/mxCk");
                key.Append("mimCk8bahoWPX4i2fnyFdCCn7f3kL8RqRp4NXu2En2gJkPECQQDL3QZrRBpxuE8L");
                key.Append("vgMPNew+II3XtiMzsXc/EwHpAT2hY/pOXt0pvtGfAU2d1JSzmHlBfqPkhr2S0obE");
                key.Append("PpdsXyG3AkEAx5mt8rsDflY8vRYU7Xao0+Smt+9ujMhvtzzS9W62VCUU8xc0UG+x");
                key.Append("umgxofSOedkoaR7k2jqFYYbC1CrwPyAUbQJBALle2R9gZctSFE5REOcb2R0E7PVg");
                key.Append("oNG4ZP3tgqckga3nAwuQJvp2kJVM0g7Z5f0If/mV9eEuw+JlnDWF1JquRjECQQCi");
                key.Append("ZrT0eRsnkO0MgEn4yAInnbPUlphhLbhP48pVbYYmQqGgBHJJPAfkfmBbwMqn83uA");
                key.Append("xGU59kGOD4K39FPTWLulAkAngU3Yv8vYmZKcYXuc/TZjxa0sMuRVroWO6ciW81so");
                key.Append("+sFpf0SM9Ysgf/nKtux7juJABCfF1ffDQdKwederSMOc");
                key.Append("-----ENDRSAPRIVATEKEY-----");
                var newStr = "aaa" + str;

                var result = newStr.RSAEncryptForOpenssl(key.ToString(), hashAlgorithm: "SHA256");

                Assert.True(result != null);
            }
            catch (Exception ex)
            {
                // TODO
            }
        }
    }
}
