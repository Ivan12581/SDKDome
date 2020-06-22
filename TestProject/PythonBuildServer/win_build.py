#coding=utf-8

import os
import subprocess
import base_build

# Windows
def build():
    cmd_dict = {
        'a-sdk0': ['Platform:Android', 'Level:Alpha', 'SV:dev'],
        'b-sdk0': ['Platform:Android', 'Level:Beta', 'SV:dev'],

        'a-sdk1': ['Platform:Android', 'Level:Alpha', 'SV:dev', 'SDK:1'],
        'b-sdk1': ['Platform:Android', 'Level:Beta', 'SV:dev', 'SDK:1'],

        'a-sdk2': ['Platform:Android', 'Level:Alpha', 'SV:dev', 'SDK:2'],
        'b-sdk2': ['Platform:Android', 'Level:Beta', 'SV:dev', 'SDK:2'],

        'a-sdk3': ['Platform:Android', 'Level:Alpha', 'SV:dev', 'SDK:3'],
        'b-sdk3': ['Platform:Android', 'Level:Beta', 'SV:dev', 'SDK:3'],
    }

    print("========Android构建开始========")

    print("Alpha版APK无SDK 开始构建...")
    base_build.build(cmd_dict['a-sdk0'])
    print("Alpha版APK无SDK构建完成")

    print("Alpha版APK+国内SDK1 开始构建...")
    base_build.build(cmd_dict['a-sdk1'])
    print("Alpha版APK+国内SDK1 构建完成")

    print("Alpha版APK+海外SDK1 开始构建...")
    base_build.build(cmd_dict['a-sdk2'])
    print("Alpha版APK+海外SDK1 构建完成")

    print("Alpha版APK+海外SDK2 开始构建...")
    base_build.build(cmd_dict['a-sdk3'])
    print("Alpha版APK+海外SDK2 构建完成")

    print("========Android构建结束========")


