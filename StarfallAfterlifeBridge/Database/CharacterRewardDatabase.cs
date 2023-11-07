using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class CharacterRewardDatabase
    {
        public List<CharacterReward> Rewards { get; } = new()
        {
            new(){ Id = 2000000001, RewardId = 817494840, Count = 1 }, // WarpMirrorProject
            new(){ Id = 2000000002, RewardId = 2030601551, Count = 1 }, // K273
            new(){ Id = 2000000003, RewardId = 455171052, Count = 1 }, // LargeLifeSupportSystem
            new(){ Id = 2000000004, RewardId = 1577344764, Count = 1 }, // LargePowerCore
        };

        private readonly object _locker = new object();

        public CharacterReward GetReward(int rewardId)
        {
            lock (_locker)
                return Rewards.FirstOrDefault(r => r.Id == rewardId);
        }

        public List<CharacterReward> GetStationAttackRewards()
        {
            lock (_locker)
            {
                return new List<CharacterReward>
                {
                    GetReward(2000000001),
                    GetReward(2000000002),
                    GetReward(2000000003),
                    GetReward(2000000004),
                };
            }
        }
    }
}
