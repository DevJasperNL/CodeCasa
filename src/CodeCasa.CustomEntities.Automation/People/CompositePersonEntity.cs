﻿using NetDaemon.Extensions.Observables;
using NetDaemon.Notifications.InputSelect.Interact;
using NetDaemon.Notifications.Phone;
using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;
using CodeCasa.NetDaemon.Utilities.Entities;

namespace CodeCasa.CustomEntities.Automation.People;

public abstract class CompositePersonEntity(
    string name,
    Genders gender,
    PersonStateEntity personStateEntity,
    TypeSafePersonEntity personEntity,
    IInputSelectNotificationEntity dashboardNotifications,
    PhoneNotificationEntity phoneNotifications)
{
    public string Name { get; } = name;
    public Genders Gender { get; } = gender;
    public PersonStateEntity PersonStateEntity { get; } = personStateEntity;
    public TypeSafePersonEntity PersonEntity { get; } = personEntity;
    public IInputSelectNotificationEntity Dashboard { get; } = dashboardNotifications;
    public PhoneNotificationEntity Phone { get; } = phoneNotifications;

    public PersonStates? State
    {
        get => PersonStateEntity.State;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            PersonStateEntity.SelectOption(value.Value);
        }
    }

    public PersonEntityStates? EntityState => PersonEntity.State;

    public bool Home => State != PersonStates.Away;

    public IObservable<PersonStates> CreateStateObservable() =>
        PersonStateEntity.StateChangesWithCurrent()
            .Select(state => state.New!.State)
            .Where(s => s != null)
            .Select(s => s!.Value);

    public IObservable<StateChange<PersonStates?>> CreateStateChangeObservable() =>
        PersonStateEntity.StateChangesWithCurrent()
            .Select(state => new StateChange<PersonStates?>(state.Old?.State, state.New?.State));

    public IObservable<bool> CreateStateEqualsObservable(PersonStates personState) =>
        PersonStateEntity.ToBooleanObservable(state => state.State == personState);

    public IObservable<bool> CreateHomeObservable() =>
        PersonEntity.ToBooleanObservable(state => state.State == PersonEntityStates.Home);
}