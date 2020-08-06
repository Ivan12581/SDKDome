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
- (BOOL)shareMessage:(NSString *)message
{
    NSString *contentType = @"text";
    NSString *urlString = [NSString stringWithFormat:@"line://msg/%@/%@",contentType, message];
    [urlString stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    NSURL *url = [NSURL URLWithString:urlString];
    
    if ([[UIApplication sharedApplication]canOpenURL:url]) {
        return [[UIApplication sharedApplication]openURL:url];
    }else{
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
    
    NSString *contentKey = [pasteboard.name stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    
    NSString *urlString = [NSString stringWithFormat:@"line://msg/%@/%@",contentType, contentKey];
    NSURL *url = [NSURL URLWithString:urlString];
    
    if ([[UIApplication sharedApplication] canOpenURL:url]) {
        return [[UIApplication sharedApplication] openURL:url];
    }else{
        return NO;
    }

}


@end
