using System;
using System.Collections.Generic;
using Toucan.Runtime.Bytecode;

namespace Toucan.Runtime.Memory
{

public class FastMemorySpace
{
    public FastMemorySpace m_EnclosingSpace;

    public Dictionary < string, DynamicToucanVariable > NamesToProperties =
        new Dictionary < string, DynamicToucanVariable >();

    public DynamicToucanVariable[] Properties;
    public string Name;
    public int StackCountAtBegin = 0;

    public BinaryChunk CallerChunk;
    public int CallerIntructionPointer;
    
    public int CallerLineNumberPointer;

    public bool IsRunningCallback = false;

    public int CurrentMemoryPointer { get; set; } = 0;

    #region Public

    public FastMemorySpace(
        string name,
        FastMemorySpace enclosingSpace,
        int stackCount,
        BinaryChunk callerChunk,
        int callerInstructionPointer,
        int callerLineNumberPointer,
        int memberCount )
    {
        CallerChunk = callerChunk;
        CallerIntructionPointer = callerInstructionPointer;
        CallerLineNumberPointer = callerLineNumberPointer;
        m_EnclosingSpace = enclosingSpace;
        Name = name;
        StackCountAtBegin = stackCount;
        Properties = new DynamicToucanVariable[memberCount];
    }

    public virtual void Define( DynamicToucanVariable value )
    {
        if ( CurrentMemoryPointer >= Properties.Length )
        {
            if ( CurrentMemoryPointer == 0 )
            {
                DynamicToucanVariable[] newProperties = new DynamicToucanVariable[2];
                Array.Copy( Properties, newProperties, Properties.Length );
                Properties = newProperties;
            }
            else
            {
                DynamicToucanVariable[] newProperties = new DynamicToucanVariable[Properties.Length * 2];
                Array.Copy( Properties, newProperties, Properties.Length );
                Properties = newProperties;
            }
        }

        Properties[CurrentMemoryPointer] = value;

        CurrentMemoryPointer++;
    }

    public virtual void Define( DynamicToucanVariable value, string idStr, bool addToProperties = true )
    {
        if ( addToProperties )
        {
            if ( CurrentMemoryPointer >= Properties.Length )
            {
                if ( CurrentMemoryPointer == 0 )
                {
                    DynamicToucanVariable[] newProperties = new DynamicToucanVariable[2];
                    Array.Copy( Properties, newProperties, Properties.Length );
                    Properties = newProperties;
                }
                else
                {
                    DynamicToucanVariable[] newProperties = new DynamicToucanVariable[Properties.Length * 2];
                    Array.Copy( Properties, newProperties, Properties.Length );
                    Properties = newProperties;
                }
            }

            Properties[CurrentMemoryPointer] = value;

            if ( !string.IsNullOrEmpty( idStr ) )
            {
                NamesToProperties.Add( idStr, Properties[CurrentMemoryPointer] );
            }

            CurrentMemoryPointer++;
        }
        else
        {
            if ( !string.IsNullOrEmpty( idStr ) )
            {
                NamesToProperties.Add( idStr, value );
            }
        }
    }

    public virtual bool Exist( string idStr, bool calledFromGlobalMemorySpace = false )
    {
        if ( NamesToProperties.ContainsKey( idStr ) )
        {
            return true;
        }

        if ( m_EnclosingSpace != null && !calledFromGlobalMemorySpace )
        {
            return m_EnclosingSpace.Exist( idStr );
        }

        return false;
    }

    public virtual bool Exist( int moduleId, int depth, int classId, int id )
    {
        if ( moduleId >= 0 )
        {
            FastMemorySpace currentMemorySpace = this;

            while ( currentMemorySpace.m_EnclosingSpace != null )
            {
                currentMemorySpace = currentMemorySpace.m_EnclosingSpace;
            }

            if ( currentMemorySpace is FastGlobalMemorySpace fastGlobalMemorySpace )
            {
                if ( classId >= 0 )
                {
                    FastMemorySpace fms =
                        fastGlobalMemorySpace.GetModule( moduleId ).Properties[classId].ObjectData as FastMemorySpace;

                    return fms.CurrentMemoryPointer > id;
                }

                return fastGlobalMemorySpace.GetModule( moduleId ).CurrentMemoryPointer > id;
            }

            return false;
        }

        FastMemorySpace memorySpace = this;

        for ( int i = 0; i < depth; i++ )
        {
            memorySpace = m_EnclosingSpace;
        }

        if ( memorySpace != null )
        {
            if ( classId >= 0 )
            {
                FastMemorySpace fms = memorySpace.Properties[classId].ObjectData as FastMemorySpace;

                return fms.CurrentMemoryPointer > id;
            }

            return memorySpace.CurrentMemoryPointer > id;
        }

        return false;
    }

    public virtual DynamicToucanVariable Get( string idStr, bool calledFromGlobalMemorySpace = false )
    {
        if ( NamesToProperties.ContainsKey( idStr ) )
        {
            return NamesToProperties[idStr];
        }

        if ( m_EnclosingSpace != null && !calledFromGlobalMemorySpace )
        {
            return m_EnclosingSpace.Get( idStr );
        }

        return DynamicVariableExtension.ToDynamicVariable();
    }

    public virtual DynamicToucanVariable Get( int moduleId, int depth, int classId, int id )
    {
        if ( moduleId >= 0 )
        {
            FastMemorySpace currentMemorySpace = this;

            while ( currentMemorySpace.m_EnclosingSpace != null )
            {
                currentMemorySpace = currentMemorySpace.m_EnclosingSpace;
            }

            if ( currentMemorySpace is FastGlobalMemorySpace fastGlobalMemorySpace )
            {
                if ( classId >= 0 )
                {
                    FastMemorySpace fms =
                        fastGlobalMemorySpace.GetModule( moduleId ).Properties[classId].ObjectData as FastMemorySpace;

                    return fms.Properties[id];
                }

                return fastGlobalMemorySpace.GetModule( moduleId ).Properties[id];
            }

            return DynamicVariableExtension.ToDynamicVariable();
        }

        FastMemorySpace memorySpace = this;

        for ( int i = 0; i < depth; i++ )
        {
            memorySpace = memorySpace.m_EnclosingSpace;
        }

        if ( memorySpace != null && id < memorySpace.Properties.Length )
        {
            return memorySpace.Properties[id];
        }

        return DynamicVariableExtension.ToDynamicVariable();
    }

    public FastMemorySpace GetEnclosingSpace()
    {
        if ( m_EnclosingSpace != null )
        {
            return m_EnclosingSpace;
        }

        return null;
    }

    public virtual DynamicToucanVariable GetLocalVar( int id )
    {
        if ( id < Properties.Length )
        {
            return Properties[id];
        }

        return DynamicVariableExtension.ToDynamicVariable();
    }

    public virtual void Put( string idStr, DynamicToucanVariable value )
    {
        if ( NamesToProperties.ContainsKey( idStr ) )
        {
            NamesToProperties[idStr].Change( value );

            return;
        }

        if ( m_EnclosingSpace != null )
        {
            m_EnclosingSpace.Put( idStr, value );
        }
    }

    public virtual void Put( int moduleId, int depth, int classId, int id, DynamicToucanVariable value )
    {
        if ( moduleId >= 0 )
        {
            FastMemorySpace currentMemorySpace = this;

            while ( currentMemorySpace.m_EnclosingSpace != null )
            {
                currentMemorySpace = currentMemorySpace.m_EnclosingSpace;
            }

            if ( currentMemorySpace is FastGlobalMemorySpace fastGlobalMemorySpace )
            {
                if ( classId >= 0 )
                {
                    FastMemorySpace fms =
                        fastGlobalMemorySpace.GetModule( moduleId ).Properties[classId].ObjectData as FastMemorySpace;

                    fms.Properties[id].Change( value );

                    return;
                }

                fastGlobalMemorySpace.GetModule( moduleId ).Properties[id].Change( value );
            }

            return;
        }

        FastMemorySpace memorySpace = this;

        for ( int i = 0; i < depth; i++ )
        {
            memorySpace = memorySpace.m_EnclosingSpace;
        }

        if ( memorySpace != null && id < memorySpace.CurrentMemoryPointer )
        {
            memorySpace.Properties[id].Change( value );
        }
    }

    public virtual void PutLocalVar( int id, DynamicToucanVariable value )
    {
        if ( id < CurrentMemoryPointer )
        {
            Properties[id].Change( value );
        }
    }

    public void SetNameOfVariable( int varIndex, string name )
    {
        NamesToProperties.Add( name, Properties[varIndex] );
    }

    /*public void ResetPropertiesArray( int memberCount )
    {
        Properties = new DynamicToucanVariable[memberCount];
        CurrentMemoryPointer = 0;
    }*/

    public override string ToString()
    {
        return Name;
    }

    #endregion
}

}
