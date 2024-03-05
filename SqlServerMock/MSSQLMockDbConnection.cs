using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Codibre.SqlServerMock
{
    public class MssQlMockDbCommand : DbCommand, IDbCommand
    {
        private readonly SqliteCommand _comm;
        public override string CommandText {
            get => _comm.CommandText;
            set {
                var parser = new TSql160Parser(false);
                var generator = new Sql160ScriptGenerator();
                var expr = parser.Parse(new StringReader(value), out var list);
                if (list.Count > 0) {
                    _comm.CommandText = value;
                    return;
                }
                try
                {
                    var newValue = new StringBuilder();
                    if (expr is not TSqlScript script)
                        throw new InvalidCastException("Not supported yet!");
                    newValue.Append(string.Join(',', script.Batches.Select(command =>
                        PrepareSelectStatement(command, generator)
                    )));
                    _comm.CommandText = ReplaceFunctions(newValue);
                }
                catch {
                    _comm.CommandText = value;
                }
            }
        }

        private static StringBuilder PrepareSelectStatement(TSqlBatch command, Sql160ScriptGenerator generator)
        {
            var statementBuilder = new StringBuilder();
            foreach (var statement in command.Statements)
            {
                if (
                    statement is not SelectStatement select
                    || select.QueryExpression is not QuerySpecification spec
                ) throw new InvalidCastException("Not supported yet!");

                ParseSelect(generator, statementBuilder, spec);
                PrepareFrom(generator, statementBuilder, spec);
                PrepareWhere(generator, statementBuilder, spec);
                PrepareLimit(statementBuilder, spec);
            }
            return statementBuilder;
        }

        private static void PrepareLimit(StringBuilder statementBuilder, QuerySpecification spec)
        {
            if (spec.TopRowFilter is not null)
            {
                if (spec.TopRowFilter.Expression is IntegerLiteral topValue)
                    statementBuilder.Append(" LIMIT ").Append(topValue.Value);
                else throw new InvalidCastException("Not supported yet!");
            }
        }

        private static void PrepareWhere(Sql160ScriptGenerator generator, StringBuilder statementBuilder, QuerySpecification spec)
        {
            if (spec.WhereClause is not null)
            {
                generator.GenerateScript(spec.WhereClause, out var fragment);
                if (fragment is null) throw new InvalidCastException("Not supported yet!");
                statementBuilder.Append(" ").Append(fragment);
            }

        }

        private static void PrepareFrom(Sql160ScriptGenerator generator, StringBuilder statementBuilder, QuerySpecification spec)
        {
            foreach (var table in spec.FromClause.TableReferences) RemoveNoLock(table);
            generator.GenerateScript(spec.FromClause, out var fragment);
            if (fragment is null) throw new InvalidCastException("Not supported yet!");
            statementBuilder.Append(fragment);
        }

        private static void ParseSelect(Sql160ScriptGenerator generator, StringBuilder statementBuilder, QuerySpecification spec)
        {  
            string fragment;
            statementBuilder
                .Append("SELECT ")
                .Append(string.Join(',', spec.SelectElements.Select(x =>
                {
                    generator.GenerateScript(x, out fragment);
                    return fragment is not null
                        ? fragment
                        : throw new InvalidCastException("Not supported yet!");
                })));
        }

        private static string ReplaceFunctions(StringBuilder newValue)
        {
            var result = Regex.Replace(newValue.ToString(), "CONCAT(\\((?:.+?,?)+?\\))", x => {
                return x.Groups.Values.Skip(1).First().Value.Replace(",", "||");
            });
            return result;
        }

        private static void RemoveNoLock(TableReference? table)
        {
            if (table is NamedTableReference namedTable) namedTable.TableHints.Clear();
            else if (table is QualifiedJoin qualifiedJoin) {
                RemoveNoLock(qualifiedJoin.FirstTableReference);
                RemoveNoLock(qualifiedJoin.SecondTableReference);
            }
        }

        public override int CommandTimeout { get => _comm.CommandTimeout; set => _comm.CommandTimeout = value; }
        public override CommandType CommandType { get => _comm.CommandType; set => _comm.CommandType = value; }
        public IDbConnection? Connection { get => _comm.Connection; set => _comm.Connection = (SqliteConnection?)value; }

        public IDataParameterCollection Parameters => _comm.Parameters;

        public IDbTransaction? Transaction { get => _comm.Transaction; set => _comm.Transaction = (SqliteTransaction?)value; }
        public override UpdateRowSource UpdatedRowSource { get => _comm.UpdatedRowSource; set => _comm.UpdatedRowSource = value; }
        public override bool DesignTimeVisible { get => _comm.DesignTimeVisible; set => _comm.DesignTimeVisible = value; }
        protected override DbConnection? DbConnection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();

        protected override DbTransaction? DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal MssQlMockDbCommand(SqliteCommand comm) {
            _comm = comm;
        }

        public override void Cancel() => _comm.Cancel();

        public IDbDataParameter CreateParameter() => _comm.CreateParameter();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            _comm.Dispose();
        }

        public override int ExecuteNonQuery() => _comm.ExecuteNonQuery();

        public IDataReader ExecuteReader() => _comm.ExecuteReader();

        public IDataReader ExecuteReader(CommandBehavior behavior) => _comm.ExecuteReader(behavior);

        public override object? ExecuteScalar() => _comm.ExecuteScalar();

        public override void Prepare() => _comm.Prepare();

        protected override DbParameter CreateDbParameter() => _comm.CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => _comm.ExecuteReader();
    }

    public class MssqlMockDbConnection : DbConnection
    {
        public SqliteConnection _conn = new SqliteConnection("Filename=:memory:");
        public override string ConnectionString { get => _conn.ConnectionString; set => throw new NotImplementedException(); }

        public override int ConnectionTimeout => 999999999;

        public override string Database => _conn.Database;

        public override ConnectionState State => _conn.State;

        public override string DataSource => _conn.DataSource;

        public override string ServerVersion => _conn.ServerVersion;

        public override void ChangeDatabase(string databaseName) => _conn.ChangeDatabase(databaseName);

        public override void Close() => _conn.Close();
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            _conn.Dispose();
        }

        public override void Open() => _conn.Open();

        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) => _conn.BeginTransaction();

        protected override DbCommand CreateDbCommand() => new MssQlMockDbCommand(_conn.CreateCommand());
    }
}