using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleLucene.Impl;
using Lucene.Net.QueryParsers;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Search;
using Lucene.Net.Index;
using PingApp.Entity;
using System.Text.RegularExpressions;

namespace PingApp.Utility.Lucene {
    public class AppQuery : QueryBase {
        private static readonly Regex stopWords = new Regex(@"[!""\\:\[\]\{\}\(\)\^\+]", RegexOptions.Compiled);

        public Sort Sort { get; private set; }

        public AppQuery WithKeywords(string keywords) {
            if (!String.IsNullOrEmpty(keywords)) {
                BooleanQuery criteria = new BooleanQuery();
                keywords = stopWords.Replace(keywords, String.Empty);
                QueryParser nameParser = new QueryParser(Version.LUCENE_29, "Name", new PanGuAnalyzer());
                Query nameQuery = nameParser.Parse(keywords);
                nameQuery.SetBoost(10000);
                string[] fields = { "Description", "DeveloperName" };
                MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_29, fields, new PanGuAnalyzer());
                Query query = parser.Parse(keywords);
                criteria.Add(nameQuery, BooleanClause.Occur.SHOULD);
                criteria.Add(query, BooleanClause.Occur.SHOULD);
                AddQuery(criteria, BooleanClause.Occur.MUST);
            }
            return this;
        }

        public AppQuery WithCategory(int category) {
            if (category != 0) {
                Query query = new TermQuery(new Term("Category", category.ToString()));
                AddQuery(query, BooleanClause.Occur.MUST);
            }
            return this;
        }

        public AppQuery WithDeviceType(DeviceType deviceType) {
            if (deviceType != DeviceType.NotProvided) {
                BooleanQuery criteria = new BooleanQuery();
                criteria.Add(
                    NumericRangeQuery.NewIntRange(
                        "DeviceType", (int)deviceType, (int)deviceType, true, true), 
                    BooleanClause.Occur.SHOULD
                );
                criteria.Add(
                    NumericRangeQuery.NewIntRange(
                        "DeviceType", (int)DeviceType.Universal, (int)DeviceType.Universal, true, true),
                    BooleanClause.Occur.SHOULD
                );
                AddQuery(criteria, BooleanClause.Occur.MUST);
            }
            return this;
        }

        public AppQuery WithLanguagePriority(int priority) {
            if (priority != 0) {
                Query query = NumericRangeQuery.NewIntRange("LanguagePriority", priority, null, true, true);
                AddQuery(query, BooleanClause.Occur.MUST);
            }
            return this;
        }

        public AppQuery SortByRelavance() {
            Sort = Sort.RELEVANCE;
            return this;
        }

        public AppQuery SortByName(bool desc) {
            Sort = new Sort(new SortField("NameForSort", SortField.STRING, desc));
            return this;
        }

        public AppQuery SortByPrice(bool desc) {
            Sort = new Sort(new SortField("Price", SortField.FLOAT, desc));
            return this;
        }

        public AppQuery SortByUpdate(bool desc) {
            Sort = new Sort(new SortField("LastValidUpdateTime", SortField.LONG, desc));
            return this;
        }

        public AppQuery SortByRating(bool desc) {
            Sort = new Sort(new SortField("Rating", SortField.INT, desc));
            return this;
        }

        public AppQuery SortBy(AppSortType type, bool desc) {
            switch (type) {
                case AppSortType.Relevance:
                    SortByRelavance();
                    break;
                case AppSortType.Name:
                    SortByName(desc);
                    break;
                case AppSortType.Price:
                    SortByPrice(desc);
                    break;
                case AppSortType.Update:
                    SortByUpdate(desc);
                    break;
                case AppSortType.Rating:
                    SortByRating(desc);
                    break;
                default:
                    break;
            }
            return this;
        }

        public AppQuery() {
            SortByRelavance();
        }
    }
}
