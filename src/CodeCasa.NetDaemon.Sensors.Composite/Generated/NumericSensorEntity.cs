using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Sensors.Composite.Generated
{
    internal partial record NumericSensorEntity : NumericEntity<NumericSensorEntity, NumericEntityState<NumericSensorAttributes>, NumericSensorAttributes>, ISensorEntityCore
    {
        public NumericSensorEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
        {
        }

        public NumericSensorEntity(IEntityCore entity) : base(entity)
        {
        }
    }
}
