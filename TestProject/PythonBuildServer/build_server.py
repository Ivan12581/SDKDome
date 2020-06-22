#coding=utf-8

import os
import time
import traceback
import importlib
import http.server
import socketserver
from threading import Thread
from threading import Event

import task_build
import base_build
import build_utils
import mac_build_ipa

# 构建线程锁
buildAvailable = Event()
# 上次构建时间
lastStartTime = "None"
lastOverTime = "None"

def BuildThread(isIOS, buildParams, branchName):
    importlib.reload(base_build)
    global lastStartTime
    lastStartTime = time.strftime('%Y-%m-%d %H:%M:%S',time.localtime(time.time()))

    print("-----------远程构建------------")
    global buildAvailable
    buildAvailable.set()# 构建中设置标志
    try:
        # build_utils.git_clear_and_pull(branchName)
        if not isIOS:
            base_build.build(buildParams)
        else:
            projectPath = 'iOS/Project/'
            buildParams = buildParams + ['Path:'+projectPath]
            base_build.build(buildParams)
            mac_build_ipa.build(projectPath, buildParams)

        buildAvailable.clear()# 构建完成解除标志
        global lastOverTime
        lastOverTime = time.strftime('%Y-%m-%d %H:%M:%S',time.localtime(time.time()))
        print("-----------远程构建结束------------")
    except Exception as e:
        buildAvailable.clear()# 异常解锁
        lastOverTime = "Error: \n" + str(e)
        print("-----------远程构建异常------------\n" + str(e))

# 把URL转化为打包参数
def parse_params(platform_key, params_str):
    platform_dict = {
        'and': ['Platform:Android'],
        'ios': ['Platform:IOS'],
    }
    if not (platform_key in platform_dict):
        return False

    params = platform_dict[platform_key]

    param_keys = params_str.split('-')
    param_dict = {
        'a' : ['Level:Alpha'],
        'b' : ['Level:Beta'],
        'sdk0' : ['SDK:0'],
        'sdk1' : ['SDK:1'],
        'sdk2' : ['SDK:2'],
        'sdk3' : ['SDK:3'],
        'svdev' : ['SV:dev'],
        'svrel' : ['SV:rel'],
        'ad' : ['Sign:Ad'],
        'dis' : ['Sign:Dis'],
    }
    for key in param_keys:
        if key in param_dict:
            params = params + param_dict[key]
        else:
            return False
    return params

def cmd_handle(path):
    args = path.split('/')
    if len(args) >= 4 and args[1] == 'build':
        buildParams = parse_params(args[2], args[3])
        if not buildParams:
            return "构建参数错误"

        global buildAvailable
        if buildAvailable.isSet():
            return "构建中..."

        isIOS = args[2] == 'ios'
        buildBranch = args[4] if len(args) > 4 else 'develop'
        print((isIOS, buildParams, buildBranch,))
        Thread(target=BuildThread, args=(isIOS, buildParams, buildBranch,)).start()
        return "开始构建!"
    else:
        return "URL格式错误"

# 线程监听
class ThreadingHTTPServer(socketserver.ThreadingMixIn, http.server.HTTPServer):
    pass
# HTTP网页请求处理
class HTTPServer_RequestHandler(http.server.BaseHTTPRequestHandler):
    # GET
    def do_GET(self):
        self.protocal_version = "HTTP/1.1"
        self.send_response(200)
        self.send_header("Content-type",'text/html; charset=utf-8')
        self.end_headers()

        global lastOverTime
        build_state = cmd_handle(self.path)
        back_words = '<pre>' + '上次结束时间: ' + lastOverTime + '\n\n' + '状态: ' + build_state + '\n\n' + '上次开始时间: ' + lastStartTime + '</pre>'
        self.wfile.write(back_words.encode('utf-8'))

# 远程http构建响应
def open_server():
    PORT = 8000
    server_address = ('', PORT)
    httpd = ThreadingHTTPServer(server_address, HTTPServer_RequestHandler)
    print('远程构建服务开启 port:', PORT)
    httpd.serve_forever()

# 线程监听
class ThreadedTCPServer(socketserver.ThreadingMixIn, socketserver.TCPServer):
    pass
# 文件共享
def output_fold_shared():
    target_dir = os.path.join(os.path.dirname(__file__), '../Outputs')
    os.chdir(target_dir)
    PORT = 8888
    Handler = http.server.SimpleHTTPRequestHandler
    with ThreadedTCPServer(("", PORT), Handler) as httpd:
        print("输出文件夹分享服务开启 port:", PORT)
        httpd.serve_forever()

# 定时构建任务
def start_task_build():
    global buildAvailable
    importlib.reload(task_build)
    thread = Thread(target=task_build.start, args=(buildAvailable,))
    thread.start()

if __name__ == '__main__':
    Thread(target=open_server).start()
    Thread(target=output_fold_shared).start()
    # start_task_build()
