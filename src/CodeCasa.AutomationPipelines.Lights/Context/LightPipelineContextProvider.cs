namespace CodeCasa.AutomationPipelines.Lights.Context
{
    internal class LightPipelineContextProvider
    {
        private ILightPipelineContext? _lightPipelineContext;

        public ILightPipelineContext GetLightPipelineContext()
        {
            return _lightPipelineContext ?? throw new InvalidOperationException("Current context not set.");
        }

        public void SetLightPipelineContext(ILightPipelineContext context)
        {
            _lightPipelineContext = context;
        }

        public void ResetLight()
        {
            _lightPipelineContext = null;
        }
    }
}
