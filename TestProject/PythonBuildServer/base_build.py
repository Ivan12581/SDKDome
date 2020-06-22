#coding=utf-8

import os
import time
import subprocess
from build_utils import *

def build(args):
    build_time = time.strftime('%Y-%m-%d %H:%M:%S',time.localtime(time.time()))
    print("========构建开始========")
    print(args)
    print("开始时间： " + build_time)
    print("========================")

    print(subprocess.check_output([unity_path(), '-quit', '-batchmode', '-projectPath', 
    	project_path(), '-logFile', os.path.join(os.path.dirname(__file__), 'buildLog.txt'), '-executeMethod', 'celia.game.editor.CeliaBuilder.StartBuild'] + args))

    print("========构建结束========")
    print(args)
    print("开始时间： " + build_time)
    build_time = time.strftime('%Y-%m-%d %H:%M:%S',time.localtime(time.time()))
    print("结束时间： " + build_time)
    print("========================")