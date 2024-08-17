using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Functions.Interop;
using Toucan.Runtime.Memory;

namespace Benchmarks
{

public class MethodInvocationBenchmarks
{
    private readonly TypeRegistry typeRegistry = new TypeRegistry();
    private readonly ForeignLibraryInterfaceVm fli;
    private readonly InteropGetMethod interop;
    private readonly MethodInvoker m_MethodInvoker;
    private readonly TestClass instance;
    private readonly MethodInfo method;

    #region Public

    public MethodInvocationBenchmarks()
    {
        typeRegistry.RegisterType < TestClass >();
        fli = new ForeignLibraryInterfaceVm( typeRegistry );
        interop = new InteropGetMethod( typeRegistry );
        instance = new TestClass();

        Type type = Type.GetType( "Benchmarks.TestClass, Benchmarks" );
        method = type.GetMethod( "TestMethod", new[] { typeof( string ), typeof( int ), typeof( float ) } );

        m_MethodInvoker = ( MethodInvoker ) interop.Call( new DynamicToucanVariable[]
        {
            new DynamicToucanVariable( "TestClass" ),
            new DynamicToucanVariable( "TestMethod" ),
            new DynamicToucanVariable( "string" ),
            new DynamicToucanVariable( "int" ),
            new DynamicToucanVariable( "float" ),
        } );
    }

    [Benchmark]
    public void RunForeignLibraryInterfaceVm()
    {
        fli.Call( new DynamicToucanVariable[]
        {
            new DynamicToucanVariable( instance ),
            new DynamicToucanVariable( "TestMethod" ),
            new DynamicToucanVariable( "Hello" ),
            new DynamicToucanVariable( "string" ),
            new DynamicToucanVariable( 1 ),
            new DynamicToucanVariable( "int" ),
            new DynamicToucanVariable( 2f ),
            new DynamicToucanVariable( "float" ),
        } );
    }

    [Benchmark]
    public void RunReflection()
    {
        method.Invoke( instance, new object[] { "Hello", 1, 2f } );
    }

    [Benchmark]
    public void RunInteropGetMethod()
    {
        m_MethodInvoker.Call( new DynamicToucanVariable[]
        {
            new DynamicToucanVariable( instance ),
            new DynamicToucanVariable( "Hello" ),
            new DynamicToucanVariable( 1 ),
            new DynamicToucanVariable( 2f ),
        } );
    }


    [Benchmark]
    public void RunNative()
    {
        instance.TestMethod( "Hello", 1, 2f );
    }

    #endregion
}

}
