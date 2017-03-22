using System;
using System.Collections.Generic;
using Combat;


public class HeapTestItem : HeapItem
{
    public int value = 0;

    public HeapTestItem(int _value)
    {
        value = _value;
    }

    public override int CompareTo(object obj)
    {
        HeapTestItem item = obj as HeapTestItem;
        if (item == null)
            return -1;
        int result = value - item.value;
        if (result != 0)
            return result;
        else
            return _insertion_index - item._insertion_index;
    }

    static public int CustomComparerLess(HeapTestItem a, HeapTestItem b)
    {
        return a.value - b.value;
    }

    static public int CustomComparerGreater(HeapTestItem a, HeapTestItem b)
    {
        return b.value - a.value;
    }
}

public class HeapTester
{
    const int MAX_ITEM_NUM = 10;
    int[] random_1_to_10 = { 5, 8, 2, 6, 3, 1, 4, 9, 10, 7 };
    Random random;

    public HeapTester()
    {
        random = new Random();
    }

    HeapTestItem CreateTestItem(int value)
    {
        return new HeapTestItem(value);
    }

    public void Shuffle()
    {
        for (int i = 0; i < random_1_to_10.Length; ++i)
        {
            int index = random.Next(i, random_1_to_10.Length - 1);
            int temp = random_1_to_10[i];
            random_1_to_10[i] = random_1_to_10[index];
            random_1_to_10[index] = temp;
        }
    }

    public bool RunUnitTest()
    {
        if (!Test_1())
            return false;
        if (!Test_2())
            return false;
        if (!Test_3())
            return false;
        if (!Test_4())
            return false;
        if (!Test_5())
            return false;
        if (!Test_6())
            return false;
        if (!Test_7())
            return false;
        if (!Test_8())
            return false;
        if (!Test_9())
            return false;
        return true;
    }

    bool Test_1()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_LESS);
        if (queue.Size() != 0)
        {
            Print("Test_1, 1");
            return false;
        }
        if (!queue.Empty())
        {
            Print("Test_1, 2");
            return false;
        }
        if (queue.Peek() != null)
        {
            Print("Test_1, 3");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = CreateTestItem(random_1_to_10[i]);
            if(queue.Contains(item))
            {
                Print("Test_1, 10, " + i);
                return false;
            }
            queue.Enqueue(item);
            if(!queue.Contains(item))
            {
                Print("Test_1, 11, " + i);
                return false;
            }
        }
        if(queue.Size() != MAX_ITEM_NUM)
        {
            Print("Test_1, 12");
            return false;
        }
        if(queue.Empty())
        {
            Print("Test_1, 13");
            return false;
        }
        if(queue.Peek().value != 1)
        {
            Print("Test_1, 14");
            return false;
        }
        HeapTestItem must_less_priority_than_this = CreateTestItem(0);
        if(!queue.Check(must_less_priority_than_this))
        {
            Print("Test_1, 15");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.Dequeue();
            if(queue.Contains(item))
            {
                Print("Test_1, 16");
                return false;
            }
            if(item.value != (i + 1))
            {
                Print("Test_1, 17, " + i);
                return false;
            }
        }
        if(queue.Size() != 0)
        {
            Print("Test_1, 18");
            return false;
        }
        if (!queue.Empty())
        {
            Print("Test_1, 19");
            return false;
        }
        if(queue.Peek() != null)
        {
            Print("Test_1, 20");
            return false;
        }
        return true;
    }

    bool Test_2()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_GREATER);
        for(int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            queue.Enqueue(CreateTestItem(random_1_to_10[i]));
        }
        HeapTestItem must_less_priority_than_this = CreateTestItem(11);
        if(!queue.Check(must_less_priority_than_this))
        {
            Print("Test_2, 1");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.Dequeue();
            if(item.value != (10 - i))
            {
                Print("Test_2, 2");
                return false;
            }

        }
        return true;
    }

    bool Test_3()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_CUSTOM, HeapTestItem.CustomComparerLess);
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            queue.Enqueue(CreateTestItem(random_1_to_10[i]));
        }
        HeapTestItem must_less_priority_than_this = CreateTestItem(0);
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_3, 1");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.Dequeue();
            if (item.value != (i + 1))
            {
                Print("Test_3, 2");
                return false;
            }
        }
        return true;
    }

    bool Test_4()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_CUSTOM, HeapTestItem.CustomComparerGreater);
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            queue.Enqueue(CreateTestItem(random_1_to_10[i]));
        }
        HeapTestItem must_less_priority_than_this = CreateTestItem(11);
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_4, 1");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.Dequeue();
            if (item.value != (10 - i))
            {
                Print("Test_4, 2");
                return false;
            }
        }
        return true;
    }

    bool Test_5()
    {
        List<HeapTestItem> list = new List<HeapTestItem>();
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_LESS);
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = CreateTestItem(random_1_to_10[i]);
            queue.Enqueue(item);
            list.Add(item);
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            list[i].value = i + 1;
        }
        queue.Build();
        HeapTestItem must_less_priority_than_this = CreateTestItem(0);
        if(!queue.Check(must_less_priority_than_this))
        {
            Print("Test_5, 1");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.Dequeue();
            if(item.value != (i + 1))
            {
                Print("Test_5, 2");
                return false;
            }
        }
        return true;
    }

    bool Test_6()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_LESS);
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            queue.Enqueue(CreateTestItem(random_1_to_10[i]));
        }
        queue.Clear();
        if(queue.Size() != 0)
        {
            Print("Test_6, 1");
            return false;
        }
        if (!queue.Empty())
        {
            Print("Test_6, 2");
            return false;
        }
        if (queue.Peek() != null)
        {
            Print("Test_6, 3");
            return false;
        }
        return true;
    }

    bool Test_7()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_CUSTOM, HeapTestItem.CustomComparerLess);
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            queue.Enqueue(CreateTestItem(random_1_to_10[i]));
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.GetAt(i);
            if(item.value == 6)
            {
                item.value = 12;
                queue.UpdatePriorityByIndex(i);
                break;
            }
        }
        HeapTestItem must_less_priority_than_this = CreateTestItem(0);
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_7, 1");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.GetAt(i);
            if (item.value == 12)
            {
                item.value = 6;
                queue.UpdatePriorityByIndex(i);
                break;
            }
        }
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_7, 2");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.GetAt(i);
            if (item.value == 7)
            {
                item.value = -1;
                queue.UpdatePriority(item);
                break;
            }
        }
        must_less_priority_than_this = CreateTestItem(-2);
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_7, 3");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.GetAt(i);
            if (item.value == -1)
            {
                item.value = 7;
                queue.UpdatePriority(item);
                break;
            }
        }
        must_less_priority_than_this = CreateTestItem(0);
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_7, 4");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.Dequeue();
            if (item.value != (i + 1))
            {
                Print("Test_7, 5");
                return false;
            }
        }
        return true;
    }

    bool Test_8()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_CUSTOM, HeapTestItem.CustomComparerLess);
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            queue.Enqueue(CreateTestItem(random_1_to_10[i]));
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            queue.Enqueue(CreateTestItem(random_1_to_10[i]));
        }
        HeapTestItem must_less_priority_than_this = CreateTestItem(0);
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_8, 1");
            return false;
        }
        for (int turn = 0; turn < MAX_ITEM_NUM; ++turn)
        {
            int value = random_1_to_10[turn];
            for (int i = 0; i < queue.Size(); ++i)
            {
                HeapTestItem item = queue.GetAt(i);
                if (item.value == value)
                {
                    if (i % 2 != 0)
                    {
                        queue.Remove(item);
                        if (queue.Contains(item))
                        {
                            Print("Test_8, 2, " + i);
                            return false;
                        }
                    }
                    else
                    {
                        queue.RemoveByIndex(i);
                        if (queue.Contains(item))
                        {
                            Print("Test_8, 3, " + i);
                            return false;
                        }
                    }
                    break;
                }
            }              
        }
        if (queue.Size() != 10)
        {
            Print("Test_8, 4");
            return false;
        }
        for (int i = 0; i < MAX_ITEM_NUM; ++i)
        {
            HeapTestItem item = queue.Dequeue();
            if (item.value != (i + 1))
            {
                Print("Test_8, 5");
                return false;
            }
        }
        return true;
    }

    bool Test_9()
    {
        Heap<HeapTestItem> queue = new Heap<HeapTestItem>(Heap<HeapTestItem>.CheckPriorityMethod.CPM_CUSTOM, HeapTestItem.CustomComparerLess);
        for (int i = 0; i < 2000; ++i)
        {
            queue.Enqueue(CreateTestItem(random.Next(1, 100000)));
        }
        HeapTestItem must_less_priority_than_this = CreateTestItem(0);
        if (!queue.Check(must_less_priority_than_this))
        {
            Print("Test_9, 1");
            return false;
        }
        int[] counter = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < 10000; ++i)
        {
            int op = random.Next(0, 9);
            counter[op] = counter[op] + 1;
            int size = queue.Size();
            if (size == 0)
            {
                string t = "HeapTester:Test_9, i = " + i + ", [";
                for (int j = 0; j < MAX_ITEM_NUM; ++j)
                {
                    t += ",";
                    t += counter[j];
                }
                t += "]";
                Print(t);
                break;
            }
            if (op == 0)
                queue.Enqueue(CreateTestItem(random.Next(1, 100000)));
            else if (op == 1)
                queue.Dequeue();
            else if (op == 2 || op == 3)
            {
                int hi = random.Next(0, size - 1);
                HeapTestItem item = queue.GetAt(hi);
                item.value = random.Next(1, 10000);
                queue.UpdatePriority(item);
            }
            else if (op == 4 || op == 5)
            {
                int hi = random.Next(0, size - 1);
                HeapTestItem item = queue.GetAt(hi);
                item.value = random.Next(1, 10000);
                queue.UpdatePriorityByIndex(hi);
            }
            else if (op == 6)
            {
                int hi = random.Next(0, size - 1);
                HeapTestItem item = queue.GetAt(hi);
                queue.Remove(item);
            }
            else if (op == 7)
            {
                int hi = random.Next(0, size - 1);
                queue.RemoveByIndex(hi);
            }
            else if (op == 8)
                queue.Enqueue(CreateTestItem(random.Next(1, 100000)));
            else if (op == 9)
                queue.Enqueue(CreateTestItem(random.Next(1, 100000)));

            if(i % 100 == 0)
            {
                if(!queue.Check(must_less_priority_than_this))
                {
                    Print("Test_9, 2, " + i);
                    return false;
                }
            }
        }
        return true;
    }

    void Print(string str)
    {
        LogWrapper.LogError(str);
    }
}