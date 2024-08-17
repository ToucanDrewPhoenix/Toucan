﻿using System.Collections.Generic;

namespace Toucan.Symbols
{

public class ParametersSymbol : BaseSymbol
{
    public List < ParameterSymbol > ParameterSymbols;

    #region Public

    public ParametersSymbol( string name ) : base( name )
    {
    }

    #endregion
}

}
