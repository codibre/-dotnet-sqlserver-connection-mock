using System.Data;
using Dapper;
using Xunit;

namespace Codibre.SqlServerMock
{
    public class PaymentPlanRepositoryTest
    {

        private IDbConnection _connection;
        
        record ENTITY {
            public int PK_FIELD;
            public int INT_FIELD;
            public int? INT_FIELD3;
        }

        public PaymentPlanRepositoryTest()
        {
            _connection = new MssqlMockDbConnection();
            _connection.Open();
            _connection.Execute(@"CREATE TABLE Table1(
    PK_FIELD INTEGER,
    INT_FIELD INTEGER,
    INT_FIELD2 INTEGER NOT NULL,
    PRIMARY KEY (PK_FIELD))");
            _connection.Execute(@"CREATE TABLE Table2(
    PK_FIELD2 TEXT,
    TEXT_FIELD TEXT,
    PRIMARY KEY (PK_FIELD2))");
        }

        [Theory]
        [InlineData(1, null)]  
        [InlineData(2, 180)]
        public async Task Should_Work_With_Join_Concat_And_NOLOCK_Combined(
            int intField2,
            int? textFieldConverted
        ) {
            // Arrange
            var pkField = 1;
            await _connection.ExecuteAsync($@"INSERT INTO Table1 (
                PK_FIELD,
                INT_FIELD,
                INT_FIELD2
            ) VALUES (
                1,
                123,
                {intField2}
            )");
            if (textFieldConverted is not null)
                await _connection.ExecuteAsync($@"INSERT INTO Table2(
                    PK_FIELD2,
                    TEXT_FIELD
                ) VALUES (
                    'Test{intField2}Suffix',
                    '{textFieldConverted}'
                )");

            
            // Act
            var result = await _connection.QueryFirstOrDefaultAsync<ENTITY>(
                    @"SELECT
                PP.PK_FIELD,
                PP.INT_FIELD,
                PS.TEXT_FIELD INT_FIELD3
            FROM Table1 PP (NOLOCK)
                LEFT JOIN Table2 PS (NOLOCK)
                ON PK_FIELD2 = CONCAT('Test', INT_FIELD2, 'Suffix')
            WHERE PK_FIELD = @pkField;",
                    new { pkField }
                );

            // Assert
            result.Should().BeEquivalentTo(new {
                PK_FIELD = 1,
                INT_FIELD = 123,
                INT_FIELD3 = textFieldConverted
            });
        }
    }
}
