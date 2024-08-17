using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace Toucan.Compiler
{

public class ToucanCompilerSyntaxErrorListener : BaseErrorListener
{
    public readonly List < ToucanCompilerSyntaxError > Errors = new List < ToucanCompilerSyntaxError >();

    #region Public

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e )
    {
        Errors.Add( new ToucanCompilerSyntaxError( recognizer, offendingSymbol, line, charPositionInLine, msg, e ) );
    }

    #endregion
}

}
