using JF.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace CoreTestProject
{
    public class ValidateAttributeUnitTest
    {
        [Fact]
        public void TestHasErrors()
        {
            AuthUser user = new AuthUser();

            bool canInsert = user.CanInsert(out var errors);

            Assert.False(canInsert);
            Assert.Equal(3, errors.Keys.Count);
        }

        [Fact]
        public void TestNoErrors()
        {
            AuthUser user = new AuthUser
            {
                CompanyID = 0,
                CreatedTime = DateTime.Now,
                CreateUserId = 0,
                DelayNum = Guid.NewGuid().ToString(),
                Email = "abc@qq.com",
                ID = 100,
                IsActive = 1,
                Name = "tester",
                Password = "123456",
                UpdatedTime = DateTime.Now,
                UserName = "tester"
            };

            bool canInsert = user.CanInsert(out var addErrors);
            bool canUpdate = user.CanUpdate(out var upErrors);

            Assert.True(user.IsValid());
            Assert.True(canInsert);
            Assert.True(canUpdate);
        }
    }

    /// <summary>
    /// 授权用户
    /// </summary>
    [Table("tb_auth_user")]
    public class AuthUser : DataEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Key]
        [Column("id")]
        public int ID { get; set; }

        /// <summary>
        /// 所属公司ID
        /// </summary>
        [Column("company_id")]
        public int? CompanyID { get; set; }

        /// <summary>
        /// 用户名，用于登录
        /// </summary>
        [Column("username")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "UserName是必须的。")]
        [StringLength(100, ErrorMessage = "UserName最多允许的长度为100。")]
        public string UserName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        [Column("pwd")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password是必须的。")]
        [StringLength(100, ErrorMessage = "Password最多允许的长度为100。")]
        public string Password { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Column("name"), StringLength(100, ErrorMessage = "Name最多允许的长度为100。")]
        public string Name { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("email"), StringLength(255, ErrorMessage = "Email最多允许的长度为255。")]
        public string Email { get; set; }

        /// <summary>
        /// 是否可用，枚举：<see cref="EnumLib.AuthUserActiveStatus"/>
        /// </summary>
        [Column("isactive")]
        public int IsActive { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// 加密延迟数据
        /// </summary>
        [Column("delaynum")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "DelayNum是必须的。")]
        [StringLength(100, ErrorMessage = "DelayNum最多允许的长度为100。")]
        public string DelayNum { get; set; }

        /// <summary>
        /// 创建者用户ID
        /// </summary>
        [Column("create_id")]
        public int CreateUserId { get; set; }
    }
}
