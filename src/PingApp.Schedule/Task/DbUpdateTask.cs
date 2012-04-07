﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Diagnostics;
using System.IO;
using PingApp.Entity;
using MySql.Data.MySqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Data;
using PingApp.Schedule.Storage;
using PingApp.Utility;
using System.Data.Common;
using Ninject.Syntax;
using Ninject;
using NHibernate;
using PingApp.Repository.NHibernate;
using Ninject.Activation.Blocks;
using PingApp.Repository;
using PingApp.Repository.NHibernate.Dependency;
using System.Collections;

namespace PingApp.Schedule.Task {
    class DbUpdateTask : TaskNode {
        private readonly DbCheckType checkType;

        private readonly bool checkOffUpdates;

        private readonly IKernel kernel;

        private readonly List<AppUpdate> validUpdates = new List<AppUpdate>();

        public DbUpdateTask(DbCheckType checkType, bool checkOffUpdates, IKernel kernel) {
            this.checkType = checkType;
            this.checkOffUpdates = checkOffUpdates;
            this.kernel = kernel;
        }

        protected override IStorage RunTask(IStorage input) {
            Log.Info("Start update to database using --check-type={0} --check-off-updates={1}", checkType, checkOffUpdates);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Directory.CreateDirectory(LogRoot);
            IStorage output = Action == ActionType.Initialize ?
                (IStorage)new FileSystemStorage(Path.Combine(LogRoot, "Output")) : new MemoryStorage();
            if (Action != ActionType.Initialize) {
                // UpdateWithCheck用
                output.Add("New", new List<App>());
                output.Add("Updated", new List<App>());
            }
            Buffer<App> buffer = new Buffer<App>(Program.BatchSize + 200, list => UpdateToDb(list, output));

            while (input.HasMore) {
                App[] apps = input.Get<App[]>();
                buffer.AddRange(apps);
            }
            buffer.Flush();

            // 对于增量更新的情况，再处理一下下架的应用，就是not-found.txt中的内容
            if (checkOffUpdates) {
                List<int> notFound = input.Get<List<int>>("NotFound");
                AddOffUpdates(notFound);
            }

            if (checkType == DbCheckType.CheckForUpdate) {
                Log.Info("Found {0} valid updates", validUpdates.Count);
                output.Add("Updates", validUpdates);
            }

            watch.Stop();
            Log.Info("Work done using {0:00}:{1:00}:{2:00}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);

            return output;
        }

        private void UpdateToDb(App[] list, IStorage output) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            using (SessionStore sessionStore = new SessionStore()) {
                kernel.Rebind<IDictionary>().ToConstant(sessionStore);
                RepositoryEmitter repository = kernel.Get<RepositoryEmitter>();
                sessionStore.BeginTransaction();
                try {
                    int count = checkType == DbCheckType.ForceInsert ?
                        UpdateWithNoCheck(list, output, repository) : // 纯写入
                        UpdateWithCheck(list, output, repository); // 检查更新项
                    
                    sessionStore.CommitTransaction();
                    watch.Stop();
                    if (count >= 0) {
                        Log.Info("{0} out of {1} records commited via type using {2}ms", count, list.Length, watch.ElapsedMilliseconds);
                    }
                }
                catch (Exception ex) {
                    Log.ErrorException("Update to db failed with --db-check-type=" + checkType, ex);
                    sessionStore.RollbackTransaction();
                }
            }
        }

        private int UpdateWithCheck(App[] list, IStorage output, RepositoryEmitter repository) {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            List<App> updated = new List<App>();
            List<App> added = new List<App>();

            if (checkType == DbCheckType.DiscardUpdate) {
                // 只找出需要插入的那些
                // 虽然前面RssFeed之类的已经找过，但这里有事务，更安全
                ISet<int> exists;
                try {
                    exists = repository.App.FindExists(list.Select(a => a.Id));
                }
                catch (Exception ex) {
                    Log.ErrorException("Error checking apps with db", ex);
                    string filename = Path.Combine(LogRoot, "error-" + list.GetHashCode() + "-" + list.Length + ".txt");
                    File.WriteAllText(filename, Utility.JsonSerialize(list), Encoding.UTF8);
                    return -1;
                }

                watch.Stop();
                Log.Info("{0} records checked with db using {1}ms, {2} new apps assumed", exists.Count, watch.ElapsedMilliseconds, list.Length - exists.Count);

                foreach (App app in list.Where(a => !exists.Contains(a.Id))) {
                    InsertNewApp(app, repository);
                    added.Add(app);
                }
            }
            else {
                // 全量更新
                Dictionary<int, App> compareBase;
                try {
                    compareBase = repository.App.Retrieve(list.Select(a => a.Id)).ToDictionary(a => a.Id);
                }
                catch (Exception ex) {
                    Log.ErrorException("Error retrieving apps from db", ex);
                    string filename = Path.Combine(LogRoot, "error-" + list.GetHashCode() + "-" + list.Length + ".txt");
                    File.WriteAllText(filename, Utility.JsonSerialize(list), Encoding.UTF8);
                    return -1;
                }

                watch.Stop();
                Log.Info("{0} records retrieved from db using {1}ms, {2} new apps assumed", compareBase.Count, watch.ElapsedMilliseconds, list.Length - compareBase.Count);

                foreach (App app in list) {
                    // 更新
                    if (compareBase.ContainsKey(app.Id)) {
                        App origin = compareBase[app.Id];
                        if (!origin.Equals(app)) {
                            try {
                                UpdateApp(origin, app, repository);
                                updated.Add(app);
                            }
                            catch (DbException ex) {
                                Log.ErrorException("Failed update app " + app.Id + " manually change hash to " + app.Brief.Hash, ex);
                            }
                        }
                    }
                    // 新建，理论上不会有这一环节
                    else {
                        Log.Warn("Weired hit at insert branch on --check-type=CheckForUpdate");
                        InsertNewApp(app, repository);
                        added.Add(app);
                    }
                }
            }

            output.Get<List<App>>("New").AddRange(added);
            output.Get<List<App>>("Updated").AddRange(updated);

            Log.Debug("Added: " + String.Join(",", added.Select(a => a.Id).ToArray()));
            Log.Debug("Updated: " + String.Join(",", updated.Select(a => a.Id).ToArray()));

            return added.Count + updated.Count;
        }

        private int UpdateWithNoCheck(App[] list, IStorage output, RepositoryEmitter repository) {
            try {
                foreach (App app in list) {
                    InsertNewApp(app, repository);
                }

            }
            catch (Exception ex) {
                Log.ErrorException("InsertWithNoCheck failed", ex);
                File.WriteAllText(
                    Path.Combine(LogRoot, "error-" + list.GetHashCode() + "-" + list.Length + ".txt"),
                    Utility.JsonSerialize(list),
                    Encoding.UTF8
                );
            }

            if (Action == ActionType.Initialize) {
                output.Add(list);
            }

            return list.Length;
        }

        private void InsertNewApp(App app, RepositoryEmitter repository) {
            app.Brief.LastValidUpdate = new AppUpdate() {
                Time = DateTime.Now,
                Type = AppUpdateType.New,
                NewValue = String.Empty,
                OldValue = String.Empty
            };
            // App和AppBrief之间没有Cascade设置
            repository.App.Save(app);
            AppUpdate updateForNew = new AppUpdate() {
                App = app.Id,
                Time = app.Brief.ReleaseDate.Date,
                Type = AppUpdateType.New,
                OldValue = app.Brief.Version + ", $" + app.Brief.Price
            };
            repository.AppUpdate.Save(updateForNew);
            AppUpdate updateForAdd = new AppUpdate() {
                App = app.Id,
                Time = app.Brief.LastValidUpdate.Time,
                Type = AppUpdateType.AddToNote,
                OldValue = app.Brief.Version + ", $" + app.Brief.Price
            };
            repository.AppUpdate.Save(updateForAdd);
        }

        private void UpdateApp(App origin, App app, RepositoryEmitter repository) {
            List<AppUpdate> updates = origin.Brief.CheckForUpdate(app.Brief);
            validUpdates.AddRange(updates.Where(u => AppUpdate.IsValidUpdate(u.Type)));
            // 添加更新信息
            foreach (AppUpdate update in updates) {
                repository.AppUpdate.Save(update);
                if (AppUpdate.IsValidUpdate(update.Type)) {
                    app.Brief.LastValidUpdate = update;
                }
            }
            if (app.Brief.LastValidUpdate == null) {
                app.Brief.LastValidUpdate = origin.Brief.LastValidUpdate;
            }

            // App和AppBrief之间没有Cascade设置
            repository.App.Update(app);

            // 更新Track数据
            int rows = repository.AppTrack.ResetReadStatusByApp(app.Id);
            Log.Debug("{0} track infos updated for app {1} - {2}", rows, app.Id, app.Brief.Name);
        }

        private void AddOffUpdates(List<int> list) {
            Log.Info("Start add off updates");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            DateTime now = DateTime.Now;
            int count = 0;

            using (SessionStore sessionStore = new SessionStore()) {
                kernel.Rebind<IDictionary>().ToConstant(sessionStore);
                RepositoryEmitter repository = kernel.Get<RepositoryEmitter>();
                sessionStore.BeginTransaction();
                try {
                    foreach (int id in list) {
                        App app = repository.App.Retrieve(id);
                        // 先确定是不是已经Off了
                        if (app.Brief.IsActive) {
                            AppUpdate update = new AppUpdate() {
                                App = id,
                                Time = now,
                                Type = AppUpdateType.Off,
                                OldValue = String.Format("{0}, ￥{1}", app.Brief.Version, app.Brief.Price)
                            };
                            repository.AppUpdate.Save(update);
                            count++;

                            // 更新为IsValid为false
                            app.Brief.IsActive = false;
                            repository.App.Update(app);
                        }
                        else {
                            Log.Trace("Off update for {0} is dismissed because the app is already invalid", id);
                        }
                        // Off不作为ValidUpdate更新
                        sessionStore.CommitTransaction();
                    }
                }
                catch (Exception ex) {
                    Log.ErrorException("Add off updates failed", ex);
                    sessionStore.RollbackTransaction();
                }
            }

            watch.Stop();
            Log.Info("{0} off updates added using {1}ms, {2} dismissed", count, watch.ElapsedMilliseconds, list.Count - count);
        }
    }
}
