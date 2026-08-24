using Proto;
using Protocol;
using ServerCore;
using ServerCore.Extension;
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
        // 리스트를 반환하는 이유는 TOTAL_CASH 뿐이다. 나머지는 항상 1개다.
        public static async Task<List<ChangeSet>> PayAsync(UserScope userScope, ObjValue cost, string reason)
        {
            var changeList = await PayInternalAsync(userScope, cost, reason);
            LogChanges(userScope, "PAY", reason, changeList);
            return changeList;
        }

        private static async Task<List<ChangeSet>> PayInternalAsync(UserScope userScope, ObjValue cost, string reason)
        {
            ReqHelper.ValidUnderFlowParam(cost.Value, reason);
            var amount = ReqHelper.ValidWithoutDecimal(cost.Value, reason);

            switch (cost.Key.Type.ToObjTyeCategory())
            {
                case EObjType.EXP:
                    return [await DecExpAsync(userScope, amount, reason)];
                case EObjType.GOLD:
                    return [await DecGoldAsync(userScope, amount, reason)];
                case EObjType.TOTAL_CASH:
                    return await DecCashAsync(userScope, amount, reason);
                case EObjType.POINT_START:
                    return [await DecPointAsync(userScope, cost.Key, amount, reason)];
                case EObjType.TICKET_START:
                    return [await DecTicketAsync(userScope, cost.Key, amount, reason)];
                case EObjType.ITEM:
                    return [await DecItemAsync(userScope, cost.Key, amount, reason)];
                default:
                    throw new GameException(EErrorCode.PARAM, "NO_HANDLING_COST_OBJ_TYPE", new { cost.Key.Type });
            }
        }

        public static async Task<List<ChangeSet>> GrantListAsync(UserScope userScope, List<ObjValue> rewardList, string reason)
        {
            var changeList = new List<ChangeSet>(rewardList.Count);
            foreach (var reward in rewardList)
            {
                changeList.AddRange(await GrantAsync(userScope, reward, reason));
            }

            return changeList;
        }

        // 리스트를 반환하는 이유는 쿠키뿐이다. 한 장을 받으면 소울스톤 수량과 보유 수량이 같이 바뀐다.
        // PayAsync 가 TOTAL_CASH 때문에 리스트인 것과 같은 이유다.
        public static async Task<List<ChangeSet>> GrantAsync(UserScope userScope, ObjValue reward, string reason)
        {
            var changeList = await GrantInternalAsync(userScope, reward, reason);
            LogChanges(userScope, "GRANT", reason, changeList);
            return changeList;
        }

        // 유료 재화 원장은 캐시를 바꾸는 메서드가 직접 쓴다. 여기는 전 축을 로그로만 남긴다.
        private static void LogChanges(UserScope userScope, string action, string reason, List<ChangeSet> changeList)
        {
            foreach (var change in changeList)
            {
                Logger.Get().Info("ObjChange Action({Action}) PlayerId({PlayerId}) Reason({Reason}) Type({Type}) Num({Num}) Bef({Bef}) Aft({Aft})",
                    action, userScope.PlayerId, reason, change.Type, change.Num, change.Before, change.After);
            }
        }

        private static async Task<List<ChangeSet>> GrantInternalAsync(UserScope userScope, ObjValue reward, string reason)
        {
            ReqHelper.ValidUnderFlowParam(reward.Value, reason);
            var amount = ReqHelper.ValidWithoutDecimal(reward.Value, reason);

            switch (reward.Key.Type.ToObjTyeCategory())
            {
                case EObjType.GOLD:
                    return [await IncGoldAsync(userScope, amount)];
                case EObjType.EXP:
                    return [await IncExpAsync(userScope, amount)];
                case EObjType.REAL_CASH:
                    return [await IncRealCashAsync(userScope, amount, reason)];
                case EObjType.FREE_CASH:
                    return [await IncFreeCashAsync(userScope, amount, reason)];
                case EObjType.POINT_START:
                    return [await IncPointAsync(userScope, reward.Key, amount)];
                case EObjType.TICKET_START:
                    return [await IncTicketAsync(userScope, reward.Key, amount)];
                case EObjType.ITEM:
                    return [await IncItemAsync(userScope, reward.Key, amount)];
                case EObjType.COOKIE:
                    return await IncCookieAsync(userScope, reward.Key, (int)amount);
                case EObjType.SOUL_STONE:
                    return [await IncSoulStoneAsync(userScope, reward.Key, (int)amount)];
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

        // TOTAL_CASH 는 RealCash + FreeCash 라서 ChgObj 하나에 담기지 않는다.
        // 실제로 바뀐 컬럼만 REAL_CASH / FREE_CASH 로 돌려준다. TOTAL_CASH 는 와이어에 안 나간다.
        public static async Task<List<ChangeSet>> DecCashAsync(UserScope userScope, double amount, string reason)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();

            var befReal = detail.RealCash;
            var befFree = detail.FreeCash;
            detail.DecCash(amount, reason);
            await detailSet.UpdateAsync(detail);
            await AuditService.WriteCashChangeAsync(userScope, reason, detail, befReal, befFree);

            var changeList = new List<ChangeSet>(2);
            if (detail.RealCash != befReal)
            {
                changeList.Add(ChangeSet.Of(EObjType.REAL_CASH, 0, befReal, detail.RealCash));
            }

            if (detail.FreeCash != befFree)
            {
                changeList.Add(ChangeSet.Of(EObjType.FREE_CASH, 0, befFree, detail.FreeCash));
            }

            return changeList;
        }

        public static async Task<ChangeSet> IncFreeCashAsync(UserScope userScope, double amount, string reason)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var befReal = detail.RealCash;
            var before = detail.FreeCash;
            var after = detail.IncFreeCash(amount);
            await detailSet.UpdateAsync(detail);
            await AuditService.WriteCashChangeAsync(userScope, reason, detail, befReal, before);
            return ChangeSet.Of(EObjType.FREE_CASH, 0, before, after);
        }

        public static async Task<ChangeSet> IncRealCashAsync(UserScope userScope, double amount, string reason)
        {
            var detailSet = userScope.Owned<PlayerDetailModel>();
            var detail = await detailSet.GetOrCreateAsync();
            var befFree = detail.FreeCash;
            var before = detail.RealCash;
            var after = detail.IncRealCash(amount);
            await detailSet.UpdateAsync(detail);
            await AuditService.WriteCashChangeAsync(userScope, reason, detail, before, befFree);
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

        // 쿠키 행은 수량이 둘이다. 소울스톤은 SOUL_STONE 번호로, 보유 수량은 COOKIE 번호로 나간다.
        // 쿠키를 뽑아 소울스톤으로 환산된 몫도 SOUL_STONE 쪽에 실린다.
        public static async Task<List<ChangeSet>> IncCookieAsync(UserScope userScope, ObjKey key, int amount)
        {
            var changeList = new List<ChangeSet>(2);

            var prtCookie = ProtoDb.Get<CookieProto>(key.Num);
            var cookieSet = userScope.Owned<CookieModel>();
            var cookie = await cookieSet.GetOrCreateAsync(key.Num);

            // 보유 수량이 바뀌는 건 첫 획득 때만.
            if (cookie.State != ECookieState.AVAILABLE)
            {
                changeList.Add(ChangeSet.Of(EObjType.COOKIE, prtCookie.Num, 0, 1));
            }

            var befSoulStone = (double)cookie.SoulStone;
            cookie.IncCookie(amount, prtCookie);
            await cookieSet.UpdateAsync(cookie);

            changeList.Add(ChangeSet.Of(EObjType.SOUL_STONE, prtCookie.SoulStoneNum, befSoulStone, cookie.SoulStone));
            return changeList;
        }

        // 소울스톤만 늘어난다. 보유 수량은 IncCookie 만 바꾸므로 여기선 나올 수 없다.
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
