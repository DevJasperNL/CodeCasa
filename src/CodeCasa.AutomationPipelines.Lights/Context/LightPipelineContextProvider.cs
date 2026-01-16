using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context
{
    internal class LightPipelineContextProvider
    {
        private ILightPipelineContext<ILight>? _lightPipelineContext;

        public ILightPipelineContext<ILight> GetLightPipelineContext()
        {
            return _lightPipelineContext ?? throw new InvalidOperationException("Current context not set.");
        }

        public void SetLightPipelineContext<TLight>(ILightPipelineContext<TLight> context) where TLight : ILight
        {
            _lightPipelineContext = (ILightPipelineContext<ILight>)context;
        }

        public void ResetLight()
        {
            _lightPipelineContext = null;
        }
    }
}
