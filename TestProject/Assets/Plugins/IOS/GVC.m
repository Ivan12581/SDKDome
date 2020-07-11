//
//  GVC.m
//  Unity-iPhone
//
//  Created by mini on 7/10/20.
//

#import "GVC.h"
#import <GoogleSignIn/GoogleSignIn.h>
@interface GVC ()

@end

@implementation GVC

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view.
    [GIDSignIn sharedInstance].presentingViewController = self;
    [[GIDSignIn sharedInstance] signIn];
}

/*
#pragma mark - Navigation

// In a storyboard-based application, you will often want to do a little preparation before navigation
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    // Get the new view controller using [segue destinationViewController].
    // Pass the selected object to the new view controller.
}
*/

@end
