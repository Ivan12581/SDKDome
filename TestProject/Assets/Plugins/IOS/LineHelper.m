//
//  LineHelper.m
//  Unity-iPhone
//
//  Created by mini on 8/6/20.
//

#import "LineHelper.h"

@implementation LineHelper
static LineHelper *LineHelperIns = nil;
+(LineHelper*)sharedInstance{
    if (LineHelperIns == nil) {
        LineHelperIns = [LineHelper new];
    }
    return LineHelperIns;
}

-(void)InitSDK{
//    //https://github.com/SoberTong/LineDemoIos/blob/master/LineDemoIos/ViewController.m
//    [LineSDKLogin sharedInstance].delegate = self;
//    apiClient = [[LineSDKAPI alloc] initWithConfiguration:[LineSDKConfiguration defaultConfig]];
}

-(void)Login{

//    [[LineSDKLogin sharedInstance] startLogin]
}
//- (void)didLogin:(LineSDKLogin *)login
//      credential:(LineSDKCredential *)credential
//         profile:(LineSDKProfile *)profile
//           error:(NSError *)error {
//    if (error) {
//        NSLog(@"Login error. info: %@", error.description);
//    }else {
//        NSString *accessToken = credential.accessToken.accessToken;
//        NSLog(@"Login success. accessToken: %@", accessToken);
//
//        NSString * userID = profile.userID;
//        NSString * displayName = profile.displayName;
//        NSString * statusMessage = profile.statusMessage;
//        NSURL * pictureURL = profile.pictureURL;
//
//        NSString * pictureUrlString;
//
//        // If the user does not have a profile picture set, pictureURL will be nil
//        if (pictureURL) {
//            pictureUrlString = profile.pictureURL.absoluteString;
//        }
//        NSLog(@"Login success. userID: %@; displayName: %@; statusMessage: %@; pictureUrlStringios: %@", userID, displayName, statusMessage, pictureUrlString);
//    }
//}
- (void)logout{
//    [apiClient logoutWithCompletion:^(BOOL success, NSError * _Nullable error) {
//        if (success) {
//            NSLog(@"Logout success.");
//        }else {
//            NSLog(@"Logout error. info: %@", error.description);
//        }
//    }];
//    /*
//    [apiClient logoutWithCallbackQueue:dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0)
//        completion:^(BOOL success, NSError * _Nullable error) {
//            if (success) {
//                NSLog(@"Logout success.");
//            }else {
//                NSLog(@"Logout error. info: %@", error.description);
//            }
//        }];
//     */
}
- (BOOL)shareMessage:(NSString *)message
{

    NSString *contentType = @"text";
    NSString *urlString = [NSString stringWithFormat:@"line://msg/%@/%@",contentType, message];
    NSString *characterString = [urlString stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
//    [urlString stringByAddingPercentEncodingWithAllowedCharacters:urlString];
    NSURL *url = [NSURL URLWithString:characterString];
    
    if ([[UIApplication sharedApplication]canOpenURL:url]) {
        return [[UIApplication sharedApplication]openURL:url];
    }else{
        //如果使用者沒有安裝，連結到App Store
//        NSURL *itunesURL = [NSURL URLWithString:@"itms-apps://itunes.apple.com/app/id443904275"];
//        [[UIApplication sharedApplication] openURL:itunesURL];
        return NO;
    }
}
- (BOOL)sharePicture:(NSString *)pictureUrl
{
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];

    NSData *data = [NSData dataWithContentsOfURL:[NSURL URLWithString:pictureUrl]];
    UIImage *image = [UIImage imageWithData:data];
    [pasteboard setData:UIImageJPEGRepresentation(image, 0.9) forPasteboardType:@"public.jpeg"];
    NSString *contentType =@"image";

    NSString *contentKey = [pasteboard.name stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
//    NSString *contentKey = [pasteboard.name stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    
    NSString *urlString = [NSString stringWithFormat:@"line://msg/%@/%@",contentType, contentKey];
    NSURL *url = [NSURL URLWithString:urlString];
    
    if ([[UIApplication sharedApplication] canOpenURL:url]) {
        return [[UIApplication sharedApplication] openURL:url];
    }else{
        //如果使用者沒有安裝，連結到App Store
        //        NSURL *itunesURL = [NSURL URLWithString:@"itms-apps://itunes.apple.com/app/id443904275"];
        //        [[UIApplication sharedApplication] openURL:itunesURL];
        return NO;
    }

}


@end
