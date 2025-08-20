using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

/// <summary>
/// Tests for ExpressionTranslator to ensure proper LINQ expression to SQL translation.
/// </summary>
public sealed class ExpressionTranslatorTests
{
    private readonly ITestOutputHelper _output;

    public ExpressionTranslatorTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    private static ExpressionTranslator CreateTranslator(out List<SqlParameter> parameters)
    {
        parameters = new List<SqlParameter>();
        var parameterCounter = 0;
        return new ExpressionTranslator(ref parameterCounter, parameters);
    }

    [Fact]
    public void ShouldTranslateConstantExpression()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        Expression<Func<bool>> expr = () => 42 == 42;
        var constantExpr = Expression.Constant(123);

        // Act
        var sql = translator.Translate(constantExpr);

        // Assert
        sql.ShouldBe("@p0");
        parameters.Count.ShouldBe(1);
        parameters[0].ParameterName.ShouldBe("@p0");
        parameters[0].Value.ShouldBe(123);

        _output.WriteLine($"Constant expression translated: {sql}");
        _output.WriteLine($"Parameter: {parameters[0].ParameterName} = {parameters[0].Value}");
    }

    [Fact]
    public void ShouldTranslateMemberExpressionForParameter()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var memberExpr = Expression.Property(param, nameof(TestEntity.Id));

        // Act
        var sql = translator.Translate(memberExpr);

        // Assert
        sql.ShouldBe("[Id]");
        parameters.Count.ShouldBe(0);

        _output.WriteLine($"Member expression translated: {sql}");
    }

    [Fact]
    public void ShouldTranslateBinaryExpressionEqual()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var memberExpr = Expression.Property(param, nameof(TestEntity.Id));
        var constantExpr = Expression.Constant(123);
        var binaryExpr = Expression.Equal(memberExpr, constantExpr);

        // Act
        var sql = translator.Translate(binaryExpr);

        // Assert
        sql.ShouldBe("([Id] = @p0)");
        parameters.Count.ShouldBe(1);
        parameters[0].ParameterName.ShouldBe("@p0");
        parameters[0].Value.ShouldBe(123);

        _output.WriteLine($"Binary expression (equal) translated: {sql}");
    }

    [Theory]
    [InlineData(ExpressionType.Equal, "=")]
    [InlineData(ExpressionType.NotEqual, "!=")]
    [InlineData(ExpressionType.LessThan, "<")]
    [InlineData(ExpressionType.LessThanOrEqual, "<=")]
    [InlineData(ExpressionType.GreaterThan, ">")]
    [InlineData(ExpressionType.GreaterThanOrEqual, ">=")]
    public void ShouldTranslateComparisonOperators(ExpressionType nodeType, string expectedOperator)
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var memberExpr = Expression.Property(param, nameof(TestEntity.Id));
        var constantExpr = Expression.Constant(100);
        var binaryExpr = Expression.MakeBinary(nodeType, memberExpr, constantExpr);

        // Act
        var sql = translator.Translate(binaryExpr);

        // Assert
        sql.ShouldBe($"([Id] {expectedOperator} @p0)");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe(100);

        _output.WriteLine($"Comparison operator {nodeType} translated to: {sql}");
    }

    [Fact]
    public void ShouldTranslateAndAlsoExpression()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var idMember = Expression.Property(param, nameof(TestEntity.Id));
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        
        var condition1 = Expression.Equal(idMember, Expression.Constant(123));
        var condition2 = Expression.Equal(nameMember, Expression.Constant("Test"));
        var andExpr = Expression.AndAlso(condition1, condition2);

        // Act
        var sql = translator.Translate(andExpr);

        // Assert
        sql.ShouldBe("(([Id] = @p0) AND ([Name] = @p1))");
        parameters.Count.ShouldBe(2);
        parameters[0].Value.ShouldBe(123);
        parameters[1].Value.ShouldBe("Test");

        _output.WriteLine($"AndAlso expression translated: {sql}");
    }

    [Fact]
    public void ShouldTranslateOrElseExpression()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var idMember = Expression.Property(param, nameof(TestEntity.Id));
        var statusMember = Expression.Property(param, nameof(TestEntity.IsActive));
        
        var condition1 = Expression.Equal(idMember, Expression.Constant(1));
        var condition2 = Expression.Equal(statusMember, Expression.Constant(true));
        var orExpr = Expression.OrElse(condition1, condition2);

        // Act
        var sql = translator.Translate(orExpr);

        // Assert
        sql.ShouldBe("(([Id] = @p0) OR ([IsActive] = @p1))");
        parameters.Count.ShouldBe(2);
        parameters[0].Value.ShouldBe(1);
        parameters[1].Value.ShouldBe(true);

        _output.WriteLine($"OrElse expression translated: {sql}");
    }

    [Fact]
    public void ShouldTranslateStringContainsMethod()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var methodCall = Expression.Call(nameMember, containsMethod!, Expression.Constant("test"));

        // Act
        var sql = translator.Translate(methodCall);

        // Assert
        sql.ShouldBe("[Name] LIKE @p0");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe("%test%");

        _output.WriteLine($"String.Contains method translated: {sql}");
        _output.WriteLine($"Parameter value: '{parameters[0].Value}'");
    }

    [Fact]
    public void ShouldTranslateStringStartsWithMethod()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        var methodCall = Expression.Call(nameMember, startsWithMethod!, Expression.Constant("prefix"));

        // Act
        var sql = translator.Translate(methodCall);

        // Assert
        sql.ShouldBe("[Name] LIKE @p0");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe("prefix%");

        _output.WriteLine($"String.StartsWith method translated: {sql}");
        _output.WriteLine($"Parameter value: '{parameters[0].Value}'");
    }

    [Fact]
    public void ShouldTranslateStringEndsWithMethod()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        var methodCall = Expression.Call(nameMember, endsWithMethod!, Expression.Constant("suffix"));

        // Act
        var sql = translator.Translate(methodCall);

        // Assert
        sql.ShouldBe("[Name] LIKE @p0");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe("%suffix");

        _output.WriteLine($"String.EndsWith method translated: {sql}");
        _output.WriteLine($"Parameter value: '{parameters[0].Value}'");
    }

    [Fact]
    public void ShouldTranslateComplexExpression()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var idMember = Expression.Property(param, nameof(TestEntity.Id));
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        var isActiveMember = Expression.Property(param, nameof(TestEntity.IsActive));

        // Build: (Id > 10 AND Name.StartsWith("Test")) OR IsActive = true
        var condition1 = Expression.GreaterThan(idMember, Expression.Constant(10));
        
        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        var condition2 = Expression.Call(nameMember, startsWithMethod!, Expression.Constant("Test"));
        
        var condition3 = Expression.Equal(isActiveMember, Expression.Constant(true));
        
        var andExpr = Expression.AndAlso(condition1, condition2);
        var finalExpr = Expression.OrElse(andExpr, condition3);

        // Act
        var sql = translator.Translate(finalExpr);

        // Assert
        sql.ShouldBe("((([Id] > @p0) AND ([Name] LIKE @p1)) OR ([IsActive] = @p2))");
        parameters.Count.ShouldBe(3);
        parameters[0].Value.ShouldBe(10);
        parameters[1].Value.ShouldBe("Test%");
        parameters[2].Value.ShouldBe(true);

        _output.WriteLine($"Complex expression translated: {sql}");
        foreach (var parameter in parameters)
        {
            _output.WriteLine($"  {parameter.ParameterName}: {parameter.Value}");
        }
    }

    [Fact]
    public void ShouldHandleNullConstant()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        var nullConstant = Expression.Constant(null, typeof(string));
        var equalExpr = Expression.Equal(nameMember, nullConstant);

        // Act
        var sql = translator.Translate(equalExpr);

        // Assert
        sql.ShouldBe("([Name] = @p0)");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe(DBNull.Value);

        _output.WriteLine($"Null constant translated: {sql}");
    }

    [Fact]
    public void ShouldHandleMemberExpressionWithComplexAccess()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var value = "test value";
        var constantExpr = Expression.Constant(value);
        var lengthProperty = Expression.Property(constantExpr, "Length");

        // Act
        var sql = translator.Translate(lengthProperty);

        // Assert
        sql.ShouldBe("@p0");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe(value.Length);

        _output.WriteLine($"Complex member expression translated: {sql}");
    }

    [Fact]
    public void ShouldHandleMethodCallWithDynamicInvocation()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var mathMaxMethod = typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) });
        var methodCall = Expression.Call(mathMaxMethod!, Expression.Constant(5), Expression.Constant(10));

        // Act
        var sql = translator.Translate(methodCall);

        // Assert
        sql.ShouldBe("@p0");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe(10); // Max(5, 10) = 10

        _output.WriteLine($"Method call with dynamic invocation translated: {sql}");
    }

    [Fact]
    public void ShouldThrowForUnsupportedExpressionType()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var idMember = Expression.Property(param, nameof(TestEntity.Id));
        var constantExpr = Expression.Constant(10);
        
        // Create an unsupported expression type (Add)
        var addExpr = Expression.Add(idMember, constantExpr);

        // Act & Assert
        var exception = Should.Throw<NotSupportedException>(() => translator.Translate(addExpr));
        exception.Message.ShouldContain("Expression type Add is not supported");

        _output.WriteLine($"Unsupported expression type correctly throws: {exception.Message}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldHandleEmptyStringConstants(string value)
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        var constantExpr = Expression.Constant(value);
        var equalExpr = Expression.Equal(nameMember, constantExpr);

        // Act
        var sql = translator.Translate(equalExpr);

        // Assert
        sql.ShouldBe("([Name] = @p0)");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe(value);

        _output.WriteLine($"Empty/whitespace string constant translated: {sql}");
    }

    [Theory]
    [InlineData(typeof(int), 42)]
    [InlineData(typeof(string), "test")]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(DateTime), "2023-06-15")]
    [InlineData(typeof(decimal), 123.45)]
    public void ShouldHandleVariousDataTypes(Type dataType, object value)
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var actualValue = dataType == typeof(DateTime) ? DateTime.Parse((string)value) : 
                         dataType == typeof(decimal) ? Convert.ToDecimal(value) :
                         value;
        var constantExpr = Expression.Constant(actualValue, dataType);

        // Act
        var sql = translator.Translate(constantExpr);

        // Assert
        sql.ShouldBe("@p0");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe(actualValue);

        _output.WriteLine($"Data type {dataType.Name} with value '{actualValue}' translated correctly");
    }

    [Fact]
    public void ShouldHandleParameterCounterCorrectly()
    {
        // Arrange
        var parameters = new List<SqlParameter>();
        var parameterCounter = 5; // Start from 5
        var translator = new ExpressionTranslator(ref parameterCounter, parameters);
        
        var constantExpr1 = Expression.Constant(100);
        var constantExpr2 = Expression.Constant(200);

        // Act
        translator.Translate(constantExpr1);
        translator.Translate(constantExpr2);

        // Assert
        parameters.Count.ShouldBe(2);
        parameters[0].ParameterName.ShouldBe("@p5");
        parameters[1].ParameterName.ShouldBe("@p6");

        _output.WriteLine($"Parameter counter handled correctly: {parameters[0].ParameterName}, {parameters[1].ParameterName}");
    }

    [Fact]
    public void ConstructorShouldThrowWhenParametersIsNull()
    {
        // Arrange
        var parameterCounter = 0;

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new ExpressionTranslator(ref parameterCounter, null!));
        exception.ParamName.ShouldBe("parameters");

        _output.WriteLine("Constructor correctly throws ArgumentNullException when parameters is null");
    }

    [Fact]
    public void ShouldHandleUnicodeStrings()
    {
        // Arrange
        var translator = CreateTranslator(out var parameters);
        var unicodeValue = "тест 测试 🚀";
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var nameMember = Expression.Property(param, nameof(TestEntity.Name));
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var methodCall = Expression.Call(nameMember, containsMethod!, Expression.Constant(unicodeValue));

        // Act
        var sql = translator.Translate(methodCall);

        // Assert
        sql.ShouldBe("[Name] LIKE @p0");
        parameters.Count.ShouldBe(1);
        parameters[0].Value.ShouldBe($"%{unicodeValue}%");

        _output.WriteLine($"Unicode string translated correctly: {parameters[0].Value}");
    }

    // Test helper class
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}