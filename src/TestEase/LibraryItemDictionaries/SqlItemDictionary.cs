namespace TestEase.LibraryItemDictionaries
{
    // ReSharper disable UnusedMember.Global
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Dynamic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using TestEase.Helpers;
    using TestEase.LibraryItems;

    /// <inheritdoc />
    /// <summary>
    /// The sql item dictionary.
    /// </summary>
    public class SqlItemDictionary : BaseItemDictionary
    {
        /// <summary>
        /// Sql that is queued and ready to be executed
        /// </summary>
        public readonly IDictionary<string, StringBuilder> GetQueuedSql = new Dictionary<string, StringBuilder>();

        /// <summary>
        /// Connection mappings used to execute sql scripts
        /// </summary>
        private readonly IDictionary<string, string> connections = new Dictionary<string, string>();

        /// <inheritdoc />
        public override string FileExtension => ".sql";

        /// <inheritdoc />
        public override ItemFileType FileType => ItemFileType.Sql;

        /// <summary>
        /// Executes all queued sql
        /// </summary>
        /// <returns>
        /// Dynamic object containing the result sets of each queued sql statement
        /// </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when a connection is not found</exception>
        /// <exception cref="ArgumentException">No sql is currently queued</exception>
        public dynamic Execute()
        {
            if (this.GetQueuedSql.Count == 0)
            {
                throw new ArgumentException("No sql is currently queued");
            }

            var results = new List<ExpandoObject>();

            foreach (var queuedSqlKey in this.GetQueuedSql.Keys)
            {
                if (!this.connections.ContainsKey(queuedSqlKey.ToUpper()))
                {
                    throw new KeyNotFoundException($"Connections does not contain a key for {queuedSqlKey.ToUpper()}. Keys present: {string.Join(", ", this.connections.Keys.ToList())}");
                }

                results.AddRange(RunSql(this.connections[queuedSqlKey.ToUpper()], this.GetQueuedSql[queuedSqlKey].ToString()));

                this.GetQueuedSql[queuedSqlKey].Clear();
            }

            return results;
        }

        /// <summary>
        /// Gets the currently configured connections
        /// </summary>
        /// <returns>
        /// The collection of connections
        /// </returns>
        public IDictionary<string, string> GetConnections()
        {
            return this.connections;
        }

        /// <summary>
        /// QueueLibraryItem a sql statement to be executed
        /// </summary>
        /// <param name="scriptKey">
        /// Key of the script to queue.
        /// </param>
        /// <param name="replacementValues">
        /// Any replacement values that should be supplied to the script
        /// </param>
        /// <returns>
        /// The <see cref="SqlItemDictionary"/>.
        /// </returns>
        /// <exception cref="ArgumentException">This flavor can only be run if you pre-configure the connections.</exception>
        /// <exception cref="RegexMatchTimeoutException">A time-out occurred. For more information about time-outs, see the Remarks section.</exception>
        /// <exception cref="System.IO.FileNotFoundException">The file cannot be found, such as when <see cref="System.IO.FileMode"/> is FileMode.Truncate or FileMode.Open, and the file specified by path does not exist. The file must already exist in these modes. </exception>
        /// <exception cref="System.IO.IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by path already exists, occurred. -or-The stream has been closed.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
        /// <exception cref="UnauthorizedAccessException">The requested is not permitted by the operating system for the specified path, such as when <see cref="System.IO.FileAccess"/> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
        /// <exception cref="KeyNotFoundException">Library item requested does not exist in the collection.</exception>
        public SqlItemDictionary QueueLibraryItem(string scriptKey, IDictionary<string, object> replacementValues = null)
        {
            if (this.connections.Keys.Count <= 0)
            {
                throw new ArgumentException("This flavor can only be run if you pre-configure the connections.");
            }

            if (!this.ContainsKey(scriptKey))
            {
                throw new KeyNotFoundException($"Library item does not exist in the collection: {scriptKey}");
            }

            var sql = this[scriptKey].LibraryItemText;

            sql = ItemParser.Parse(sql, replacementValues, this);

            var scriptSplitter = string.Join("|", this.connections.Select(x => x.Key));
            var scripts = Regex.Split(
                sql,
                $"--DbType \\s*=\\s* ({scriptSplitter})",
                RegexOptions.IgnorePatternWhitespace);

            if (scripts.Length <= 1)
            {
                throw new ArgumentException($"DB type split did not work correctly. Expression used: --DbType \\\\s*=\\\\s* ({scriptSplitter})");
            }

            for (var i = 1; i < scripts.Length; i += 2)
            {
                var connectionType = scripts[i].ToUpper();
                var sqlCode = scripts[i + 1];

                this.Queue(connectionType, sqlCode);
            }

            return this;
        }

        /// <summary>
        /// Queues a SQL statement
        /// </summary>
        /// <param name="connectionName">
        /// Connection name that should be used
        /// </param>
        /// <param name="sqlToQueue">
        /// Sql script/statement to queue
        /// </param>
        /// <returns>
        /// The <see cref="SqlItemDictionary"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The connections have not been configured
        /// </exception>
        public SqlItemDictionary QueueSql(string connectionName, string sqlToQueue)
        {
            if (this.connections.Keys.Count <= 0)
            {
                throw new ArgumentException("This flavor can only be run if you pre-configure the connections.");
            }

            this.Queue(connectionName, sqlToQueue);

            return this;
        }

        /// <summary>
        /// Overwrites the current connections with the supplied collection of connections
        /// </summary>
        /// <param name="connectionsToAdd">
        /// Keyed collection of connections to use where the key is the name/alias and the value is the connection string
        /// </param>
        public void SetupConnections(IDictionary<string, string> connectionsToAdd)
        {
            this.connections.Clear();

            foreach (var key in connectionsToAdd.Keys)
            {
                this.connections.Add(key.ToUpper(), connectionsToAdd[key]);
            }
        }

        /// <summary>
        /// Wrapper around actual sql connection and execution
        /// </summary>
        /// <param name="sqlText">
        /// Sql to be executed
        /// </param>
        /// <param name="connection">
        /// Connection object to be used
        /// </param>
        /// <returns>
        /// List of dynamic objects representing the data sets returned
        /// </returns>
        private static IEnumerable<ExpandoObject> RunIt(string sqlText, SqlConnection connection)
        {
            var cmd = connection.CreateCommand();
            var results = new List<ExpandoObject>();

            cmd.CommandText = sqlText;
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
                }
                while (dr.NextResult());
            }

            return results;
        }

        /// <summary>
        /// Runs the supplied sql
        /// </summary>
        /// <param name="connectionString">
        /// Connection string to be used
        /// </param>
        /// <param name="sql">
        /// Sql to be executed
        /// </param>
        /// <returns>
        /// List of dynamic objects
        /// </returns>
        private static IEnumerable<ExpandoObject> RunSql(string connectionString, string sql)
        {
            var connection = new SqlConnection(connectionString);
            var results = new List<ExpandoObject>();

            connection.Open();
            using (connection)
            {
                foreach (var batch in sql.Split(new[] { "GO" }, StringSplitOptions.None))
                {
                    results.AddRange(RunIt(batch, connection));
                }
            }

            return results;
        }

        /// <summary>
        /// Queues a sql string to be executed
        /// </summary>
        /// <param name="connectionName">
        /// Name value of the connection
        /// </param>
        /// <param name="sqlCode">
        /// The sql that will be executed
        /// </param>
        private void Queue(
            string connectionName,
            string sqlCode)
        {
            if (!this.GetQueuedSql.ContainsKey(connectionName))
            {
                this.GetQueuedSql.Add(connectionName, new StringBuilder());
            }

            this.GetQueuedSql[connectionName].AppendLine(sqlCode);
        }
    }
}