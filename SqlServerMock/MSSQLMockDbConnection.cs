
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Collections;

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
                if (list.Count > 0) _comm.CommandText = value;
                else {
                    try {
                        var newValue = new StringBuilder();
                        if (expr is TSqlScript script) {
                            newValue.Append(string.Join(',', script.Batches.Select(command => {
                                var statementBuilder = new StringBuilder();
                                foreach (var statement in command.Statements) {
                                    if (
                                        statement is SelectStatement select
                                        && select.QueryExpression is QuerySpecification spec
                                    ) {
                                        string fragment;
                                        statementBuilder.Append("SELECT ")
                                            .Append(string.Join(',', spec.SelectElements.Select(x => {
                                                generator.GenerateScript(x, out fragment);
                                                return fragment is not null ? fragment : throw new InvalidCastException("Not supported yet!");
                                            })));
                                        generator.GenerateScript(spec.FromClause, out fragment);
                                        if (fragment is null) throw new InvalidCastException("Not supported yet!");
                                        statementBuilder.Append(fragment);
                                        if (spec.WhereClause is not null) {
                                            generator.GenerateScript(spec.WhereClause, out fragment);
                                            if (fragment is null) throw new InvalidCastException("Not supported yet!");
                                            statementBuilder.Append(fragment);
                                        }
                                        if (spec.TopRowFilter is not null) {
                                            if (spec.TopRowFilter.Expression is IntegerLiteral topValue)
                                                statementBuilder.Append(" LIMIT ").Append(topValue.Value);
                                            else throw new InvalidCastException("Not supported yet!");
                                        }
                                    } else throw new InvalidCastException("Not supported yet!");
                                }
                                return statementBuilder;
                            })));
                        } else throw new InvalidCastException("Not supported yet!");
                        _comm.CommandText = newValue.ToString();
                    } catch {
                        _comm.CommandText = value;
                    }
                }
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

    public class MssqlMockDbConnection : IDbConnection
    {
        public SqliteConnection _conn = new SqliteConnection("Filename=:memory:");
        public string ConnectionString { get => _conn.ConnectionString; set => throw new NotImplementedException(); }

        public int ConnectionTimeout => 999999999;

        public string Database => _conn.Database;

        public ConnectionState State => _conn.State;

        public IDbTransaction BeginTransaction() => _conn.BeginTransaction();

        public IDbTransaction BeginTransaction(System.Data.IsolationLevel il) => _conn.BeginTransaction(il);

        public void ChangeDatabase(string databaseName) => _conn.ChangeDatabase(databaseName);

        public void Close() => _conn.Close();

        public IDbCommand CreateCommand() => new MssQlMockDbCommand(_conn.CreateCommand());
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            _conn.Dispose();
        }

        public void Open() => _conn.Open();
    }
}