using NLog.LayoutRenderers.Wrappers;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public partial class ContextSystem
    {
        public async Task RequestWorldFinishStage()
        {
            var req = new WorldFinishStageReqPacket();
            var res = await _rpcSystem.RequestAsync<WorldFinishStageReqPacket, WorldFinishStageResPacket>(req);

        }

        public async Task RequestWorldRewardStar()
        {
            var req = new WorldRewardStarReqPacket();
            var res = await _rpcSystem.RequestAsync<WorldRewardStarReqPacket, WorldRewardStarResPacket>(req);

        }

        public void PrintWorldList()
        {
           
        }

    }
}
