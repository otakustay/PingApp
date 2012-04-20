using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public class Category {
        private int id;

        private string name;

        private string alias;

        private readonly bool frozen = false;

        public int Id {
            get {
                return id;
            }
            set {
                if (frozen) {
                    throw new InvalidOperationException("Entity is fixed");
                }
                id = value;
            }
        }

        public string Name {
            get {
                return name;
            }
            set {
                if (frozen) {
                    throw new InvalidOperationException("Entity is fixed");
                }
                name = value;
            }
        }

        public string Alias {
            get {
                return alias;
            }
            set {
                if (frozen) {
                    throw new InvalidOperationException("Entity is fixed");
                }
                alias = value;
            }
        }

        private static readonly Dictionary<int, Category> all;

        private static readonly Dictionary<int, Category> games;

        private static readonly Dictionary<string, Category> byAlias;

        private static readonly Dictionary<int, Category> translator;

        public Category() {
        }

        private Category(int id, string name, string alias) {
            Id = id;
            Name = name;
            Alias = alias;
            frozen = true;
        }

        public static Category Get(int id) {
            if (all.ContainsKey(id)) {
                return all[id];
            }
            else if (translator.ContainsKey(id)) {
                return translator[id];
            }
            else {
                return null;
            }
        }

        public static Category Get(string name) {
            if (String.IsNullOrEmpty(name)) {
                return null;
            }
            return byAlias.ContainsKey(name) ? byAlias[name] : null;
        }

        public static IEnumerable<Category> All {
            get {
                return all.Values;
            }
        }

        public static IEnumerable<Category> Games {
            get {
                return games.Values;
            }
        }

        static Category() {
            all = new Dictionary<int, Category>() {
                { 6018, new Category(6018, "图书", "books") },
                { 6000, new Category(6000, "商业", "business") },
                { 6022, new Category(6022, "目录", "catalogs") },
                { 6017, new Category(6017, "教育", "education") },
                { 6016, new Category(6016, "娱乐", "entertainment") },
                { 6015, new Category(6015, "财务", "finance") },
                { 6014, new Category(6014, "游戏", "games") },
                { 6013, new Category(6013, "健康健美", "health") },
                { 6012, new Category(6012, "生活", "lifestyle") },
                { 6020, new Category(6020, "医疗", "medical") },
                { 6011, new Category(6011, "音乐", "music") },
                { 6010, new Category(6010, "导航", "navigation") },
                { 6009, new Category(6009, "新闻", "news") },
                { 6021, new Category(6021, "报刊杂志", "newsstand") },
                { 6008, new Category(6008, "摄影与录像", "photography") },
                { 6007, new Category(6007, "效率", "productivity") },
                { 6006, new Category(6006, "参考", "reference") },
                { 6005, new Category(6005, "社交", "social") },
                { 6004, new Category(6004, "体育", "sports") },
                { 6003, new Category(6003, "旅行", "travel") },
                { 6002, new Category(6002, "工具", "utilities") },
                { 6001, new Category(6001, "天气", "weather") }
            };
            games = new Dictionary<int, Category>() {
                { 7001, new Category(7001, "动作游戏", "action") },
                { 7002, new Category(7002, "探险游戏", "adventure") },
                { 7003, new Category(7003, "街机游戏", "arcade") },
                { 7004, new Category(7004, "桌面游戏", "board") },
                { 7005, new Category(7005, "扑克牌游戏", "card") },
                { 7006, new Category(7006, "娱乐场游戏", "casino") },
                { 7007, new Category(7007, "骰子游戏", "dice") },
                { 7008, new Category(7008, "教育游戏", "educational") },
                { 7009, new Category(7009, "家庭游戏", "family") },
                { 7010, new Category(7010, "儿童游戏", "kids") },
                { 7011, new Category(7011, "音乐", "music") },
                { 7012, new Category(7012, "智力游戏", "puzzle") },
                { 7013, new Category(7013, "赛车游戏", "racing") },
                { 7014, new Category(7014, "角色扮演游戏", "role Playing") },
                { 7015, new Category(7015, "模拟游戏", "simulation") },
                { 7016, new Category(7016, "体育", "sports") },
                { 7017, new Category(7017, "策略游戏", "strategy") },
                { 7018, new Category(7018, "小游戏", "trivia") },
                { 7019, new Category(7019, "文字游戏", "word") }
            };
            translator = new Dictionary<int, Category>() {
                { 12001, all[6000] },
                { 12003, all[6017] },
                { 12004, all[6016] },
                { 12005, all[6015] },
                { 12006, all[6014] },
                { 12007, all[6013] },
                { 12008, all[6012] },
                { 12009, all[6020] },
                { 12011, all[6011] },
                { 12012, all[6009] },
                { 12013, all[6008] },
                { 12014, all[6007] },
                { 12015, all[6006] },
                { 12016, all[6005] },
                { 12017, all[6004] },
                { 12018, all[6003] },
                { 12019, all[6002] },
                { 12021, all[6001] }
            };
            foreach (int id in Enumerable.Range(7001, 19)) {
                translator.Add(id, all[6014]);
            }
            byAlias = All.ToDictionary(c => c.Alias);
        }

        public override bool Equals(object obj) {
            Category other = obj as Category;
            if (other == null) {
                return false;
            }

            return Id == other.Id;
        }

        public override int GetHashCode() {
            return id;
        }
    }
}
