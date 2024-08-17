namespace Toucan.Ast
{

public interface IAstVisitor
{
    object Visit( AstBaseNode astBaseNode );
}

}
