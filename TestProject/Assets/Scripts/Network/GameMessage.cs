using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace celia.game
{
    public class GameMessage
    {
        public const int header_length = 4;                         // 消息头长度
        public bool connectionCheck = false;              // 是否需要网络检查来保证发送完成
        const int max_body_length = 4096;                           // 消息体最大长度
        const int max_length = header_length + max_body_length;     // 消息缓存总长度

        public int proto;                                           //消息号
        public string guid = "";                                    //guid
        public int reconnectCount = 1;                              //重发次数

        byte[] _data = new byte[max_length];                        // 消息缓存
        int _body_length = 0;                                       // 消息体长度

        public GameMessage() { }

        public GameMessage(byte[] data)
        {
            Buffer.BlockCopy(data, 0, _data, header_length, data.Length);
            _body_length = data.Length;
        }

        // 缓存
        public byte[] Data
        {
            get { return _data; }
        }

        // 消息体长度
        public int BodyLength
        {
            get { return _body_length; }
        }

        // 消息总长度
        public int Length
        {
            get { return header_length + _body_length; }
        }

        // 加密消息头
        public void encode_header(int seed)
        {
            int _header = _body_length ^ seed;
            BitConverter.GetBytes(_header).CopyTo(_data, 0);
        }

        // 解密消息头并获取消息体长度
        public bool decode_header(int seed)
        {
            _body_length = BitConverter.ToInt32(_data, 0) ^ seed;

            return (0 <= _body_length) && (_body_length <= max_body_length);
        }
    }

    // 双缓冲消息队列(线程安全)
    class MessageQue<T>
    {
        public MessageQue()
        {

        }
        
        public MessageQue<T> Clone()
        {
            var result = new MessageQue<T>();
            
            lock(_queFront)
            {
                result._queFront = new Queue<T>(_queFront);
            }
            result._queBack = new Queue<T>(_queBack);

            return result;
        }

        Queue<T> _queFront = new Queue<T>();        // 前缓冲(插入用)
        Queue<T> _queBack = new Queue<T>();         // 后缓冲(读取用)

        // 队列空判断(会触发缓冲交换)
        public bool Empty()
        {
            if (!(0 < _queBack.Count))
            {
                lock (_queFront)
                {
                    if (!(0 < _queFront.Count))
                    {
                        return true;
                    }
                    else
                    {
                        Queue<T> _temp = _queBack;
                        _queBack = _queFront;
                        _queFront = _temp;
                    }
                }
            }

            return false;
        }

        // 插入元素(会触发线程冲突)
        public void Enqueue(T e)
        {
            lock (_queFront)
            {
                _queFront.Enqueue(e);
            }
        }

        // 在后缓冲队列删除元素
        public void Dequeue()
        {
            _queBack.Dequeue();
        }

        public bool Contains(T item, Func<T, T, bool> check)
        {
            foreach (var _item in _queFront)
            {
                if (check(item, _item))
                {
                    return true;
                }
            }
            return false;
        }

        // 后缓冲队列元素数量
        public int Count
        {
            get { return _queBack.Count; }
        }

        // 取后缓冲队列头元素
        public T Peek()
        {
            return _queBack.Peek();
        }

        // 清空队列
        public void Clear()
        {
            lock (_queFront)
            {
                _queFront.Clear();
            }

            _queBack.Clear();
        }
    }
}
