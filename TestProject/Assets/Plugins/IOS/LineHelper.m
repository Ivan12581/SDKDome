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
    NSLog(@"--LineHelper---InitSDK---");
}


-(void)share:(const char*) jsonString{
    NSString *jsonNSString = [NSString stringWithUTF8String:jsonString];
    NSData *data = [jsonNSString dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    NSLog(@" ---ios LIneshare---: %@", dict);
    NSString *imgPath =[dict valueForKey:@"img"];
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];

//    NSData *data = [NSData dataWithContentsOfURL:[NSURL URLWithString:pictureUrl]];
     UIImage *image = [UIImage imageWithContentsOfFile:imgPath];
    [pasteboard setData:UIImageJPEGRepresentation(image, 1) forPasteboardType:@"public.jpeg"];
    NSString *contentType =@"image";

    NSString *contentKey = [pasteboard.name stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];

    NSString *urlString = [NSString stringWithFormat:@"line://msg/%@/%@",contentType, contentKey];
    NSURL *url = [NSURL URLWithString:urlString];
    
    if ([[UIApplication sharedApplication]canOpenURL:url]) {
        [[UIApplication sharedApplication]openURL:url];
        [IOSBridgeHelper LineShareCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"1", @"state",nil]];
    }else{
//            如果使用者沒有安裝，連結到App Store
        NSURL *itunesURL = [NSURL URLWithString:@"itms-apps://itunes.apple.com/app/id443904275"];
        [[UIApplication sharedApplication] openURL:itunesURL];
        [IOSBridgeHelper LineShareCallBack:[NSMutableDictionary dictionaryWithObjectsAndKeys:@"0", @"state",nil]];
        
    }
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
