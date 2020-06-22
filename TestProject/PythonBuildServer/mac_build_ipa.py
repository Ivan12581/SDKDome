#coding=utf-8

import argparse
import subprocess
import time
import os
import shutil

SCHEME = "Unity-iPhone"

# 清除文件夹/文件
def DeletePath(path):
    if os.path.isdir(path):
        shutil.rmtree(path)
        print("Cleaned Path: %s" %(path))
    elif os.path.isfile(path):
        os.remove(path)
        print("Cleaned Path: %s" %(path))

def exportIPA(archivePath, exportOptionsPlistPath, sdkIndex):
    fileDir = os.path.dirname(__file__)
    # 导出ipa路径
    exportFolder = os.path.join(fileDir, '../Outputs/iOS/')
    buildTime = time.strftime('%y_%m_%d_%H_%M_%S',time.localtime(time.time()))
    global SCHEME
    exportPath = "%s%s%s%s" %(exportFolder, buildTime, SCHEME, sdkIndex)
    # 开始导出
    exportCmd = ['xcodebuild', '-exportArchive', '-archivePath', archivePath, '-exportPath', exportPath, '-exportOptionsPlist', exportOptionsPlistPath]
    try:
        subprocess.check_output(exportCmd, stderr=subprocess.STDOUT)
    except subprocess.CalledProcessError as e:
        print('ExportIPA Error:', e.output)
    # 导出dSYM文件
    shutil.copytree(os.path.join(archivePath, 'dSYMs'), os.path.join(exportPath, 'dSYMs'))

# 导出archive
def exportArchive(projectFolder):
    fileDir = os.path.dirname(__file__)
    outputsDir = os.path.join(fileDir, '../Outputs/') 
    projectPath = os.path.join(outputsDir, projectFolder, 'Unity-iPhone.xcodeproj')
    global SCHEME
    archivePath = os.path.join(projectPath, '../%s.xcarchive' %(SCHEME))
    configuration = "Debug"
    # 开始导出
    archiveCmd = ['xcodebuild', '-project', projectPath, '-scheme', SCHEME, '-configuration', configuration, 'archive', '-archivePath', archivePath, '-destination', 'generic/platform=iOS', 'DEBUG_INFORMATION_FORMAT=dwarf-with-dsym']
    try:
        subprocess.check_output(archiveCmd, stderr=subprocess.STDOUT)
    except subprocess.CalledProcessError as e:
        print('ExportArchive Error:', e.output)

    return archivePath

def gotSDKIndex(buildParams):
    for param in buildParams:
        if param.find('SDK:') != -1:
            return param.strip('SDK:')
    return ""

# 用SDK编号来获取对应导出IPA设置文件路径
def exportOptionsPlistPath(buildParams):
    optDict = {
        'SDK:0': '1',
        'SDK:1': '1',
        'SDK:2': '2',
        'SDK:3': '2',
    }
    signTypeDict = {
        'Sign:Ad': 'Ad',
        'Sign:Dis': 'Dis',
    }
    optIndex = '1'
    for param in buildParams:
        if param in optDict:
            optIndex = optDict[param]
            break
    
    signType = 'Ad'
    for param in buildParams:
        if param in signTypeDict:
            signType = signTypeDict[param]
            break

    optionFileName = "exportOptions" + optIndex + signType + ".plist"
    fileDir = os.path.dirname(__file__)
    exportOptionsPlistPath = os.path.join(fileDir, optionFileName)
    return exportOptionsPlistPath


def build(projectFolder, buildParams):
    print("-----开始执行Archive-----")
    # 导出archive
    archivePath = exportArchive(projectFolder)
    print("-----导出Archive结束-----")
    print("-------开始导出ipa-------\n", exportOptionsPlistPath(buildParams))
    # 导出ipa包
    exportIPA(archivePath, exportOptionsPlistPath(buildParams), gotSDKIndex(buildParams))
    print("-------导出ipa结束-------")
    # 删除archive
    DeletePath(archivePath)