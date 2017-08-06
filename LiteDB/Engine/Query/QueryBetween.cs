﻿using System;
using System.Collections.Generic;

namespace LiteDB
{
    internal class QueryBetween : Query
    {
        private BsonValue _start;
        private BsonValue _end;

        public QueryBetween(string field, BsonValue start, BsonValue end)
            : base(field)
        {
            _start = start;
            _end = end;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            // define order
            var order = _start.CompareTo(_end) <= 0 ? Query.Ascending : Query.Descending;

            // find first indexNode
            var node = indexer.Find(index, _start, true, order);

            // navigate using next[0] do next node - if less or equals returns
            while (node != null)
            {
                var diff = node.Key.CompareTo(_end);

                if (diff == 0 || diff != order)
                {
                    yield return node;
                }
                else
                {
                    break;
                }

                node = indexer.GetNode(node.NextPrev(0, order));
            }
        }

        internal override bool FilterDocument(BsonDocument doc)
        {
            var value = doc.Get(this.Field);

            return value.CompareTo(_start) >= 0 && value.CompareTo(_end) <= 0;
        }

        public override string ToString()
        {
            return string.Format("{0} between {1} and {2}", this.Field, _start, _end);
        }
    }
}