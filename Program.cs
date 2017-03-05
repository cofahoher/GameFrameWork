using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajongShanten
{
    class Program
    {
        static void Main(string[] args)
        {
            double[] cost = new double[10];
            for (int wanneng = 0; wanneng < 9; ++wanneng)
            {
                cost[wanneng] = Test(14 - wanneng);
            }
            cost[9] = 0;
        }

        static double Test(int TEST_HAND_TILE_COUNT)
        {
            Random sys_ran = new Random();
            int seed = sys_ran.Next();

            RandomGenerator ran = new RandomGenerator();
            ran.ResetSeed(seed);

            Wall wall = new Wall(ran);
            Hand hand = new Hand();
            ShantenCalculator calculator = new ShantenCalculator();

            int total_hand_cnt = 0;
            int total_case_cnt = 0;
            int test_count = 10000;

            DateTime dt1 = DateTime.Now;
            for (int i = 0; i < test_count; ++i)
            {
                wall.Shuffle();
                for (int j = 0; j < MJ.TOTAL_NUMBER_TILES / TEST_HAND_TILE_COUNT; ++j)
                {
                    hand.Clear();
                    hand.Draw(wall, TEST_HAND_TILE_COUNT);
                    calculator.Reset(hand);
                    int shanten = calculator.CalculateShanten();
                    if (shanten <= 0)
                        shanten = 1;
                    int case_count = calculator.GetCaseCount();
                    total_hand_cnt += 1;
                    total_case_cnt += case_count;
                }
            }
            DateTime dt2 = DateTime.Now;
            TimeSpan ts12 = dt2 - dt1;
            double cost_ms_12 = ts12.TotalMilliseconds;


            ran.ResetSeed(seed);
            DateTime dt3 = DateTime.Now;
            for (int i = 0; i < test_count; ++i)
            {
                wall.Shuffle();
                for (int j = 0; j < MJ.TOTAL_NUMBER_TILES / TEST_HAND_TILE_COUNT; ++j)
                {
                    hand.Clear();
                    hand.Draw(wall, TEST_HAND_TILE_COUNT);
                    calculator.Reset(hand);
                }
            }
            DateTime dt4 = DateTime.Now;
            TimeSpan ts34 = dt4 - dt3;
            double cost_ms_34 = ts34.TotalMilliseconds;

            double net_cost = cost_ms_12 - cost_ms_34;
            double average_each_shanten_cos = net_cost / total_hand_cnt;
            return average_each_shanten_cos;
        }
    }
}
