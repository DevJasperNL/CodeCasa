﻿using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutoGenerated;

public partial record InputNumberEntity : NumericEntity<InputNumberEntity, NumericEntityState<InputNumberAttributes>, InputNumberAttributes>, IInputNumberEntityCore
{
    public InputNumberEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public InputNumberEntity(IEntityCore entity) : base(entity)
    {
    }
}