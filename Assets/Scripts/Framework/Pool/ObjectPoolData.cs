using System.Collections.Generic;

namespace GuiFramework
{
    /// <summary>
    /// 普通 C# 对象池数据
    /// </summary>
    public class ObjectPoolData
    {
        public ObjectPoolData(object obj)
        {
            PushObj(obj);
        }

        /// <summary>对象容器</summary>
        public Queue<object> poolQueue = new Queue<object>();

        /// <summary>回收对象</summary>
        public void PushObj(object obj)
        {
            poolQueue.Enqueue(obj);
        }

        /// <summary>取出对象</summary>
        public object GetObj()
        {
            return poolQueue.Dequeue();
        }
    }
}