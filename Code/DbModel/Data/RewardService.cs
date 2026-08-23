using Proto;
using Protocol;
using ServerCore;
using WebStudyServer.Data.Queries;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // ObjKey -> 모델 라우터. 재화 하나를 로드하고, 모델에게 바꾸게 하고, 저장한다.
    //
    // Domain 이 아니라 Data 에 둔다 - DB 를 지나가기 때문이다. 대신 상태가 없다:
    // 대상 플레이어는 UserScope 인자로 들어오므로 다른 플레이어에게 지급하는 것도 같은 API 다.
    //
    // 타입별로 개별 구현한다. 중복이 몇 줄 생기지만, 어떤 ObjType 이 어떤 모델을 어떻게
    // 건드리는지가 한눈에 보이는 쪽이 값이 크다.
    public static class RewardService
    {
        // ── 라우팅 ────────────────────────────────────────────────────────
        public static async Task<ChangeSet> PayAsync(UserScope userScope, ObjValue cost, string reason)
        {
            ReqHelper.ValidUnderFlowParam(cost.Value, reason);
            var amount = ReqHelper.ValidWithoutDecimal(cost.Value, reason);

            switch (cost.Key.Type.ToObjTyeCategory())
            {
                case EObjType.EXP:
                    return await DecExpAsync(userScope, amount, reason);
                case EObjType.GOLD:
                    return await DecGoldAsync(userScope, amount, reason);
                case EObjType.TOTAL_CASH:
                    return await DecCashAsync(userScope, amount, reason);
                case EObjType.POINT_START:
                    return await DecPointAsync(userScope, cost.Key, amount, reason);
                case EObjType.TICKET_START:
                    return await DecTicketAsync(userScope, cost.Key, amount, reason);
                case EObjType.ITEM:
                    return await DecItemAsync(userScope, cost.Key, amount, reason);
                default:
                    throw new GameException(EErrorCode.PARAM, "NO_HANDLING_COST_OBJ_TYPE", new { cost.Key.Type });
            }
        }

        public static async Task<List<ChangeSet>> GrantListAsync(UserScope userScope, List<ObjValue> rewardList, string reason)
        {
            var changeList = new List<ChangeSet>(rewardList.Count);
            foreach (var reward in rewardList)
            {
                changeList.Add(await GrantAsync(userScope, reward, reason));
            }

            return changeList;
        }

        public static async Task<ChangeSet> GrantAsync(UserScope userScope, ObjValue reward, string reason)
        {
            ReqHelper.ValidUnderFlowParam(reward.Value, reason);
            var amount = ReqHelper.ValidWithoutDecimal(reward.Value, reason);

            switch (reward.Key.Type.ToObjTyeCategory())
            {
                case EObjType.GOLD:
                    return await IncGoldAsync(userScope, amount);
                case EObjType.EXP:
                    return await IncExpAsync(userScope, amount);
                case EObjType.REAL_CASH:
                    return await IncRealCashAsync(userScope, amount);
                case EObjType.FREE_CASH:
                    return await IncFreeCashAsync(userScope, amount);
                case EObjType.POINT_START:
                    return await IncPointAsync(userScope, reward.Key, amount);
                case EObjType.TICKET_START:
                    return await IncTicketAsync(userScope, reward.Key, amount);
                case EObjType.ITEM:
                    return await IncItemAsync(userScope, reward.Key, amount);
                case EObjType.COOKIE:
                    return await IncCookieAsync(userScope, reward.Key, (int)amount);
                case EObjType.SOUL_STONE:
                    return await IncSoulStoneAsync(userScope, reward.Key, (int)amount);
                default:
                    throw new GameException(EErrorCode.PARAM, "NO_HANDLING_REWARD_OBJ_TYPE", new { reward.Key.Type });
            }
        }

        // ── PlayerDetail 에 있는 재화. Type/Num 이 고정이라 ObjKey 를 안 받는다 ──
        public static async Task<ChangeSet> DecGoldAsync(UserScope userScope, double amount, string reason)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var before = detail.Gold;
            var after = detail.DecGold(amount, reason);
            await detailSet.UpdateAsync(detail);
            return ChangeSet.Of(EObjType.GOLD, 0, before, after);
        }

        public static async Task<ChangeSet> IncGoldAsync(UserScope userScope, double amount)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var before = detail.Gold;
            var after = detail.IncGold(amount);
            await detailSet.UpdateAsync(detail);
            return ChangeSet.Of(EObjType.GOLD, 0, before, after);
        }

        public static async Task<ChangeSet> DecExpAsync(UserScope userScope, double amount, string reason)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var before = detail.Exp;
            var after = detail.DecExp(amount, reason);
            await detailSet.UpdateAsync(detail);
            return ChangeSet.Of(EObjType.EXP, 0, before, after);
        }

        public static async Task<ChangeSet> IncExpAsync(UserScope userScope, double amount)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var before = detail.Exp;
            var after = detail.IncExp(amount);
            await detailSet.UpdateAsync(detail);
            return ChangeSet.Of(EObjType.EXP, 0, before, after);
        }

        public static async Task<ChangeSet> DecCashAsync(UserScope userScope, double amount, string reason)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var before = detail.TotalCash();
            var after = detail.DecCash(amount, reason);
            await detailSet.UpdateAsync(detail);
            return ChangeSet.Of(EObjType.TOTAL_CASH, 0, before, after);
        }

        public static async Task<ChangeSet> IncFreeCashAsync(UserScope userScope, double amount)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var before = detail.FreeCash;
            var after = detail.IncFreeCash(amount);
            await detailSet.UpdateAsync(detail);
            return ChangeSet.Of(EObjType.FREE_CASH, 0, before, after);
        }

        public static async Task<ChangeSet> IncRealCashAsync(UserScope userScope, double amount)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var before = detail.RealCash;
            var after = detail.IncRealCash(amount);
            await detailSet.UpdateAsync(detail);
            return ChangeSet.Of(EObjType.REAL_CASH, 0, before, after);
        }

        // ── 자기 행을 가진 재화. 포인트/티켓의 행 번호는 ObjKey.Num 이 아니라 (int)ObjKey.Type 이다 ──
        public static async Task<ChangeSet> DecPointAsync(UserScope userScope, ObjKey key, double amount, string reason)
        {
            var pointSet = userScope.Owned<PointModel>();
            var point = await pointSet.GetOrCreateAsync((int)key.Type);
            var before = point.Amount;
            var after = point.DecAmount(amount, reason);
            await pointSet.UpdateAsync(point);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }

        public static async Task<ChangeSet> IncPointAsync(UserScope userScope, ObjKey key, double amount)
        {
            var pointSet = userScope.Owned<PointModel>();
            var point = await pointSet.GetOrCreateAsync((int)key.Type);
            var before = point.Amount;
            var after = point.IncAmount(amount);
            await pointSet.UpdateAsync(point);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }

        public static async Task<ChangeSet> DecTicketAsync(UserScope userScope, ObjKey key, double amount, string reason)
        {
            var ticketSet = userScope.Owned<TicketModel>();
            var ticket = await ticketSet.GetOrCreateAsync((int)key.Type);
            var before = ticket.Amount;
            var after = ticket.DecAmount(amount, reason);
            await ticketSet.UpdateAsync(ticket);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }

        public static async Task<ChangeSet> IncTicketAsync(UserScope userScope, ObjKey key, double amount)
        {
            var ticketSet = userScope.Owned<TicketModel>();
            var ticket = await ticketSet.GetOrCreateAsync((int)key.Type);
            var before = ticket.Amount;
            var after = ticket.IncAmount(amount);
            await ticketSet.UpdateAsync(ticket);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }

        public static async Task<ChangeSet> DecItemAsync(UserScope userScope, ObjKey key, double amount, string reason)
        {
            var itemSet = userScope.Owned<ItemModel>();
            var item = await itemSet.GetOrCreateAsync(key.Num);
            var before = item.Amount;
            var after = item.DecAmount(amount, reason);
            await itemSet.UpdateAsync(item);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }

        public static async Task<ChangeSet> IncItemAsync(UserScope userScope, ObjKey key, double amount)
        {
            var itemSet = userScope.Owned<ItemModel>();
            var item = await itemSet.GetOrCreateAsync(key.Num);
            var before = item.Amount;
            var after = item.IncAmount(amount);
            await itemSet.UpdateAsync(item);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }

        // 쿠키는 획득해도 바뀌는 값이 소울스톤이라 Before/After 가 소울스톤이다.
        public static async Task<ChangeSet> IncCookieAsync(UserScope userScope, ObjKey key, int amount)
        {
            var cookieSet = userScope.Owned<CookieModel>();
            var cookie = await cookieSet.GetOrCreateAsync(key.Num);
            var before = (double)cookie.SoulStone;
            var after = cookie.IncCookie(amount, ProtoDb.Get<CookieProto>(key.Num));
            await cookieSet.UpdateAsync(cookie);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }

        // 소울스톤 번호로 대상 쿠키를 찾지만, 응답에는 요청이 지목한 소울스톤 번호가 실린다.
        public static async Task<ChangeSet> IncSoulStoneAsync(UserScope userScope, ObjKey key, int amount)
        {
            var cookieSet = userScope.Owned<CookieModel>();
            var cookie = await cookieSet.GetOrCreateBySoulStoneAsync(key.Num);
            var before = (double)cookie.SoulStone;
            var after = cookie.IncSoulStone(amount);
            await cookieSet.UpdateAsync(cookie);
            return ChangeSet.Of(key.Type, key.Num, before, after);
        }
    }
}
