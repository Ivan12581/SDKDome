#coding=utf-8

from sys import platform as _platform
import importlib
import subprocess
import datetime
import time
import build_utils
import win_build
import mac_build

def build(buildAvailable):
    while buildAvailable.isSet():
        print("准备开始定时构建，其他构建正在进行中，等待...")
        time.sleep(10)

    print("================================开始定时构建================================")
    buildAvailable.set()
    
    importlib.reload(build_utils)
    build_utils.git_clear_and_pull()

    if _platform == "darwin":
        # MAC OS X
        importlib.reload(mac_build)
        mac_build.build()
    else:
        # Win
        importlib.reload(win_build)
        win_build.build()

    buildAvailable.clear()
    print("================================结束定时构建================================")


def start(buildAvailable):
    print("持续集成程序开始")
    while True:
        print("持续集成检查" + "当前时间为"+ str(datetime.datetime.now().year) + "/" + str(datetime.datetime.now().month) + "/" + str(datetime.datetime.now().day) + " " + str(datetime.datetime.now().hour)+":"+str(datetime.datetime.now().minute))
        if datetime.datetime.now().hour == 13 or datetime.datetime.now().hour == 20:
        
            try:
                build(buildAvailable)
            except Exception as e:
                print("Task Build Error: \n" + str(e))

        else:
            print("非构建时段，等待下次检查")
        time.sleep(3600)

if __name__ == '__main__':
    start()