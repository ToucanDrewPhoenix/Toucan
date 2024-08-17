using System.Collections.Generic;
using Toucan.Compiler;
using Toucan.Runtime;
using Toucan.Runtime.CodeGen;
using Toucan.Symbols;
using Xunit;

namespace UnitTests
{

public class Bar
{
    public int i;
    public float f;
    public double d;

    public int I { get; set; }

    public float F { get; set; }

    public double D { get; set; }
}

public class StatementUnitTests
{
    private ToucanResult ExecStatements( string statements, Dictionary < string, object > externalObjects = null )
    {
        ToucanCompiler compiler = new ToucanCompiler();

        ToucanProgram program = compiler.CompileStatements( statements );

        return program.Run( externalObjects );
    }

    //[Fact]
    //Corner Case
    public void PostfixDecrement()
    {
        ToucanResult result = ExecStatements( "var a = 7; a--;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    //[Fact]
    //Corner Case
    public void PostfixIncrement()
    {
        ToucanResult result = ExecStatements( "var a = 7; a++;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 7, result.ReturnValue.NumberData );
    }

    //[Fact]
    //Corner Case
    public void PostFixReturn()
    {
        // While this code probably doesn't make sense, 
        // i.e. you never return a postix, it is still valid code
        ToucanResult result = ExecStatements( "var a = 1; a++;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    //[Fact]
    //Corner Case
    public void PostFixReturnFromFunction()
    {
        // While this code probably doesn't make sense, 
        // i.e. you never return a postix, it is still valid code
        ToucanResult result = ExecStatements( "function fn(a) { return a++; } fn(1);" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    [Fact]
    public void AddTwoNumbers()
    {
        string statements = @"var c = 1 + 2; c;";

        ToucanResult result = ExecStatements(
            statements,
            new Dictionary < string, object > { { "a", 1 }, { "b", 2 } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void AfterMultiArgumentPostFix()
    {
        string statements = @"
            function foo(a, b){
              return a + b;
            }
            var a = 1;
            var b = 2;
            foo(a++, b++);
            a + b;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void AfterMultiPostFix()
    {
        string statements = @"
            var a = 1;
            var b = 2;
            a++; 
            b++;
            a + b;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticAdditionAssignment()
    {
        string statements = @"
            class foo {
                var x = 5;
                var y = 2;
            }

            var a = 1;
            var b = new foo();

            a += 2;
            b.x += 2;
            b[""y""] += 3;

            bar.i += 1;
            bar.f += 2;
            bar.d += 3;

            bar.I += 4;
            bar.F += 5;
            bar.D += 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar();

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 15, result.ReturnValue.NumberData );
        Assert.Equal( 1, bar.i );
        Assert.Equal( 2f, bar.f );
        Assert.Equal( 3d, bar.d );
        Assert.Equal( 4, bar.I );
        Assert.Equal( 5f, bar.F );
        Assert.Equal( 6d, bar.D );
    }

    [Fact]
    public void ArithmeticAssignmentAssignment()
    {
        string statements = @"var a = 1;
            var b = a += 2;
            b;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ArithmeticBitwiseAndAssignment()
    {
        string statements = @"
            class foo {
                var x = 4;
                var y = 7;
            }

            var a = 8;
            var b = new foo();

            a &= 3;

            b.x &= 2;
            b[""y""] &= 3;

            bar.i &= 1;
            bar.f &= 2;
            bar.d &= 3;

            bar.I &= 4;
            bar.F &= 5;
            bar.D &= 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar
        {
            i = 1,
            f = 1f,
            d = 2d,
            I = 2,
            F = 4f,
            D = 5d
        };

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 3, result.ReturnValue.NumberData );
        Assert.Equal( 1, bar.i );
        Assert.Equal( 0f, bar.f );
        Assert.Equal( 2d, bar.d );
        Assert.Equal( 0, bar.I );
        Assert.Equal( 4f, bar.F );
        Assert.Equal( 4d, bar.D );
    }

    [Fact]
    public void ArithmeticBitwiseOrAssignment()
    {
        string statements = @"
            class foo {
                var x = 4;
                var y = 7;
            }

            var a = 8;
            var b = new foo();

            a |= 3;

            b.x |= 2;
            b[""y""] |= 3;

            bar.i |= 1;
            bar.f |= 2;
            bar.d |= 3;

            bar.I |= 4;
            bar.F |= 5;
            bar.D |= 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar
        {
            i = 1,
            f = 1f,
            d = 2d,
            I = 2,
            F = 4f,
            D = 5d
        };

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 24, result.ReturnValue.NumberData );
        Assert.Equal( 1, bar.i );
        Assert.Equal( 3f, bar.f );
        Assert.Equal( 3d, bar.d );
        Assert.Equal( 6, bar.I );
        Assert.Equal( 5f, bar.F );
        Assert.Equal( 7d, bar.D );
    }

    [Fact]
    public void ArithmeticBitwiseShiftLeftAssignment()
    {
        string statements = @"
            class foo {
                var x = 4;
                var y = 7;
            }

            var a = 8;
            var b = new foo();

            a <<= 3;

            b.x <<= 2;
            b[""y""] <<= 3;

            bar.i <<= 1;
            bar.f <<= 2;
            bar.d <<= 3;

            bar.I <<= 4;
            bar.F <<= 5;
            bar.D <<= 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar
        {
            i = 1,
            f = 1f,
            d = 2d,
            I = 2,
            F = 4f,
            D = 5d
        };

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 136, result.ReturnValue.NumberData );
        Assert.Equal( 2, bar.i );
        Assert.Equal( 4f, bar.f );
        Assert.Equal( 16d, bar.d );
        Assert.Equal( 32, bar.I );
        Assert.Equal( 128f, bar.F );
        Assert.Equal( 320d, bar.D );
    }

    [Fact]
    public void ArithmeticBitwiseShiftRightAssignment()
    {
        string statements = @"
            class foo {
                var x = 4;
                var y = 7;
            }

            var a = 8;
            var b = new foo();

            a >>= 3;

            b.x >>= 2;
            b[""y""] >>= 3;

            bar.i >>= 1;
            bar.f >>= 2;
            bar.d >>= 3;

            bar.I >>= 4;
            bar.F >>= 5;
            bar.D >>= 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar
        {
            i = 256,
            f = 256,
            d = 256,
            I = 256,
            F = 256,
            D = 256
        };

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 2, result.ReturnValue.NumberData );
        Assert.Equal( 128, bar.i );
        Assert.Equal( 64f, bar.f );
        Assert.Equal( 32d, bar.d );
        Assert.Equal( 16, bar.I );
        Assert.Equal( 8f, bar.F );
        Assert.Equal( 4d, bar.D );
    }

    [Fact]
    public void ArithmeticDivisionAssignment()
    {
        string statements = @"
            class foo {
                var x = 10;
                var y = 6;
            }

            var a = 15;
            var b = new foo();

            a /= 3;

            b.x /= 2;
            b[""y""] /= 3;

            bar.i /= 1;
            bar.f /= 2;
            bar.d /= 3;

            bar.I /= 4;
            bar.F /= 5;
            bar.D /= 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar
        {
            i = 60,
            f = 60f,
            d = 60d,
            I = 60,
            F = 60f,
            D = 60d
        };

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 12, result.ReturnValue.NumberData );
        Assert.Equal( 60, bar.i );
        Assert.Equal( 30f, bar.f );
        Assert.Equal( 20d, bar.d );
        Assert.Equal( 15, bar.I );
        Assert.Equal( 12f, bar.F );
        Assert.Equal( 10d, bar.D );
    }

    [Fact]
    public void ArithmeticModuloAssignment()
    {
        string statements = @"
            class foo {
                var x = 7;
                var y = 6;
            }

            var a = 17;
            var b = new foo();

            a %= 3;

            b.x %= 2;
            b[""y""] %= 3;

            bar.i %= 1;
            bar.f %= 2;
            bar.d %= 3;

            bar.I %= 4;
            bar.F %= 5;
            bar.D %= 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar
        {
            i = 10,
            f = 10f,
            d = 10d,
            I = 10,
            F = 10f,
            D = 10d
        };

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 3, result.ReturnValue.NumberData );
        Assert.Equal( 0, bar.i );
        Assert.Equal( 0f, bar.f );
        Assert.Equal( 1d, bar.d );
        Assert.Equal( 2, bar.I );
        Assert.Equal( 0f, bar.F );
        Assert.Equal( 4d, bar.D );
    }

    [Fact]
    public void ArithmeticMultiplicationAssignment()
    {
        string statements = @"
            class foo {
                var x = 5;
                var y = 2;
            }

            var a = 5;
            var b = new foo();

            a *= 3;

            b.x *= 2;
            b[""y""] *= 3;

            bar.i *= 1;
            bar.f *= 2;
            bar.d *= 3;

            bar.I *= 4;
            bar.F *= 5;
            bar.D *= 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar
        {
            i = 2,
            f = 2f,
            d = 2d,
            I = 2,
            F = 2f,
            D = 2d
        };

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 31, result.ReturnValue.NumberData );
        Assert.Equal( 2, bar.i );
        Assert.Equal( 4f, bar.f );
        Assert.Equal( 6d, bar.d );
        Assert.Equal( 8, bar.I );
        Assert.Equal( 10f, bar.F );
        Assert.Equal( 12d, bar.D );
    }

    [Fact]
    public void ArithmeticSubtractionAssignment()
    {
        string statements = @"
            class foo {
                var x = 5;
                var y = 2;
            }

            var a = 5;
            var b = new foo();

            a -= 3;

            b.x -= 2;
            b[""y""] -= 3;

            bar.i -= 1;
            bar.f -= 2;
            bar.d -= 3;

            bar.I -= 4;
            bar.F -= 5;
            bar.D -= 6;

            a + b.x + b.y;
";

        Bar bar = new Bar();

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 4, result.ReturnValue.NumberData );
        Assert.Equal( -1, bar.i );
        Assert.Equal( -2f, bar.f );
        Assert.Equal( -3d, bar.d );
        Assert.Equal( -4, bar.I );
        Assert.Equal( -5f, bar.F );
        Assert.Equal( -6d, bar.D );
    }

    [Fact]
    public void ArrayInitializers()
    {
        string statements = @"
            function foo() {
                return 5;
            }

            var a = [ 10, 20, 30, ""Hello"", foo(), foo ];";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void Assignment()
    {
        string statements = @"
            class foo {
                var x = 5;
                var y = 2;
            }

            var a = 1;
            var b = new foo();

            b.x = 2;
            b[""y""] = 3;

            bar.i = 1;
            bar.f = 2;
            bar.d = 3;

            bar.I = 4;
            bar.F = 5;
            bar.D = 6;

            a + b.x + b.y;
    ";

        Bar bar = new Bar();

        ToucanResult result = ExecStatements( statements, new Dictionary < string, object > { { "bar", bar } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );

        Assert.Equal( 6, result.ReturnValue.NumberData );
        Assert.Equal( 1, bar.i );
        Assert.Equal( 2f, bar.f );
        Assert.Equal( 3d, bar.d );
        Assert.Equal( 4, bar.I );
        Assert.Equal( 5f, bar.F );
        Assert.Equal( 6d, bar.D );
    }

    [Fact]
    public void ClassConstructorArguments()
    {
        string statements = @"class TestClass
            {
                var x = 5;

                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);

            a.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 150, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassConstructorMultipleArguments()
    {
        string statements = @"class TestClass
            {
                var x = 0;
                var y = 0;
                var z = 0;

                function TestClass( a, b, c )
                {
                    x = a;
                    y = b;
                    z = c;
                }
            }

            var a = new TestClass(1, 3, 7);

            a.x + a.y + a.z;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 11, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassExpressionArgument()
    {
        string statements = @"class Foo {
              var x = 5;
            }

            function bar(foo) {
                return foo.x;
            }

            var a = bar(new Foo());
            
            a;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassExpressionAssignment()
    {
        string statements = @"class Foo {
              var x = 5;
            }

            class Bar {
               var foo;
            }

            var a = new Bar();

            a.foo = new Foo();

            a.foo.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassFields()
    {
        string statements = @"class TestClass
            {
                var x = 5;
            }

            var a = new TestClass();

            a.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassFieldsAssignment()
    {
        string statements = @"class TestClass
            {
                var x;
                var y;
            }

            var a = new TestClass();

            function foo () {
               return 5;
            }

            a.x = foo();
            a.y = ""five"";

            a.x + a.y;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "5five", result.ReturnValue.StringData );
    }

    [Fact]
    public void ClassFieldsInitializers()
    {
        string statements = @"class TestClass
            {
                var x;
                var y;
            }

            function foo () {
               return 5;
            }

            var a = new TestClass() {
                x = ""Hello"",
                y = 10,
                z = foo()
            }

            a.x + a.y + a.z;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello105", result.ReturnValue.StringData );
    }

    [Fact]
    public void ClassFunctionCall()
    {
        string statements = @"class TestClass
            {
                var x = 5;

                function TestClass()
                {
                }

                function foo() {
                    x++;
                }

                function bar() {
                    x--;
                }
            }

            var a = new TestClass();

            a.foo();
            a.foo();
            a.bar();
            
            a.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ClassInstanceAsArgument()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            function TestFunction(n)
            {
                n.x = 10;
            }

            var a = new TestClass(150);

            TestFunction(a);

            a.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DictionaryInitializers()
    {
        string statements = @"
            function foo() {
                return 5;
            }

            var a = {
                x: 10,
                y: 20,
                z: 30,
                a: ""Hello"",
                b: foo(),
                c: foo 
            };";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void DynamicMemberFunctionCall()
    {
        string statements = @"class TestClass
            {
                var x = 5;

                function TestClass()
                {
                }

                function foo() {
                    x++;
                }

                function bar() {
                    x--;
                }
            }

            var a = new TestClass();

            a[""foo""]();
            
            a.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DynamicMemberGet()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);
            
            a[""x""];";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 150, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DynamicMemberSet()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);
            
            a[""x""] = 10;

            a.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void DynamicMemberSetWithVariable()
    {
        string statements = @"class TestClass
            {
                var x = 5;
                function TestClass(n)
                {
                    x = n;
                }
            }

            var a = new TestClass(150);
            var propName = ""x"";

            a[propName] = 10;

            a.x;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ExternalValues()
    {
        string statements = @"var c = a + b; c;";

        ToucanResult result = ExecStatements(
            statements,
            new Dictionary < string, object > { { "a", 1 }, { "b", 2 } } );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForEverLoopBreak()
    {
        ToucanResult result = ExecStatements( "var i = 0; for (;;) { i++; if (i == 10) { break; } } i;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForInitLoopBreak()
    {
        ToucanResult result = ExecStatements( "var a = 0; for (var i = 0;;) { i++; a++; if (i == 10) { break; } } a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoop()
    {
        ToucanResult result = ExecStatements( "var j = 0; for (var i = 0; i < 10; i++) { j++; } j;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoopBreak()
    {
        ToucanResult result =
            ExecStatements( "var j = 0; for (var i = 0; i < 10; i++) { j++; if (j == 6) { break; } } j;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoopDecrement()
    {
        ToucanResult result = ExecStatements( "var j = 0; for (var i = 10; i > 0; i--) { j++; } j;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForLoopScope()
    {
        ToucanResult result = ExecStatements( "var j = 0; for (var i = 0; i < 10; i++) { j++; i++; } j;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void ForMultipleVarAndIterator()
    {
        ToucanResult result = ExecStatements( "var a = 0; for (var i = 0, j = 0; i < 10; i++, j++ ) { a += j + i; } a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 90, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionCall()
    {
        ToucanResult result = ExecStatements( "function foo(a) { a = a + 1; } foo(1);" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void FunctionCallNoReturnShouldThrowErrorWhenAssigned()
    {
        // Or assign null or undefined?
        ToucanResult result = ExecStatements( "function foo(a) { a = a + 1; } var a = 1; a = foo(a);" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void FunctionCallUseBeforeDeclarationShouldThrowException()
    {
        Assert.Throws < ToucanSymbolTableException >(
            () =>
            {
                ToucanResult result = ExecStatements(
                    "var a = foo(1); function foo(a) { return a + bar(a); } function bar(a) { return a + a; } a;" );
            } );
    }

    [Fact]
    public void FunctionCallWith3Arguments()
    {
        ToucanResult result = ExecStatements( "function foo(a, b, c) { return a + b + c; } var a = foo(1,2,3); a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionCallWithMultipleArguments()
    {
        ToucanResult result = ExecStatements( "function foo(a, b) { return a + b; } var a = foo(1, 2); a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionCallWithReturn()
    {
        ToucanResult result = ExecStatements( "function foo(a) { a = a + 1; return a; } var a = foo(1); a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 2, result.ReturnValue.NumberData );
    }

    [Fact]
    public void FunctionDefinition()
    {
        ToucanResult result = ExecStatements( "function foo(a) { a = a + 1; }" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void FunctionDefinitionWithReturn()
    {
        ToucanResult result = ExecStatements( "function foo(a) { a = a + 1; return a; }" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void If()
    {
        ToucanResult result = ExecStatements( "var a = true; var b = 0; if (a) { b = 123; } b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseFalse()
    {
        ToucanResult result = ExecStatements( "var a = false; var b = 0; if (a) { b = 123; } else { b = 456; } b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfElse()
    {
        ToucanResult result = ExecStatements(
            "var a = \"fee\"; var b = \"beer\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } else { c = 111; } c;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 111, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfFalseThenFalse()
    {
        ToucanResult result = ExecStatements(
            "var a = \"fee\"; var b = \"beer\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } c;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 789, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfFalseThenTrue()
    {
        ToucanResult result = ExecStatements(
            "var a = \"fee\"; var b = \"bar\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } c;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseIfTrue()
    {
        ToucanResult result = ExecStatements(
            "var a = \"foo\"; var b = \"bar\"; var c = 789; if (a == \"foo\") { c = 123; } else if (b == \"bar\") { c = 456; } c;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseTrue()
    {
        ToucanResult result = ExecStatements( "var a = true; var b = 0; if (a) { b = 123; } else { b = 456; } b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseWithoutBracesFalse()
    {
        ToucanResult result = ExecStatements( "var a = true; var b = 0; if (a) b = 123; else b = 456; b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfElseWithoutBracesTrue()
    {
        ToucanResult result = ExecStatements( "var a = true; var b = 0; if (a) b = 123; else b = 456; b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void IfWithoutBraces()
    {
        ToucanResult result = ExecStatements( "var a = true; var b = 0; if (a) b = 123; b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void MultiArgumentPostFix()
    {
        string statements = @"
            function foo(a, b){
              return a + b;
            }
            var a = 1;
            var b = 2;
            foo(a++, b++);";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void NestedFunctionCall()
    {
        ToucanResult result = ExecStatements(
            "function bar(a) { return a + a; } function foo(a) { return a + bar(a); } var a = foo(1); a;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void NestedFunctionCallReverseDeclaration()
    {
        Assert.Throws < ToucanSymbolTableException >(
            () =>
            {
                ExecStatements(
                    "function foo(a) { return a + bar(a); } function bar(a) { return a + a; } var a = foo(1); a;" );
            } );
    }

    [Fact]
    public void NonStringInterpolation()
    {
        ToucanResult result = ExecStatements(
            @"
            var Hello = ""Hello""; 
            var World = ""World""; 
            var s = ""\${Hello} \${World}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "${Hello} ${World}", result.ReturnValue.StringData );
    }

    [Fact]
    public void PostFixArgumentMultiple()
    {
        string statements = @"
            function foo(a){
              // nop
            }
            var a = 1;
            foo(a++);
            foo(a++);
            foo(a++);
            foo(a++);
            a;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixAssignment()
    {
        string statements = @"var a = 1;
            var b = a++;
            b;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 1, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostfixDecrementAsArgument()
    {
        ToucanResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(a++); b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixInBinaryOperationAssignment()
    {
        string statements = @"var a = 1;
            var b = a++ + 2;
            b;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 3, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostfixIncrementAsArgument()
    {
        ToucanResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(a++); b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PostFixMultiple()
    {
        string statements = @"var a = 1;
            a++;
            a++;
            a++;
            a++;
            a;";

        ToucanResult result = ExecStatements( statements );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixDecrement()
    {
        ToucanResult result = ExecStatements( "var a = 7; --a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 6, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixDecrementAsArgument()
    {
        ToucanResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(--a); b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 455, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixIncrement()
    {
        ToucanResult result = ExecStatements( "var a = 7; ++a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 8, result.ReturnValue.NumberData );
    }

    [Fact]
    public void PrefixIncrementAsArgument()
    {
        ToucanResult result = ExecStatements( "function fn(a) { return a; } var a = 456; var b = fn(++a); b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 457, result.ReturnValue.NumberData );
    }

    [Fact]
    public void StringInterpolatioInsideStringInterpolation()
    {
        ToucanResult result = ExecStatements(
            @"
            var x = ""Inception"";
            var s = ""${""${x}"" + "" "" + ""World""}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Inception World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationAsFunctionArgument()
    {
        ToucanResult result = ExecStatements(
            @"

            function bar(b) {
                return b + "" World"";
            }

            var Hello = ""Hello"";
            var s = bar(""${Hello}"");
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationFunctionExpressions()
    {
        ToucanResult result = ExecStatements(
            @"
            function foo(a) {
                return a + 1;
            }

            function bar(b) {
                return b + 2;
            }

            var X = 200; 
            var Y = 100; 
            var s = ""${foo(X)}:${bar(Y)}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "201:102", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationNumericExpressions()
    {
        ToucanResult result = ExecStatements(
            @"
            var X = 200; 
            var Y = 100; 
            var s = ""${X}:${Y}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "200:100", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationStringExpressions()
    {
        ToucanResult result = ExecStatements(
            @"
            var Hello = ""Hello""; 
            var World = ""World""; 
            var s = ""${Hello} ${World}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolationStringLiteralsWithClosingCurlyBrace()
    {
        ToucanResult result = ExecStatements(
            @"
            var s = ""${""Hello}""} ${""{World""}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello} {World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolatioWithMultiLine()
    {
        ToucanResult result = ExecStatements(
            @"
            var s = ""${""Hello"" 
                    + "" "" 
                    + ""World""}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolatioWithStringLiteralConcatenation()
    {
        ToucanResult result = ExecStatements(
            @"
            var s = ""${""Hello"" + "" "" + ""World""}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void StringInterpolatioWithStringLiterals()
    {
        ToucanResult result = ExecStatements(
            @"
            var s = ""${""Hello""} ${""World""}"";
            s;" );

        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( "Hello World", result.ReturnValue.StringData );
    }

    [Fact]
    public void TernaryFalse()
    {
        ToucanResult result = ExecStatements( "var a = false; var b = a ? 123 : 456; b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 456, result.ReturnValue.NumberData );
    }

    [Fact]
    public void TernaryTrue()
    {
        ToucanResult result = ExecStatements( "var a = true; var b = a ? 123 : 456; b;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 123, result.ReturnValue.NumberData );
    }

    [Fact]
    public void VariableDeclaration()
    {
        ToucanResult result = ExecStatements( "var a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
    }

    [Fact]
    public void VariableDefintition()
    {
        ToucanResult result = ExecStatements( "var a = 12345; a;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void WhileFalseNeverExecutes()
    {
        ToucanResult result = ExecStatements( "var i = 12345; while(false) { i++; } i;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 12345, result.ReturnValue.NumberData );
    }

    [Fact]
    public void WhileLoop()
    {
        ToucanResult result = ExecStatements( "var i = 0; while(i < 10) { i++; } i;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 10, result.ReturnValue.NumberData );
    }

    [Fact]
    public void WhileTrueLoopBreak()
    {
        ToucanResult result = ExecStatements( "var i = 0; while(true) { i++; if (i == 5) { break; } } i;" );
        Assert.Equal( ToucanVmInterpretResult.InterpretOk, result.InterpretResult );
        Assert.Equal( 5, result.ReturnValue.NumberData );
    }
}

}
