using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode
{
    /// <summary>
    /// Represents the context for dimming operations, containing the ordered output parameters of all dimmer nodes.
    /// </summary>
    /// <param name="dimmerNodeOutputParametersInOrder">An array of tuples containing entity IDs and their current light parameters, ordered for dimming operations.</param>
    public class DimmingContext(
        (string entityId, LightParameters? parametersAfterDim)[] dimmerNodeOutputParametersInOrder)
    {
        /// <summary>
        /// Gets the ordered output parameters of all dimmer nodes, used to coordinate dimming behavior across multiple lights.
        /// </summary>
        public (string entityId, LightParameters? parametersAfterDim)[] DimmerNodeOutputParametersInOrder { get; } = dimmerNodeOutputParametersInOrder;
    }

    /// <summary>
    /// A light transition node that handles dimming and brightening operations in response to dimmer input.
    /// </summary>
    internal class ReactiveDimmerNode : LightTransitionNode
    {
        private readonly int _minBrightness;
        private readonly int _brightnessStep;
        private int _dimSteps; // negative is dimming, positive is brightening.

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveDimmerNode"/> class.
        /// </summary>
        /// <param name="reactiveNode">The reactive node to monitor for state changes.</param>
        /// <param name="lightEntityId">The entity ID of the light this dimmer node controls.</param>
        /// <param name="minBrightness">The minimum brightness level before the light turns off.</param>
        /// <param name="brightnessStep">The step size for each dimming/brightening increment.</param>
        /// <param name="scheduler">The scheduler used for timing operations.</param>
        public ReactiveDimmerNode(
            ReactiveNode reactiveNode,
            string lightEntityId,
            int minBrightness, 
            int brightnessStep, 
            IScheduler scheduler) : base(scheduler)
        {
            reactiveNode.NodeChanged.Subscribe(_ => Reset());
            PassThrough = true;

            LightEntityId = lightEntityId;
            _minBrightness = minBrightness;
            _brightnessStep = brightnessStep;
        }

        /// <summary>
        /// Gets the entity ID of the light this dimmer node controls.
        /// </summary>
        public string LightEntityId { get; }

        /// <summary>
        /// Resets the dimming state back to pass-through mode.
        /// </summary>
        public void Reset()
        {
            PassThrough = true;
            _dimSteps = 0;
        }

        /// <inheritdoc />
        protected override void InputReceived(LightTransition? input)
        {
            if (input == null)
            {
                Output = null;
                return;
            }

            var newBrightness = CalculateBrightness(input.LightParameters.Brightness ?? 0);
            Output = input with { LightParameters = input.LightParameters with { Brightness = newBrightness } };
        }

        /// <summary>
        /// Executes one step of dimming in response to dimmer input.
        /// </summary>
        /// <param name="context">The dimming context containing current light parameters.</param>
        public void DimStep(DimmingContext context)
        {
            if (!ShouldDim(context))
            {
                return;
            }
            _dimSteps--;
            if (_dimSteps == 0)
            {
                PassThrough = true;
                return;
            }

            ScheduleInterpolatedLightTransition();
        }

        /// <summary>
        /// Executes one step of brightening in response to dimmer input.
        /// </summary>
        /// <param name="context">The dimming context containing current light parameters.</param>
        public void BrightenStep(DimmingContext context)
        {
            if (!ShouldBrighten(context))
            {
                return;
            }
            _dimSteps++;
            if (_dimSteps == 0)
            {
                PassThrough = true;
                return;
            }

            ScheduleInterpolatedLightTransition();
        }

        private bool ShouldDim(DimmingContext context)
        {
            var subjectParameters = context.DimmerNodeOutputParametersInOrder.Single(x => x.entityId == LightEntityId).parametersAfterDim;
            var subjectBrightness = subjectParameters?.Brightness ?? 0;
            if (subjectBrightness > _minBrightness)
            {
                // If we are brighter than minimum brightness, we have to dim anyway.
                return true;
            }
            if (subjectBrightness <= 0)
            {
                return false;
            }
            // At this point we are at min brightness and have to check if any other light is going to dim. If so, we don't have to.
            string? lightToTurnOff = null;
            foreach (var (entityId, parametersAfterDim) in context.DimmerNodeOutputParametersInOrder)
            {
                var brightness = parametersAfterDim?.Brightness ?? 0;
                if (brightness == 0)
                {
                    continue;
                }
                if (brightness > _minBrightness)
                {
                    // If any light is brighter than MinBrightness, we let them dim first.
                    return false;
                }

                lightToTurnOff ??= entityId;
            }

            return lightToTurnOff == LightEntityId;
        }

        private bool ShouldBrighten(DimmingContext context)
        {
            var subjectParameters = context.DimmerNodeOutputParametersInOrder.Single(x => x.entityId == LightEntityId).parametersAfterDim;
            var subjectBrightness = subjectParameters?.Brightness ?? 0;
            if (subjectBrightness > _minBrightness)
            {
                // If we are brighter than minimum brightness, we have to brighten anyway.
                return subjectBrightness < byte.MaxValue;
            }
            // At this point we are either off or at min brightness and have to check if any other light is going to turn on. If so, we don't have to turn on or brighten.
            string? lightToTurnOn = null;
            foreach (var (entityId, parametersAfterDim) in context.DimmerNodeOutputParametersInOrder.Reverse())
            {
                var brightness = parametersAfterDim?.Brightness ?? 0;
                if (brightness >= _minBrightness) // On
                {
                    if (lightToTurnOn != null || brightness > _minBrightness)
                    {
                        return false;
                    }
                    continue;
                }

                lightToTurnOn ??= entityId;
            }

            return lightToTurnOn == null || lightToTurnOn == LightEntityId;
        }

        private void ScheduleInterpolatedLightTransition()
        {
            if (Input == null)
            {
                Output = new LightParameters { Brightness = CalculateBrightness(0) }.AsTransition();
            }
            else
            {
                ScheduleInterpolatedLightTransitionUsingInputTransitionTime(
                    InputLightSourceParameters == null
                        ? null
                        : InputLightSourceParameters with
                        {
                            Brightness = CalculateBrightness(InputLightSourceParameters.Brightness ?? 0)
                        },
                    Input.LightParameters with
                    {
                        Brightness = CalculateBrightness(Input.LightParameters.Brightness ?? 0)
                    });
            }
        }

        private double? CalculateBrightness(double inputBrightness)
        {
            var calculatedBrightness = inputBrightness + _brightnessStep * _dimSteps;
            if (_dimSteps < 0)
            {
                // Make sure we always show minimum brightness before turning off.
                if (calculatedBrightness <= _minBrightness && calculatedBrightness + _brightnessStep > _minBrightness)
                {
                    return _minBrightness;
                }
            }
            if (_dimSteps > 0)
            {
                // Make sure we always show minimum brightness after turning on.
                if (calculatedBrightness > _minBrightness && calculatedBrightness - _brightnessStep <= _minBrightness)
                {
                    return _minBrightness;
                }
            }
            return Math.Min(byte.MaxValue, Math.Max(0, calculatedBrightness));
        }
    }
}
