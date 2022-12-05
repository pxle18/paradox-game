using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Ranks
 {
     public class RankModule : SqlModule<RankModule, Rank, uint>
     {
         protected override string GetQuery()
         {
             return "SELECT * FROM `player_ranks`;";
         }
 
         public override void OnPlayerConnected(DbPlayer dbPlayer)
         {
             dbPlayer.Rank = this[dbPlayer.RankId] ?? this[0];             
         }
     }
 }