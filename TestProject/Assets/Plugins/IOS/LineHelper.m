//
//  LineHelper.m
//  Unity-iPhone
//
//  Created by mini on 8/6/20.
//

#import "LineHelper.h"
@implementation LineHelper{
        id IOSBridgeHelper;
}
static LineHelper *LineHelperIns = nil;
+(LineHelper*)sharedInstance{
    if (LineHelperIns == nil) {
        LineHelperIns = [LineHelper new];
    }
    return LineHelperIns;
}

-(void)InitSDK:(id<cDelegate>) delegate{
    IOSBridgeHelper = delegate;
//    //https://github.com/SoberTong/LineDemoIos/blob/master/LineDemoIos/ViewController.m
//    [LineSDKLogin sharedInstance].delegate = self;
//    apiClient = [[LineSDKAPI alloc] initWithConfiguration:[LineSDKConfiguration defaultConfig]];
    NSLog(@"--ElvaHelper---InitSDK---");
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
-(void)share:(const char*) jsonData{
        UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    NSString * pictureUrl = @"https://image.baidu.com/search/detail?z=0&word=%E7%8F%8D%E7%8F%8DOo&hs=0&pn=2&spn=0&di=0&pi=533454875148338836&tn=baiduimagedetail&is=0%2C0&ie=utf-8&oe=utf-8&cs=3765407342%2C2815074957&os=1357259984%2C3887820448&simid=&adpicid=0&lpn=0&fm=&sme=&cg=&bdtype=-1&oriquery=&objurl=http%3A%2F%2Ft7.baidu.com%2Fit%2Fu%3D3616242789%2C1098670747%26fm%3D79%26app%3D86%26f%3DJPEG%3Fw%3D900%26h%3D1350&fromurl=&gsm=30000000003&catename=pcindexhot&islist=&querylist=";
        NSData *data = [NSData dataWithContentsOfURL:[NSURL URLWithString:pictureUrl]];
        UIImage *image = [UIImage imageWithData:data];
        [pasteboard setData:UIImageJPEGRepresentation(image, 0.9) forPasteboardType:@"public.jpeg"];
        NSString *contentType =@"image";

        NSString *contentKey = [pasteboard.name stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
        
        NSString *urlString = [NSString stringWithFormat:@"line://msg/%@/%@",contentType, contentKey];
        NSURL *url = [NSURL URLWithString:urlString];
    [[UIApplication sharedApplication]openURL:url];
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
