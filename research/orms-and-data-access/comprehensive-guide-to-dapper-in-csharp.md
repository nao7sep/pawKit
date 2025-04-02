<!-- 2025-03-31T03:00:06Z -->

# Comprehensive Guide to Dapper in C#

Dapper is a lightweight and high-performance micro-ORM (Object-Relational Mapper) for .NET. It extends the IDbConnection interface with additional methods for executing queries and mapping results to objects in a simple and efficient manner.

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Basic Usage](#basic-usage)
4. [Advanced Usage](#advanced-usage)
5. [Best Practices](#best-practices)
6. [Sample Code](#sample-code)
7. [Conclusion](#conclusion)

## Introduction

Dapper was developed by the team at Stack Exchange. It is designed to be minimalistic, working directly on top of ADO.NET, and provides a way to easily map query results to objects. While it does not provide full change tracking like more extensive ORMs, its focus on speed and simplicity makes it ideal for performance-critical applications.

## Installation

To include Dapper in your project, you can install it via NuGet. Run the following command in the Package Manager Console:

```shell
Install-Package Dapper
```

Alternatively, you can add it through the NuGet Package Manager in Visual Studio.

## Basic Usage

Dapper works as an extension to the IDbConnection interface. This enables you to perform database operations without the overhead of a full ORM. Below is a simple example to query data from a SQL Server database.

```csharp
using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;

namespace DapperExample
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class Program
    {
        private const string connectionString = "your_connection_string_here";

        public static void Main()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT Id, Username, Email FROM Users";
                IEnumerable<User> users = db.Query<User>(sqlQuery);
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.Id}, Username: {user.Username}, Email: {user.Email}");
                }
            }
        }
    }
}
```

## Advanced Usage

Dapper supports advanced scenarios such as multi-mapping, stored procedures, and transactions.

### Multi-Mapping

Multi-mapping allows you to map data from different tables to multiple objects. For example:

```csharp
using (IDbConnection db = new SqlConnection(connectionString))
{
    string sql = @"
    SELECT
        o.Id, o.OrderDate, o.UserId,
        u.Id, u.Username, u.Email
    FROM Orders o
    INNER JOIN Users u ON o.UserId = u.Id";

    var orderDictionary = new Dictionary<int, Order>();
    var orders = db.Query<Order, User, Order>(
        sql,
        (order, user) =>
        {
            if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
            {
                currentOrder = order;
                orderDictionary.Add(currentOrder.Id, currentOrder);
            }
            currentOrder.User = user;
            return currentOrder;
        },
        splitOn: "Id");

    foreach (var order in orders)
    {
        Console.WriteLine($"Order ID: {order.Id}, Order Date: {order.OrderDate}, User: {order.User.Username}");
    }
}
```

Assume appropriate Order class exists with a property "User" of type User.

### Stored Procedures

Dapper makes executing stored procedures straightforward. For instance:

```csharp
using (IDbConnection db = new SqlConnection(connectionString))
{
    var parameters = new DynamicParameters();
    parameters.Add("@UserId", 1);
    var user = db.QueryFirstOrDefault<User>(
        "GetUserById",
        parameters,
        commandType: CommandType.StoredProcedure
    );
    Console.WriteLine($"ID: {user.Id}, Username: {user.Username}");
}
```

## Best Practices

- **Parameterized Queries**: Always use parameters to pass user input to queries to avoid SQL injection attacks.
- **Connection Management**: Use `using` statements to ensure connections are properly opened and closed.
- **Minimize Data Transfer**: Query only the columns you need to reduce memory overhead.
- **Handle Exceptions**: Implement error handling to gracefully manage database errors.
- **Optimize Performance**: Leverage Dapper's light abstraction to optimize data access in performance-sensitive applications.

## Sample Code

Below is a complete example that demonstrates how to use Dapper in a console application:

```csharp
using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;

namespace DapperConsoleApp
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    class Program
    {
        private const string connectionString = "your_connection_string_here";

        static void Main(string[] args)
        {
            Console.WriteLine("Dapper Sample Application");

            try
            {
                using (IDbConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    IEnumerable<User> users = connection.Query<User>("SELECT Id, Username, Email FROM Users");

                    foreach (var user in users)
                    {
                        Console.WriteLine($"User: {user.Username} - Email: {user.Email}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
```

## Conclusion

Dapper provides a fast, flexible, and straightforward method for handling data access in C#. It allows developers to execute SQL queries directly while leveraging the power of ADO.NET. By following best practices, you can build efficient and secure data access layers in your .NET applications.
