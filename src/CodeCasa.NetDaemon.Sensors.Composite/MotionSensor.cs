using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.NetDaemon.Extensions.Observables;
using CodeCasa.NetDaemon.Sensors.Composite.Generated;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Sensors.Composite
{
    public abstract class MotionSensor : IObservable<bool>
    {
        private readonly IScheduler _scheduler;
        private readonly IObservable<bool> _defaultObservable;

        protected MotionSensor(IScheduler scheduler,
            IBinarySensorEntityCore motionOccupancySensor,
            ISensorEntityCore motionIlluminanceLuxSensor)
        {
            _scheduler = scheduler;
            MotionOccupancySensor = motionOccupancySensor;
            MotionIlluminanceLuxSensor = motionIlluminanceLuxSensor;

            _defaultObservable = CreateMotionObservable();
        }

        public IBinarySensorEntityCore MotionOccupancySensor { get; }
        public ISensorEntityCore MotionIlluminanceLuxSensor { get; }
        public IObservable<Unit> Triggered => _defaultObservable.Where(b => b).Select(_ => Unit.Default);
        public IObservable<Unit> Cleared => _defaultObservable.Where(b => !b).Select(_ => Unit.Default);

        public IObservable<bool> CreateMotionObservable(double brightnessThreshold = 5, TimeSpan? offDelay = null)
        {
            offDelay ??= TimeSpan.FromSeconds(60);

            var motionLastXTime =
                new BinarySensorEntity(MotionOccupancySensor).PersistOnFor(offDelay.Value, _scheduler);

            var brightnessLessThanX = new NumericSensorEntity(MotionIlluminanceLuxSensor)
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

        public IDisposable Subscribe(IObserver<bool> observer) => _defaultObservable.Subscribe(observer);
    }
}
