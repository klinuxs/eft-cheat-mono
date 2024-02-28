using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;
using EFT;
using Comfort.Common;
using EFT.NextObservedPlayer;
using BSG.CameraEffects;
using System.Reflection;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Visual;
using Diz.Skinning;
using UnityEngine.Rendering;
using System.Runtime.Remoting.Messaging;
using EFT.UI;
using UnityDiagnostics;

namespace BasicMonoSDK
{
    public static class Globals
    {
        public static Camera MainCamera;
        public static GameWorld GameWorld;
        public static Player LocalPlayer;

        public static List<IPlayer> Players = new List<IPlayer>();
        public static List<Throwable> Grenades = new List<Throwable>();

        public static bool IsMenuOpen = false;

        public static Vector3 W2S(Vector3 pos)
        {
            if (!Globals.MainCamera)
                return new Vector3(0, 0, 0);

            var screenPoint = Globals.MainCamera.WorldToScreenPoint(pos);
            var scale = Screen.height / (float)Globals.MainCamera.scaledPixelHeight;
            screenPoint.y = Screen.height - screenPoint.y * scale;
            screenPoint.x *= scale;
            if (screenPoint.x < -10 || screenPoint.y < -10 || screenPoint.z < 0)
                return new Vector3(0, 0, 0);

            return screenPoint;

        }

        // Not the best way but works.
        public static bool IsBossByName(string name)
        {
            if (name == "Килла" || name == "Решала" || name == "Глухарь" || name == "Штурман" || name == "Санитар" || name == "Тагилла" || name == "Зрячий" || name == "Кабан" || name == "Big Pipe" || name == "Birdeye" || name == "Knight" || name == "Дед Мороз" || name == "Коллонтай")
                return true;
            else
                return false;
        }

        internal static void SetPrivateField(this object obj, string name, object value)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(name, bindingFlags);
            fieldInfo.SetValue(obj, value);
        }
    }

    public static class MenuVars
    {
        public static bool EnableESP = true;
        public static bool EnableName = true;
        public static bool EnableBox = true;
        public static bool EnableLines = false;
        public static bool EnableChams = false;//no
        public static bool EnableGrenadeESP = true;
        public static bool watermark = true;
        public static bool norecoil = true;
        public static bool novsior = false;
        public static bool nosway = false;
        public static bool crosshair = true;



        public static bool BigHead = false;
        public static float MaxScavRenderDistance = 200f;

        public static bool ForceNightVision = false;
        public static bool ForceThermalVision = false;
    }

    public class Cheat : MonoBehaviour
    {
        public static Shader? OutlineShader { get; private set; }

        private void OnGUI()
        {

            if (MenuVars.watermark)
            {
                GUI.Label(new Rect(10f, 10f, 300f, 100f), "Brandon Tarrant's Menu");

            }

            if (Globals.IsMenuOpen)
            {
                GUI.Box(new Rect(100f, 50f, 400f, 400f), " ");
                GUILayout.BeginArea(new Rect(100f, 50f, 400f, 400f));

                MenuVars.EnableESP = GUILayout.Toggle(MenuVars.EnableESP, "Player ESP");
                if (MenuVars.EnableESP == true)
                {
                    MenuVars.EnableName = GUILayout.Toggle(MenuVars.EnableName, "Player Name");
                    MenuVars.EnableBox = GUILayout.Toggle(MenuVars.EnableBox, "Player Box");
                }
                MenuVars.EnableGrenadeESP = GUILayout.Toggle(MenuVars.EnableGrenadeESP, "Grenade ESP");
               // MenuVars.EnableChams = GUILayout.Toggle(MenuVars.EnableChams, "Player Chams (broken ASF) ");
                MenuVars.norecoil = GUILayout.Toggle(MenuVars.norecoil, "No recoil");
                MenuVars.nosway = GUILayout.Toggle(MenuVars.nosway, "No sway");
                MenuVars.novsior = GUILayout.Toggle(MenuVars.novsior, "No visor");
                MenuVars.crosshair = GUILayout.Toggle(MenuVars.crosshair, "crosshair");
                MenuVars.ForceNightVision = GUILayout.Toggle(MenuVars.ForceNightVision, "Force Night Vision");
                MenuVars.ForceThermalVision = GUILayout.Toggle(MenuVars.ForceThermalVision, "Force Thermal Vision");
                MenuVars.watermark = GUILayout.Toggle(MenuVars.watermark, "Watermark");

                GUILayout.EndArea();

            }

            if (MenuVars.EnableESP)
                PlayerESP();
                


        //    if (MenuVars.BigHead)
        //        BigHeads();

            if (MenuVars.EnableGrenadeESP)
                GrenadeESP();

            if (MenuVars.norecoil)
                norecoil();

            if (MenuVars.nosway)
                nosway();

            if (MenuVars.novsior)
                novisor();

            if (MenuVars.crosshair)
                crosshair();



        }

        private void Update()
        {
            float LastCacheTime = 0f;

            if (Input.GetKeyUp(KeyCode.Insert))
                Globals.IsMenuOpen = !Globals.IsMenuOpen;

            if (Globals.IsMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Updates every 0.25f seconds.
            if (Time.time >= LastCacheTime)
            {
                if (Camera.main != null)
                    Globals.MainCamera = Camera.main;

                if (Singleton<GameWorld>.Instance != null)
                    Globals.GameWorld = Singleton<GameWorld>.Instance;

                if (Globals.GameWorld != null && Globals.GameWorld.RegisteredPlayers != null)
                {
                    List<IPlayer> RegisteredPlayers = Globals.GameWorld.RegisteredPlayers;

                    Globals.Players.Clear();
                    Globals.Grenades.Clear();

                    foreach (var Player in RegisteredPlayers)
                    {
                        if (Player == null)
                            continue;
                       

                        if (Player.IsYourPlayer)
                            Globals.LocalPlayer = Player as Player;

                        Globals.Players.Add(Player);
                    }

                    for (int i = 0; i < Globals.GameWorld.Grenades.Count; i++)
                    {
                        Throwable Throwables = Globals.GameWorld.Grenades.GetByIndex(i);
                        if (Throwables == null)
                            continue;

                        Globals.Grenades.Add(Throwables);
                    }
                }
                else
                {
                    Globals.Players.Clear();
                    Globals.Grenades.Clear();
                }

                LastCacheTime = Time.time + 0.25f;
            }

            if (Globals.MainCamera != null)
            {
                Globals.MainCamera.GetComponent<NightVision>().SetPrivateField("_on", MenuVars.ForceNightVision);

                Globals.MainCamera.GetComponent<ThermalVision>().On = MenuVars.ForceThermalVision;

            }
        }

        private void Awake()
        {


            DontDestroyOnLoad(this);
        }





        void crosshair()
        {
            var centerx = Screen.width / 2;
            var centery = Screen.height / 2;
            var texture = Texture2D.whiteTexture;
            GUI.DrawTexture(new Rect(centerx - 5, centery, 5 * 2 + 2, 2), texture);
            GUI.DrawTexture(new Rect(centerx, centery - 5, 2, 5 * 2 + 2), texture);
        }

        void nosway()
        {
            var lp = Globals.LocalPlayer;
            var weapon = lp.ProceduralWeaponAnimation;
            if (weapon == null)
                return;
            weapon.Breath.Intensity = 0;
            weapon.Walk.Intensity = 0;
            weapon.AimSwayMax = Vector3.zero;
            weapon.AimSwayMin = Vector3.zero;
            weapon.ForceReact.Intensity = 0;
            weapon.WalkEffectorEnabled = false;
        }

        /*   void nospread()
           {
               var lp = Globals.LocalPlayer;
               var weapon = lp.ProceduralWeaponAnimation;
               if (weapon == null)
                   return;

           }*/

        void norecoil()
        {
            var lp = Globals.LocalPlayer;
            var weapon = lp.ProceduralWeaponAnimation;

            if (Globals.GameWorld == null || lp == null || Globals.MainCamera == null || Globals.Players.IsNullOrEmpty())
                return;

            if (weapon == null)
                return;

            weapon.Shootingg.NewShotRecoil.RecoilEffectOn = false;

        }
        void novisor()
        {

            var camera = Globals.MainCamera;

            var component = camera.GetComponent<VisorEffect>();
            if (component == null || Mathf.Abs(component.Intensity - Convert.ToInt32(!true)) < Mathf.Epsilon)
                return;

            component.Intensity = Convert.ToInt32(!true);
        }

        void GrenadeESP()
        {
            if (Globals.GameWorld == null || Globals.LocalPlayer == null || Globals.MainCamera == null || Globals.Grenades.IsNullOrEmpty())
                return;

            for (int i = 0; i < Globals.Grenades.Count(); i++)
            {
                Throwable Throwables = Globals.Grenades.ElementAt(i);

                if (Throwables == null)
                    continue;

                Grenade Grenades = Throwables as Grenade;

                if (Grenades == null)
                    continue;

                Vector3 GrenadePos = Globals.W2S(Grenades.transform.position);

                if (GrenadePos == Vector3.zero)
                    continue;

                GUI.Label(new Rect(GrenadePos.x, GrenadePos.y, 200f, 25f), Grenades.name);
            }
        }
        private void PlayerESP()
        {
            if (Globals.GameWorld == null || Globals.LocalPlayer == null || Globals.MainCamera == null || Globals.Players.IsNullOrEmpty())
                return;

            for (int i = 0; i < Globals.Players.Count(); i++)
            {
                IPlayer _Player = Globals.Players.ElementAt(i);

                if (_Player == null)
                    continue;

                // FOR ONLINE RAIDS
                if (_Player.GetType() != typeof(ObservedPlayerView))
                    continue;

                ObservedPlayerView Player = _Player as ObservedPlayerView;
               if (Player == null)
                   continue;
                // FOR ONLINE RAIDS

                // FOR OFFLINE RAIDS
               // Player OfflinePlayer = _Player as Player;
            //    if (Player == null)
             //       continue;
                // FOR OFFLINE RAIDS
                // im gonna do this here since idk c# and how to do other classes :(

                Vector3 Head = Globals.W2S(Player.PlayerBones.Head.position);
                Vector3 Foot = Globals.W2S(Player.PlayerBones.KickingFoot.position);

                if (Head == Vector3.zero)
                    continue;
                if (Foot == Vector3.zero)
                    continue;

                float Distance2Player = Vector3.Distance(Globals.MainCamera.transform.position, Player.PlayerBones.Head.position);

                bool IsScav = false;
                if (Player.ObservedPlayerController != null && Player.ObservedPlayerController.InfoContainer != null)
                    IsScav = Player.ObservedPlayerController.InfoContainer.Side == EPlayerSide.Savage;

                // Not the best way, see if you (the reader) can improve this.
                bool IsBoss = Globals.IsBossByName(Player.NickName.Localized());

                if (IsBoss)
                {
                    if (MenuVars.EnableName == true)
                    {
                        GUI.color = Color.green;
                        GUI.Label(new Rect(Head.x, Head.y, 200f, 25f), "BOSS " + Math.Round(Distance2Player).ToString() + "M");
                        GUI.color = Color.white;
                    }
                        DrawBoxESP(Head, Foot, Color.green);
                    
                }
                else if (IsScav)
                {
                    if (Distance2Player <= MenuVars.MaxScavRenderDistance)
                    {
                        if (MenuVars.EnableName == true)
                        {
                            GUI.color = Color.magenta;
                            GUI.Label(new Rect(Head.x, Head.y, 200f, 25f), "SCAV " + Math.Round(Distance2Player).ToString() + "M");
                            GUI.color = Color.white;
                        }
                        if (MenuVars.EnableBox == true)
                        {
                            DrawBoxESP(Head, Foot, Color.magenta);
                        }
                    }
                }
                else
                {
                    if (MenuVars.EnableName == true)
                    {
                        GUI.Label(new Rect(Head.x, Head.y, 200f, 25f), Player.NickName + " " + Math.Round(Distance2Player).ToString() + "M");
                    }
                    if (MenuVars.EnableBox == true)
                    {
                        DrawBoxESP(Head, Foot, Color.white);
                    }
                    

                }
            }
        }

        public void DrawBoxESP(Vector3 footpos, Vector3 headpos, Color color) //Rendering the boxie
        {
            float height = headpos.y - footpos.y;
            float widthOffset = 2f;
            float width = height / widthOffset;

            if (MenuVars.EnableBox == true)
            {
                Render.DrawBox(footpos.x, footpos.y, width, height, color, 2.0f);
            }
            

            //Snapline annoying as fucking shit
            if (MenuVars.EnableLines == true)
            {
                Render.DrawLine(new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), new Vector2(footpos.x, (float)Screen.height - footpos.y), color, 2f);
            }
            
        }
    }
}
