﻿using DddCore.Contracts.Crosscutting.Base;

namespace DddCore.Contracts.Crosscutting.Ioc.Base
{
    public interface IIocBootstrapper : IBootstrapper<IContainer, IContainerConfig, IIocModule>
    {
    }
}