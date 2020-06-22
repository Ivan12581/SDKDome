#coding=utf-8

import os
import subprocess
from sys import platform as _platform

def git_clear_and_pull(branch_name='develop'):
    # 清除本地暂存区和工作区
    print(subprocess.check_output(['git', 'reset', '--hard']))
    # 清除非跟踪文件
    print(subprocess.check_output(['git', 'clean', '-d', '-f']))
    print("已清除本地所有修改")
    # 切换到目标分支
    print(subprocess.check_output(['git', 'checkout', branch_name]))
    print("已切换分支")
    # 再Fetch
    print(subprocess.check_output(['git', 'fetch', 'origin', branch_name]))
    print("获取成功")
    # 最后拉取
    print(subprocess.check_output(['git', 'pull', 'origin', branch_name]))
    print("拉取成功")

def project_path():
    # dir_path = os.path.realpath(os.path.dirname(__file__))
    dir_path = os.path.realpath( os.path.join(os.path.dirname(__file__), '..') )
    return dir_path

def unity_path():
    path_file = open(os.path.join(os.path.dirname(__file__), "path.txt"), "r")
    unity_path = path_file.read()
    path_file.close()
    return unity_path
    
if __name__ == '__main__':
    print(unity_path())
    os.system("pause")