// See https://aka.ms/new-console-template for more information
using Client;
using Protocol;
var initPath = args.Length > 0 ? args[0] : "../../Data/Csv/Proto";
APP.Init(initPath);
APP.Prt.Bind();

var funcDict = new Dictionary<int, ApiFunc>()
{
    { 1, new ApiFunc(){ ApiPath = typeof(AuthSignUpReqPacket).ToString(), Desc = "회원 가입",
        Action = async (valueArr) =>  await APP.Ctx.RequestSignUpAsync(Guid.NewGuid().ToString())} },
    { 2, new ApiFunc(){ ApiPath = typeof(AuthSignInReqPacket).ToString(), Desc = "기존 계정 로그인 (ChannelKey)",
        Action = async (valueArr) =>  await APP.Ctx.RequestSignInAsync(valueArr[0])} },


    { 100, new ApiFunc(){ ApiPath = typeof(GameEnterReqPacket).ToString(), Desc = "플레이어 로드",
        Action = async (valueArr) =>  await APP.Ctx.RequestEnterAsync()} },
    { 101, new ApiFunc(){ ApiPath = typeof(GameChangeNameReqPacket).ToString(), Desc = "닉네임 초기 설정 (Name)",
        Action = async (valueArr) =>  await APP.Ctx.RequestChangeNameAsync(valueArr[0])} },

    { 200, new ApiFunc(){ ApiPath = "Kingdom Print", Desc = "",
        Action = (valueArr) => { APP.Ctx.PrintKingdom(); return Task.CompletedTask; } } },
    { 201, new ApiFunc(){ ApiPath = typeof(KingdomBuyStructureReqPacket).ToString(), Desc = "KingdomStructure 구매 (Num)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomBuyStructure((int.Parse(valueArr[0]))) } },
    { 202, new ApiFunc(){ ApiPath = typeof(KingdomConstructStructureReqPacket).ToString(), Desc = "KingdomStructure 건설 (StructureId, X, Y)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomConstructureStructure(ulong.Parse(valueArr[0]), int.Parse(valueArr[1]), int.Parse(valueArr[2])) } },
    { 203, new ApiFunc(){ ApiPath = typeof(KingdomFinishConstructStructureReqPacket).ToString(), Desc = "KingdomStructure 건설 종료 (StructureId, Num)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomFinishConstructStructure(ulong.Parse(valueArr[0]), int.Parse(valueArr[1]))} },
    { 204, new ApiFunc(){ ApiPath = typeof(KingdomBuyDecoReqPacket).ToString(), Desc = "KingdomDeco 구매 (Num)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomBuyDeco((int.Parse(valueArr[0]))) } },
    { 205, new ApiFunc(){ ApiPath = typeof(KingdomConstructDecoReqPacket).ToString(), Desc = "KingdomDeco 건설 (Num , X, Y)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomConstructDeco(int.Parse(valueArr[0]), int.Parse(valueArr[1]), int.Parse(valueArr[2])) } },
    { 206, new ApiFunc(){ ApiPath = typeof(KingdomFinishCraftStructureReqPacket).ToString(), Desc = "KingdomStructure 생산 물품 받기 (StructureId)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomFinishCraftStructure(ulong.Parse(valueArr[0]))} },

    { 300, new ApiFunc(){ ApiPath = "CookieList Print", Desc = "", Action = (valueArr) => { APP.Ctx.PrintCookieList(); return Task.CompletedTask; } } },
    { 301, new ApiFunc(){ ApiPath = typeof(CookieEnhanceStarReqPacket).ToString(), Desc = "Cookie Enhance Star (CookieNum, Star)",
        Action = async (valueArr) =>  await APP.Ctx.RequestCookieEnhanceStar(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },
    { 302, new ApiFunc(){ ApiPath = typeof(CookieEnhanceLvReqPacket).ToString(), Desc = "Cookie Enhance Lv (CookieNum, Lv)",
        Action = async (valueArr) =>  await APP.Ctx.RequestCookieEnhanceLv(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },


    { 400, new ApiFunc(){ ApiPath = typeof(GachaNormalReqPacket).ToString(), Desc = "GachaNormal (ScheduleNum, Cnt)",
        Action = async (valueArr) =>  await APP.Ctx.RequestGachaNormal(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },

    { 500, new ApiFunc(){ ApiPath = typeof(ScheduleLoadReqPacket).ToString(), Desc = "ScheduleLoad ",
        Action = async (valueArr) =>  await APP.Ctx.RequestLoadSchedule() }},

    { 9001, new ApiFunc(){ ApiPath = typeof(CheatRewardReqPacket).ToString(), Desc = "Chaet 보상 획득 (ObjType, ObjNum, ObjAmount)",
        Action = async (valueArr) =>  {
            var objType = valueArr.Length > 0 ? valueArr[0] : "";
            var objNum = valueArr.Length > 1 ? int.Parse(valueArr[1]) : 0;
            var objAmount = valueArr.Length > 2 ? int.Parse(valueArr[2]) : 10000;
            await APP.Ctx.RequestCheatReward(objType, objNum, objAmount);
            }
        }
    },

    { 0, new ApiFunc(){ ApiPath = "", Desc = "종료" } }
};

var isRunning = true;
while (isRunning)
{
    Console.WriteLine($"\n--- 명령 선택 --- (현재 세션: {APP.Ctx.SessionId})");
    foreach (var num in funcDict.Keys.OrderBy(x => x))
    {
        var apiPath = funcDict[num].ApiPath;
        var desc = funcDict[num].Desc;
        Console.WriteLine($"{num}. {apiPath}, {desc}");
    }
    Console.WriteLine("숫자를 입력하세요: ");

    try
    {
        var input = Console.ReadLine();
        var inputArr = input?.Split(" ");
        var inputNum = inputArr == null ? 0 : int.Parse(inputArr[0]);
        //var inputString = inputArr == null || inputArr.Length <= 1 ? string.Empty : inputArr[1];

        if (inputNum == 0)
        {
            isRunning = false;
        }

        if (!funcDict.TryGetValue(inputNum, out var outApiFund))
        {
            Console.WriteLine($"잘못 입력했습니다. {inputNum}");
            continue;
        }

        var inputStrArr = inputArr.Skip(1).ToArray();
        await outApiFund.Action.Invoke(inputStrArr);
    }
    catch(Exception ex)
    {
        Console.WriteLine($"ERROR:{ex.Message.ToString()}, {ex.StackTrace}");
        //APP.Ctx.Clear();
        //APP.Ctx.Init();
    }
}

APP.Ctx.Clear();