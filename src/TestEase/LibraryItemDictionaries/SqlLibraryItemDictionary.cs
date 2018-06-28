using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using TestEase.LibraryItems;

namespace TestEase.LibrarItemDictionaries
{
    public class SqlLibraryItemDictionary : LibraryItemDictionary<SqlLibraryItem>
    {
        public SqlLibraryItemDictionary()
        {
            AutoCommitTransaction = true;
        }

        public new SqlLibraryItem this[string key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return base[key];
                }
                throw new InvalidOperationException(string.Format("\"{0}\" does not exist in the Test Data Library!",
                    key));
            }
            set
            {
                value.MyDictionary = this;
                base[key] = value;
            }
        }

        public bool AutoCommitTransaction { get; set; }

        public new void Add(string key, SqlLibraryItem value)
        {
            value.MyDictionary = this;
            base.Add(key, value);
        }

        public void CommitTransaction()
        {
            //CTConnection.CommitTransaction();
            //WSConnection.CommitTransaction();
            //ObxConnection.CommitTransaction();
        }

        public void RollbackTransactions()
        {
            //CTConnection.RollbackTransaction();
            //WSConnection.RollbackTransaction();
            //ObxConnection.RollbackTransaction();
        }

        public void QueueSQL(string sqlScript)
        {
            // TODO needs to be generic
            //var Scripts = Regex.Split(sqlScript, @"--DbType \s*=\s* (WORKSPACE|CARETRACKER|OBXCHANGE|ACL)",
            //    RegexOptions.IgnorePatternWhitespace);

            //for (var i = 1; i < Scripts.Length; i += 2)
            //{
            //    var DbType = Scripts[i];
            //    var SQLCode = Scripts[i + 1];

            //    QueueSQL(DbType, SQLCode);
            //}
        }

        public dynamic ExecuteSQL()
        {
            var Results = new List<ExpandoObject>();

            //var WS_SQL = _QueuedSQL_Workspace == null ? "" : _QueuedSQL_Workspace.ToString();
            //var CT_SQL = _QueuedSQL_CareTracker == null ? "" : _QueuedSQL_CareTracker.ToString();
            //var OBX_SQL = _QueuedSQL_Obxchange == null ? "" : _QueuedSQL_Obxchange.ToString();
            //var ACL_SQL = _QueuedSQL_ACL == null ? "" : _QueuedSQL_ACL.ToString();

            //if (CT_SQL != "")
            //{
            //    //Results.AddRange(RunSQL(CT_SQL, VisionConnection.ConnectionStringTypeEnum.CareTracker));
            //}

            //if (WS_SQL != "")
            //{
            //    //Results.AddRange(RunSQL(WS_SQL, VisionConnection.ConnectionStringTypeEnum.Workspace));
            //}

            //if (OBX_SQL != "")
            //{
            //    //Results.AddRange(RunSQL(OBX_SQL, VisionConnection.ConnectionStringTypeEnum.ObxChange));
            //}

            //if (ACL_SQL != "")
            //{
            //    //Results.AddRange(RunSQL(ACL_SQL, VisionConnection.ConnectionStringTypeEnum.ACL));
            //}

            //_QueuedSQL_Workspace = null;
            //_QueuedSQL_CareTracker = null;
            //_QueuedSQL_Obxchange = null;
            //_QueuedSQL_ACL = null;
            return Results;
        }

        private IEnumerable<ExpandoObject> RunSQL(string SQL)
        {
            var Results = new List<ExpandoObject>();


            return Results;
        }
    }
}