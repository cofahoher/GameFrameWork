using System;
using System.Collections;
using System.Collections.Generic;

namespace MajongShanten
{
    public class MJ
    {
        public static int TYPES_OF_TILES = 34;                                      //共34种牌，每张牌用【0，33】表示
        public static int NUMBER_SAME_TILES = 4;                                    //每种牌4张
        public static int TOTAL_NUMBER_TILES = TYPES_OF_TILES * NUMBER_SAME_TILES;  //总共136张牌
        public static int NUMBERS_PER_SUIT = 9;                                     //一门牌有9种
        public static int MANS = 0;                                                 //万
        public static int PINS = 1;                                                 //筒
        public static int SOUS = 2;                                                 //索
        public static int HONORS = 3;                                               //风箭
        public static char[] SUIT_LETTER = new char[4]{'m','p', 's', 'z'};

        //【0，33】到【0，3】
        public static int Tile2Suit(int tile)
        {
            return tile / NUMBERS_PER_SUIT;
        }
        //【0，33】到mpsz
        public static char Tile2SuitLetter(int tile)
        {
            int suit = Tile2Suit(tile);
            return SUIT_LETTER[suit];
        }
        //【0，33】到【1，9】
        public static int Tile2Number(int tile)
        {
            return 1 + tile % NUMBERS_PER_SUIT;
        }
    }

    // 随机数生成器
    public class RandomGenerator
    {
        int m_seed = 0;
        public RandomGenerator(int seed = 0)
        {
            m_seed = seed;
        }
        public void ResetSeed(int seed)
        {
            m_seed = seed;
        }
        int Rand()
        {
            m_seed = m_seed * 214013 + 2531011;
            int rand = (m_seed >> 16) & 0x7fff;
            return rand;
        }
        public int RandBetween(int min_value, int max_value)
        {
            return min_value + Rand() % (max_value - min_value + 1);
        }
    }

    // 牌墙
    public class Wall
    {
        RandomGenerator m_ran;
        List<int> m_tiles = new List<int>(MJ.TOTAL_NUMBER_TILES);
        int m_next_index = 0;

        public Wall(RandomGenerator ran)
        {
            m_ran = ran;
            Shuffle();
        }

        public void Shuffle()
        {
            m_tiles.Clear();
            for (int i = 0; i < MJ.TYPES_OF_TILES; ++i)
            {
                for (int j = 0; j < MJ.NUMBER_SAME_TILES; ++j)
                    m_tiles.Add(i);
            }
            for (int i = 0; i < MJ.TOTAL_NUMBER_TILES; ++i)
            {
                int index = m_ran.RandBetween(i, MJ.TOTAL_NUMBER_TILES - 1);
                int temp = m_tiles[i];
                m_tiles[i] = m_tiles[index];
                m_tiles[index] = temp;
            }
            m_next_index = 0;
        }

        public int GetNext()
        {
            int result = -1;
            if (m_next_index < m_tiles.Count)
                result = m_tiles[m_next_index];
            ++m_next_index;
            return result;
        }
    }

    //手牌
    public class Hand
    {
        List<int> m_tiles = new List<int>();

        public Hand()
        {
        }

        public Hand(List<int> tiles)
        {
            for (int i = 0; i < tiles.Count; ++i)
                m_tiles.Add(tiles[i]);
            Sort();
        }

        public Hand(string str)
        {
            int suit = -1;
            for(int i = str.Length - 1; i >= 0; ++i)
            {
                char c = str[i];
                if (c >= '1' && c <= '9')
                {
                    if (suit == -1)
                        continue;
                    int number = c - '1';
                    int tile = suit * MJ.NUMBERS_PER_SUIT + number;
                    m_tiles.Add(tile);
                }
                else
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        if (c == MJ.SUIT_LETTER[j])
                        {
                            suit = j;
                            break;
                        }
                    }
                }
            }
            Sort();
        }

        public void Clear()
        {
            m_tiles.Clear();
        }

        public bool Draw(Wall wall, int count)
        {
            while(count > 0)
            {
                int tile = wall.GetNext();
                if (tile < 0)
                    return false;
                m_tiles.Add(tile);
                --count;
            }
            Sort();
            return true;
        }

        public void Add(int tile)
        {
            m_tiles.Add(tile);
            Sort();
        }

        public void Remove(int tile)
        {
            int index = m_tiles.IndexOf(tile);
            m_tiles.RemoveAt(index);
        }

        public void Sort()
        {
            m_tiles.Sort();
        }

        public List<int> GetTiles()
        {
            return m_tiles;
        }

        public string ToString()
        {
            List<char> list = new List<char>();
            int pre_suit = -1;
            for (int i = 0; i < m_tiles.Count; ++i)
            {
                int tile = m_tiles[i];
                int number = MJ.Tile2Number(tile);
                char c = (char)('1' + number - 1);
                int suit = MJ.Tile2Suit(tile);
                if (suit != pre_suit)
                {
                    if(pre_suit != -1)
                        list.Add(MJ.SUIT_LETTER[pre_suit]);
                    pre_suit = suit;
                }
                list.Add(c);
            }
            if (pre_suit != -1)
                list.Add(MJ.SUIT_LETTER[pre_suit]);
            return string.Concat(list);
        }
    }

    //记牌器
    public class Tally
    {
        //当拿着14张牌时，需要考虑弃掉哪张对向听有利
        //可以枚举13 * （34-1）中情况，考虑哪些牌可以往向听小的方向发展，并且计算每张牌可能在牌墙里的数量，算吃扔掉某张牌后，向听能减少的牌数
    }

    //牌型
    public class Shape
    {
        public Shape()
        {
        }

        public Shape(Shape rhs)
        {
        }

        public int m_gang = 0;      //杠，完成
        public int m_peng = 0;      //碰，完成
        public int m_chi = 0;       //吃/顺子，完成
        public int m_pair = 0;      //对子，第一个对子是对子，其余只是三缺一
        public int m_twosides = 0;  //顺子缺两端，三缺一
        public int m_middle = 0;    //顺子缺中间，三缺一
        public int m_single = 0;    //单张
        public int m_shanten = 8;   //向听数
    }

    //向听计算
    public class ShantenCalculator
    {
        int[] m_tile_count = new int[MJ.TYPES_OF_TILES];

        int m_gang = 0;      //杠，完成
        int m_peng = 0;      //碰，完成
        int m_chi = 0;       //吃/顺子，完成
        int m_pair = 0;      //对子，第一个对子是对子，其余只是三缺一
        int m_twosides = 0;  //顺子缺两端，三缺一
        int m_middle = 0;    //顺子缺中间，三缺一
        int m_single = 0;    //单张
        int m_shanten = 8;   //向听数

        int m_shanten_7duzi = 8;
        int m_shanten_13yao = 8;
        int m_shanten_7xingbukao = 8;
        int m_case_count = 0;//可能性数量

        List<Shape> m_best_shanten_shapes = new List<Shape>();

        public ShantenCalculator()
        {
        }

        public void Reset(Hand hand)
        {
            InitFromHand(hand);
            m_gang = 0;
            m_peng = 0;
            m_chi = 0;
            m_pair = 0;
            m_twosides = 0;
            m_middle = 0;
            m_single = 0;
            m_shanten = 8;
            m_case_count = 0;
        }

        public int CalculateShanten()
        {
            CheckSpecial();
            Process(0);
            return m_shanten;
        }

        public int GetCaseCount()
        {
            return m_case_count;
        }

        public List<Shape> GetAllBestCases()
        {
            return m_best_shanten_shapes;
        }

        public int CalculateDiscard(out int card_cnt)
        {
            card_cnt = 0;
            return 0;
        }

        void InitFromHand(Hand hand)
        {
            for (int i = 0; i < MJ.TYPES_OF_TILES; ++i)
                m_tile_count[i] = 0;
            List<int> hand_tiles = hand.GetTiles();
            for (int i = 0; i < hand_tiles.Count; ++i)
                m_tile_count[hand_tiles[i]] += 1;
        }

        void Process(int i)
        {
            while (i < MJ.TYPES_OF_TILES && m_tile_count[i] == 0)
                ++i;
            if (i >= MJ.TYPES_OF_TILES)
            {
                CheckShanten();
                return;
            }
            int suit = MJ.Tile2Suit(i);
            if (suit == MJ.HONORS)
            {
                ProcessHonor(i);
                return;
            }
            else
            {
                ProcessManPinSou(i);
                return;
            }
        }

        void ProcessManPinSou(int i)
        {
            int count = m_tile_count[i];
            switch (count)
            {
            case 4:
                DoGang(i, 1);
                Process(i + 1);
                DoGang(i, -1);

                DoPeng(i, 1);
                ProcessSequence(i);
                DoPeng(i, -1);

                //DoPair(i, 2);
                //Process(i + 1);
                //DoPair(i, -2);
                break;
            case 3:
                DoPeng(i, 1);
                Process(i + 1);
                DoPeng(i, -1);

                DoPair(i, 1);
                ProcessSequence(i);
                DoPair(i, -1);
                break;
            case 2:
                DoPair(i, 1);
                Process(i + 1);
                DoPair(i, -1);

                DoSingle(i, 1);
                ProcessSequence(i);
                DoSingle(i, -1);
                break;
            case 1:
                DoSingle(i, 1);
                Process(i + 1);
                DoSingle(i, -1);

                ProcessSequence(i);
                break;
            case 0:
                Process(i + 1);
                break;
            default:
                break;
            }
        }

        void ProcessSequence(int i)
        {
            int number = MJ.Tile2Number(i);
            if (number <= 7 && m_tile_count[i + 1] > 0 && m_tile_count[i + 2] > 0)
            {
                DoChi(i, 1);
                Process(i + 1);
                DoChi(i, -1);
            }
            if (number <= 7 && m_tile_count[i + 2] > 0)
            {
                DoMiddle(i, 1);
                Process(i + 1);
                DoMiddle(i, -1);
            }
            if (number <= 8 && m_tile_count[i + 1] > 0)
            {
                DoTwoside(i, 1);
                Process(i + 1);
                DoTwoside(i, -1);
            }
        }

        void ProcessHonor(int i)
        {
            int count = m_tile_count[i];
            switch (count)
            {
            case 4:
                DoGang(i, 1);
                Process(i + 1);
                DoGang(i, -1);

                //DoPeng(i, 1);
                //DoSingle(i, 1);
                //Process(i + 1);
                //DoSingle(i, -1);
                //DoPeng(i, -1);

                //DoPair(i, 2);
                //Process(i + 1);
                //DoPair(i, -2);
                break;
            case 3:
                DoPeng(i, 1);
                Process(i + 1);
                DoPeng(i, -1);

                if (m_pair == 0)  //TODO，有对子还拆肯定不合适吧
                {
                    DoPair(i, 1);
                    DoSingle(i, 1);
                    Process(i + 1);
                    DoSingle(i, -1);
                    DoPair(i, -1);
                }
                break;
            case 2:
                DoPair(i, 1);
                Process(i + 1);
                DoPair(i, -1);
                break;
            case 1:
                DoSingle(i, 1);
                Process(i + 1);
                DoSingle(i, -1);
                break;
            case 0:
                Process(i + 1);
                break;
            default:
                break;
            }
        }

        #region 可以在这些基本函数里记录和回滚更多的数据（比如牌型）
        /*
         * 三缺一补齐后都可以减少向听
         * 单牌补成对子，如果原来没有对子或者原来的 完成 + 三缺一 < 4，也能减少向听
         * 但不确定是否这之外的外加入也会减少向听？
         */
        void DoGang(int i, int c)
        {
            m_tile_count[i] -= 4 * c;
            m_gang += c;
        }

        void DoPeng(int i, int c)
        {
            m_tile_count[i] -= 3 * c;
            m_peng += c;
        }

        void DoChi(int i, int c)
        {
            m_tile_count[i] -= c;
            m_tile_count[i + 1] -= c;
            m_tile_count[i + 2] -= c;
            m_chi += c;
        }

        void DoPair(int i, int c)
        {
            m_tile_count[i] -= 2 * c;
            m_pair += c;
        }

        void DoTwoside(int i, int c)
        {
            m_tile_count[i] -= c;
            m_tile_count[i + 1] -= c;
            m_twosides += c;
        }

        void DoMiddle(int i, int c)
        {
            m_tile_count[i] -= c;
            m_tile_count[i + 2] -= c;
            m_middle += c;
        }

        void DoSingle(int i, int c)
        {
            m_tile_count[i] -= c;
            m_single += c;
        }
        #endregion

        void CheckShanten()
        {
            //只有第一个对子是对子，其他都是3缺1
            //完成牌型 + 3缺1 <= 4
            //枚举各种情况，或者解方程，总向听是8，已完成牌型向听是2，对子和3缺1的向听是1
            ++m_case_count;
            int complete = m_gang + m_peng + m_chi;
            int missone = m_twosides + m_middle;
            int pair = m_pair;
            if (pair > 1)
            {
                missone += pair - 1;
                pair = 1;
            }
            int max_missone = 4 - complete;
            if (missone > max_missone)
                missone = max_missone;
            int shanten = 8 - complete * 2 - pair - missone;

            if (shanten < m_shanten)
            {
                m_shanten = shanten;
                if (shanten <= 4)
                {
                    m_best_shanten_shapes.Clear();
                    RecordCurrentShape();
                    
                }
            }
            else if (shanten == m_shanten)
            {
                if (shanten <= 4)
                    RecordCurrentShape();
            }
        }

        void RecordCurrentShape()
        {
            Shape shape = new Shape();
            shape.m_gang = m_gang;
            shape.m_peng = m_peng;
            shape.m_chi = m_chi;
            shape.m_pair = m_pair;
            shape.m_twosides = m_twosides;
            shape.m_middle = m_middle;
            shape.m_single = m_single;
            shape.m_shanten = m_shanten;
            m_best_shanten_shapes.Add(shape);
        }

        void CheckSpecial()
        {
            Check7DuiZi();
            Check13Yao();
            Check7XingBuKao();
        }

        void Check7DuiZi()
        {
            int pairs = 0;
            for (int i = 0; i < MJ.TYPES_OF_TILES; ++i)
            {
                if (m_tile_count[i] >= 2)
                    ++pairs;
            }
            m_shanten_7duzi = 6 - pairs;
        }

        int[] m_13yao_check_list = new int[13] { 0, 8, 9, 17, 18, 26, 27, 28, 29, 30, 31, 32, 33 };
        void Check13Yao()
        {
            int type_cnt = 0;
            int pair = 0;
            for (int i = 0; i < m_13yao_check_list.Length; ++i)
            {
                int count = m_tile_count[m_13yao_check_list[i]];
                if (count > 0)
                {
                    type_cnt += 1;
                    if (count > 1)
                        pair += 1;
                }
            }
            m_shanten_13yao = 13 - type_cnt;
            if (pair > 0)
                m_shanten_13yao -= 1;
        }

        int[] m_7xingbukao_check_list = new int[16] { 0, 3, 6, 10, 13, 16, 20, 23, 26, 27, 28, 29, 30, 31, 32, 33 };
        void Check7XingBuKao()
        {
            int type_cnt = 0;
            for (int i = 0; i < m_7xingbukao_check_list.Length; ++i)
            {
                int count = m_tile_count[m_7xingbukao_check_list[i]];
                if (count > 0)
                    type_cnt += 1;
                m_shanten_7xingbukao = 13 - type_cnt;
            }
        }
    }
}
