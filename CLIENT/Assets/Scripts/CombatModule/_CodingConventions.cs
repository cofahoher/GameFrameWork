using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    /*
     * 命名：
     *      类名：SomeClass
     *      函数名：SomeFunction
     *      类成员变量：m_some_variable
     *      临时变量：some_variable
     *      其他：尽量用全称，不用简写
     *      
     * Getter、Setter函数和属性：
     *      基本类型用属性，自己写的类用函数吧
     *      不然，假设有个类叫TaskScheduler，其他类拿他做成员变量 TaskScheduler m_task_scheduler
     *      写成属性就是 
     *      public TaskScheduler TaskScheduler  //个人觉得太难看了吧
     *      {
     *          get { return m_task_scheduler; }
     *      }
     *      自己写的业务功能类基本已经表明用途了，再为这个类想个当C#属性用时的名字好痛苦的
     *      除非是算法数据结构的扩展，比如我写了个Heap，使用的时候，肯定不会 Heap m_heap，就像你一般不会这么做 int m_int
     *      
     *      interface里的也都用函数吧，不然个人觉得看起来好乱
     *      
     * 函数名前缀顺序
     *      public/protected/private    static/virtual/override/abstract    返回类型    函数名（参数列表）
     * 
     * 显式析构
     *      主要类，都应该继承IDestruct，控制好引用，不需要的的时候调用Destruct()
     *      反正优化时也需要解引用，不如代码写好一点，自己先析构了
     * 
     * 减少GC
     *      需要回收重用的类，可以继承IRecyclable，并且提供Create和Recycle两个静态函数，参见TestRecyclable
     * 
     * 单件模板
     *      参见TestSingleton
     *      
     * 
     * 从TPS继承的
     *      禁止用enum当dictionary的键！
     */
}