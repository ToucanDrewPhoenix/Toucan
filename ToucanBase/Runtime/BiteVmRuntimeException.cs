using System;

namespace Toucan.Runtime
{

public class ToucanVmRuntimeException : ApplicationException
{
    public string ToucanVmRuntimeExceptionMessage { get; }

    #region Public

    public ToucanVmRuntimeException( string message ) : base( message )
    {
        ToucanVmRuntimeExceptionMessage = message;
    }

    #endregion
}

}
