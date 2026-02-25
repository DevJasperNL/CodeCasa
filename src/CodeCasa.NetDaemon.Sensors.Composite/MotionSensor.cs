using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.NetDaemon.Extensions.Observables;
using CodeCasa.NetDaemon.Sensors.Composite.Generated;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Sensors.Composite
{
    /// <summary>
    /// Provides a base implementation for a reactive motion sensor that integrates illuminance data.
    /// </summary>
    /// <remarks>
    /// This class handles the logic for persistent motion detection, ensuring that motion triggers 
    /// are light-sensitive upon activation but remain active regardless of light changes until motion ceases.
    /// </remarks>
    public abstract class MotionSensor : IObservable<bool>
    {
        private readonly IScheduler _scheduler;
        private readonly BinarySensorEntity _binarySensorEntity;
        private readonly NumericSensorEntity _numericSensorEntity;

        private readonly IObservable<bool> _defaultObservable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MotionSensor"/> class.
        /// </summary>
        /// <param name="scheduler">The scheduler used for time-based operations.</param>
        /// <param name="motionOccupancySensor">The occupancy sensor entity core.</param>
        /// <param name="motionIlluminanceLuxSensor">The illuminance sensor entity core.</param>
        protected MotionSensor(IScheduler scheduler,
            IBinarySensorEntityCore motionOccupancySensor,
            ISensorEntityCore motionIlluminanceLuxSensor)
        {
            _scheduler = scheduler;
            _binarySensorEntity = new BinarySensorEntity(motionOccupancySensor);
            _numericSensorEntity = new NumericSensorEntity(motionIlluminanceLuxSensor);

            MotionOccupancySensor = motionOccupancySensor;
            MotionIlluminanceLuxSensor = motionIlluminanceLuxSensor;

            _defaultObservable = CreatePersistentMotionObservable();
        }

        /// <summary>
        /// Gets the occupancy sensor entity core.
        /// </summary>
        public IBinarySensorEntityCore MotionOccupancySensor { get; }

        /// <summary>
        /// Gets the illuminance sensor entity core.
        /// </summary>
        public ISensorEntityCore MotionIlluminanceLuxSensor { get; }

        /// <summary>
        /// An event stream that fires once when the motion criteria are first met (low light and movement).
        /// </summary>
        /// <remarks>
        /// Emits a <see cref="Unit"/> when the persistent motion state transitions from <c>false</c> to <c>true</c>.
        /// </remarks>
        public IObservable<Unit> Triggered => _defaultObservable.Where(b => b).Select(_ => Unit.Default);

        /// <summary>
        /// An event stream that fires once when the motion state is reset.
        /// </summary>
        /// <remarks>
        /// Emits a <see cref="Unit"/> when the motion sensor's <c>offDelay</c> has expired, 
        /// signaling that occupancy is no longer detected.
        /// </remarks>
        public IObservable<Unit> Cleared => _defaultObservable.Where(b => !b).Select(_ => Unit.Default);

        /// <summary>
        /// Gets an observable representing the motion state from the occupancy sensor.
        /// </summary>
        public IObservable<bool> Motion => _binarySensorEntity.ToBooleanObservable();

        /// <summary>
        /// Creates an observable that tracks motion persistence based on a brightness threshold.
        /// </summary>
        /// <param name="brightnessThreshold">The maximum brightness level allowed to initially trigger the motion state.</param>
        /// <param name="offDelay">The duration to keep the motion state active after the sensor stops detecting movement. Defaults to 60 seconds.</param>
        /// <returns>
        /// An <see cref="IObservable{T}"/> that emits <c>true</c> when motion is detected under the brightness threshold, 
        /// and remains <c>true</c> until the motion <paramref name="offDelay"/> expires.
        /// </returns>
        /// <remarks>
        /// This method implements a "latch" logic: the observable only flips to <c>true</c> if both motion is detected 
        /// AND brightness is low. However, once triggered, it stays <c>true</c> even if brightness increases, 
        /// until the motion sensor itself resets.
        /// </remarks>
        public IObservable<bool> CreatePersistentMotionObservable(double brightnessThreshold = 5, TimeSpan? offDelay = null)
        {
            offDelay ??= TimeSpan.FromSeconds(60);

            var motionLastXTime =
                _binarySensorEntity.PersistOnFor(offDelay.Value, _scheduler);

            var brightnessLessThanX = _numericSensorEntity
                .ToBooleanObservable(s => s.State <= brightnessThreshold);

            var triggered = false;
            return motionLastXTime.CombineLatest(brightnessLessThanX, (motionTriggered, brightnessTriggered) =>
            {
                if (motionTriggered && brightnessTriggered)
                {
                    triggered = true;
                }
                else if (!motionTriggered)
                {
                    triggered = false;
                }

                return triggered;
            }).DistinctUntilChanged();
        }

        /// <summary>
        /// Subscribes an observer to the default persistent motion stream.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        public IDisposable Subscribe(IObserver<bool> observer) => _defaultObservable.Subscribe(observer);
    }
}
