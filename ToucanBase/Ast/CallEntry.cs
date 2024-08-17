﻿using System.Collections.Generic;

namespace Toucan.Ast
{

public class CallEntry
{
    public PrimaryBaseNode PrimaryBase;
    public ArgumentsBaseNode ArgumentsBase;
    public List < CallElementEntry > ElementAccess;
    public bool IsFunctionCall;
}

}
