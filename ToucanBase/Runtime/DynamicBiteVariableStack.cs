using System;
using Toucan.Runtime.Memory;

namespace Toucan.Runtime
{

public class DynamicToucanVariableStack
{
    private DynamicToucanVariable[] m_DynamicVariables = new DynamicToucanVariable[128];

    public int Count { get; set; } = 0;

    #region Public

    public DynamicToucanVariable Peek()
    {
        DynamicToucanVariable dynamicVariable = m_DynamicVariables[Count - 1];

        return dynamicVariable;
    }

    public DynamicToucanVariable Peek( int i )
    {
        DynamicToucanVariable dynamicVariable = m_DynamicVariables[i];

        return dynamicVariable;
    }

    public DynamicToucanVariable Pop()
    {
        DynamicToucanVariable dynamicVariable = m_DynamicVariables[--Count];

        return dynamicVariable;
    }

    public void Push( DynamicToucanVariable dynamicVar )
    {
        if ( Count >= m_DynamicVariables.Length )
        {
            DynamicToucanVariable[] newProperties = new DynamicToucanVariable[m_DynamicVariables.Length * 2];
            Array.Copy( m_DynamicVariables, newProperties, m_DynamicVariables.Length );
            m_DynamicVariables = newProperties;
        }

        m_DynamicVariables[Count] = dynamicVar;

        Count++;
    }

    #endregion
}

}
