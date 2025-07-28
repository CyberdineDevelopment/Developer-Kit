using System;
using System.Data.SqlClient;
using System.Linq.Expressions;
using FractalDataWorks.Connections.Data.Commands;
using FractalDataWorks.Connections.SqlServer;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.SqlServer.Tests;

public class SqlCommandTranslatorTests
{
    private readonly ILogger<SqlCommandTranslator> _logger;
    private readonly SqlCommandTranslator _sut;
    
    public SqlCommandTranslatorTests()
    {
        _logger = Substitute.For<ILogger<SqlCommandTranslator>>();
        _sut = new SqlCommandTranslator(_logger);
    }
    
    [Fact]
    public void TranslateQueryWithAllFieldsShouldGenerateSelectStar()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldBe("SELECT * FROM [Users]");
        sqlCommand.Parameters.Count.ShouldBe(0);
    }
    
    [Fact]
    public void TranslateQueryWithSpecificFieldsShouldGenerateSelectColumns()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .Select("Id", "Name", "Email")
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldBe("SELECT [Id], [Name], [Email] FROM [Users]");
    }
    
    [Fact]
    public void TranslateQueryWithWhereClauseShouldGenerateWhereStatement()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .Where(u => u.Active == true)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldContain("WHERE [Active] = @p0");
        sqlCommand.Parameters.Count.ShouldBe(1);
        sqlCommand.Parameters["@p0"].Value.ShouldBe(true);
    }
    
    [Fact]
    public void TranslateQueryWithIdentifierShouldGenerateWhereId()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .WithId(42)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldContain("WHERE [Id] = @Id");
        sqlCommand.Parameters["@Id"].Value.ShouldBe(42);
    }
    
    [Fact]
    public void TranslateQueryWithOrderByShouldGenerateOrderByClause()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .OrderBy(u => u.Name)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldContain("ORDER BY [Name]");
        sqlCommand.CommandText.ShouldNotContain("DESC");
    }
    
    [Fact]
    public void TranslateQueryWithOrderByDescendingShouldGenerateOrderByDesc()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .OrderBy(u => u.CreatedDate, descending: true)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldContain("ORDER BY [CreatedDate] DESC");
    }
    
    [Fact]
    public void TranslateQueryWithPaginationShouldGenerateOffsetFetch()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .OrderBy(u => u.Id)
            .Skip(20)
            .Take(10)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldContain("OFFSET 20 ROWS");
        sqlCommand.CommandText.ShouldContain("FETCH NEXT 10 ROWS ONLY");
    }
    
    [Fact]
    public void TranslateQueryWithPaginationButNoOrderByShouldAddDefaultOrderBy()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .Skip(10)
            .Take(5)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldContain("ORDER BY (SELECT NULL)");
        sqlCommand.CommandText.ShouldContain("OFFSET 10 ROWS");
        sqlCommand.CommandText.ShouldContain("FETCH NEXT 5 ROWS ONLY");
    }
    
    [Fact]
    public void TranslateInsertShouldGenerateInsertStatement()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 1, 
            Name = "Test User", 
            Email = "test@example.com",
            Active = true
        };
        
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "Users")
            .WithEntity(entity)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldStartWith("INSERT INTO [Users]");
        sqlCommand.CommandText.ShouldContain("VALUES");
        sqlCommand.Parameters["@Id"].Value.ShouldBe(1);
        sqlCommand.Parameters["@Name"].Value.ShouldBe("Test User");
        sqlCommand.Parameters["@Email"].Value.ShouldBe("test@example.com");
        sqlCommand.Parameters["@Active"].Value.ShouldBe(true);
    }
    
    [Fact]
    public void TranslateInsertWithSpecificAttributesShouldOnlyIncludeThoseColumns()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 1, 
            Name = "Test", 
            Email = "test@example.com",
            Active = true
        };
        
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "Users")
            .WithEntity(entity)
            .OnlyAttributes("Name", "Email")
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldBe("INSERT INTO [Users] ([Name], [Email]) VALUES (@Name, @Email)");
        sqlCommand.Parameters.Count.ShouldBe(2);
        sqlCommand.Parameters.Contains("@Id").ShouldBeFalse();
        sqlCommand.Parameters.Contains("@Active").ShouldBeFalse();
    }
    
    [Fact]
    public void TranslateInsertWithMultipleEntitiesShouldThrowNotSupported()
    {
        // Arrange
        var entities = new[] 
        { 
            new TestEntity { Id = 1 }, 
            new TestEntity { Id = 2 } 
        };
        
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "Users")
            .WithEntities(entities)
            .Build();
        
        // Act & Assert
        Should.Throw<NotSupportedException>(() => _sut.Translate(command))
            .Message.ShouldContain("bulk copy");
    }
    
    [Fact]
    public void TranslateUpdateWithValuesShouldGenerateUpdateStatement()
    {
        // Arrange
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "Users")
            .Where(u => u.Id == 1)
            .SetValue("Name", "Updated Name")
            .SetValue("Email", "updated@example.com")
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldStartWith("UPDATE [Users] SET");
        sqlCommand.CommandText.ShouldContain("[Name] = @Name");
        sqlCommand.CommandText.ShouldContain("[Email] = @Email");
        sqlCommand.CommandText.ShouldContain("WHERE");
        sqlCommand.Parameters["@Name"].Value.ShouldBe("Updated Name");
        sqlCommand.Parameters["@Email"].Value.ShouldBe("updated@example.com");
    }
    
    [Fact]
    public void TranslateUpdateWithActionShouldThrowNotSupported()
    {
        // Arrange
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "Users")
            .Where(u => u.Id == 1)
            .Set(u => u.Name = "Updated")
            .Build();
        
        // Act & Assert
        Should.Throw<NotSupportedException>(() => _sut.Translate(command))
            .Message.ShouldContain("UpdateValues instead of UpdateAction");
    }
    
    [Fact]
    public void TranslateDeleteWithWhereClauseShouldGenerateDeleteStatement()
    {
        // Arrange
        var command = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .Where(u => u.Active == false)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldStartWith("DELETE FROM [Users]");
        sqlCommand.CommandText.ShouldContain("WHERE");
        sqlCommand.CommandText.ShouldContain("[Active] = @p0");
        sqlCommand.Parameters["@p0"].Value.ShouldBe(false);
    }
    
    [Fact]
    public void TranslateDeleteWithIdentifierShouldGenerateDeleteById()
    {
        // Arrange
        var command = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .WithId(42)
            .Build();
        
        // Act
        var sqlCommand = _sut.Translate(command);
        
        // Assert
        sqlCommand.CommandText.ShouldBe("DELETE FROM [Users] WHERE [Id] = @Id");
        sqlCommand.Parameters["@Id"].Value.ShouldBe(42);
    }
    
    [Fact]
    public void TranslateDeleteWithoutCriteriaShouldThrow()
    {
        // Arrange
        var command = new DeleteCommand<TestEntity>
        {
            DataStore = "SqlServer",
            Container = "TestDB",
            Record = "Users"
        };
        
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => _sut.Translate(command))
            .Message.ShouldContain("must have WhereClause or Identifier");
    }
    
    [Fact]
    public void CanTranslateShouldReturnTrueForValidCommand()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "Users")
            .Build();
        
        // Act
        var result = _sut.CanTranslate(command);
        
        // Assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void CanTranslateShouldReturnFalseForInvalidCommand()
    {
        // Arrange
        var command = new QueryCommand<TestEntity>
        {
            DataStore = "",
            Record = ""
        };
        
        // Act
        var result = _sut.CanTranslate(command);
        
        // Assert
        result.ShouldBeFalse();
    }
    
    [Fact]
    public void ParseSelectStatementShouldReturnQueryCommand()
    {
        // Arrange
        var sqlCommand = new SqlCommand("SELECT Id, Name FROM Users");
        
        // Act
        var command = _sut.Parse(sqlCommand);
        
        // Assert
        command.ShouldBeOfType<QueryCommand<object>>();
        command.DataStore.ShouldBe("SqlServer");
        command.Record.ShouldBe("Users");
        var queryCommand = (QueryCommand<object>)command;
        queryCommand.Attributes.ShouldBe(new[] { "Id", "Name" });
    }
    
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}