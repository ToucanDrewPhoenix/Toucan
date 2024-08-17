using System.Collections.Generic;

namespace Toucan.Symbols
{

public class ToucanClassType : Type
{
    private static readonly List < string > s_ToucanClassTypes = new List < string >();
    private static int s_ClassTypeIndex = 0;

    public string Name { get; }

    public int TypeIndex => s_ClassTypeIndex;

    #region Public

    public ToucanClassType( string typeName )
    {
        Name = typeName;

        if ( s_ToucanClassTypes.Contains( typeName ) )
        {
            s_ClassTypeIndex = s_ToucanClassTypes.FindIndex( s => s == typeName );
        }
        else
        {
            s_ClassTypeIndex = s_ToucanClassTypes.Count;
            s_ToucanClassTypes.Add( typeName );
        }
    }

    public override string ToString()
    {
        return $" Type: {Name}";
    }

    #endregion
}

}
