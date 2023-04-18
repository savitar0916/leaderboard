using Grpc.Core;
using Score.Protos;
using StackExchange.Redis;

namespace ScoreService.Services
{
    public class ScorerService : Scorer.ScorerBase
    {
        private readonly ILogger<ScorerService> _logger;
        private readonly ConnectionMultiplexer _redis;
        public ScorerService(ILogger<ScorerService> logger, ConnectionMultiplexer redis) 
        {
            _logger = logger;
            _redis = redis;
        }

        public override async Task<AddScroreResponse> AddScrore(AddScroreRequest request, ServerCallContext context)
        {

            var db = _redis.GetDatabase();
            var leaderboardKey = $"{request.Subject}_leaderboard";
            bool success = await db.SortedSetAddAsync(leaderboardKey, request.Player, request.Score);

            return new AddScroreResponse 
            {
                Status = success,
                Message = "新增成功",
                Statuscode = 400
            };
        }

        public override async Task<GetTopPlayersResponse> GetTopPlayers(GetTopPlayersRequest request, ServerCallContext context)
        {
            var db = _redis.GetDatabase();
            var leaderboardKey = $"{request.Subject}_leaderboard";
            var topPlayers = await db.SortedSetRangeByRankWithScoresAsync(leaderboardKey, 0, request.Count - 1, Order.Descending);

            var response = new GetTopPlayersResponse();
            foreach (var player in topPlayers)
            {
                response.Players.Add(new PlayerScore { Player = player.Element, Score = (int)player.Score });
            }
            response.Message = "取得成功";
            response.Status = true;
            response.Statuscode = 200;
            return response;
        }

        public override Task<TestResponse> Test(TestRequest request, ServerCallContext context)
        {
            var db = _redis.GetDatabase();

            //string
            db.StringSet("test1", "this is test1");
            db.StringIncrement("visitCount");
            db.StringDecrement("visitCount");
            var result_string = db.StringGet("test1");
            Console.WriteLine(result_string);
            //delete
            db.KeyDelete("test1");
            result_string = db.StringGet("test1");
            Console.WriteLine(result_string);

            //set
            db.SetAdd("event", "001");
            db.SetAdd("event", "002");
            db.SetAdd("event", "003");
            var result_set = db.SetScan("event", "00*");
            Console.WriteLine(result_set.ToString());
            //delete
            db.SetRemove("event", "002");
            Console.WriteLine(result_set);


            //Hash
            db.HashSet("employee", new HashEntry[] {
                new HashEntry("1","anson"),
                new HashEntry("2","kin"),
                new HashEntry("3","jacky"),
            });
            var result_hash = db.HashGetAll("employee").ToList();
            Console.WriteLine(result_hash);
            var result_hashget2 = db.HashGet("employee", 2);
            Console.WriteLine(result_hashget2);
            //delete
            db.HashDelete("employee", 1);
            db.HashSet("employee", 2, "anson");
            result_hash = db.HashGetAll("employee").ToList();
            Console.WriteLine(result_hash);

            //List
            db.ListRightPush("company", "samsung");
            db.ListRightPush("company", "wanin");
            db.ListRightPush("company", "tesla");
            db.ListRightPush("company", "docker");
            db.ListRightPush("company", "apple");
            db.ListRightPush("company", "google");
            var result_List_all = db.ListRange("company").ToList();
            var result_List_0_1 = db.ListRange("company", 0, 1).ToList();
            Console.WriteLine(result_List_all);
            Console.WriteLine(result_List_0_1);
            //delete
            db.ListRightPop("company");
            result_List_all = db.ListRange("company").ToList();
            Console.WriteLine(result_List_all);
            db.ListLeftPop("company");
            result_List_all = db.ListRange("company").ToList();
            Console.WriteLine(result_List_all);
            db.ListRemove("company", "apple");
            result_List_all = db.ListRange("company").ToList();
            Console.WriteLine(result_List_all);

            //zset
            db.SortedSetAdd("leaderboard", "Adam", 100);
            db.SortedSetAdd("leaderboard", "Archer", 90);
            db.SortedSetAdd("leaderboard", "Brian", 60);
            db.SortedSetAdd("leaderboard", "Anna", 70);
            db.SortedSetAdd("leaderboard", "Ellen", 95);
            db.SortedSetAdd("leaderboard", "Hellen", 88);
            var result_ZSet = db.SortedSetRangeByScore("leaderboard", 0, -1);
            Console.WriteLine(result_ZSet);
            //delete
            db.SortedSetRemove("leaderboard", "Archer");
            result_ZSet = db.SortedSetRangeByScore("leaderboard", 0, -1);
            Console.WriteLine(result_ZSet);
            string empty = "";
            return Task.FromResult(new TestResponse { Empty = empty});
        }
    }
}
