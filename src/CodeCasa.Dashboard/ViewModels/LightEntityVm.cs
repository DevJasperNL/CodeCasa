﻿using CodeCasa.AutoGenerated;
using CodeCasa.AutoGenerated.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.Dashboard.ViewModels
{
    public class LightEntityVm
    {
        private readonly LightEntity _lightEntity;

        public LightEntityVm (LightEntity lightEntity)
        {
            _lightEntity = lightEntity;
        }

        public bool IsOn
        {
            get => _lightEntity.IsOn();
            set
            {
                if (value)
                {
                    _lightEntity.TurnOn();
                }
                else
                {
                    _lightEntity.TurnOff();
                }
            }
        }
    }
}
