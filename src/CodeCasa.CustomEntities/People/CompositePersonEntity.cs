﻿using CodeCasa.AutoGenerated;
using NetDaemon.Extensions.Observables;
using NetDaemon.Notifications.InputSelect.Interact;
using NetDaemon.Notifications.Phone;
using System.Reactive.Linq;

namespace CodeCasa.CustomEntities.People;

public abstract class CompositePersonEntity(
    string name,
    Genders gender,
    InputSelectEntity stateInputSelectEntity,
    PersonEntity personEntity,
    IInputSelectNotificationEntity dashboardNotifications,
    PhoneNotificationEntity phoneNotifications)
{
    public string Name { get; } = name;
    public Genders Gender { get; } = gender;
    public InputSelectEntity StateInputSelectEntity { get; } = stateInputSelectEntity;
    public PersonEntity PersonEntity { get; } = personEntity;
    public IInputSelectNotificationEntity Dashboard { get; } = dashboardNotifications;
    public PhoneNotificationEntity Phone { get; } = phoneNotifications;

    public PersonStates? State
    {
        get => StateValueToPersonState(StateInputSelectEntity.State);
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            StateInputSelectEntity.SelectOption(PersonStateMappings.PeopleStatesToStateValues[value.Value]);
        }
    }

    public PersonEntityStates? EntityState => StateValueToPersonEntityState(PersonEntity.State);

    private static PersonEntityStates? StateValueToPersonEntityState(string? stateValue)
    {
        return stateValue == null ||
               !PersonStateMappings.StateValuesToPeopleEntityStates.TryGetValue(stateValue,
                   out var state)
            ? null
            : state;
    }

    public bool Home => State != PersonStates.Away;

    public IObservable<PersonStates> CreateStateObservable() =>
        StateInputSelectEntity.Stateful()
            .Select(state => StateValueToPersonState(state.New!.State))
            .Where(s => s != null)
            .Select(s => s!.Value);

    public IObservable<StateChange<PersonStates?>> CreateStateChangeObservable() =>
        StateInputSelectEntity.Stateful()
            .Select(state => new StateChange<PersonStates?>(StateValueToPersonState(state.Old?.State), StateValueToPersonState(state.New?.State)));

    public IObservable<bool> CreateStateEqualsObservable(PersonStates personState) =>
        StateInputSelectEntity.ToBooleanObservable(state => StateValueToPersonState(state.State) == personState);

    public IObservable<bool> CreateHomeObservable() =>
        PersonEntity.ToBooleanObservable(state => StateValueToPersonEntityState(state.State) == PersonEntityStates.Home);

    private static PersonStates? StateValueToPersonState(string? stateValue)
    {
        return stateValue == null ||
               !PersonStateMappings.StateValuesToPeopleStates.TryGetValue(stateValue,
                   out var state)
            ? null
            : state;
    }
}