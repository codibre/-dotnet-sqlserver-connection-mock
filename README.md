[![Actions Status](https://github.com/Codibre/dotnet-sqlserver-connection-mock/workflows/build/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions)
[![Actions Status](https://github.com/Codibre/dotnet-sqlserver-connection-mock/workflows/test/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions)
[![Actions Status](https://github.com/Codibre/dotnet-sqlserver-connection-mock/workflows/lint/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions)
[![benchmark](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions/workflows/benchmark.yml/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions/workflows/benchmark.yml)
[![Test Coverage](https://api.codeclimate.com/v1/badges/d97994e24ee6b162c626/test_coverage)](https://codeclimate.com/github/codibre/dotnet-sqlserver-connection-mock/test_coverage)
[![Maintainability](https://api.codeclimate.com/v1/badges/d97994e24ee6b162c626/maintainability)](https://codeclimate.com/github/codibre/dotnet-sqlserver-connection-mock/maintainability)

SqlServer mocking library for unit tests that uses Sqlite in-memory under the hood.

## Why

Although one can argue that having a in memory real database is not unit testing, the results of using such approach for repositories unit tests is way better than mocking it using a IEnumerable, specially when working with Dapper.

## How to use it

First, import the application namespace:

```c#
using Codibre.SqlServerMock
```

Now, create your connection using MssqlMockDbConnection.
You'll need to create every database entity using SqlLite commands in your unit tests.
For now, this library will only work with a limited number of SELECT queries.

Here's a list of different idiomatics already treated here:

* TOP => LIMIT

## License

Licensed under [MIT](https://en.wikipedia.org/wiki/MIT_License).
