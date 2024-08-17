using Toucan.Symbols;

namespace Toucan.Ast
{

public abstract class AstBaseNode
{
    public Scope AstScopeNode;
    public DebugInfo DebugInfoAstNode = new DebugInfo();

    #region Public

    public abstract object Accept( IAstVisitor visitor );

    #endregion
}

}
