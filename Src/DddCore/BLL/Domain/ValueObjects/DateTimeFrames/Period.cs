﻿using System.Collections.Generic;
using DddCore.BLL.Domain.ValueObjects;

namespace DddCore.BLL.Domain
{
    public class Period : ValueObjectBase<Period>
    {
        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            throw new System.NotImplementedException();
        }
    }
}