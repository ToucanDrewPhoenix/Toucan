using Toucan.Compiler;
using Toucan.Runtime;
using Toucan.Runtime.CodeGen;
using Toucan.Runtime.Memory;
using Xunit;

namespace UnitTests
{

public class ExpressionUnitTests
{
    private ToucanResult ExecExpression( string expression ) //TODO: Rewrite these tests
    {
        ToucanCompiler compiler = new ToucanCompiler();

        ToucanProgram program = compiler.CompileExpression( expression );

        return program.Run();
    }

    [Fact]
    public void ArithmeticAddNumbers()
    {
        ToucanResult result = ExecExpression( "1 + 1" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 2, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticDivideNumbers()
    {
        ToucanResult result = ExecExpression( "1 / 2" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 0.5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticMultipleAddition()
    {
        ToucanResult result = ExecExpression( "1 + 2 + 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticMultiplyNumbers()
    {
        ToucanResult result = ExecExpression( "4 * 4" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 16, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedence()
    {
        ToucanResult result = ExecExpression( "2 * 5 - 4 / 2 + 6" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 14, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceDivideBeforeSubtract()
    {
        ToucanResult result = ExecExpression( "1 - 3 / 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 0, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceMultiplyBeforeAdd()
    {
        ToucanResult result = ExecExpression( "1 + 2 * 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticOperatorPrecedenceMultiplyBeforeAddOuterFirst()
    {
        ToucanResult result = ExecExpression( "1 * 2 + 3 * 4" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 14, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticParenthesesPrecedenceGroupByAdditionSubtraction()
    {
        ToucanResult result = ExecExpression( "2 * (5 - 4) / (2 + 6)" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 0.25, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticParenthesesPrecedenceGroupByMultiplicationDivision()
    {
        ToucanResult result = ExecExpression( "(2 * 5) - (4 / 2) + 6" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 14, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticSubtractNumbers()
    {
        ToucanResult result = ExecExpression( "6 - 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseAnd()
    {
        ToucanResult result = ExecExpression( "5 & 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseCompliment()
    {
        ToucanResult result = ExecExpression( "~127" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( -128, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseLeftShift()
    {
        ToucanResult result = ExecExpression( "2 << 4" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 32, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseOr()
    {
        ToucanResult result = ExecExpression( "5 | 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseRightShift()
    {
        ToucanResult result = ExecExpression( "64 >> 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 8, result.ReturnValue.NumberData );
    }

    [Fact]
    public void BitwiseXor()
    {
        ToucanResult result = ExecExpression( "5 ^ 3" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void Equal()
    {
        {
            ToucanResult result = ExecExpression( "3 == 3" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "3 == 4" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void GreaterThan()
    {
        {
            ToucanResult result = ExecExpression( "2 > 1" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "1 > 2" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void GreaterThanOrEqual()
    {
        {
            ToucanResult result = ExecExpression( "1 >= 1" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "1 >= 0" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "2 >= 1" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "1 >= 2" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LessThan()
    {
        {
            ToucanResult result = ExecExpression( "1 < 2" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "2 < 1" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LessThanOrEqual()
    {
        {
            ToucanResult result = ExecExpression( "1 <= 1" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "0 <= 1" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "1 <= 2" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "2 <= 1" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LogicalAnd()
    {
        {
            ToucanResult result = ExecExpression( "true && false" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "false && true" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "false && false" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "true && true" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LogicalNot()
    {
        {
            ToucanResult result = ExecExpression( "!true" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "!false" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void LogicalOr()
    {
        {
            ToucanResult result = ExecExpression( "true || false" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "false || true" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "false || false" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "true || true" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void Negate()
    {
        ToucanResult result = ExecExpression( "-127" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( -127, result.ReturnValue.NumberData );
    }

    [Fact]
    public void NotEqual()
    {
        {
            ToucanResult result = ExecExpression( "3 != 3" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.False, result.ReturnValue.DynamicType );
        }

        {
            ToucanResult result = ExecExpression( "3 != 4" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( DynamicVariableType.True, result.ReturnValue.DynamicType );
        }
    }

    [Fact]
    public void String()
    {
        {
            ToucanResult result = ExecExpression( "\"Hello World\"" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( "Hello World", result.ReturnValue.StringData );
        }
    }

    [Fact]
    public void StringConcatenation()
    {
        {
            ToucanResult result = ExecExpression( "\"Hello\" + \" \" + \"World\"" );
            Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
            Assert.Equal( "Hello World", result.ReturnValue.StringData );
        }
    }
}

}
