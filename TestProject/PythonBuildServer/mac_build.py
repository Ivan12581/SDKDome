#coding=utf-8

import subprocess
import os
import base_build
import mac_build_ipa

# Mac
def build():
    projectPath = 'iOS/Project/'
    cmd_dict = {
        'a': ['Platform:IOS', 'Level:Alpha', 'SV:dev', 'Path:'+projectPath],
        'b': ['Platform:IOS', 'Level:Beta', 'SV:dev', 'Path:'+projectPath],
    }

    print("========iOS构建开始========")

    print("Alpha版Xcode工程 开始构建...")
    base_build.build(cmd_dict['a'])
    print("Alpha版Xcode工程 构建完成")
    mac_build_ipa.build(projectPath)
    print("Alpha版IPA 构建完成")

    print("Beta版Xcode工程 开始构建...")
    base_build.build(cmd_dict['b'])
    print("Beta版Xcode工程 构建完成")
    mac_build_ipa.build(projectPath)
    print("Beta版IPA 构建完成")

    print("========iOS构建结束========")