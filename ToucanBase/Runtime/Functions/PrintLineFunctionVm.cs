using System;
using System.Collections.Generic;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions
{

public class PrintLineFunctionVm : IToucanVmCallable
{
    #region Public

    public object Call( DynamicToucanVariable[] arguments )
    {
        int argumentsCount = arguments.Length;

        for ( int i = 0; i < argumentsCount; i++ )
        {
            Console.WriteLine( arguments[i].ToString() );
        }

        return null;
    }

    #endregion
}

}
