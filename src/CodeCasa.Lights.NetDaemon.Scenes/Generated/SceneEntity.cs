using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.Lights.NetDaemon.Scenes.Generated
{
    internal record SceneEntity : Entity<SceneEntity, EntityState<SceneAttributes>, SceneAttributes>, ISceneEntityCore
    {
        public SceneEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
        {
        }

        public SceneEntity(IEntityCore entity) : base(entity)
        {
        }
    }
}
