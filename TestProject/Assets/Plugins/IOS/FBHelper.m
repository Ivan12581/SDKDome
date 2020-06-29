//
//  FBHelper.m
//  Unity-iPhone
//
//  Created by mini on 6/24/20.
//

#import "FBHelper.h"
#import "IOSBridgeHelper.h"

@implementation FBHelper
//  AppDelegate.m
- (BOOL)application:(UIApplication *)application
    didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
  NSLog(@"-ios---FBHelper---application--111--");
  // You can skip this line if you have the latest version of the SDK installed
  [[FBSDKApplicationDelegate sharedInstance] application:application
    didFinishLaunchingWithOptions:launchOptions];
  // Add any custom logic here.
  return YES;
}

- (BOOL)application:(UIApplication *)application
            openURL:(NSURL *)url
            options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
  NSLog(@"-ios---FBHelper---application--222--");
  BOOL handled = [[FBSDKApplicationDelegate sharedInstance] application:application
    openURL:url
    sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey]
    annotation:options[UIApplicationOpenURLOptionsAnnotationKey]
  ];
  // Add any custom logic here.
  return handled;
}

//******************************************************
//****************FaceBook Share
//******************************************************
- (void)facebookShareWithMessage:(id)message {
    NSData *data = [message dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *dictionary = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:nil];
    
    NSString *contentUrlString = dictionary[@"content_url"];
//    NSString *imageUrlString = dictionary[@"image_url"];
//    NSString *description = dictionary[@"description"];
//    NSString *title = dictionary[@"title"];
    NSString *quote = dictionary[@"quote"];
    
    FBSDKShareLinkContent *content = [[FBSDKShareLinkContent alloc] init];
    content.contentURL = [NSURL URLWithString:contentUrlString];
//    content.imageURL = [NSURL URLWithString:imageUrlString];
//    content.contentDescription = description;
//    content.contentTitle = title;
    content.quote = quote;
    
    FBSDKShareDialog *dialog = [[FBSDKShareDialog alloc] init];
    dialog.shareContent = content;
    dialog.fromViewController = self;
    dialog.delegate = self;
    dialog.mode = FBSDKShareDialogModeNative;
    [dialog show];
}

#pragma mark - FaceBook Share Delegate
- (void)sharer:(id<FBSDKSharing>)sharer didCompleteWithResults:(NSDictionary *)results {
    NSString *postId = results[@"postId"];
    FBSDKShareDialog *dialog = (FBSDKShareDialog *)sharer;
    if (dialog.mode == FBSDKShareDialogModeBrowser && (postId == nil || [postId isEqualToString:@""])) {
        // 如果使用webview分享的，但postId是空的，
        // 这种情况是用户点击了『完成』按钮，并没有真的分享
        NSLog(@"share Cancel");
    } else {
        NSLog(@"share Success");
    }
    
}

- (void)sharer:(id<FBSDKSharing>)sharer didFailWithError:(NSError *)error {
    FBSDKShareDialog *dialog = (FBSDKShareDialog *)sharer;
    if (error == nil && dialog.mode == FBSDKShareDialogModeNative) {
        // 如果使用原生登录失败，但error为空，那是因为用户没有安装Facebook app
        // 重设dialog的mode，再次弹出对话框
        dialog.mode = FBSDKShareDialogModeBrowser;
        [dialog show];
    } else {
        NSLog(@"share Failure");
    }
}

- (void)sharerDidCancel:(id<FBSDKSharing>)sharer {
    NSLog(@"sahre Cancel");
}
@end
