//
//  BYJumpEachOther.h
//  Unity-iPhone
//
//  Created by mini on 7/10/20.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@interface BYJumpEachOther : NSObject
+(id)sharedInstance;
-(void)setupIOS;
-(void)setupUnity;
// 存储的iOS控制器
@property (nonatomic, strong) UIViewController *vc;
@end
