using System;
using System.Collections.Generic;
using System.Text;

namespace Toucan.Compiler
{

public class ToucanCompilerException : ApplicationException
{
    public string ToucanCompilerExceptionMessage { get; }

    public IReadOnlyCollection < ToucanCompilerSyntaxError > SyntaxErrors { get; }

    public override string Message
    {
        get
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append( ToucanCompilerExceptionMessage );

            stringBuilder.AppendLine();

            foreach ( ToucanCompilerSyntaxError syntaxError in SyntaxErrors )
            {
                stringBuilder.AppendLine( syntaxError.Message );
            }

            return stringBuilder.ToString();
        }
    }

    #region Public

    public ToucanCompilerException( string message, IReadOnlyCollection < ToucanCompilerSyntaxError > syntaxErrors ) :
        base( message )
    {
        ToucanCompilerExceptionMessage = message;
        SyntaxErrors = syntaxErrors;
    }

    #endregion
}

}
