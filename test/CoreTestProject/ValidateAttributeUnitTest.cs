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
    /// ��Ȩ�û�
    /// </summary>
    [Table("tb_auth_user")]
    public class AuthUser : DataEntity
    {
        /// <summary>
        /// �û�ID
        /// </summary>
        [Key]
        [Column("id")]
        public int ID { get; set; }

        /// <summary>
        /// ������˾ID
        /// </summary>
        [Column("company_id")]
        public int? CompanyID { get; set; }

        /// <summary>
        /// �û��������ڵ�¼
        /// </summary>
        [Column("username")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "UserName�Ǳ���ġ�")]
        [StringLength(100, ErrorMessage = "UserName�������ĳ���Ϊ100��")]
        public string UserName { get; set; }

        /// <summary>
        /// ��¼����
        /// </summary>
        [Column("pwd")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password�Ǳ���ġ�")]
        [StringLength(100, ErrorMessage = "Password�������ĳ���Ϊ100��")]
        public string Password { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [Column("name"), StringLength(100, ErrorMessage = "Name�������ĳ���Ϊ100��")]
        public string Name { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [Column("email"), StringLength(255, ErrorMessage = "Email�������ĳ���Ϊ255��")]
        public string Email { get; set; }

        /// <summary>
        /// �Ƿ���ã�ö�٣�<see cref="EnumLib.AuthUserActiveStatus"/>
        /// </summary>
        [Column("isactive")]
        public int IsActive { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        [Column("create_time")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        [Column("update_time")]
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// �����ӳ�����
        /// </summary>
        [Column("delaynum")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "DelayNum�Ǳ���ġ�")]
        [StringLength(100, ErrorMessage = "DelayNum�������ĳ���Ϊ100��")]
        public string DelayNum { get; set; }

        /// <summary>
        /// �������û�ID
        /// </summary>
        [Column("create_id")]
        public int CreateUserId { get; set; }
    }
}
