namespace Toucan.Runtime.Memory
{

public static class DynamicVariableExtension
{
    #region Public

    public static DynamicToucanVariable ToDynamicVariable()
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();
        ToucanVariable.DynamicType = DynamicVariableType.Null;
        ToucanVariable.NumberData = 0;
        ToucanVariable.ObjectData = null;

        return ToucanVariable;
    }

    public static DynamicToucanVariable ToDynamicVariable( int data )
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();
        ToucanVariable.DynamicType = 0;
        ToucanVariable.NumberData = data;
        ToucanVariable.ObjectData = null;

        return ToucanVariable;
    }

    public static DynamicToucanVariable ToDynamicVariable( double data )
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();
        ToucanVariable.DynamicType = 0;
        ToucanVariable.NumberData = data;
        ToucanVariable.ObjectData = null;

        return ToucanVariable;
    }

    public static DynamicToucanVariable ToDynamicVariable( bool data )
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();
        ToucanVariable.NumberData = 0;
        ToucanVariable.ObjectData = null;

        if ( data )
        {
            ToucanVariable.DynamicType = DynamicVariableType.True;
        }
        else
        {
            ToucanVariable.DynamicType = DynamicVariableType.False;
        }

        return ToucanVariable;
    }

    public static DynamicToucanVariable ToDynamicVariable( string data )
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();
        ToucanVariable.NumberData = 0;
        ToucanVariable.ObjectData = null;
        ToucanVariable.ArrayData = null;
        ToucanVariable.StringData = data;
        ToucanVariable.DynamicType = DynamicVariableType.String;

        return ToucanVariable;
    }

    public static DynamicToucanVariable ToDynamicVariable( object data )
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();

        switch ( data )
        {
            case int i:
                ToucanVariable.DynamicType = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ObjectData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.NumberData = i;

                break;

            case double d:
                ToucanVariable.DynamicType = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ObjectData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.NumberData = d;

                break;

            case float f:
                ToucanVariable.DynamicType = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ObjectData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.NumberData = f;

                break;

            case bool b when b:
                ToucanVariable.NumberData = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ObjectData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.DynamicType = DynamicVariableType.True;

                break;

            case bool b:
                ToucanVariable.NumberData = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ObjectData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.DynamicType = DynamicVariableType.False;

                break;

            case string s:
                ToucanVariable.NumberData = 0;
                ToucanVariable.ObjectData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.StringData = s;
                ToucanVariable.DynamicType = DynamicVariableType.String;

                break;

            case object[] oa:
                ToucanVariable.NumberData = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ObjectData = null;
                ToucanVariable.ArrayData = oa;
                ToucanVariable.DynamicType = DynamicVariableType.Array;

                break;

            case FastMemorySpace fastMemorySpace:
                ToucanVariable.NumberData = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.ObjectData = fastMemorySpace;
                ToucanVariable.DynamicType = DynamicVariableType.Object;

                break;

            case DynamicToucanVariable dynamicToucanVariable:
                switch ( dynamicToucanVariable.DynamicType )
                {
                    case DynamicVariableType.Null:
                        ToucanVariable.DynamicType = DynamicVariableType.Null;

                        break;

                    case DynamicVariableType.True:
                        ToucanVariable.DynamicType = DynamicVariableType.True;

                        break;

                    case DynamicVariableType.False:
                        ToucanVariable.DynamicType = DynamicVariableType.False;

                        break;

                    case DynamicVariableType.String:
                        ToucanVariable.StringData = dynamicToucanVariable.StringData;
                        ToucanVariable.DynamicType = DynamicVariableType.String;

                        break;

                    case DynamicVariableType.Array:
                        ToucanVariable.ArrayData = dynamicToucanVariable.ArrayData;
                        ToucanVariable.DynamicType = DynamicVariableType.Array;

                        break;

                    case DynamicVariableType.Object:
                        ToucanVariable.ObjectData = dynamicToucanVariable.ObjectData;
                        ToucanVariable.DynamicType = DynamicVariableType.Object;

                        break;

                    default:
                        ToucanVariable.DynamicType = 0;
                        ToucanVariable.NumberData = dynamicToucanVariable.NumberData;

                        break;
                }

                break;

            default:
                ToucanVariable.NumberData = 0;
                ToucanVariable.StringData = null;
                ToucanVariable.ArrayData = null;
                ToucanVariable.ObjectData = data;
                ToucanVariable.DynamicType = DynamicVariableType.Object;

                break;
        }

        return ToucanVariable;
    }

    public static DynamicToucanVariable ToDynamicVariable( DynamicToucanVariable dynamicToucanVariable )
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();
        switch ( dynamicToucanVariable.DynamicType )
        {
            case DynamicVariableType.Null:
                ToucanVariable.DynamicType = DynamicVariableType.Null;

                break;

            case DynamicVariableType.True:
                ToucanVariable.DynamicType = DynamicVariableType.True;

                break;

            case DynamicVariableType.False:
                ToucanVariable.DynamicType = DynamicVariableType.False;

                break;

            case DynamicVariableType.String:
                ToucanVariable.StringData = dynamicToucanVariable.StringData;
                ToucanVariable.DynamicType = DynamicVariableType.String;

                break;

            case DynamicVariableType.Array:
                ToucanVariable.ArrayData = dynamicToucanVariable.ArrayData;
                ToucanVariable.DynamicType = DynamicVariableType.Array;

                break;

            case DynamicVariableType.Object:
                ToucanVariable.ObjectData = dynamicToucanVariable.ObjectData;
                ToucanVariable.DynamicType = DynamicVariableType.Object;

                break;

            default:
                ToucanVariable.DynamicType = 0;
                ToucanVariable.NumberData = dynamicToucanVariable.NumberData;

                break;
        }

        return ToucanVariable;
    }

    public static DynamicToucanVariable ToDynamicVariable( object[] data )
    {
        DynamicToucanVariable ToucanVariable = new DynamicToucanVariable();
        ToucanVariable.NumberData = 0;
        ToucanVariable.StringData = null;
        ToucanVariable.ObjectData = null;
        ToucanVariable.ArrayData = data;
        ToucanVariable.DynamicType = DynamicVariableType.Array;

        return ToucanVariable;
    }

    #endregion
}

}
