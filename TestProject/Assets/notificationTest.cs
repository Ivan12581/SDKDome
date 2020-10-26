using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class notificationTest : MonoBehaviour
{
    public void RegisterNotification()
    {
#if UNITY_IOS
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
        UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
        localNotification.fireDate = DateTime.Now.AddSeconds(10);
        localNotification.alertBody = "WDNMD";
        localNotification.applicationIconBadgeNumber = 1;
        localNotification.hasAction = true;
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
#endif
    }

}
