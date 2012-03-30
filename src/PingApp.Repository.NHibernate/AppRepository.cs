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

        public ISet<int> FindExists(IEnumerable<int> apps) {
            IList<int> list = session.CreateCriteria<AppBrief>()
                .Add(Restrictions.InG("Id", apps))
                .SetProjection(Projections.Property<AppBrief>(a => a.Id))
                .List<int>();

            return new HashSet<int>(list);
        }

        public ICollection<App> Retrieve(IEnumerable<int> required) {
            ICollection<App> result = session.QueryOver<App>()
                .Where(Restrictions.InG("Id", required))
                .List();

            return result;
        }

        public void Save(App app) {
            session.Save(app);
        }

        public void Update(App app) {
            session.Update(app);
        }
    }
}
