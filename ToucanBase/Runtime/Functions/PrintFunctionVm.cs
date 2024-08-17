using System;
using System.Collections.Generic;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime.Functions
{

public class PrintFunctionVm : IToucanVmCallable
{
    #region Public

    public object Call( DynamicToucanVariable[] arguments )
    {
        if ( arguments[0].DynamicType != DynamicVariableType.Null )
        {
            int arraySize = arguments.Length;

            for ( int i = 0; i < arraySize; i++ )
            {
                Console.Write( arguments[i].ToString() );
            }
        }
        else
        {
            Console.WriteLine( "Error: Passed Null Reference to Function!" );
        }

        return null;
    }

    #endregion
}

}
