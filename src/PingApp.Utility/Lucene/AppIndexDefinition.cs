using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingApp.Entity;
using SimpleLucene;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using PingApp.Utility;

namespace PingApp.Utility.Lucene {
    public class AppIndexDefinition : IIndexDefinition<App> {
        public Document Convert(App app) {
            Document doc = new Document();
            Field id = new Field("Id", app.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
            Field name = new Field("Name", app.Brief.Name, Field.Store.NO, Field.Index.ANALYZED);
            Field description = new Field("Description", app.Description ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED);
            Field developerName = new Field("DeveloperName", app.Brief.Developer.Name ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED);
            Field category = new Field("Category", app.Categories.JoinField(" ", c => c.Id), Field.Store.NO, Field.Index.ANALYZED);
            NumericField deviceType = new NumericField("DeviceType", Field.Store.NO, true);
            deviceType.SetIntValue((int)app.Brief.DeviceType);
            NumericField languagePriority = new NumericField("LanguagePriority", Field.Store.NO, true);
            languagePriority.SetIntValue(app.Brief.LanguagePriority);

            doc.AddField(id);
            doc.AddField(name);
            doc.AddField(description);
            doc.AddField(developerName);
            doc.AddField(category);
            doc.AddField(deviceType);
            doc.AddField(languagePriority);

            // 排序字段
            Field nameForSort = new Field("NameForSort", app.Brief.Name, Field.Store.NO, Field.Index.NOT_ANALYZED);
            NumericField lastValidUpdateTimee = new NumericField("LastValidUpdateTime", Field.Store.NO, true);
            lastValidUpdateTimee.SetLongValue(app.Brief.LastValidUpdate.Time.Ticks);
            NumericField price = new NumericField("Price", Field.Store.NO, true);
            price.SetFloatValue(app.Brief.Price);
            NumericField ratingCount = new NumericField("Rating", Field.Store.NO, true);
            ratingCount.SetIntValue(app.Brief.AverageUserRatingForCurrentVersion.HasValue ? (int)app.Brief.AverageUserRatingForCurrentVersion : 0);

            doc.AddField(nameForSort);
            doc.AddField(lastValidUpdateTimee);
            doc.AddField(price);
            doc.AddField(ratingCount);

            return doc;
        }

        public Term GetIndex(App app) {
            Term term = new Term("Id", app.Id.ToString());
            return term;
        }
    }
}
