// See https://aka.ms/new-console-template for more information
using Client;
using Protocol;

var ctxSystem = new ContextSystem();
ctxSystem.Init();

var funcDict = new Dictionary<int, ApiFunc>()
{
    { 1, new ApiFunc(){ ApiPath = typeof(AuthSignUpReqPacket).ToString(), Desc = "회원 가입", Action = async (value) =>  await ctxSystem.RequestSignUpAsync(Guid.NewGuid().ToString())} },
    { 2, new ApiFunc(){ ApiPath = typeof(AuthSignInReqPacket).ToString(), Desc = "기존 계정 로그인",Action = async (value) =>  await ctxSystem.RequestSignInAsync(value)} },


    { 100, new ApiFunc(){ ApiPath = typeof(GameEnterReqPacket).ToString(), Desc = "플레이어 로드", Action = async (value) =>  await ctxSystem.RequestEnterAsync()} },
    { 101, new ApiFunc(){ ApiPath = typeof(GameChangeNameReqPacket).ToString(), Desc = "닉네임 초기 설정", Action = async (value) =>  await ctxSystem.RequestChangeNameAsync(value)} },

    { 0, new ApiFunc(){ ApiPath = "", Desc = "종료" } }
};

var isRunning = true;
while (isRunning)
{
    Console.WriteLine("\n--- 명령 선택 ---");
    foreach (var num in funcDict.Keys.OrderBy(x => x))
    {
        var apiPath = funcDict[num].ApiPath;
        Console.WriteLine($"{num}. {apiPath}");
    }
    Console.WriteLine("숫자를 입력하세요: ");

    try
    {
        var input = Console.ReadLine();
        var inputArr = input?.Split(" ");
        var inputNum = inputArr == null ? 0 : int.Parse(inputArr[0]);
        var inputString = inputArr == null || inputArr.Length <= 1 ? string.Empty : inputArr[1];

        if (inputNum == 0)
        {
            isRunning = false;
        }

        if (!funcDict.TryGetValue(inputNum, out var outApiFund))
        {
            Console.WriteLine($"잘못 입력했습니다. {inputNum}");
            continue;
        }

        await outApiFund.Action.Invoke(inputString);
    }
    catch(Exception ex)
    {
        Console.WriteLine($"ERROR:{ex.Message.ToString()}, {ex.StackTrace}");
        ctxSystem.Clear();
        ctxSystem.Init();
    }
}

ctxSystem.Clear();