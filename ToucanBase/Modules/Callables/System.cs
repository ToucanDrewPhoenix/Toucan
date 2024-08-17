using Toucan.Runtime;
using Toucan.Runtime.Functions;
using Toucan.Runtime.Functions.ForeignInterface;
using Toucan.Runtime.Functions.Interop;

namespace Toucan.Modules.Callables
{

public static class SystemModule
{
    #region Public

    public static void RegisterSystemModuleCallables( this ToucanVm ToucanVm, TypeRegistry typeRegistry = null )
    {
        ToucanVm.RegisterCallable( "GetConstructor", new InteropGetConstructor( typeRegistry ) );
        ToucanVm.RegisterCallable( "GetStaticMember", new InteropGetStaticMember( typeRegistry ) );
        ToucanVm.RegisterCallable( "GetStaticMethod", new InteropGetStaticMethod( typeRegistry ) );
        ToucanVm.RegisterCallable( "GetMethod", new InteropGetMethod( typeRegistry ) );
        ToucanVm.RegisterCallable( "GetStaticClass", new InteropGetStaticClass( typeRegistry ) );
        ToucanVm.RegisterCallable( "GetGenericMethod", new InteropGetGenericMethod( typeRegistry ) );
        ToucanVm.RegisterCallable( "Print", new PrintFunctionVm() );
        ToucanVm.RegisterCallable( "PrintLine", new PrintLineFunctionVm() );
    }

    #endregion
}

}
