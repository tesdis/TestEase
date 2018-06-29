namespace TestEase.LibraryItemDictionaries
{
    // ReSharper disable UnusedMember.Global
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;

    using TestEase.Helpers;
    using TestEase.LibraryItems;

    /// <summary>
    /// The sql item dictionary.
    /// </summary>
    public class SqlItemDictionary : BaseItemDictionary
    {
        /// <summary>
        /// Sql that is queued and ready to be executed
        /// </summary>
        public readonly IDictionary<string, StringBuilder> QueuedSql = new Dictionary<string, StringBuilder>();

        /// <summary>
        /// Connection mappings used to execute sql scripts
        /// </summary>
        private IDictionary<string, string> connections = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlItemDictionary"/> class.
        /// </summary>
        public SqlItemDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlItemDictionary"/> class.
        /// </summary>
        /// <param name="connections">
        /// Keyed collection of connections to use where the key is the name/alias and the value is the connection string
        /// </param>
        public SqlItemDictionary(IDictionary<string, string> connections)
        {
            if (connections == null)
            {
                return;
            }

            foreach (var connectionsKey in connections.Keys)
            {
                this.connections.Add(connectionsKey, connections[connectionsKey]);
            }
        }

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
        public dynamic Execute()
        {
            var results = new List<ExpandoObject>();

            foreach (var queuedSqlKey in this.QueuedSql.Keys)
            {
                if (this.QueuedSql[queuedSqlKey].Length <= 0)
                {
                    continue;
                }

                results.AddRange(this.RunSql(queuedSqlKey, this.QueuedSql[queuedSqlKey].ToString()));

                this.QueuedSql[queuedSqlKey].Clear();
            }

            return results;
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
        /// <exception cref="FileNotFoundException">The file cannot be found, such as when <see cref="FileMode"/> is FileMode.Truncate or FileMode.Open, and the file specified by path does not exist. The file must already exist in these modes. </exception>
        /// <exception cref="IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by path already exists, occurred. -or-The stream has been closed.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
        /// <exception cref="UnauthorizedAccessException">The requested is not permitted by the operating system for the specified path, such as when <see cref="FileAccess"/> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
        public SqlItemDictionary QueueLibraryItem(string scriptKey, IDictionary<string, object> replacementValues = null)
        {
            if (this.connections.Keys.Count <= 0)
            {
                throw new ArgumentException("This flavor can only be run if you pre-configure the connections.");
            }

            var sql = this[scriptKey].LibraryItemText;
            sql = ItemParser.Parse(sql, replacementValues, this);

            var scripts = Regex.Split(
                sql,
                $"--DbType \\s*=\\s* ({string.Join("|", this.connections.Select(x => x.Key))})",
                RegexOptions.IgnorePatternWhitespace);

            for (var i = 1; i < scripts.Length; i += 2)
            {
                var connectionType = scripts[i];
                var sqlCode = scripts[i + 1];

                this.Queue(connectionType, sqlCode, replacementValues);
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
        public void SetupConnections(IDictionary<string, string> connectionsToAdd) => this.connections = connectionsToAdd;

        /// <summary>
        /// Get the raw connection string for a given key
        /// </summary>
        /// <param name="key">
        /// Key of the connection to retrieve
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The connection does not exist for the given key
        /// </exception>
        public string GetConnection(string key)
        {
            if (this.connections.ContainsKey(key))
            {
                return this.connections[key];
            }

            throw new ArgumentException($"Connection was not found for key {key}");
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
        /// <param name="replacementValues">
        /// Any replacement values that should - name/value
        /// </param>
        private void Queue(
            string connectionName,
            string sqlCode,
            IDictionary<string, object> replacementValues = null)
        {
            if (!this.QueuedSql.ContainsKey(connectionName))
            {
                this.QueuedSql.Add(connectionName, new StringBuilder());
            }

            var parsedSql = ItemParser.Parse(sqlCode, replacementValues, this);

            this.QueuedSql[connectionName].AppendLine(parsedSql);
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
        private IEnumerable<ExpandoObject> RunSql(string connectionString, string sql)
        {
            var connection = new SqlConnection(connectionString);
            var results = new List<ExpandoObject>();

            try
            {
                foreach (var batch in sql.Split(new[] { "GO" }, StringSplitOptions.None))
                {
                    results.AddRange(this.RunIt(batch, connection));
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException)
                {
                    throw new InvalidOperationException(ex.InnerException.Message + "\n\n" + sql, ex);
                }

                throw;
            }

            return results;
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
        private List<ExpandoObject> RunIt(string sqlText, SqlConnection connection)
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
    }
}