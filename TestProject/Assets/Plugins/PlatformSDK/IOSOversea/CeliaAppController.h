
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 100000
#define XC8_AVAILABLE 1
#import <UserNotifications/UserNotifications.h>
#import "NotificationDelegate.h"
#endif

@interface AppDelegate : UIResponder <UIApplicationDelegate>{
    
#if XC8_AVAILABLE
    NotificationDelegate *notificationDelegate;
#endif
}

@property (strong, nonatomic) UIWindow *window;


@end
