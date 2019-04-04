using System;
using System.ComponentModel;

namespace Dapper
{
    public class DatabaseFacade : Database<DatabaseFacade>
    {
        private DbContext dbContext;
        
        public DatabaseFacade() { }

        public DatabaseFacade(DbContext context, int? commandTimeout = null)
        {
            this.dbContext = context ?? throw new ArgumentNullException(nameof(context));
            base.InitDatabase(this.dbContext.Connection, commandTimeout);
        }
        
        #region Hidden System.Object members

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
