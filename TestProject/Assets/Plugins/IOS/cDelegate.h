//
//  cDelegate.h
//  Unity-iPhone
//
//  Created by mini on 7/18/20.
//

#ifndef cDelegate_h
#define cDelegate_h

@protocol  cDelegate <NSObject>

//@required   //@required表示必须要做的

@optional   //@optional表示可以做但不用必须做
- (void)InitSDKCallBack:(NSMutableDictionary *) dict;

- (void)LoginCallBack:(NSMutableDictionary *) dict;

- (void)PayCallBack:(NSMutableDictionary *) dict;

- (void)GetConfigInfoCallBack:(NSMutableDictionary *) dict;
- (void)UploadInfoCallBack:(NSMutableDictionary *) dict;

- (void)ShareCallBack:(NSMutableDictionary *) dict;

- (void)SwitchCallBack:(NSMutableDictionary *) dict;
- (void)ExitGameCallBack:(NSMutableDictionary *) dict;

- (void)OpenServiceCallBack:(NSMutableDictionary *) dict;

- (void)FaceBookShareCallBack:(NSMutableDictionary *) dict;
- (void)LineShareCallBack:(NSMutableDictionary *) dict;


@end
#endif /* cDelegate_h */
