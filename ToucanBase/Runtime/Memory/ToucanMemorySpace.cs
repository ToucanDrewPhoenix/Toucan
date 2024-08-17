using System.Collections.Generic;

namespace Toucan.Runtime.Memory
{

public class ToucanMemorySpace
{
    private Dictionary < string, DynamicToucanVariable > m_FunctionByName =
        new Dictionary < string, DynamicToucanVariable >();
    
    private Dictionary < string, DynamicToucanVariable > m_ClassInstanceByName =
        new Dictionary < string, DynamicToucanVariable >();


    private DynamicToucanVariable[] m_Properties;

    public ToucanMemorySpace(int size)
    {
        m_Properties = new DynamicToucanVariable[size];
    }

    public void DefineVariable( int i, DynamicToucanVariable dynamicToucanVariable )
    {
        
    }
}

}
