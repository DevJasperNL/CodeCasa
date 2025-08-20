using NetDaemon.HassModel.Entities;
using System.Reactive.Linq;
using System.Text.Json;

namespace CodeCasa.NetDaemon.Utilities.Extensions
{
    public static class EntityExtensions
    {
        public static IObservable<TParsed> StateAllChangesDeserialized<TParsed>(
            this Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return entity
                .StateAllChangesWithCurrent()
                .Where(s => s.New != null && s.New.AttributesJson != null)
                .Select(s => JsonSerializer.Deserialize<TParsed>(s.New!.AttributesJson!.Value.GetRawText()))
                .Where(p => p != null).Select(p => p!);
        }
    }
}
