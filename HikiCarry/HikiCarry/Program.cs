using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Geometry = LeagueSharp.Common.Geometry;


namespace HikiCarry
{
    class Program
    {
        public const string ChampionName = "Vayne";

        //Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static bool PacketCast;
 

        //Menu
        public static Menu Config;

        private static Obj_AI_Hero Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
           

            //Create Spells
            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f);
            E.SetTargetted(0.25f, 1600f);
            R = new Spell(SpellSlot.R);


            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);





            //Menu
            Config = new Menu("HikiCarryVayne", "HikiCarry - Vayne", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            //Add orbwalker
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Info", "Info"));
            Config.SubMenu("Info").AddItem(new MenuItem("Author", "@Hikigaya"));


            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
           
           
           

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("RushEHarass", "Use E", true).SetValue(true));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));


            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushERange", "E Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

          
            Config.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
          
     
        }

       


       

        

       

        
        static bool IsAllyFountain(Vector3 position)
        {
            float fountainRange = 750;
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(true);
          //COMBO
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

                
                if (target.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2) && Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }

                if (E.IsReady() && Config.Item("RushECombo").GetValue<bool>())
                {
                    foreach (var en in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                    {
                        //credits VayneHunterRework

                        var ePred = E.GetPrediction(en);
                        int pushDist = 425;
                        var FinalPosition = ePred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -pushDist).To3D();

                        for (int i = 1; i < pushDist; i += (int)en.BoundingRadius)
                        {
                            Vector3 loc3 = ePred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -i).To3D();

                            if (loc3.IsWall() || IsAllyFountain(FinalPosition))
                                E.Cast(en);
                        }
                    }
                }


               
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
                var pT = HeroManager.Enemies.Find(enemy => enemy.IsValidTarget(E.Range));

                if (target.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2) && E.IsReady())
                {
                    if (pT != null && (pT is Obj_AI_Hero))
                    {
                        E.Cast(pT);
                    }
                }

   
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {

               
            }


        }

      


        private static void Drawing_OnDraw(EventArgs args)
        {

            var menuItem2 = Config.Item("RushERange").GetValue<Circle>();
            if (menuItem2.Active) Utility.DrawCircle(Player.Position, E.Range, menuItem2.Color);

        }
    }
}