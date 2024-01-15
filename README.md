[![Actions Status](https://github.com/Codibre/dotnet-sqlserver-connection-mock/workflows/build/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions)
[![Actions Status](https://github.com/Codibre/dotnet-sqlserver-connection-mock/workflows/test/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions)
[![Actions Status](https://github.com/Codibre/dotnet-sqlserver-connection-mock/workflows/lint/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions)
[![benchmark](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions/workflows/benchmark.yml/badge.svg)](https://github.com/Codibre/dotnet-sqlserver-connection-mock/actions/workflows/benchmark.yml)
[![Test Coverage](https://api.codeclimate.com/v1/badges/e1533fd2a8f3fb66fd27/test_coverage)](https://codeclimate.com/github/codibre/dotnet-sqlserver-connection-mock/test_coverage)
[![Maintainability](https://api.codeclimate.com/v1/badges/e1533fd2a8f3fb66fd27/maintainability)](https://codeclimate.com/github/codibre/dotnet-sqlserver-connection-mock/maintainability)

SqlServer mocking library for unit tests that uses Sqlite in-memory under the hood.

## Why

Although one can argue that having a in memory real database is not unit testing, the results of using such approach for repositories unit tests is way better than mocking it using a IEnumerable, specially when working with Dapper.

## How to use it

First, import the application namespace:

```c#
using Codibre.SqlServerMock
```

Now, create your connection using **MssqlMockDbConnection**.
You'll need to create every table entity using SqlLite commands in your unit tests.
For now, this library will only work with a limited number of SELECT queries.

Here's a list of idiomatics differences already treated here:

* TOP => LIMIT;
* (NOLOCK) => Removed as Sqlite doesn't have anything like that;
* CONCAT Function => Little hacky but replaced using regex by || operator. Need to be refactored by using parsed TSqlFragment;  

## What comes next?

Anything that comes in the way of unit testing sql server repositories can be converted here, but be aware, that this library has no intention at all of reflecting every possible SQL Server functionality using in-memory SqlLite: it is absolutely impossible!
But we believe is commom sense that the most commom used ones are expected to be possible to convert.
Although we know that not everything will be possible to cover, having a least a good part of the repository layer unit tested, can really put the quality of a project to another level!

## License

Licensed under [MIT](https://en.wikipedia.org/wiki/MIT_License).
