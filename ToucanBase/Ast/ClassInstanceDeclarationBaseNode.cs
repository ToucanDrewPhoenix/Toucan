using System.Collections.Generic;

namespace Toucan.Ast
{

public class ClassInstanceDeclarationBaseNode : DeclarationBaseNode
{
    public ModifiersBaseNode ModifiersBase;
    public Identifier InstanceId;
    public ArgumentsBaseNode ArgumentsBase;
    public Identifier ClassName;
    public List < Identifier > ClassPath;
    public bool IsVariableRedeclaration;
    public List < MemberInitializationNode > Initializers;

    #region Public

    public override object Accept( IAstVisitor visitor )
    {
        return visitor.Visit( this );
    }

    #endregion
}

}
