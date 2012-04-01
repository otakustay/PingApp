using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using PingApp.Entity;

namespace PingApp.Repository.NHibernate {
    public class AppRepository : IAppRepository {
        private readonly ISession session;

        public AppRepository(ISession session) {
            this.session = session;
        }

        public AppBrief RetrieveBrief(int id) {
            return session.Get<AppBrief>(id);
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            ICollection<App> result = session.QueryOver<App>()
                .Where(Restrictions.InG("Id", required))
                .List();

            return result;
        }

        public IDictionary<int, string> RetrieveHash(int offset, int limit) {
            ICollection<object[]> result = session.CreateCriteria<AppBrief>()
                .SetFirstResult(offset)
                .SetMaxResults(limit)
                .SetProjection(Projections.Property<AppBrief>(b => b.Id), Projections.Property<AppBrief>(b => b.Hash))
                .List<object[]>();

            return result.ToDictionary(o => (int)o[0], o => (string)o[1]);
        }

        public IDictionary<int, string> RetrieveHash(IEnumerable<int> apps) {
            throw new NotImplementedException();
        }

        public ICollection<int> RetrieveIdentities(int offset, int limit) {
            IList<int> list = session.CreateCriteria<AppBrief>()
                .SetFirstResult(offset)
                .SetMaxResults(limit)
                .SetProjection(Projections.Property<AppBrief>(b => b.Id))
                .List<int>();

            return list;
        }

        public ISet<int> FindExists(IEnumerable<int> apps) {
            IList<int> list = session.CreateCriteria<AppBrief>()
                .Add(Restrictions.InG("Id", apps))
                .SetProjection(Projections.Property<AppBrief>(a => a.Id))
                .List<int>();

            return new HashSet<int>(list);
        }

        public void Save(App app) {
            session.Save(app);
        }

        public void Save(AppBrief brief) {
            session.Save(brief);
        }

        public void Update(App app) {
            session.Merge(app);
        }

        public void Update(AppBrief brief) {
            session.Merge(brief);
        }
    }
}
