using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TestEase.Helpers;
using TestEase.LibraryItems;
// ReSharper disable UnusedMember.Global

namespace TestEase.LibrarItemDictionaries
{
    public class SqlItemDictionary : BaseItemDictionary, IItemDictionary
    {
        public IDictionary<string, StringBuilder> QueuedSql = new Dictionary<string, StringBuilder>();

        private IDictionary<string, string> _connections = new Dictionary<string, string>();

        public SqlItemDictionary() { }

        public SqlItemDictionary(IDictionary<string, string> connections)
        {
            if (connections == null) return;

            foreach (var connectionsKey in connections.Keys)
            {
                _connections.Add(connectionsKey, connections[connectionsKey]);
            }
        }

        public string FileExtension => ".sql";

        public ItemFileType FileType => ItemFileType.Sql;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connections">Collection of connections, keyed by dbConnectionString alias (this alias is used in scripts to signify which dbConnectionString to use)</param>
        public void SetupConnections(IDictionary<string, string> connections)
        {
            _connections = connections;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptKey"></param>
        /// <param name="replacementValues"></param>
        public SqlItemDictionary QueueSql(string scriptKey, IDictionary<string, object> replacementValues = null)
        {
            if (_connections.Keys.Count <= 0) throw new ArgumentException("This flavor can only be run if you pre-configure the connections.");

            var sql = this[scriptKey].LibraryItemText;
            var scripts = Regex.Split(sql, $"--DbType \\s*=\\s* ({string.Join("|", _connections.Select(x => x.Key))})",
                RegexOptions.IgnorePatternWhitespace);

            for (var i = 1; i < scripts.Length; i += 2)
            {
                var dbType = scripts[i];
                var sqlCode = scripts[i + 1];

                QueueSql(dbType, sqlCode, replacementValues);
            }

            return this;
        }

        public dynamic ExecuteSql()
        {
            var results = new List<ExpandoObject>();

            foreach (var queuedSqlKey in QueuedSql.Keys)
            {
                if (QueuedSql[queuedSqlKey].Length <= 0) continue;

                results.AddRange(RunSql(queuedSqlKey, QueuedSql[queuedSqlKey].ToString()));

                QueuedSql[queuedSqlKey].Clear();
            }

            return results;
        }

        private void QueueSql(string connectionName, string sqlCode, IDictionary<string, object> replacementValues = null)
        {
            if (!QueuedSql.ContainsKey(connectionName)) QueuedSql.Add(connectionName, new StringBuilder());

            var parsedSql = replacementValues == null? sqlCode:ItemParser.Parse(sqlCode,replacementValues, this);

            QueuedSql[connectionName].AppendLine(parsedSql);
        }

        private IEnumerable<ExpandoObject> RunSql(string dbConnectionString, string sql)
        {
            var connection = new SqlConnection(dbConnectionString);
            var results = new List<ExpandoObject>();

            try
            {
                void RunIt(string code)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = code;
                    using (var dr = cmd.ExecuteReader())
                    {
                        do
                        {
                            while (dr.Read())
                            {
                                dynamic result = new ExpandoObject();

                                for (var i = 0; i < dr.FieldCount; i++)
                                {
                                    var columnName = dr.GetName(i);
                                    var dict = (IDictionary<string, object>)result;
                                    dict[columnName] = dr[columnName];
                                }

                                results.Add(result);
                            }
                        } while (dr.NextResult());
                    }
                }

                foreach (var batch in sql.Split(new[] { "GO" }, StringSplitOptions.None))
                {
                    RunIt(batch);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException)
                {
                    throw new InvalidOperationException(ex.InnerException.Message + "\n\n" + sql);
                }
                throw;
            }

            return results;
        }
    }
}