using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace Dapper
{
   internal class ColumnInfo
    {
        public PropertyInfo Property { get; set; }

        public string ColumnName { get; set; }

        public bool IsKey { get; set; }

        public DatabaseGeneratedOption DatabaseGeneratedOption { get; set; }
    }
}
