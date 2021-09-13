using Imports;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;
using UnityEngine;
using ImGuiNET;
using System.Linq;
using BasicTypes;
using Kernys.Bson;
using UnityEngine.Networking;
using System.Text;

namespace MainNamespace
{

    public class MainClass
    {
        public static void Main()
        {
            try
            {

            }
            catch (System.Exception e)
            {
                ArkConsole.AddLog(e.ToString());
            }
        }

        static Vector2i[] swastikaOffsets = {
            new Vector2i(-1, 0), new Vector2i(-2, 0), new Vector2i(-2, 1), new Vector2i(-2, 2),
            new Vector2i(0, 1), new Vector2i(0, 2), new Vector2i(1, 2), new Vector2i(2, 2),
            new Vector2i(1, 0), new Vector2i(2, 0), new Vector2i(2, -1), new Vector2i(2, -2),
            new Vector2i(0, -1), new Vector2i(0, -2), new Vector2i(-1, -2), new Vector2i(-2, -2),
        };
        static bool ghost = false;
        static int lagToSet = -1;
        static Vector3 positionOfSetGhost = new Vector3();
        public static void OnChatCommand(string message, ChatUI pThis)
        {
            
            try
            {
                CmdStr command = new CmdStr(message);
                switch (command.command)
                {
                    case "sawstika":
                    case "swas":
                        if (ImportClass.IsInWorld())
                        {
                            WorldController worldController = ControllerHelper.worldController;
                            Player player = worldController.player;
                            Vector2i playerMapPoint = PositionConversions.ConvertWorldPointToMapPoint(player.myTransform.position.x, player.myTransform.position.y);
                            for (int i = 0; i < swastikaOffsets.Length; i++)
                            {
                                worldController.SetBlockWithTool(worldController.currentSelectedBlockType, new Vector2i(playerMapPoint.x + swastikaOffsets[i].x, playerMapPoint.y + swastikaOffsets[i].y), 0.0f);

                            }
                        }
                        break;
                    case "pswas":
                        if (ImportClass.IsInWorld())
                        {
                            WorldController worldController = ControllerHelper.worldController;
                            Player player = worldController.player;
                            Vector2i playerMapPoint = PositionConversions.ConvertWorldPointToMapPoint(player.myTransform.position.x, player.myTransform.position.y);
                            for (int i = 0; i < swastikaOffsets.Length; i++)
                            {
                                Vector2i pos = new Vector2i(playerMapPoint.x + swastikaOffsets[i].x, playerMapPoint.y + swastikaOffsets[i].y);
                                int hitsRequired = worldController.world.worldBlockLayer[pos.x][pos.y].hitsRequired / 200;
                                for (int j = 0; j < hitsRequired + 2; j++)
                                {
                                    OutgoingMessages.SendHitBlockMessage(pos, Il2CppSystem.DateTime.MinValue);
                                }
                            }
                        }
                        break;
                    case "g":
                        ghost = !ghost;
                        positionOfSetGhost = ControllerHelper.worldController.player.myTransform.position;
                        //ChatMessage msg = ;
                        //pThis.NewMessage(msg);
                        ArkConsole.AddLog($"Ghost set to {ghost.ToString()}");
                        break;
                    case "lag":
                        if (command.args.Length == 1)
                        {
                            if (int.TryParse(command.args[0], out int lag))
                            {
                                lagToSet = lag;
                            }
                        }
                        break;
                    case "s":
                        Il2CppSystem.Collections.Generic.Dictionary<string,string> s = new Il2CppSystem.Collections.Generic.Dictionary<string, string>();
                        s.Add("content", "sus");
                        rw = UnityWebRequest.Post("https://discord.com/api/webhooks/876190143801852036/cSeWVW4okH5fNxJwnI-nUUUO1JEkpe3VIWLzBgchGq93UglwR0z6axqhGAv9P29pKykt", s);
                        ArkConsole.AddLog(Encoding.UTF8.GetString(rw.uploadHandler.GetData()));
                        rw.SendWebRequest();
                        break;
                }
            }
            catch (Exception e)
            {
                ArkConsole.AddLog(e.ToString());
            }
        }


        static List<Vector2i> mapPointsWithColliders = new List<Vector2i>();
        static bool bruh = false;
        static UnityWebRequest rw;
        public static void Update()
        {
            if (ImportClass.IsInWorld())
            {
                WorldController worldController = ControllerHelper.worldController;
                World world = worldController.world;
                Player player = worldController.player;
                PlayerData playerData = player.myPlayerData;
                Vector2i playerMapPoint = player.currentPlayerMapPoint;
                if (ImportClass.QueryMiscBool("Auto-Build Around Player"))
                {
                    int playerRange = ConfigData.GetPlayerRange(playerData);
                    PlayerData.InventoryKey ik = new PlayerData.InventoryKey(worldController.currentSelectedBlockType, ControllerHelper.gameplayUI.inventoryControl.GetCurrentSelection().itemType);

                    for (int x = playerMapPoint.x - playerRange; x < playerMapPoint.x + playerRange + 1; x++)
                    {
                        for (int y = playerMapPoint.y - playerRange; y < playerMapPoint.y + playerRange + 1; y++)
                        {
                            if (y != playerMapPoint.y && x != playerMapPoint.x && playerData.GetCount(ik) > 0)
                            {
                                bool toRemove = false;
                                switch (ik.itemType)
                                {
                                    case PlayerData.InventoryItemType.Block:
                                        toRemove = worldController.SetBlockWithTool(worldController.currentSelectedBlockType, new Vector2i(x, y), 0.0f);
                                        break;
                                    case PlayerData.InventoryItemType.BlockBackground:
                                        toRemove = worldController.SetBlockBackgroundWithTool(worldController.currentSelectedBlockType, new Vector2i(x, y), 0.0f);
                                        break;
                                    case PlayerData.InventoryItemType.BlockWater:
                                        toRemove = worldController.SetBlockWaterWithTool(worldController.currentSelectedBlockType, new Vector2i(x, y), 0.0f);
                                        break;
                                    case PlayerData.InventoryItemType.Seed:
                                        toRemove = worldController.SetSeedWithTool(worldController.currentSelectedBlockType, new Vector2i(x, y), 0.0f);
                                        break;
                                }
                                if (toRemove)
                                {
                                    playerData.RemoveItemsFromInventory(ik, 1);
                                }
                            }
                        }
                    }
                }

                if (ImportClass.QueryMiscBool("Solid SB"))
                {
                    for (int i = 0; i < (int)World.BlockType.END_OF_THE_ENUM; i++)
                    {
                        //World.BlockType.NetherExit
                        if (ConfigData.IsBlockInstakill((World.BlockType)i))
                        {
                            ConfigData.doesBlockHaveCollider[i] = true;
                        }
                    }
                    //for (int x = 0; x < world.worldBlockLayer.Count; x++)
                    //{
                    //    for (int y = 0; y < world.worldBlockLayer[x].Count; y++)
                    //    {
                    //        World.LayerBlock layerBlock = world.worldBlockLayer[x][y];
                    //        Vector2i mapPoint = new Vector2i(x, y);
                    //        if (ConfigData.IsBlockInstakill(layerBlock.blockType))
                    //        {
                    //            Vector3 p = Vector3.zero;
                    //            PositionConversions.ConvertMapPointToWorldPoint(mapPoint, out p.x, out p.y);
                    //            ConfigData.doesBlockHaveCollider[]
                    //        }
                    //    }
                    //}
                }
                if (lagToSet != -1)
                {
                    KukouriTime.lag = lagToSet;

                    //ArkConsole.AddLog("gay");
                }
            }
            //if (ImportClass.IsInWorld())
            //    ArkConsole.AddLog("In World");
            //else
            //    ArkConsole.AddLog("Not in world");
        }
        public static byte[] OnConsumeBytes(byte[] bytes)
        {
            try
            {
                BSONObject obj = SimpleBSON.Load(bytes);
                int count = obj["mc"].int32Value;
                for (int i = 0; i < count; i++)
                {
                    BSONObject packet = obj[$"m{i}"].Cast<BSONObject>();
                    string packetId = packet["ID"].stringValue;
                    switch (packetId)
                    {
                        /*case "HOP":
                            ector3 pp = ControllerHelper.worldController.player.transform.position;
                            ector2i pos = PositionConversions.ConvertWorldPointToMapPoint(pp.x, pp.y);
                            
                            nt hitX = packet["x"].int32Value;
                            nt hitY = packet["y"].int32Value;
                            
                            nt range = 2;
                            
                            f (pos.x < hitX) hitX -= range;
                            lse hitX += range;
                            
                            f (pos.y < hitY) hitY -= range;
                            lse hitY += range;
                            
                            acket["x"] = hitX;
                            //packet["y"] = hitY;
                            break;
                        case "mP":
                            if (packet.ContainsKey("t"))
                            {
                                long timeStamp = packet["t"].int64Value;
                                packet["t"] = new BSONValue(DateTime.Today.Ticks);
                            }
                            if (ghost && packet.ContainsKey("x"))
                            {
                                packet["x"] = new BSONValue(positionOfSetGhost.x);
                                packet["y"] = new BSONValue(positionOfSetGhost.y);
                                packet["d"] = new BSONValue(7);
                            }
                            break;
                        case "ST":
                            if (packet.ContainsKey("T"))
                            {
                                long timeStamp = packet["T"].int64Value;
                                packet["T"] = new BSONValue(DateTime.Today.Ticks);
                            }
                            break;
                        case "mp":
                            if (packet.ContainsKey("pM"))
                            {
                                byte[] mapPointBytes = packet["pM"].binaryValue;
                                List<Vector2i> mapPointList = new List<Vector2i>();
                                for (int j = 0; j < mapPointBytes.Length; j += 8)
                                {
                                    Vector2i v = new Vector2i(BitConverter.ToInt32(mapPointBytes, j), BitConverter.ToInt32(mapPointBytes, j+4));
                                    mapPointList.Add(v);
                                }
                                //int totalSplits = 0;
                                //List<byte> currentList = new List<byte>();
                                //bool wentToNewChunk = false;
                                //for (int j = 0; j < mapPointList.Count; j++)
                                //{
                                //    if (wentToNewChunk)
                                //    {
                                //        currentList.AddRange(BitConverter.GetBytes(mapPointList[j-1].x));
                                //        currentList.AddRange(BitConverter.GetBytes(mapPointList[j-1].y));
                                //        wentToNewChunk = false;
                                //    }
                                //    currentList.AddRange(BitConverter.GetBytes(mapPointList[j].x));
                                //    currentList.AddRange(BitConverter.GetBytes(mapPointList[j].y));
                                //    if (j % 2 == 0 && j != 0)
                                //    {
                                //        BSONObject newObj = new BSONObject();
                                //        newObj.Add("ID", new BSONValue("mp"));
                                //        newObj.Add("pM", new BSONValue(currentList.ToArray()));
                                //        if (totalSplits == 0)
                                //        {
                                //            obj[$"m{i}"] = newObj;
                                //            totalSplits++;
                                //        }
                                //        else
                                //        {
                                //            obj.Add($"m{obj["mc"].int32Value}", newObj);
                                //            obj["mc"] = new BSONValue(obj["mc"].int32Value + 1);
                                //            count++;
                                //        }
                                //        wentToNewChunk = true;
                                //        totalSplits++;
                                //        currentList.Clear();
                                //    }
                                //    else if (j + 1 >= mapPointList.Count && totalSplits != 0 && currentList.Count > 0)
                                //    {
                                //        ArkConsole.AddLog("adding last element");
                                //        BSONObject newObj = new BSONObject();
                                //        newObj.Add("ID", new BSONValue("mp"));
                                //        newObj.Add("pM", new BSONValue(currentList.ToArray()));
                                //        obj.Add($"m{obj["mc"].int32Value}", newObj);
                                //        obj["mc"] = new BSONValue(obj["mc"].int32Value + 1);
                                //        count++;
                                //        currentList.Clear();
                                //    }
                                //}
                                //if (totalSplits > 0)
                                //{
                                //    for (int j = 0; j < count; j++)
                                //    {
                                //        BSONObject packet2 = obj[$"m{j}"].Cast<BSONObject>();
                                //        string packetId2 = packet["ID"].stringValue;
                                //        if (packetId2 == "mP")
                                //        {
                                //            obj[$"m{j}"] = new BSONObject();
                                //            obj[$"m{j}"].Add("ID", new BSONValue("r"));
                                //        }
                                //    }
                                //}
                                //ArkConsole.AddLog("{");
                                //for (int j = 0; j < mapPointList.Count; j++)
                                //{
                                //    ArkConsole.AddLog($"\t{{{mapPointList[j].x}, {mapPointList[j].y}}},");
                                //}
                                ////packet["pM"] = mapPointList.GetRange(0, 16);
                                //ArkConsole.AddLog("}");
                            }
                            break;*/
                        default:
                            break;
                    }
                }
                //ArkConsole.AddLog(Utility.BsonFormat(obj));
                bytes = SimpleBSON.Dump(obj);
            }
            catch (Exception e)
            {
                ArkConsole.AddLog(e.ToString());
            }
            return bytes;
        }


        //public static void OnImGui()
        //{
        //    ImGui.Begin("C# Test Window");
        //    if (ImGui.Button("sex?"))
        //    {
        //        ArkConsole.AddLog("100% sex");
        //    }
        //    ImGui.End();
        //}
    }
    //internal class UnhollowerDetour : IManagedDetour
    //{
    //    private static readonly List<object> PinnedDelegates = new List<object>();
    //
    //    unsafe public T Detour<T>(IntPtr @from, T to) where T : Delegate
    //    {
    //        IntPtr* targetVarPointer = &from;
    //        PinnedDelegates.Add(to);
    //        ImportClass.hook((IntPtr)targetVarPointer, Marshal.GetFunctionPointerForDelegate(to));
    //        return Utility.GetDelegate(from, typeof(T)) as T;
    //    }
    //}
    public class CmdStr
    {
        public string command = "";
        public string fullArgs = "";
        public string fullMessage = "";
        public string[] args = { };
        public CmdStr(string f)
        {
            fullMessage = f;
            int index = fullMessage.IndexOf(' ');
            if (index > 0)
                command = fullMessage.Substring(0, index);
            else
                command = fullMessage;

            fullArgs = fullMessage.Remove(0, Math.Min(command.Length+1, fullMessage.Length));
            args = fullArgs.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
namespace Imports
{
    public class ImportClass
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern string getcwd();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void hook(System.IntPtr ppOriginal, System.IntPtr pTarget, IntPtr original);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool IsInWorld();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool QueryMiscBool(string text);

    }
    public class ArkConsole
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void AddLog(string s);
    }
}

