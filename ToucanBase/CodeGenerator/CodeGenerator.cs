using System;
using System.Collections.Generic;
using Toucan.Ast;
using Toucan.Runtime.Bytecode;
using Toucan.Symbols;

namespace Toucan.Runtime.CodeGen
{

public class CodeGenerator : AstVisitor < object >, IAstVisitor
{
    private int m_CurrentEnterBlockCount = 0;
    private string m_CurrentModuleName = "";
    private string m_CurrentClassName = "";

    private bool m_IsCompilingAssignmentLhs = false;
    private bool m_IsCompilingAssignmentRhs = false;

    private bool m_IsCompilingPostfixOperation = false;

    private int m_PreviousLoopBlockCount = 0;
    private ToucanVmOpCodes m_ConstructingOpCode;
    private List < int > m_ConstructingOpCodeData;
    private int m_ConstructingLine;

    private readonly BytecodeListStack m_PostfixInstructions = new BytecodeListStack();
    private readonly ToucanCompilationContext m_Context;

    #region Public

    public CodeGenerator( ToucanCompilationContext context )
    {
        m_Context = context;
    }

    public void Compile( ProgramBaseNode programBaseNode )
    {
        programBaseNode.Accept( this );
    }

    public void Compile( ModuleBaseNode moduleBaseNode )
    {
        moduleBaseNode.Accept( this );
    }

    public override object Visit( ProgramBaseNode node )
    {
        foreach ( ModuleBaseNode module in node.GetModulesInDepedencyOrder() )
        {
            Compile( module );
        }

        return null;
    }

    public override object Visit( ModuleBaseNode node )
    {
        m_CurrentModuleName = node.ModuleIdent.ToString();

        m_Context.PushChunk();

        int d = 0;

        ModuleSymbol mod =
            m_Context.BaseScope.resolve( m_CurrentModuleName, out int moduleId, ref d ) as ModuleSymbol;

        // TODO: What happens when we encounter a module that has a name that is already in the program?

        if ( m_Context.HasChunk( m_CurrentModuleName ) )
        {
            m_Context.RestoreChunk( m_CurrentModuleName );
        }
        else
        {
            m_Context.CurrentChunk.
                      WriteToChunk(
                          ToucanVmOpCodes.OpDefineModule,
                          new ConstantValue( m_CurrentModuleName ),
                          mod.NumberOfSymbols,
                          0 );

            m_Context.NewChunk();

            m_Context.SaveCurrentChunk( m_CurrentModuleName );
        }

        /*foreach ( ModuleIdentifier importedModule in node.ImportedModules )
        {
            EmitByteCode( ToucanVmOpCodes.OpImportModule, importedModule.ToString(), importedModule.DebugInfoAstNode.LineNumber );
        }*/

        foreach ( StatementBaseNode statement in node.Statements )
        {
            switch ( statement )
            {
                case ClassDeclarationBaseNode classDeclarationNode:
                    Compile( classDeclarationNode );

                    break;

                case StructDeclarationBaseNode structDeclaration:
                    Compile( structDeclaration );

                    break;

                case FunctionDeclarationBaseNode functionDeclarationNode:
                    Compile( functionDeclarationNode );

                    break;

                case VariableDeclarationBaseNode variable:
                    Compile( variable );

                    break;

                case ClassInstanceDeclarationBaseNode classInstance:
                    Compile( classInstance );

                    break;

                case StatementBaseNode stat:
                    Compile( stat );

                    break;
            }
        }

        m_Context.PopChunk();
        ;

        return null;
    }

    public override object Visit( ModifiersBaseNode node )
    {
        return null;
    }

    public override object Visit( DeclarationBaseNode node )
    {
        return null;
    }

    public override object Visit( UsingStatementBaseNode node )
    {
        EmitByteCode( ToucanVmOpCodes.OpEnterBlock, ( node.AstScopeNode as BaseScope ).NestedSymbolCount, node.DebugInfoAstNode.LineNumberStart );
        Compile( node.UsingBaseNode );
        EmitByteCode( ToucanVmOpCodes.OpUsingStatmentHead, node.DebugInfoAstNode.LineNumberStart );
        Compile( node.UsingBlock );
        EmitByteCode( ToucanVmOpCodes.OpUsingStatmentEnd, node.DebugInfoAstNode.LineNumberEnd );
        EmitByteCode( ToucanVmOpCodes.OpExitBlock, node.DebugInfoAstNode.LineNumberEnd );

        return null;
    }

    public override object Visit( DeclarationsBaseNode node )
    {
        EmitByteCode( ToucanVmOpCodes.OpEnterBlock, ( node.AstScopeNode as BaseScope ).NestedSymbolCount, node.DebugInfoAstNode.LineNumberStart );

        m_CurrentEnterBlockCount++;

        if ( node.Classes != null )
        {
            foreach ( ClassDeclarationBaseNode declaration in node.Classes )
            {
                Compile( declaration );
            }
        }

        if ( node.Structs != null )
        {
            foreach ( StructDeclarationBaseNode declaration in node.Structs )
            {
                Compile( declaration );
            }
        }

        if ( node.Functions != null )
        {
            foreach ( FunctionDeclarationBaseNode declaration in node.Functions )
            {
                Compile( declaration );
            }
        }

        if ( node.ClassInstances != null )
        {
            foreach ( ClassInstanceDeclarationBaseNode declaration in node.ClassInstances )
            {
                Compile( declaration );
            }
        }

        if ( node.Variables != null )
        {
            foreach ( VariableDeclarationBaseNode declaration in node.Variables )
            {
                Compile( declaration );
            }
        }

        if ( node.Statements != null )
        {
            foreach ( StatementBaseNode declaration in node.Statements )
            {
                Compile( declaration );
            }
        }

        EmitByteCode( ToucanVmOpCodes.OpExitBlock, node.DebugInfoAstNode.LineNumberEnd );
        m_CurrentEnterBlockCount--;

        return null;
    }

    public override object Visit( ClassDeclarationBaseNode node )
    {
        int d = 0;
        ClassSymbol symbol = ( ClassSymbol ) node.AstScopeNode.resolve( node.ClassId.Id, out int moduleId, ref d );
        m_CurrentClassName = symbol.QualifiedName;

        m_Context.PushChunk();

        if ( m_Context.HasChunk( m_CurrentClassName ) )
        {
            m_Context.RestoreChunk( m_CurrentClassName );
        }
        else
        {
            EmitByteCode( ToucanVmOpCodes.OpDefineClass, new ConstantValue( m_CurrentClassName ), node.DebugInfoAstNode.LineNumberStart );

            //EmitByteCode( symbol.InsertionOrderNumber );

            m_Context.NewChunk();
            m_Context.SaveCurrentChunk( m_CurrentClassName );
        }

        foreach ( FieldSymbol field in symbol.Fields )
        {
            if ( field.DefinitionBaseNode != null &&
                 field.DefinitionBaseNode is VariableDeclarationBaseNode variableDeclarationNode )
            {
                Compile( variableDeclarationNode );
            }
            else if ( field.DefinitionBaseNode != null &&
                      field.DefinitionBaseNode is ClassInstanceDeclarationBaseNode classInstance )
            {
                Compile( classInstance );
            }
            else
            {
                if ( !field.Name.Equals( "this" ) )
                {
                    EmitByteCode( ToucanVmOpCodes.OpDefineVar, new ConstantValue( field.Name ), node.DebugInfoAstNode.LineNumberStart );
                }

                //EmitByteCode( field.Type );
            }
        }

        foreach ( MethodSymbol method in symbol.Methods )
        {
            if ( method.DefBaseNode != null )
            {
                Compile( method.DefBaseNode );
            }
            else
            {
                EmitByteCode( ToucanVmOpCodes.OpDefineMethod, new ConstantValue( method.QualifiedName ), node.DebugInfoAstNode.LineNumberStart );

                //EmitByteCode( method.InsertionOrderNumber );
            }
        }

        EmitByteCode( ToucanVmOpCodes.OpDefineVar,new ConstantValue( "this" ), node.DebugInfoAstNode.LineNumberStart );
        m_Context.PopChunk();

        return null;
    }

    public override object Visit( FunctionDeclarationBaseNode node )
    {
        m_Context.PushChunk();

        int d = 0;

        FunctionSymbol symbol =
            node.AstScopeNode.resolve( node.FunctionId.Id, out int moduleId, ref d ) as FunctionSymbol;

        EmitConstant( new ConstantValue( node.FunctionId.Id ) );

        if ( m_Context.HasChunk( symbol.QualifiedName ) )
        {
            if ( symbol.m_IsExtern && symbol.IsCallable )
            {
                EmitByteCode( ToucanVmOpCodes.OpDefineCallableMethod, new ConstantValue( symbol.QualifiedName ), node.DebugInfoAstNode.LineNumberStart );
            }
            else
            {
                EmitByteCode( ToucanVmOpCodes.OpDefineMethod, new ConstantValue( symbol.QualifiedName ), node.DebugInfoAstNode.LineNumberStart );
            }

            //m_CompilingChunk = CompilingChunks[symbol.QualifiedName];
            m_Context.RestoreChunk( symbol.QualifiedName );
        }
        else
        {
            if ( symbol.m_IsExtern && symbol.IsCallable )
            {
                EmitByteCode( ToucanVmOpCodes.OpDefineCallableMethod, new ConstantValue( symbol.QualifiedName ), node.DebugInfoAstNode.LineNumberStart );
            }
            else
            {
                EmitByteCode( ToucanVmOpCodes.OpDefineMethod, new ConstantValue( symbol.QualifiedName ), node.DebugInfoAstNode.LineNumberStart );
            }

            //EmitByteCode( symbol.InsertionOrderNumber );

            m_Context.NewChunk();
            m_Context.SaveCurrentChunk( symbol.QualifiedName );
        }

        if ( node.ParametersBase != null && node.ParametersBase.Identifiers != null )
        {
            foreach ( Identifier parametersIdentifier in node.ParametersBase.Identifiers )
            {
                EmitConstant( new ConstantValue( parametersIdentifier.Id ) );
            }

            EmitByteCode( ToucanVmOpCodes.OpSetFunctionParameterName, node.ParametersBase.Identifiers.Count, node.DebugInfoAstNode.LineNumberStart );
        }

        if ( node.FunctionBlock != null )
        {
            Compile( node.FunctionBlock );
        }

        m_Context.PopChunk();

        return null;
    }

    public override object Visit( VariableDeclarationBaseNode node )
    {
        int d = 0;

        DynamicVariable variableSymbol =
            node.AstScopeNode.resolve( node.VarId.Id, out int moduleId, ref d ) as DynamicVariable;

        if ( node.InitializerBase != null )
        {
            m_PostfixInstructions.Push( new BytecodeList() );
            m_IsCompilingAssignmentRhs = true;
            Compile( node.InitializerBase );
            m_IsCompilingAssignmentRhs = false;
            EmitByteCode( ToucanVmOpCodes.OpDefineVar, new ConstantValue( variableSymbol.Name ), node.DebugInfoAstNode.LineNumberStart );

            BytecodeList byteCodes = m_PostfixInstructions.Pop();

            foreach ( ByteCode code in byteCodes.ByteCodes )
            {
                EmitByteCode( code, node.DebugInfoAstNode.LineNumberStart );
            }
        }
        else
        {
            EmitByteCode( ToucanVmOpCodes.OpDeclareVar,new ConstantValue( variableSymbol.Name ), node.DebugInfoAstNode.LineNumberStart );
        }

        return null;
    }

    public override object Visit( ClassInstanceDeclarationBaseNode node )
    {
        int d = 0;

        DynamicVariable variableSymbol =
            node.AstScopeNode.resolve( node.InstanceId.Id, out int moduleId, ref d ) as DynamicVariable;

        int d2 = 0;

        ClassSymbol classSymbol =
            node.AstScopeNode.resolve( node.ClassName.Id, out int moduleId2, ref d2 ) as ClassSymbol;

        if ( node.IsVariableRedeclaration )
        {
            ByteCode byteCode = new ByteCode(
                ToucanVmOpCodes.OpSetInstance,
                moduleId,
                d,
                variableSymbol.InsertionOrderNumber,
                moduleId2,
                d2,
                classSymbol.InsertionOrderNumber,
                classSymbol.NumberOfSymbols );

            EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
        }
        else
        {
            ByteCode byteCode = new ByteCode(
                ToucanVmOpCodes.OpDefineInstance,
                moduleId2,
                d2,
                classSymbol.InsertionOrderNumber,
                classSymbol.NumberOfSymbols );

            EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
            EmitByteCode( ToucanVmOpCodes.OpNone, new ConstantValue( variableSymbol.Name ), node.DebugInfoAstNode.LineNumberStart );
        }

        if ( node.ArgumentsBase != null && node.ArgumentsBase.Expressions.Count > 0 )
        {
            foreach ( MethodSymbol methodSymbol in classSymbol.Methods )
            {
                if ( methodSymbol.IsConstructor &&
                     node.ArgumentsBase.Expressions.Count == methodSymbol.NumberOfParameters )
                {
                    ByteCode byteCode = new ByteCode(
                        ToucanVmOpCodes.OpGetVar,
                        moduleId,
                        d,
                        -1,
                        variableSymbol.InsertionOrderNumber );

                    ByteCode byteCode2 = new ByteCode(
                        ToucanVmOpCodes.OpGetNextVarByRef );

                    m_PostfixInstructions.Push( new BytecodeList() );

                    foreach ( ExpressionBaseNode argument in node.ArgumentsBase.Expressions )
                    {
                        Compile( argument );
                    }

                    EmitByteCode( ToucanVmOpCodes.OpBindToFunction, node.ArgumentsBase.Expressions.Count, node.DebugInfoAstNode.LineNumberStart );
                    BytecodeList byteCodes = m_PostfixInstructions.Pop();

                    foreach ( ByteCode code in byteCodes.ByteCodes )
                    {
                        EmitByteCode( code, node.DebugInfoAstNode.LineNumberStart );
                    }

                    EmitByteCode( byteCode2, node.DebugInfoAstNode.LineNumberStart );
                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                    EmitByteCode( ToucanVmOpCodes.OpCallMemberFunction, new ConstantValue( methodSymbol.Name ), node.DebugInfoAstNode.LineNumberStart );
                }
            }
        }
        else
        {
            foreach ( MethodSymbol methodSymbol in classSymbol.Methods )
            {
                if ( methodSymbol.IsConstructor && methodSymbol.NumberOfParameters == 0 )
                {
                    ByteCode byteCode = new ByteCode(
                        ToucanVmOpCodes.OpGetVar,
                        moduleId,
                        d,
                        -1,
                        variableSymbol.InsertionOrderNumber );

                    ByteCode byteCode2 = new ByteCode(
                        ToucanVmOpCodes.OpGetNextVarByRef );

                    EmitByteCode( byteCode2, node.DebugInfoAstNode.LineNumberStart );
                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                    EmitByteCode( ToucanVmOpCodes.OpCallMemberFunction, new ConstantValue( methodSymbol.Name ), node.DebugInfoAstNode.LineNumberStart );
                }
            }
        }

        /*foreach ( var methodSymbol in classSymbol.Methods )
        {
            if ( methodSymbol.IsConstructor )
            {
                ByteCode byteCode = new ByteCode(
                    ToucanVmOpCodes.OpGetLocalInstance,
                    moduleId,
                    d,
                    variableSymbol.InsertionOrderNumber);
                
                EmitByteCode(byteCode);
                EmitByteCode( ToucanVmOpCodes.OpCallMemberFunction, new ConstantValue( methodSymbol.QualifiedName ) );
            }
        }*/

        if ( node.Initializers != null )
        {
            foreach ( MemberInitializationNode initializer in node.Initializers )
            {
                initializer.Expression.AstScopeNode = node.AstScopeNode;

                ExpressionBaseNode assignment = new ExpressionBaseNode
                {
                    AssignmentBase = new AssignmentBaseNode
                    {
                        AstScopeNode = node.AstScopeNode,
                        AssignmentBase = initializer.Expression.AssignmentBase,
                        CallBase = new CallBaseNode
                        {
                            AstScopeNode = node.AstScopeNode,
                            PrimaryBase =
                                new PrimaryBaseNode
                                {
                                    AstScopeNode = node.AstScopeNode,
                                    PrimaryType = PrimaryBaseNode.PrimaryTypes.Identifier,
                                    PrimaryId = node.InstanceId
                                },
                            CallEntries = new List < CallEntry >
                            {
                                new CallEntry
                                {
                                    PrimaryBase = new PrimaryBaseNode
                                    {
                                        AstScopeNode = node.AstScopeNode,
                                        PrimaryType = PrimaryBaseNode.PrimaryTypes.Identifier,
                                        PrimaryId = initializer.Identifier
                                    }
                                }
                            },
                            CallType = CallTypes.PrimaryCall
                        },
                        OperatorType = AssignmentOperatorTypes.Assign,
                        Type = AssignmentTypes.Assignment
                    }
                };

                Visit( assignment );
            }
        }

        return null;
    }

    public override object Visit( CallBaseNode node )
    {
        if ( node.ArgumentsBase != null && node.ArgumentsBase.Expressions != null )
        {
            m_PostfixInstructions.Push( new BytecodeList() );

            foreach ( ExpressionBaseNode argument in node.ArgumentsBase.Expressions )
            {
                m_IsCompilingAssignmentRhs = true;
                Compile( argument );
                m_IsCompilingAssignmentRhs = false;
            }

            ByteCode byteCode = new ByteCode(
                ToucanVmOpCodes.OpBindToFunction,
                node.ArgumentsBase.Expressions.Count );

            EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );

            BytecodeList byteCodes = m_PostfixInstructions.Pop();

            foreach ( ByteCode code in byteCodes.ByteCodes )
            {
                EmitByteCode( code, node.DebugInfoAstNode.LineNumberStart );
            }
        }

        if ( node.IsFunctionCall )
        {
            int d = 0;

            if ( node.ElementAccess != null )
            {
                if ( node.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
                {
                    if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                    {
                        int d2 = 0;

                        if ( node.AstScopeNode.resolve(
                                 node.PrimaryBase.PrimaryId.Id,
                                 out int moduleId2,
                                 ref d2,
                                 false ) !=
                             null )
                        {
                            BeginConstuctingByteCodeInstruction( ToucanVmOpCodes.OpSetVar, node.DebugInfoAstNode.LineNumberStart );
                            Compile( node.PrimaryBase );
                        }
                        else
                        {
                            EmitByteCode(
                                ToucanVmOpCodes.OpSetVarExternal,
                                new ConstantValue( node.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                        }
                    }
                    else
                    {
                        int d2 = 0;

                        Symbol var = node.AstScopeNode.resolve(
                            node.PrimaryBase.PrimaryId.Id,
                            out int moduleId2,
                            ref d2,
                            false );

                        if ( var is ModuleSymbol m )
                        {
                            BeginConstuctingByteCodeInstruction( ToucanVmOpCodes.OpGetModule, node.DebugInfoAstNode.LineNumberStart );
                            AddToConstuctingByteCodeInstruction( m.InsertionOrderNumber );
                            EndConstuctingByteCodeInstruction();
                        }
                        else
                        {
                            if ( var != null )
                            {
                                BeginConstuctingByteCodeInstruction( ToucanVmOpCodes.OpGetVar, node.DebugInfoAstNode.LineNumberStart );
                                Compile( node.PrimaryBase );
                            }
                            else
                            {
                                EmitByteCode(
                                    ToucanVmOpCodes.OpGetVarExternal,
                                    new ConstantValue( node.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                            }
                        }
                    }
                }
                else
                {
                    Compile( node.PrimaryBase );
                }

                foreach ( CallElementEntry callElementEntry in node.ElementAccess )
                {
                    if ( callElementEntry.CallElementType == CallElementTypes.Call )
                    {
                        Compile( callElementEntry.CallBase );
                    }
                    else
                    {
                        EmitConstant( new ConstantValue( callElementEntry.Identifier ) );
                    }
                }

                if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                {
                    ByteCode byteCode = new ByteCode(
                        ToucanVmOpCodes.OpSetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                }
                else
                {
                    ByteCode byteCode = new ByteCode(
                        ToucanVmOpCodes.OpGetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                }

                int dObject = 0;

                Symbol objectToCall = node.AstScopeNode.resolve(
                    node.PrimaryBase.PrimaryId.Id,
                    out int moduleIdObject,
                    ref dObject,
                    false );

                ByteCode byteCodeObj = new ByteCode(
                    ToucanVmOpCodes.OpGetVar,
                    moduleIdObject,
                    dObject,
                    -1,
                    objectToCall.InsertionOrderNumber );

                ByteCode byteCode2 = new ByteCode(
                    ToucanVmOpCodes.OpGetNextVarByRef );

                EmitByteCode( byteCode2, node.DebugInfoAstNode.LineNumberStart );
                EmitByteCode( byteCodeObj, node.DebugInfoAstNode.LineNumberStart );

                EmitByteCode( ToucanVmOpCodes.OpCallFunctionFromStack, node.DebugInfoAstNode.LineNumberStart );
            }
            else
            {
                EmitByteCode( ToucanVmOpCodes.OpCallFunctionByName, new ConstantValue( node.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
            }
        }
        else
        {
            if ( node.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
            {
                if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                {
                    int d = 0;

                    if ( node.AstScopeNode.resolve( node.PrimaryBase.PrimaryId.Id, out int moduleId, ref d, false ) !=
                         null )
                    {
                        BeginConstuctingByteCodeInstruction( ToucanVmOpCodes.OpSetVar, node.DebugInfoAstNode.LineNumberStart );
                        Compile( node.PrimaryBase );
                    }
                    else
                    {
                        EmitByteCode(
                            ToucanVmOpCodes.OpSetVarExternal,
                            new ConstantValue( node.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                    }
                }
                else
                {
                    int d = 0;

                    Symbol var = node.AstScopeNode.resolve(
                        node.PrimaryBase.PrimaryId.Id,
                        out int moduleId,
                        ref d,
                        false );

                    if ( var is ModuleSymbol m )
                    {
                        BeginConstuctingByteCodeInstruction( ToucanVmOpCodes.OpGetModule, node.DebugInfoAstNode.LineNumberStart );
                        AddToConstuctingByteCodeInstruction( m.InsertionOrderNumber );
                        EndConstuctingByteCodeInstruction();
                    }
                    else
                    {
                        if ( var == null )
                        {
                            EmitByteCode(
                                ToucanVmOpCodes.OpGetVarExternal,
                                new ConstantValue( node.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                        }
                        else
                        {
                            BeginConstuctingByteCodeInstruction( ToucanVmOpCodes.OpGetVar, node.DebugInfoAstNode.LineNumberStart );
                            Compile( node.PrimaryBase );
                        }
                    }
                }
            }
            else
            {
                Compile( node.PrimaryBase );
            }

            if ( node.ElementAccess != null )
            {
                foreach ( CallElementEntry callElementEntry in node.ElementAccess )
                {
                    if ( callElementEntry.CallElementType == CallElementTypes.Call )
                    {
                        Compile( callElementEntry.CallBase );
                    }
                    else
                    {
                        EmitConstant( new ConstantValue( callElementEntry.Identifier ), node.DebugInfoAstNode.LineNumberStart );
                    }
                }

                if ( m_IsCompilingAssignmentLhs && ( node.CallEntries == null || node.CallEntries.Count == 0 ) )
                {
                    ByteCode byteCode = new ByteCode(
                        ToucanVmOpCodes.OpSetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                }
                else
                {
                    ByteCode byteCode = new ByteCode(
                        ToucanVmOpCodes.OpGetElement,
                        node.ElementAccess.Count );

                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                }
            }
        }

        if ( node.CallEntries != null )
        {
            int i = 0;
            int d = 0;
            int d2 = 0;
            bool isModuleSymbol = false;
            bool isClassSymbol = false;
            Symbol var = node.AstScopeNode.resolve( node.PrimaryBase.PrimaryId.Id, out int moduleId, ref d, false );
            ModuleSymbol moduleSymbol = null;
            ClassSymbol classSymbol = null;

            if ( var is ModuleSymbol m )
            {
                moduleSymbol = m;
                isModuleSymbol = true;
            }
            else
            {
                if ( var is DynamicVariable dynamicVariable && dynamicVariable.Type != null )
                {
                    classSymbol =
                        node.AstScopeNode.resolve(
                            dynamicVariable.Type.Name,
                            out int moduleId2,
                            ref d2,
                            false ) as ClassSymbol;

                    isClassSymbol = classSymbol != null;
                }
            }

            //DynamicVariable dynamicVariable = node.AstScopeNode.resolve( node.Primary.PrimaryId.Id, out int moduleId, ref d) as DynamicVariable;

            foreach ( CallEntry terminalNode in node.CallEntries )
            {
                if ( terminalNode.ArgumentsBase != null && terminalNode.ArgumentsBase.Expressions != null )
                {
                    m_PostfixInstructions.Push( new BytecodeList() );

                    foreach ( ExpressionBaseNode argumentsExpression in terminalNode.ArgumentsBase.Expressions )
                    {
                        m_IsCompilingAssignmentRhs = true;
                        Compile( argumentsExpression );
                        m_IsCompilingAssignmentRhs = false;
                    }

                    ByteCode byteCode = new ByteCode(
                        ToucanVmOpCodes.OpBindToFunction,
                        terminalNode.ArgumentsBase.Expressions.Count );

                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                    BytecodeList byteCodes = m_PostfixInstructions.Pop();

                    foreach ( ByteCode code in byteCodes.ByteCodes )
                    {
                        EmitByteCode( code, node.DebugInfoAstNode.LineNumberStart );
                    }
                }

                if ( terminalNode.IsFunctionCall )
                {
                    if ( isModuleSymbol )
                    {
                        EmitByteCode(
                            ToucanVmOpCodes.OpCallMemberFunction,
                            new ConstantValue( terminalNode.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                    }
                    else if ( isClassSymbol )
                    {
                        EmitByteCode(
                            ToucanVmOpCodes.OpCallMemberFunction,
                            new ConstantValue( terminalNode.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                    }
                    else
                    {
                        EmitByteCode(
                            ToucanVmOpCodes.OpCallMemberFunction,
                            new ConstantValue( terminalNode.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                    }
                }
                else
                {
                    if ( terminalNode.PrimaryBase.PrimaryType == PrimaryBaseNode.PrimaryTypes.Identifier )
                    {
                        if ( m_IsCompilingAssignmentLhs && i == node.CallEntries.Count - 1 )
                        {
                            if ( isModuleSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = moduleSymbol.resolve(
                                    terminalNode.PrimaryBase.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4,
                                    false );

                                ByteCode byteCode = new ByteCode(
                                    ToucanVmOpCodes.OpSetMember,
                                    memberSymbol.InsertionOrderNumber );

                                EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                            }
                            else if ( isClassSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = classSymbol.resolve(
                                    terminalNode.PrimaryBase.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4,
                                    false );

                                if ( memberSymbol == null )
                                {
                                    EmitByteCode(
                                        ToucanVmOpCodes.OpSetMemberWithString,
                                        new ConstantValue( terminalNode.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                                }
                                else
                                {
                                    ByteCode byteCode = new ByteCode(
                                        ToucanVmOpCodes.OpSetMember,
                                        memberSymbol.InsertionOrderNumber );

                                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                                }
                            }
                            else
                            {
                                EmitByteCode(
                                    ToucanVmOpCodes.OpSetMemberWithString,
                                    new ConstantValue( terminalNode.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                            }
                        }
                        else
                        {
                            if ( isModuleSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = moduleSymbol.resolve(
                                    terminalNode.PrimaryBase.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4,
                                    false );

                                ByteCode byteCode = new ByteCode(
                                    ToucanVmOpCodes.OpGetMember,
                                    memberSymbol.InsertionOrderNumber );

                                EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                            }
                            else if ( isClassSymbol )
                            {
                                int d4 = 0;

                                Symbol memberSymbol = classSymbol.resolve(
                                    terminalNode.PrimaryBase.PrimaryId.Id,
                                    out int moduleId4,
                                    ref d4,
                                    false );

                                if ( memberSymbol == null )
                                {
                                    EmitByteCode(
                                        ToucanVmOpCodes.OpGetMemberWithString,
                                        new ConstantValue( terminalNode.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                                }
                                else
                                {
                                    ByteCode byteCode = new ByteCode(
                                        ToucanVmOpCodes.OpGetMember,
                                        memberSymbol.InsertionOrderNumber );

                                    EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                                }
                            }
                            else
                            {
                                EmitByteCode(
                                    ToucanVmOpCodes.OpGetMemberWithString,
                                    new ConstantValue( terminalNode.PrimaryBase.PrimaryId.Id ), node.DebugInfoAstNode.LineNumberStart );
                            }
                        }
                    }
                }

                if ( terminalNode.ElementAccess != null )
                {
                    foreach ( CallElementEntry callElementEntry in terminalNode.ElementAccess )
                    {
                        if ( callElementEntry.CallElementType == CallElementTypes.Call )
                        {
                            Compile( callElementEntry.CallBase );
                        }
                        else
                        {
                            EmitConstant( new ConstantValue( callElementEntry.Identifier ), node.DebugInfoAstNode.LineNumberStart );
                        }
                    }

                    if ( m_IsCompilingAssignmentLhs && i == node.CallEntries.Count - 1 )
                    {
                        ByteCode byteCode = new ByteCode(
                            ToucanVmOpCodes.OpSetElement,
                            terminalNode.ElementAccess.Count );

                        EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                    }
                    else
                    {
                        ByteCode byteCode = new ByteCode(
                            ToucanVmOpCodes.OpGetElement,
                            terminalNode.ElementAccess.Count );

                        EmitByteCode( byteCode, node.DebugInfoAstNode.LineNumberStart );
                    }
                }

                i++;
            }
        }

        return null;
    }

    public override object Visit( ArgumentsBaseNode node )
    {
        return null;
    }

    public override object Visit( ParametersBaseNode node )
    {
        return null;
    }

    public override object Visit( AssignmentBaseNode node )
    {
        switch ( node.Type )
        {
            case AssignmentTypes.Assignment:
                if ( m_IsCompilingAssignmentRhs )
                {
                    EmitByteCode( ToucanVmOpCodes.OpPushNextAssignmentOnStack, node.DebugInfoAstNode.LineNumberStart );
                }

                m_PostfixInstructions.Push( new BytecodeList() );
                m_IsCompilingAssignmentRhs = true;
                Compile( node.AssignmentBase );
                m_IsCompilingAssignmentRhs = false;
                m_IsCompilingAssignmentLhs = true;
                Compile( node.CallBase );
                m_IsCompilingAssignmentLhs = false;

                switch ( node.OperatorType )
                {
                    case AssignmentOperatorTypes.Assign:
                        EmitByteCode( ToucanVmOpCodes.OpAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.DivAssign:
                        EmitByteCode( ToucanVmOpCodes.OpDivideAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.MultAssign:
                        EmitByteCode( ToucanVmOpCodes.OpMultiplyAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.PlusAssign:
                        EmitByteCode( ToucanVmOpCodes.OpPlusAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.MinusAssign:
                        EmitByteCode( ToucanVmOpCodes.OpMinusAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.ModuloAssignOperator:
                        EmitByteCode( ToucanVmOpCodes.OpModuloAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.BitwiseAndAssignOperator:
                        EmitByteCode( ToucanVmOpCodes.OpBitwiseAndAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.BitwiseOrAssignOperator:
                        EmitByteCode( ToucanVmOpCodes.OpBitwiseOrAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.BitwiseXorAssignOperator:
                        EmitByteCode( ToucanVmOpCodes.OpBitwiseXorAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.BitwiseLeftShiftAssignOperator:
                        EmitByteCode( ToucanVmOpCodes.OpBitwiseLeftShiftAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    case AssignmentOperatorTypes.BitwiseRightShiftAssignOperator:
                        EmitByteCode( ToucanVmOpCodes.OpBitwiseRightShiftAssign, node.DebugInfoAstNode.LineNumberStart );

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                BytecodeList byteCodes = m_PostfixInstructions.Pop();

                foreach ( ByteCode code in byteCodes.ByteCodes )
                {
                    EmitByteCode( code, node.DebugInfoAstNode.LineNumberStart );
                }

                break;

            case AssignmentTypes.Binary:
                Compile( node.Binary );

                break;

            case AssignmentTypes.Ternary:
                Compile( node.Ternary );

                break;

            case AssignmentTypes.Call:
                Compile( node.CallBase );

                break;

            case AssignmentTypes.Primary:
                Compile( node.PrimaryBaseNode );

                break;

            case AssignmentTypes.UnaryPostfix:
                Compile( node.UnaryPostfix );

                break;

            case AssignmentTypes.UnaryPrefix:
                Compile( node.UnaryPrefix );

                break;

            default:
                throw new Exception( "Invalid Type" );
        }

        return null;
    }

    public override object Visit( ExpressionBaseNode node )
    {
        Compile( node.AssignmentBase );

        return null;
    }

    public override object Visit( BlockStatementBaseNode node )
    {
        Compile( node.DeclarationsBase );

        return null;
    }

    public override object Visit( SyncBlockNode node )
    {
        EmitByteCode( ToucanVmOpCodes.OpSwitchContext, node.DebugInfoAstNode.LineNumberStart );

        Compile( node.Block );

        EmitByteCode( ToucanVmOpCodes.OpReturnContext, node.DebugInfoAstNode.LineNumberStart );

        return null;
    }

    public override object Visit( StatementBaseNode node )
    {
        if ( node is ExpressionStatementBaseNode expressionStatementNode )
        {
            Compile( expressionStatementNode );

            return null;
        }

        if ( node is ForStatementBaseNode forStatementNode )
        {
            Compile( forStatementNode );

            return null;
        }

        if ( node is IfStatementBaseNode ifStatementNode )
        {
            Compile( ifStatementNode );

            return null;
        }

        if ( node is WhileStatementBaseNode whileStatement )
        {
            Compile( whileStatement );

            return null;
        }

        if ( node is ReturnStatementBaseNode returnStatement )
        {
            Compile( returnStatement );

            return null;
        }

        if ( node is BlockStatementBaseNode blockStatementNode )
        {
            Compile( blockStatementNode );

            return null;
        }

        if ( node is SyncBlockNode syncStatementNode )
        {
            Compile( syncStatementNode );

            return null;
        }

        return null;
    }

    public override object Visit( ExpressionStatementBaseNode node )
    {
        Compile( node.ExpressionBase );

        return null;
    }

    public override object Visit( IfStatementBaseNode node )
    {
        Compile( node.ExpressionBase );
        int thenJump = EmitByteCode( ToucanVmOpCodes.OpNone, 0, node.DebugInfoAstNode.LineNumberStart );
        Compile( node.ThenStatementBase );
        int overElseJump = EmitByteCode( ToucanVmOpCodes.OpNone, 0, node.DebugInfoAstNode.LineNumberStart );

        m_Context.CurrentChunk.Code[thenJump] = new ByteCode(
            ToucanVmOpCodes.OpJumpIfFalse,
            m_Context.CurrentChunk.SerializeToBytes().Length );

        Stack < int > endJumpStack = new Stack < int >();

        if ( node.ElseStatementBase != null )
        {
            Compile( node.ElseStatementBase );
        }

        int endJumpStackCount = endJumpStack.Count;

        for ( int i = 0; i < endJumpStackCount; i++ )
        {
            int endJump = endJumpStack.Pop();

            m_Context.CurrentChunk.Code[endJump] = new ByteCode(
                ToucanVmOpCodes.OpJump,
                m_Context.CurrentChunk.SerializeToBytes().Length );
        }

        m_Context.CurrentChunk.Code[overElseJump] = new ByteCode(
            ToucanVmOpCodes.OpJump,
            m_Context.CurrentChunk.SerializeToBytes().Length );

        return null;
    }

    public override object Visit( LocalVariableDeclarationBaseNode node )
    {
        int d = 0;

        DynamicVariable variableSymbol =
            node.AstScopeNode.resolve( node.VarId.Id, out int moduleId, ref d ) as DynamicVariable;

        Compile( node.ExpressionBase );
        EmitByteCode( ToucanVmOpCodes.OpDefineVar, new ConstantValue( variableSymbol.Name ), node.DebugInfoAstNode.LineNumberStart );

        return null;
    }

    public override object Visit( LocalVariableInitializerBaseNode node )
    {
        foreach ( LocalVariableDeclarationBaseNode variableDeclaration in node.VariableDeclarations )
        {
            Compile( variableDeclaration );
        }

        return null;
    }

    public override object Visit( ForStatementBaseNode node )
    {
        EmitByteCode( ToucanVmOpCodes.OpEnterBlock, ( node.AstScopeNode as BaseScope ).NestedSymbolCount, node.DebugInfoAstNode.LineNumberStart );
        m_CurrentEnterBlockCount++;
        m_PreviousLoopBlockCount = m_CurrentEnterBlockCount;

        if ( node.InitializerBase != null )
        {
            if ( node.InitializerBase.Expressions != null )
            {
                foreach ( ExpressionBaseNode expression in node.InitializerBase.Expressions )
                {
                    Compile( expression );
                }
            }
            else if ( node.InitializerBase.LocalVariableInitializerBase != null )
            {
                Compile( node.InitializerBase.LocalVariableInitializerBase );
            }
        }

        int jumpCodeWhileBegin = m_Context.CurrentChunk.SerializeToBytes().Length;

        if ( node.Condition != null )
        {
            Compile( node.Condition );
        }
        else
        {
            EmitConstant( new ConstantValue( true ), node.DebugInfoAstNode.LineNumberStart );
        }

        int toFix = EmitByteCode( ToucanVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, 0, 0 );

        if ( node.StatementBase != null )
        {
            Compile( node.StatementBase );
        }

        if ( node.Iterators != null )
        {
            foreach ( ExpressionBaseNode iterator in node.Iterators )
            {
                Compile( iterator );
            }
        }

        /*if ( node.Iterators != null )
        {
            foreach ( var iterator in node.Iterators )
            {
                EmitByteCode( ToucanVmOpCodes.OpPopStack );
            }
        }*/

        m_Context.CurrentChunk.Code[toFix] = new ByteCode(
            ToucanVmOpCodes.OpWhileLoop,
            jumpCodeWhileBegin,
            m_Context.CurrentChunk.SerializeToBytes().Length );

        EmitByteCode( ToucanVmOpCodes.OpNone, 0, node.DebugInfoAstNode.LineNumberStart );
        EmitByteCode( ToucanVmOpCodes.OpNone, 0, node.DebugInfoAstNode.LineNumberStart );
        EmitByteCode( ToucanVmOpCodes.OpExitBlock, node.DebugInfoAstNode.LineNumberStart );
        m_CurrentEnterBlockCount--;

        return null;
    }

    public override object Visit( WhileStatementBaseNode node )
    {
        m_PreviousLoopBlockCount = m_CurrentEnterBlockCount;

        int jumpCodeWhileBegin = m_Context.CurrentChunk.SerializeToBytes().Length;
        Compile( node.ExpressionBase );
        int toFix = EmitByteCode( ToucanVmOpCodes.OpWhileLoop, jumpCodeWhileBegin, 0, node.DebugInfoAstNode.LineNumberStart );
        Compile( node.WhileBlock );

        m_Context.CurrentChunk.Code[toFix] = new ByteCode(
            ToucanVmOpCodes.OpWhileLoop,
            jumpCodeWhileBegin,
            m_Context.CurrentChunk.SerializeToBytes().Length );

        EmitByteCode( ToucanVmOpCodes.OpNone, 0, node.DebugInfoAstNode.LineNumberStart );
        EmitByteCode( ToucanVmOpCodes.OpNone, 0, node.DebugInfoAstNode.LineNumberStart );

        return null;
    }

    public override object Visit( ReturnStatementBaseNode node )
    {
        Compile( node.ExpressionStatementBase );
        EmitByteCode( ToucanVmOpCodes.OpKeepLastItemOnStack, node.DebugInfoAstNode.LineNumberStart );

        for ( int i = 0; i < m_CurrentEnterBlockCount; i++ )
        {
            EmitByteCode( ToucanVmOpCodes.OpExitBlock, node.DebugInfoAstNode.LineNumberStart );
        }

        EmitReturn();

        return null;
    }

    public override object Visit( BreakStatementBaseNode node )
    {
        for ( int i = 0; i < m_CurrentEnterBlockCount - m_PreviousLoopBlockCount; i++ )
        {
            EmitByteCode( ToucanVmOpCodes.OpExitBlock, node.DebugInfoAstNode.LineNumberStart );
        }

        EmitByteCode( ToucanVmOpCodes.OpBreak, node.DebugInfoAstNode.LineNumberStart );

        return null;
    }

    public override object Visit( InitializerBaseNode node )
    {
        Compile( node.Expression );

        return null;
    }

    public override object Visit( BinaryOperationBaseNode node )
    {
        Compile( node.LeftOperand );
        Compile( node.RightOperand );

        switch ( node.Operator )
        {
            case BinaryOperationBaseNode.BinaryOperatorType.Plus:
                EmitByteCode( ToucanVmOpCodes.OpAdd, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Minus:
                EmitByteCode( ToucanVmOpCodes.OpSubtract, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Mult:
                EmitByteCode( ToucanVmOpCodes.OpMultiply, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Div:
                EmitByteCode( ToucanVmOpCodes.OpDivide, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Modulo:
                EmitByteCode( ToucanVmOpCodes.OpModulo, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Equal:
                EmitByteCode( ToucanVmOpCodes.OpEqual, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.NotEqual:
                EmitByteCode( ToucanVmOpCodes.OpNotEqual, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Less:
                EmitByteCode( ToucanVmOpCodes.OpLess, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.LessOrEqual:
                EmitByteCode( ToucanVmOpCodes.OpLessOrEqual, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Greater:
                EmitByteCode( ToucanVmOpCodes.OpGreater, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.GreaterOrEqual:
                EmitByteCode( ToucanVmOpCodes.OpGreaterEqual, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.And:
                EmitByteCode( ToucanVmOpCodes.OpAnd, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.Or:
                EmitByteCode( ToucanVmOpCodes.OpOr, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseOr:
                EmitByteCode( ToucanVmOpCodes.OpBitwiseOr, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseAnd:
                EmitByteCode( ToucanVmOpCodes.OpBitwiseAnd, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.BitwiseXor:
                EmitByteCode( ToucanVmOpCodes.OpBitwiseXor, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.ShiftLeft:
                EmitByteCode( ToucanVmOpCodes.OpBitwiseLeftShift, node.DebugInfoAstNode.LineNumberStart );

                break;

            case BinaryOperationBaseNode.BinaryOperatorType.ShiftRight:
                EmitByteCode( ToucanVmOpCodes.OpBitwiseRightShift, node.DebugInfoAstNode.LineNumberStart );

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public override object Visit( TernaryOperationBaseNode node )
    {
        Compile( node.RightOperand );
        Compile( node.MidOperand );
        Compile( node.LeftOperand );
        EmitByteCode( ToucanVmOpCodes.OpTernary, node.DebugInfoAstNode.LineNumberStart );

        return null;
    }

    public override object Visit( PrimaryBaseNode node )
    {
        switch ( node.PrimaryType )
        {
            case PrimaryBaseNode.PrimaryTypes.Identifier:
            {
                int d = 0;
                Symbol symbol = node.AstScopeNode.resolve( node.PrimaryId.Id, out int moduleId, ref d );
                AddToConstuctingByteCodeInstruction( moduleId );
                AddToConstuctingByteCodeInstruction( d );

                if ( symbol == null )
                {
                    throw new CompilerException(
                        $"Failed to resolve symbol '{node.PrimaryId.Id}' in scope '{node.AstScopeNode.Name}'",
                        node );
                }

                if ( symbol.SymbolScope is ClassSymbol s )
                {
                    AddToConstuctingByteCodeInstruction( s.InsertionOrderNumber );
                }
                else
                {
                    AddToConstuctingByteCodeInstruction( -1 );
                }

                AddToConstuctingByteCodeInstruction( symbol.InsertionOrderNumber );
                EndConstuctingByteCodeInstruction();

                return null;
            }

            case PrimaryBaseNode.PrimaryTypes.ThisReference:
                int d2 = 0;
                Symbol thisSymbol = node.AstScopeNode.resolve( "this", out int moduleId2, ref d2 );
                AddToConstuctingByteCodeInstruction( moduleId2 );
                AddToConstuctingByteCodeInstruction( d2 );
                AddToConstuctingByteCodeInstruction( thisSymbol.InsertionOrderNumber );
                EndConstuctingByteCodeInstruction();

                return null;

            case PrimaryBaseNode.PrimaryTypes.BooleanLiteral:
                EmitConstant( new ConstantValue( node.BooleanLiteral.Value ), node.DebugInfoAstNode.LineNumberStart );

                return null;

            case PrimaryBaseNode.PrimaryTypes.IntegerLiteral:
                EmitConstant( new ConstantValue( node.IntegerLiteral.Value ), node.DebugInfoAstNode.LineNumberStart );

                return null;

            case PrimaryBaseNode.PrimaryTypes.FloatLiteral:
                EmitConstant( new ConstantValue( node.FloatLiteral.Value ), node.DebugInfoAstNode.LineNumberStart );

                return null;

            case PrimaryBaseNode.PrimaryTypes.StringLiteral:
                EmitConstant( new ConstantValue( node.StringLiteral ), node.DebugInfoAstNode.LineNumberStart );

                return null;

            case PrimaryBaseNode.PrimaryTypes.InterpolatedString:
                int i = 0;

                foreach ( InterpolatedStringPart interpolatedStringStringPart in node.InterpolatedString.StringParts )
                {
                    EmitConstant( new ConstantValue( interpolatedStringStringPart.TextBeforeExpression ), node.DebugInfoAstNode.LineNumberStart );

                    if ( i > 0 )
                    {
                        EmitByteCode( ToucanVmOpCodes.OpAdd, node.DebugInfoAstNode.LineNumberStart );
                    }

                    Compile( interpolatedStringStringPart.ExpressionBaseNode );
                    EmitByteCode( ToucanVmOpCodes.OpAdd, node.DebugInfoAstNode.LineNumberStart );
                    i++;
                }

                EmitConstant( new ConstantValue( node.InterpolatedString.TextAfterLastExpression ) );
                EmitByteCode( ToucanVmOpCodes.OpAdd, node.DebugInfoAstNode.LineNumberStart );

                return null;

            case PrimaryBaseNode.PrimaryTypes.Expression:
                node.Expression.Accept( this );

                return null;

            case PrimaryBaseNode.PrimaryTypes.NullReference:
                object obj = null;
                EmitConstant( new ConstantValue(obj), node.DebugInfoAstNode.LineNumberStart );

                return null;

            case PrimaryBaseNode.PrimaryTypes.ArrayExpression:
                throw new NotImplementedException( "TODO" );

            case PrimaryBaseNode.PrimaryTypes.DictionaryExpression:
                throw new NotImplementedException( "TODO" );

            case PrimaryBaseNode.PrimaryTypes.Default:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof( node.PrimaryType ),
                    node.PrimaryType,
                    null );
        }
    }

    public override object Visit( StructDeclarationBaseNode node )
    {
        return null;
    }

    public override object Visit( UnaryPostfixOperation node )
    {
        m_IsCompilingPostfixOperation = true;
        int toFix = -1;

        if ( m_PostfixInstructions.Count == 0 )
        {
            toFix = EmitByteCode( ToucanVmOpCodes.OpNone, node.DebugInfoAstNode.LineNumberStart );
        }

        Compile( node.Primary );

        if ( toFix >= 0 && m_Context.CurrentChunk.Code[toFix + 1].OpCode == ToucanVmOpCodes.OpGetVar )
        {
            m_Context.CurrentChunk.Code[toFix] = new ByteCode(
                ToucanVmOpCodes.OpGetNextVarByRef );
        }

        m_IsCompilingPostfixOperation = false;

        switch ( node.Operator )
        {
            case UnaryPostfixOperation.UnaryPostfixOperatorType.PlusPlus:
                if ( m_PostfixInstructions.Count == 0 )
                {
                    EmitByteCode( ToucanVmOpCodes.OpPostfixIncrement, node.DebugInfoAstNode.LineNumberStart );
                }
                else
                {
                    if ( m_PostfixInstructions.Count > 0 )
                    {
                        m_PostfixInstructions.Peek().ByteCodes.Add( new ByteCode( ToucanVmOpCodes.OpPostfixIncrement ) );
                    }
                }

                break;

            case UnaryPostfixOperation.UnaryPostfixOperatorType.MinusMinus:
                if ( m_PostfixInstructions.Count == 0 )
                {
                    EmitByteCode( ToucanVmOpCodes.OpPostfixDecrement, node.DebugInfoAstNode.LineNumberStart );
                }
                else
                {
                    if ( m_PostfixInstructions.Count > 0 )
                    {
                        m_PostfixInstructions.Peek().ByteCodes.Add( new ByteCode( ToucanVmOpCodes.OpPostfixDecrement ) );
                    }
                }

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public override object Visit( UnaryPrefixOperation node )
    {
        Compile( node.Primary );

        switch ( node.Operator )
        {
            case UnaryPrefixOperation.UnaryPrefixOperatorType.Plus:
                EmitByteCode( ToucanVmOpCodes.OpAffirm, node.DebugInfoAstNode.LineNumberStart );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.Compliment:
                EmitByteCode( ToucanVmOpCodes.OpCompliment, node.DebugInfoAstNode.LineNumberStart );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.PlusPlus:
                EmitByteCode( ToucanVmOpCodes.OpPrefixIncrement, node.DebugInfoAstNode.LineNumberStart );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.MinusMinus:
                EmitByteCode( ToucanVmOpCodes.OpPrefixDecrement, node.DebugInfoAstNode.LineNumberStart );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.LogicalNot:
                EmitByteCode( ToucanVmOpCodes.OpNot, node.DebugInfoAstNode.LineNumberStart );

                break;

            case UnaryPrefixOperation.UnaryPrefixOperatorType.Negate:
                EmitByteCode( ToucanVmOpCodes.OpNegate, node.DebugInfoAstNode.LineNumberStart );

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public override object Visit( AstBaseNode baseNode )
    {
        switch ( baseNode )
        {
            case ProgramBaseNode program:
                return Visit( program );

            case ModuleBaseNode module:
                return Visit( module );

            case ClassDeclarationBaseNode classDeclarationNode:
                return Visit( classDeclarationNode );

            case StructDeclarationBaseNode structDeclaration:
                return Visit( structDeclaration );

            case FunctionDeclarationBaseNode functionDeclarationNode:
                return Visit( functionDeclarationNode );

            case LocalVariableDeclarationBaseNode localVar:
                return Visit( localVar );

            case LocalVariableInitializerBaseNode initializer:
                return Visit( initializer );

            case VariableDeclarationBaseNode variable:
                return Visit( variable );

            case ClassInstanceDeclarationBaseNode classInstance:
                return Visit( classInstance );

            case UsingStatementBaseNode usingStatementNode:
                return Visit( usingStatementNode );

            case ExpressionStatementBaseNode expressionStatementNode:
                return Visit( expressionStatementNode );

            case ForStatementBaseNode forStatementNode:
                return Visit( forStatementNode );

            case WhileStatementBaseNode whileStatement:
                return Visit( whileStatement );

            case IfStatementBaseNode ifStatementNode:
                return Visit( ifStatementNode );

            case ReturnStatementBaseNode returnStatement:
                return Visit( returnStatement );

            case BreakStatementBaseNode returnStatement:
                return Visit( returnStatement );

            case BlockStatementBaseNode blockStatementNode:
                return Visit( blockStatementNode );

            case SyncBlockNode syncBlockNode:
                return Visit( syncBlockNode );

            case AssignmentBaseNode assignmentNode:
                return Visit( assignmentNode );

            case CallBaseNode callNode:
                return Visit( callNode );

            case BinaryOperationBaseNode binaryOperation:
                return Visit( binaryOperation );

            case TernaryOperationBaseNode ternaryOperationNode:
                return Visit( ternaryOperationNode );

            case PrimaryBaseNode primaryNode:
                return Visit( primaryNode );

            case DeclarationsBaseNode declarationsNode:
                return Visit( declarationsNode );

            case UnaryPostfixOperation postfixOperation:
                return Visit( postfixOperation );

            case UnaryPrefixOperation prefixOperation:
                return Visit( prefixOperation );

            case StatementBaseNode stat:
                return Visit( stat );

            case ExpressionBaseNode expression:
                return Visit( expression );

            case InitializerBaseNode initializerNode:
                return Visit( initializerNode.Expression );

            default:
                return null;
        }
    }

    #endregion

    #region Private

    private void AddToConstuctingByteCodeInstruction( int opCodeData )
    {
        m_ConstructingOpCodeData.Add( opCodeData );
    }

    private void BeginConstuctingByteCodeInstruction( ToucanVmOpCodes byteCode, int line = 0 )
    {
        m_ConstructingOpCode = byteCode;
        m_ConstructingOpCodeData = new List < int >();
        m_ConstructingLine = line;
    }

    private object Compile( AstBaseNode astBaseNode )
    {
        return astBaseNode.Accept( this );
    }

    private int EmitByteCode( ToucanVmOpCodes byteCode, int line = 0 )
    {
        return m_Context.CurrentChunk.WriteToChunk( byteCode, line );
    }

    private int EmitByteCode( ToucanVmOpCodes byteCode, int opCodeData, int line )
    {
        ByteCode byCode = new ByteCode( byteCode, opCodeData );

        return m_Context.CurrentChunk.WriteToChunk( byCode, line );
    }

    private int EmitByteCode( ToucanVmOpCodes byteCode, int opCodeData, int opCodeData2, int line )
    {
        ByteCode byCode = new ByteCode( byteCode, opCodeData, opCodeData2 );

        return m_Context.CurrentChunk.WriteToChunk( byCode, line );
    }

    private int EmitByteCode( ByteCode byteCode, int line = 0 )
    {
        return m_Context.CurrentChunk.WriteToChunk( byteCode, line );
    }

    private int EmitByteCode( ToucanVmOpCodes byteCode, ConstantValue constantValue, int line = 0 )
    {
        return m_Context.CurrentChunk.WriteToChunk( byteCode, constantValue, line );
    }
    
    private int EmitByteCode( ToucanVmOpCodes byteCode, ConstantValue constantValue, int opCodeData, int line = 0 )
    {
        return m_Context.CurrentChunk.WriteToChunk( byteCode, constantValue, opCodeData,line );
    }

    private int EmitConstant( ConstantValue value, int line = 0 )
    {
        return EmitByteCode( ToucanVmOpCodes.OpConstant, value, line );
    }

    private int EmitReturn( int line = 0 )
    {
        return EmitByteCode( ToucanVmOpCodes.OpReturn, line );
    }

    private void EndConstuctingByteCodeInstruction()
    {
        ByteCode byCode = new ByteCode( m_ConstructingOpCode );
        byCode.OpCodeData = m_ConstructingOpCodeData.ToArray();
        m_Context.CurrentChunk.WriteToChunk( byCode, m_ConstructingLine );

        if ( m_PostfixInstructions.Count > 0 && m_IsCompilingPostfixOperation )
        {
            if ( m_ConstructingOpCode == ToucanVmOpCodes.OpGetVar )
            {
                ByteCode byCodeAlt = new ByteCode( ToucanVmOpCodes.OpGetNextVarByRef );
                m_PostfixInstructions.Peek().ByteCodes.Add( byCodeAlt );
            }

            m_PostfixInstructions.Peek().ByteCodes.Add( byCode );
        }

        m_ConstructingOpCodeData = null;
        m_ConstructingOpCode = ToucanVmOpCodes.OpNone;
    }

    #endregion
}

}
