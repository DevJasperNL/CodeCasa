﻿using CodeCasa.AutoGenerated;
using CodeCasa.CustomEntities.Notifications.Dashboards;
using CodeCasa.CustomEntities.Notifications.Phones;

namespace CodeCasa.CustomEntities.People;

public class Jane(
    InputSelectEntities inputSelectEntities,
    PersonEntities personEntities,
    JaneDashboardNotifications janeDashboardNotifications,
    JanePhoneNotifications janePhoneNotifications)
    : CompositePersonEntity("Jane",
        Genders.Female,
        inputSelectEntities.JaneDoeState,
        personEntities.JaneDoe,
        janeDashboardNotifications,
        janePhoneNotifications);