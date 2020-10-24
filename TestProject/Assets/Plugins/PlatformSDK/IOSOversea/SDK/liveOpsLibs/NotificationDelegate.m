#import <foundation/foundation.h>
#import "NotificationDelegate.h"
#import <LiveOps/LiveOps.h>


@implementation NotificationDelegate

- (void)userNotificationCenter:(UNUserNotificationCenter *)center
didReceiveNotificationResponse:(UNNotificationResponse *)response
         withCompletionHandler:(void(^)(void))completionHandler {
    
    [LiveOpsPush handleUserNotificationCenter:center
               didReceiveNotificationResponse:response];
    completionHandler();
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center
       willPresentNotification:(UNNotification *)notification
         withCompletionHandler:(void (^)(UNNotificationPresentationOptions))completionHandler {
    
    NSDictionary* userInfo = [notification.request.content.userInfo mutableCopy];
    [LiveOpsPush handleUserNotificationCenter:center
                      willPresentNotification:notification
                willShowSystemForegroundAlert:YES];
    
    
    completionHandler(UNNotificationPresentationOptionAlert | UNNotificationPresentationOptionBadge | UNNotificationPresentationOptionSound);
}

@end
