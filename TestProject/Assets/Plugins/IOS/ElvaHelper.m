//
//  ElvaHelper.m
//  Unity-iPhone
//
//  Created by mini on 8/5/20.
//

#import "ElvaHelper.h"

@implementation ElvaHelper
-(void)InitSDK{
    NSString *AIHelpAppID = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"AIHelpAppID"];
    NSString *AIHelpAppKey = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"AIHelpAppKey"];
    NSString *AIHelpDomain = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"AIHelpDomain"];
}
-(void)showElva{
    
}
@end
