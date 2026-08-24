using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using Server.Helper;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Model;

namespace Server.Service
{
    public class GachaService : ServiceBase
    {
        public GachaService(GameDb db, IMapper mapper, RpcContext rpcContext, ILogger<GachaService> logger) : base(db, rpcContext, logger)
        {
            _mapper = mapper;
        }

        public async Task<ScheduleLoadResponsePacket> LoadScheduleAsync(ScheduleLoadRequestPacket req)
        {
            var viewList = await Db.Center().GetFilledScheduleListAsync();
            return new ScheduleLoadResponsePacket
            {
                ScheduleList = viewList.ConvertAll(ToPacket),
            };
        }

        // 필드 6개뿐이라 AutoMapper 설정을 두지 않는다.
        private static SchedulePacket ToPacket(ScheduleView view)
        {
            return new SchedulePacket
            {
                Num = view.Num,
                State = view.State,
                ActiveStartTime = view.ActiveStartTime,
                ActiveEndTime = view.ActiveEndTime,
                ContentStartTime = view.ContentStartTime,
                ContentEndTime = view.ContentEndTime,
            };
        }

        public async Task<GachaNormalResponsePacket> GachaNormalAsync(GachaNormalRequestPacket req)
        {
            var scheduleView = await Db.Center().GetScheduleAsync(req.ScheduleNum);
            scheduleView.ValidPeriod(EScheduleTimeType.TOTAL, RpcContext.ServerTime);

            // Cost일치하는지 체크
            var valCnt = scheduleView.ValidGachaCnt(req.Cnt);
            var valCost = scheduleView.ValidGachaCost(req.CostObj, valCnt);

            // 재화 소모
            var gachaReason = scheduleView.MakeGachaReason(valCnt);
            var costChangeList = await RewardService.PayAsync(OwnScope, valCost, gachaReason);

            // 가챠 보상은 COOKIE / SOUL_STONE 뿐이고 둘 다 CookieModel 로 간다. 뽑을 때마다 이 리스트에
            // 바로 적용하고 저장은 마지막에 업서트 한 번만 한다.
            //
            // 결과는 바뀐 쿠키를 통째로 보낸다. 한 행에 소울스톤과 보유 여부가 같이 걸려 있어서
            // ChgObj 로 쪼개 보내면 클라가 다시 합쳐야 한다. ObjType 을 가리지 않는 지급 경로
            // (우편함 등)는 ChgObj 를 써야 하고 그건 RewardService.IncCookieAsync 가 맡는다.
            var cookieSet = OwnScope.Owned<CookieModel>();
            var existCookieList = await cookieSet.GetListAsync();
            var touchedCookieList = new List<CookieModel>();

            var gachaRandom = new GachaRandom(scheduleView.GachaPrt, RpcContext.ServerTime);
            var gachaResultList = new List<GachaResultPacket>(valCnt);
            for (var i = 0; i < valCnt; i++)
            {
                var objValue = gachaRandom.Roll(isNormal: true);
                var amount = (int)objValue.Value;

                var (prtCookie, gachaResult) = ResolveGachaResult(objValue, amount);
                var mdlCookie = TouchCookie(prtCookie.Num);
                ApplyGachaResult(mdlCookie, prtCookie, objValue);

                gachaResultList.Add(gachaResult);
            }

            await cookieSet.UpsertListAsync(touchedCookieList);

            // 뽑은 결과를 실어야 하므로 뽑기가 끝난 뒤에 쓴다. 같은 트랜잭션이라 원자성은 그대로다.
            await AuditService.WriteGachaAsync(OwnScope, req.ScheduleNum, valCnt, valCost, costChangeList, gachaResultList);

            return new GachaNormalResponsePacket
            {
                CostChgObjList = costChangeList.ToPacketList(),
                CookieList = _mapper.Map<List<CookiePacket>>(touchedCookieList),
                GachaResultList = gachaResultList,
            };

            // 같은 쿠키를 또 뽑으면 같은 인스턴스를 준다. 그래야 누적이 맞고 같은 행을 두 번 쓰지 않는다.
            CookieModel TouchCookie(int cookieNum)
            {
                var cookie = existCookieList.Find(x => x.Num == cookieNum);
                if (cookie == null)
                {
                    // 여기서 저장하지 않는다. GetOrCreateAsync 는 신규를 즉시 INSERT 해서 벌크로 묶을 게 없어진다.
                    cookie = CookieQueries.GetDefaultCookieModel(cookieNum);
                    existCookieList.Add(cookie);
                }

                if (!touchedCookieList.Contains(cookie))
                {
                    touchedCookieList.Add(cookie);
                }

                return cookie;
            }
        }

        // 뽑은 것 하나를 해석한다. COOKIE 는 쿠키 번호가, SOUL_STONE 은 소울스톤 번호가 키라 대상 쿠키가 갈린다.
        private static (CookieProto CookiePrt, GachaResultPacket Result) ResolveGachaResult(ObjValue objValue, int amount)
        {
            CookieProto prtCookie;
            switch (objValue.Key.Type)
            {
                case EObjType.COOKIE:
                    prtCookie = ProtoDb.Get<CookieProto>(objValue.Key.Num);
                    return (prtCookie, new GachaResultPacket
                    {
                        ResultObjValue = objValue,
                        SoulStoneNum = prtCookie.SoulStoneNum,
                        SoulStoneAmount = prtCookie.InitSoulStone * amount,
                    });
                case EObjType.SOUL_STONE:
                    var prtSoulStone = ProtoDb.Get<CookieSoulStoneProto>(objValue.Key.Num);
                    prtCookie = ProtoDb.Get<CookieProto>(prtSoulStone.CookieNum);
                    return (prtCookie, new GachaResultPacket
                    {
                        ResultObjValue = objValue,
                        SoulStoneNum = objValue.Key.Num,
                        SoulStoneAmount = amount,
                    });
                default:
                    throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NO_HANDLING_GACHA_RESULT", new { ObjType = objValue.Key.Type });
            }
        }

        // 뽑은 것을 쿠키에 적용한다. 무엇이 얼마나 바뀌었는지는 호출부가 모델에서 읽는다.
        private static void ApplyGachaResult(CookieModel mdlCookie, CookieProto prtCookie, ObjValue objValue)
        {
            var amount = (int)objValue.Value;
            if (objValue.Key.Type == EObjType.COOKIE)
            {
                mdlCookie.IncCookie(amount, prtCookie);
                return;
            }

            mdlCookie.IncSoulStone(amount);
        }

        private readonly IMapper _mapper;
    }
}
