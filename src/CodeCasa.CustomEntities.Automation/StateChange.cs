namespace CodeCasa.CustomEntities.Automation;

public record StateChange<T>(T Old, T New);