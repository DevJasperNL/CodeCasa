﻿using CodeCasa.AutoGenerated;
using CodeCasa.Pipeline;
using NetDaemon.AppModel;
using NetDaemon.Extensions.Observables;

namespace CodeCasa.Automations.Test
{
    [NetDaemonApp]
    internal class PipelineTest
    {
        public PipelineTest(IPipeline<string> messagesPipeline)
        {
            messagesPipeline
                .RegisterNode<DefaultMessageNode>()
                .RegisterNode<OfficeLightsMessageNode>()
                .RegisterNode<CherryOnTopMessageNode>()
                .SetOutputHandler(Console.WriteLine);
        }
    }

    public class DefaultMessageNode : PipelineNode<string>
    {
        public DefaultMessageNode()
        {
            Output = "Default message";
        }
    }

    public class OfficeLightsMessageNode : PipelineNode<string>
    {
        public OfficeLightsMessageNode(LightEntities lightEntities)
        {
            lightEntities.OfficeLights.SubscribeOnOff(() => Output = "ON", DisableNode);
        }
    }

    public class CherryOnTopMessageNode : PipelineNode<string>
    {
        protected override void InputReceived(string? state)
        {
            Output = state == null ? "Just a cherry" : $"{state} with a cherry on top";
        }
    }
}
