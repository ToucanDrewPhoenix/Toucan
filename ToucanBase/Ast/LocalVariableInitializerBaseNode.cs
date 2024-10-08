using System.Collections.Generic;

namespace Toucan.Ast
{

public class LocalVariableInitializerBaseNode : StatementBaseNode
{
    public IEnumerable < LocalVariableDeclarationBaseNode > VariableDeclarations { get; set; }

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
