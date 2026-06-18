// See https://aka.ms/new-console-template for more information
using ClientCore;
using Protocol;
var initPath = args.Length > 0 ? args[0] : "../../Data/Csv/Proto";
APP.Init(Path.Join(APP.GetProjPath(), initPath), "http://localhost:5157", TimeSpan.FromSeconds(5));
APP.Prt.Bind();

var deviceKeyPath = Path.Combine(APP.GetProjPath(), "devicekey.dat");
var deviceKey = DeviceKeyHelper.LoadOrCreateKey(deviceKeyPath);
APP.Ctx.RpcSystem.SetDeviceKey(deviceKey);
Console.WriteLine($"DeviceKey: {APP.Ctx.RpcSystem.DeviceKey}");

var funcDict = new Dictionary<int, ApiFunc>()
{
    { -3, new ApiFunc(){ ApiPath = "DeviceKey Reset (Memory)", Desc = "DeviceKey 메모리만 재발급 (파일 유지)",
        Action = (valueArr) =>
        {
            var newKey = DeviceKeyHelper.GenerateKey();
            APP.Ctx.RpcSystem.SetDeviceKey(newKey);
            Console.WriteLine($"새 DeviceKey (메모리): {newKey}");
            return Task.CompletedTask;
        }
    } },
    { -2, new ApiFunc(){ ApiPath = "DeviceKey Reset", Desc = "DeviceKey 재발급",
        Action = (valueArr) =>
        {
            var dkPath = Path.Combine(APP.GetProjPath(), "devicekey.dat");
            var newKey = DeviceKeyHelper.RegenerateKey(dkPath);
            APP.Ctx.RpcSystem.SetDeviceKey(newKey);
            Console.WriteLine($"새 DeviceKey: {newKey}");
            return Task.CompletedTask;
        }
    } },
    { -1, new ApiFunc(){ ApiPath = HealthCheckRequestPacket.NAME, Desc = "HealthCheck",
        Action = async (valueArr) =>  await APP.Ctx.RequestHealthCheckAsync()} },

    { 1, new ApiFunc(){ ApiPath = AuthSignUpRequestPacket.NAME, Desc = "회원 가입",
        Action = async (valueArr) =>  await APP.Ctx.RequestSignUpAsync(APP.Ctx.RpcSystem.DeviceKey)} },
    { 2, new ApiFunc(){ ApiPath = AuthSignInRequestPacket.NAME, Desc = "기존 계정 로그인 (ChannelKey)",
        Action = async (valueArr) =>  await APP.Ctx.RequestSignInAsync(valueArr[0])} },


    { 100, new ApiFunc(){ ApiPath = GameEnterRequestPacket.NAME, Desc = "플레이어 로드",
        Action = async (valueArr) =>  await APP.Ctx.RequestEnterAsync()} },
    { 101, new ApiFunc(){ ApiPath = GameChangeNameRequestPacket.NAME, Desc = "닉네임 초기 설정 (Name)",
        Action = async (valueArr) =>  await APP.Ctx.RequestChangeNameAsync(valueArr[0])} },

    { 200, new ApiFunc(){ ApiPath = "Kingdom Print", Desc = "",
        Action = (valueArr) => { APP.Ctx.PrintKingdom(); return Task.CompletedTask; } } },
    { 201, new ApiFunc(){ ApiPath = KingdomBuyStructureRequestPacket.NAME, Desc = "KingdomStructure 구매 (Num)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomBuyStructure((int.Parse(valueArr[0]))) } },
    { 202, new ApiFunc(){ ApiPath = KingdomConstructStructureRequestPacket.NAME, Desc = "KingdomStructure 건설 (StructureId, X, Y)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomConstructureStructure(ulong.Parse(valueArr[0]), int.Parse(valueArr[1]), int.Parse(valueArr[2])) } },
    { 203, new ApiFunc(){ ApiPath = KingdomFinishConstructStructureRequestPacket.NAME, Desc = "KingdomStructure 건설 종료 (StructureId, Num)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomFinishConstructStructure(ulong.Parse(valueArr[0]), int.Parse(valueArr[1]))} },
    { 204, new ApiFunc(){ ApiPath = KingdomBuyDecoRequestPacket.NAME, Desc = "KingdomDeco 구매 (Num)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomBuyDeco((int.Parse(valueArr[0]))) } },
    { 205, new ApiFunc(){ ApiPath = KingdomConstructDecoRequestPacket.NAME, Desc = "KingdomDeco 건설 (Num , X, Y)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomConstructDeco(int.Parse(valueArr[0]), int.Parse(valueArr[1]), int.Parse(valueArr[2])) } },
    { 206, new ApiFunc(){ ApiPath = KingdomFinishCraftStructureRequestPacket.NAME, Desc = "KingdomStructure 생산 물품 받기 (StructureId)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomFinishCraftStructure(ulong.Parse(valueArr[0]))} },

    { 300, new ApiFunc(){ ApiPath = "CookieList Print", Desc = "", Action = (valueArr) => { APP.Ctx.PrintCookieList(); return Task.CompletedTask; } } },
    { 301, new ApiFunc(){ ApiPath = CookieEnhanceStarRequestPacket.NAME, Desc = "Cookie Enhance Star (CookieNum, Star)",
        Action = async (valueArr) =>  await APP.Ctx.RequestCookieEnhanceStar(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },
    { 302, new ApiFunc(){ ApiPath = CookieEnhanceLvRequestPacket.NAME, Desc = "Cookie Enhance Lv (CookieNum, Lv)",
        Action = async (valueArr) =>  await APP.Ctx.RequestCookieEnhanceLv(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },


    { 400, new ApiFunc(){ ApiPath = GachaNormalRequestPacket.NAME, Desc = "GachaNormal (ScheduleNum, Cnt)",
        Action = async (valueArr) =>  await APP.Ctx.RequestGachaNormal(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },

    { 500, new ApiFunc(){ ApiPath = ScheduleLoadRequestPacket.NAME, Desc = "ScheduleLoad ",
        Action = async (valueArr) =>  await APP.Ctx.RequestLoadSchedule() }},

    { 600, new ApiFunc(){ ApiPath = "World Print", Desc = "", Action = (valueArr) => { APP.Ctx.PrintWorldList(); return Task.CompletedTask; } } },
    { 601, new ApiFunc(){ ApiPath = "WorldStage Print", Desc = "", Action = (valueArr) => { APP.Ctx.PrintWorldStageList(); return Task.CompletedTask; } } },
    { 602, new ApiFunc(){ ApiPath = WorldFinishStageFirstRequestPacket.NAME, Desc = "(WorldNum, OrderNum, Star)", Action = async (valueArr) =>  await APP.Ctx.RequestWorldFinishFirstStage(int.Parse(valueArr[0]), int.Parse(valueArr[1]), int.Parse(valueArr[2])) }},
    { 603, new ApiFunc(){ ApiPath = WorldFinishStageRepeatRequestPacket.NAME, Desc = "(WorldNum, OrderNum, Star)", Action = async (valueArr) =>  await APP.Ctx.RequestWorldFinishRepeatStage(int.Parse(valueArr[0]), int.Parse(valueArr[1]), int.Parse(valueArr[2])) }},
    { 604, new ApiFunc(){ ApiPath = WorldRewardStarRequestPacket.NAME, Desc = "(WorldNum, Star)", Action = async (valueArr) =>  await APP.Ctx.RequestWorldRewardStar(int.Parse(valueArr[0]), int.Parse(valueArr[1])) }},

    { 700, new ApiFunc(){ ApiPath = "Raid Connect", Desc = "Raid 서버 접속 (Host Port, 기본 127.0.0.1 5000)",
        Action = async (valueArr) =>  {
            var host = valueArr.Length > 0 ? valueArr[0] : "127.0.0.1";
            var port = valueArr.Length > 1 ? int.Parse(valueArr[1]) : 5000;
            await APP.Ctx.RequestRaidConnectAsync(host, port);
            }
        }
    },
    { 701, new ApiFunc(){ ApiPath = "Raid Echo", Desc = "Raid Echo 전송 (Message)",
        Action = async (valueArr) =>  {
            var message = valueArr.Length > 0 ? string.Join(" ", valueArr) : "Hello Raid";
            await APP.Ctx.RequestRaidEchoAsync(message);
            }
        }
    },
    { 702, new ApiFunc(){ ApiPath = "Raid Disconnect", Desc = "Raid 서버 접속 종료",
        Action = async (valueArr) =>  await APP.Ctx.RequestRaidDisconnectAsync() } },
    { 703, new ApiFunc(){ ApiPath = "Raid Echo (Auth)", Desc = "인증 게이트 Echo 전송 (Message)",
        Action = async (valueArr) =>  {
            var message = valueArr.Length > 0 ? string.Join(" ", valueArr) : "Hello Raid";
            await APP.Ctx.RequestRaidEchoAuthAsync(message);
            }
        }
    },
    { 704, new ApiFunc(){ ApiPath = "Raid Matching Start", Desc = "매칭 시작 (BossNum)",
        Action = async (valueArr) => {
            var bossNum = valueArr.Length > 0 ? int.Parse(valueArr[0]) : 1;
            await APP.Ctx.RequestRaidMatchingStartAsync(bossNum);
            }
        }
    },
    { 705, new ApiFunc(){ ApiPath = "Raid Matching Cancel", Desc = "매칭 취소",
        Action = async (valueArr) => await APP.Ctx.RequestRaidMatchingCancelAsync() } },

    { 9001, new ApiFunc(){ ApiPath = CheatRewardRequestPacket.NAME, Desc = "Chaet 보상 획득 (ObjType, ObjNum, ObjAmount)",
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
    Console.WriteLine($"\n--- 명령 선택 --- (세션: {APP.Ctx.RpcSystem.SessionId} | DeviceKey: {APP.Ctx.RpcSystem.DeviceKey})");
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
            continue;
        }

        if (!funcDict.TryGetValue(inputNum, out var outApiFund))
        {
            Console.WriteLine($"잘못 입력했습니다. {inputNum}");
            continue;
        }

        var inputStrArr = inputArr.Skip(1).ToArray();
        await outApiFund.Action.Invoke(inputStrArr);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR:{ex.Message.ToString()}, {ex.StackTrace}");
        //APP.Ctx.Clear();
        //APP.Ctx.Init();
    }
}

APP.Ctx.Clear();
