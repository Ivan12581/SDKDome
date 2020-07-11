//
//  BYJumpEachOther.m
//  Unity-iPhone
//
//  Created by mini on 7/10/20.
//

#import <Foundation/Foundation.h>
#import "BYJumpEachOther.h"
#import "UnityAppController.h"
#import "GoogleHelper.h"
#import "GVC.h"
@implementation BYJumpEachOther
static BYJumpEachOther *_Instance = nil;
+(BYJumpEachOther*)sharedInstance{
    if (_Instance == nil) {
        if (self == [BYJumpEachOther class]) {
            _Instance = [[self alloc] init];
        }
    }
    return _Instance;
}
// 设置iOS界面
  - (void)setupIOS
{
    // 跳转到IOS界面,Unity界面暂停

     GVC *vc = [[GVC alloc] init];
    
//    GoogleHelper *vc = [[GoogleHelper alloc] init];
    
//    UIViewController *vc = [[UIViewController alloc] init];
//    vc.view.backgroundColor = [UIColor greenColor];
//    vc.view.frame = [UIScreen mainScreen].bounds;
//
//    UIButton *btn = [[UIButton alloc]initWithFrame:CGRectMake(70, 530, 180, 30)];
//    btn.backgroundColor = [UIColor redColor];
//    [btn setTitle:@"跳转到Unity界面" forState:UIControlStateNormal];
//    [btn setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
//    [btn addTarget:self action:@selector(setupUnity) forControlEvents:UIControlEventTouchUpInside];

//    [vc.view addSubview:btn];
    vc.view.backgroundColor = [UIColor whiteColor];
    //vc.view.backgroundColor = [UIColor colorWithRed:0 green:0 blue:0 alpha:0];
    self.vc = vc;
    [self.vc.view setHidden:NO];
    NSLog(@"设置界面为IOS界面");
    [GetAppController().window bringSubviewToFront:UnityGetGLViewController().view];
    GetAppController().window.rootViewController = vc;
        UnityPause(true);
//    [[GoogleHelper sharedInstance] Login];
}
// 设置Unity界面
  - (void)setupUnity
{
    UnityPause(false);

    GetAppController().window.rootViewController = UnityGetGLViewController();
    // 等同于
    // GetAppController().window.rootViewController = GetAppController().rootViewController;
    NSLog(@"设置rootView为Unity界面");
}
@end
