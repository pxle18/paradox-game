using GTANetworkAPI;
using System;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Outfits;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerClothes
    {
        public static void SetNacked(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid() || !dbPlayer.IsFreeMode()) return;
            if (dbPlayer.HasData("naked"))
            {
                dbPlayer.ResetData("naked");
            }
            else
            {
                dbPlayer.SetData("naked", true);
            }

            dbPlayer.RefreshNacked();
        }

        public static bool IsFreeMode(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid()) return false;

            return dbPlayer.Character.Skin == PedHash.FreemodeMale01 ||
                   dbPlayer.Character.Skin == PedHash.FreemodeFemale01;
        }

        public static void RefreshNacked(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid() || !dbPlayer.IsFreeMode()) return;
            if (dbPlayer.HasData("naked"))
            {
                // Wenn naked
                if (dbPlayer.IsFemale())
                {
                    // Oberkörper frei
                    dbPlayer.SetClothes(11, 15, 0);
                    // Unterhemd frei
                    dbPlayer.SetClothes(8, 57, 0);
                    // Torso frei
                    dbPlayer.SetClothes(3, 15, 0);
                }
                else
                {
                    // Naked (.)(.)
                    dbPlayer.SetClothes(3, 15, 0);
                    dbPlayer.SetClothes(4, 15, 0);
                    dbPlayer.SetClothes(8, 0, 99);
                    dbPlayer.SetClothes(11, 0, 99);
                }
            }
        }

        public static void SetPlayerJailClothes(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsFreeMode()) return;
            if (dbPlayer.Customization != null)
            {
                dbPlayer.SetClothes(2, dbPlayer.Customization.Hair.Hair, 0);
            }
            
            if (dbPlayer.JailTime[0] > 0)
            {
                dbPlayer.SetOutfit(OutfitTypes.Jail);
            }
        }

        public static void ApplyArmorVisibility(this DbPlayer dbPlayer)
        {
            NAPI.Task.Run(() =>
            {
                if (dbPlayer == null || dbPlayer.Player == null) return;
                if (!dbPlayer.IsValid()) return;
                if (dbPlayer.Character == null) return;
                if (!dbPlayer.IsFreeMode()) return;

                if (dbPlayer.Player.Armor <= 5)
                {
                    if (dbPlayer.IsFemale())
                        dbPlayer.SetClothes(9, 0, 0);
                    else
                        dbPlayer.SetClothes(9, 0, 0);

                    return;
                }

                if (dbPlayer.VisibleArmorType > 1 && !dbPlayer.IsInDuty())
                {
                    dbPlayer.VisibleArmorType = 0;
                }

                switch (dbPlayer.VisibleArmorType)
                {
                    case -1:
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 0, 0);
                        else
                            dbPlayer.SetClothes(9, 0, 0);
                        break;
                    case 2:                     //Police Weste //Rang 0
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 11, 3);
                        else
                            dbPlayer.SetClothes(9, 12, 3);
                        break;
                    case 3:                    //Police GTF Weste //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 7, 0);
                        else
                            dbPlayer.SetClothes(9, 7, 0);
                        break;
                    case 4:                 //Police HP Weste //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 11, 0);
                        else
                            dbPlayer.SetClothes(9, 12, 0);
                        break;
                    case 5:                 //Police Corrections Weste //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 10, 2);
                        else
                            dbPlayer.SetClothes(9, 11, 2);
                        break;
                    case 6:                 //Police PIA //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 11, 4);
                        else
                            dbPlayer.SetClothes(9, 12, 4);
                        break;
                    case 7:                     //Sheriff Weste (Schwarz) //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 11, 2);
                        else
                            dbPlayer.SetClothes(9, 12, 1);
                        break;
                    case 8:                     //Sheriff GTF Weste //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 7, 1);
                        else
                            dbPlayer.SetClothes(9, 7, 1);
                        break;
                    case 9:                     //Sheriff K9 Weste (Gelb) //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 10, 4);
                        else
                            dbPlayer.SetClothes(9, 11, 4);
                        break;
                    case 10:                    //Sheriff K9 Weste (Schwarz) //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 10, 3);
                        else
                            dbPlayer.SetClothes(9, 11, 3);
                        break;
                    case 11:                    //S.W.A.T. Weste //Rang 12
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 7, 4);
                        else
                            dbPlayer.SetClothes(9, 7, 3);
                        break;
                    case 12:                    //FIB Weste (Schwarz)
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 9, 3);
                        else
                            dbPlayer.SetClothes(9, 10, 3);
                        break;
                    case 13:                    //FIB Federal Agent Weste (Blau) 
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 7, 2);
                        else
                            dbPlayer.SetClothes(9, 7, 2);
                        break;
                    case 14:                     //FIB K9 Weste
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 10, 1);
                        else
                            dbPlayer.SetClothes(9, 11, 1);
                        break;
                    case 15:                    //FIB GTF Weste (Blau)
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 10, 0);
                        else
                            dbPlayer.SetClothes(9, 11, 0);
                        break;
                    case 16:                    //IAA Weste 
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 7, 3);
                        else
                            dbPlayer.SetClothes(9, 7, 4);
                        break;
                    case 17:                    //FIB Weste (Grün)
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 9, 4);
                        else
                            dbPlayer.SetClothes(9, 10, 4);
                        break;
                    case 18:                    //Gewöhnliche Weste (Staatsfraktionen)
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 17, 2);
                        else
                            dbPlayer.SetClothes(9, 15, 2);
                        break;
                    case 19:                    //FIB GTF Weste (Schwarz)
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 9, 0);
                        else
                            dbPlayer.SetClothes(9, 10, 0);
                        break;
                    case 20:                    //FIB Federal Agent Weste (Schwarz)
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 9, 1);
                        else
                            dbPlayer.SetClothes(9, 10, 1);
                        break;
                    case 21:                    //FIB Federal Agent Weste (Grün)
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 9, 2);
                        else
                            dbPlayer.SetClothes(9, 10, 2);
                        break;
                    case 22:                    //ARMY MP Weste
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 19, 0);
                        else
                            dbPlayer.SetClothes(9, 17, 0);
                        break;
                    case 23:                    //ARMY Air Force Weste
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 19, 1);
                        else
                            dbPlayer.SetClothes(9, 17, 1);
                        break;
                    case 24:                    //ARMY Combat Weste
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 26, 0);
                        else
                            dbPlayer.SetClothes(9, 24, 0);
                        break;
                    case 25:                    //ARMY Infantry Weste
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 26, 1);
                        else
                            dbPlayer.SetClothes(9, 24, 1);
                        break;
                    case 26:                    //ARMY Coast Guard Weste
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 26, 2);
                        else
                            dbPlayer.SetClothes(9, 24, 2);
                        break;
                    case 27:                    //IAA SAD Weste 
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 4, 2);
                        else
                            dbPlayer.SetClothes(9, 3, 2);
                        break;
                    case 28:                    //FIB iwas 1 
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 4, 0);
                        else
                            dbPlayer.SetClothes(9, 3, 0);
                        break;
                    case 29:                    //FIB iwas 2
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 3, 1);
                        else
                            dbPlayer.SetClothes(9, 3, 1);
                        break;
                    case 30:                    //Underarmor coppa
                        dbPlayer.SetClothes(9, 0, 0);
                        break;
                    case 31:                    //DPOS Weste
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 40, 0);
                        else
                            dbPlayer.SetClothes(9, 38, 0);
                        break;
                    default:
                        if (dbPlayer.IsFemale())
                            dbPlayer.SetClothes(9, 17, 2);
                        else
                            dbPlayer.SetClothes(9, 15, 2);
                        break;
                }
            });
        }
    }
}